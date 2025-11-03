using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels.TaskReminderFilterViewModel;

namespace MahERP.DataModelLayer.Repository
{
    public class MainDashboardRepository : IMainDashboardRepository
    {
        private readonly AppDbContext _context;
        private readonly ITaskRepository _taskRepository; // ⭐ اضافه شده
        private readonly IMemoryCache _memoryCache;
        private readonly ActivityLoggerService _activityLogger;

        public MainDashboardRepository(
            AppDbContext context,
            ITaskRepository taskRepository, // ⭐ اضافه شده
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger)
        {
            _context = context;

            _taskRepository = taskRepository; // ⭐ اضافه شده
            _memoryCache = memoryCache;
            _activityLogger = activityLogger;
        }

        /// <summary>
        /// آماده‌سازی مدل داشبورد برای کاربر
        /// </summary>
        public async Task<DashboardViewModel> PrepareDashboardModelAsync(string userId, string userName)
        {
            var model = new DashboardViewModel
            {
                UserId = userId,
                UserName = userName,
                LastLoginDate = DateTime.Now // باید از دیتابیس بیاید
            };

            // بارگذاری آمار کلی
            var stats = await GetUserDashboardStatsAsync(userId);
            model.TasksStats = stats.TasksStats;
            model.ContractsStats = stats.ContractsStats;
            model.StakeholdersStats = stats.StakeholdersStats;
            model.RecentActivities = stats.RecentActivities;

            return model;
        }

        /// <summary>
        /// دریافت آمار کلی کاربر
        /// </summary>
        public async Task<DashboardStatsViewModel> GetUserDashboardStatsAsync(string userId)
        {
            // از cache استفاده می‌کنیم برای بهبود عملکرد
            var cacheKey = $"dashboard_stats_{userId}";

            if (_memoryCache.TryGetValue(cacheKey, out DashboardStatsViewModel cachedStats))
            {
                return cachedStats;
            }

            var stats = new DashboardStatsViewModel();

            try
            {
                // آمار تسک‌ها
                stats.TasksStats = await CalculateTaskStatsAsync(userId);

                // آمار قراردادها
                stats.ContractsStats = await CalculateContractStatsAsync(userId);

                // آمار طرف حساب‌ها
                stats.StakeholdersStats = await CalculateStakeholderStatsAsync(userId);

                // آخرین فعالیت‌ها
                stats.RecentActivities = await GetRecentActivitiesAsync(userId);

                // cache برای 5 دقیقه
                _memoryCache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "GetUserDashboardStats", "خطا در محاسبه آمار", ex);
            }

            return stats;
        }

        /// <summary>
        /// محاسبه آمار تسک‌ها - بازنویسی کامل با حذف تکرار
        /// </summary>
        public async Task<TasksStatsViewModel> CalculateTaskStatsAsync(string userId)
        {
            try
            {
                var today = DateTime.Now.Date;
                var fiveDaysAgo = today.AddDays(-5);

                // ⭐⭐⭐ 1. تسک‌های فعال کاربر (Distinct بر اساس TaskId)
                var myActiveTaskIds = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignedUserId == userId &&
                                !ta.Task.IsDeleted &&
                                (
                                    !ta.CompletionDate.HasValue ||
                                    (ta.CompletionDate.HasValue && ta.CompletionDate >= fiveDaysAgo)
                                ))
                    .Select(ta => ta.TaskId)
                    .Distinct()
                    .ToListAsync();

                // ⭐⭐⭐ 2. تسک‌های ساخته شده توسط من
                var createdByMeTaskIds = await _context.Tasks_Tbl
                    .Where(t => t.CreatorUserId == userId &&
                               !t.IsDeleted &&
                               t.TaskAssignments.Any(ta => ta.AssignedUserId != userId))
                    .Select(t => t.Id)
                    .Distinct()
                    .ToListAsync();

                // ⭐⭐⭐ 3. دریافت تسک‌های کامل (یکبار Query)
                var allMyTasks = await _context.Tasks_Tbl
                    .Where(t => myActiveTaskIds.Contains(t.Id))
                    .Include(t => t.TaskAssignments)
                    .ToListAsync();

                // ⭐⭐⭐ 4. محاسبه آمار
                var result = new TasksStatsViewModel
                {
                    // تعداد تسک‌های فعال من (Distinct)
                    MyTasksCount = myActiveTaskIds.Count,

                    // تسک‌های ساخته شده توسط من
                    AssignedByMeCount = createdByMeTaskIds.Count,

                    // تسک‌های تحت نظارت
                    SupervisedTasksCount = await GetSupervisedTasksCountAsync(userId),

                    // تسک‌های امروز (از تسک‌های من)
                    TodayTasksCount = allMyTasks.Count(t =>
                    {
                        var myAssignment = t.TaskAssignments.FirstOrDefault(ta => ta.AssignedUserId == userId);
                        return t.DueDate.HasValue &&
                               t.DueDate.Value.Date == today &&
                               (!myAssignment?.CompletionDate.HasValue ?? false);
                    }),

                    // تسک‌های عقب افتاده (از تسک‌های من)
                    OverdueTasksCount = allMyTasks.Count(t =>
                    {
                        var myAssignment = t.TaskAssignments.FirstOrDefault(ta => ta.AssignedUserId == userId);
                        return t.DueDate.HasValue &&
                               t.DueDate.Value.Date < today &&
                               (!myAssignment?.CompletionDate.HasValue ?? false);
                    }),

                    // تسک‌های به موقع (از تسک‌های من)
                    OnTimeTasksCount = allMyTasks.Count(t =>
                    {
                        var myAssignment = t.TaskAssignments.FirstOrDefault(ta => ta.AssignedUserId == userId);
                        return (!t.DueDate.HasValue || t.DueDate.Value.Date >= today) &&
                               (!myAssignment?.CompletionDate.HasValue ?? false);
                    }),

                    // یادآوری‌ها
                    RemindersCount = await CalculateActiveRemindersCountAsync(userId)
                };

                return result;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "CalculateTaskStats", "خطا در محاسبه آمار تسک‌ها", ex);
                return new TasksStatsViewModel();
            }
        }

        /// <summary>
        /// محاسبه تعداد تسک‌های نظارتی - متد جدید
        /// </summary>
        private async Task<int> GetSupervisedTasksCountAsync(string userId)
        {
            try
            {
                // تیم‌هایی که کاربر ناظر آن‌هاست (عضو است ولی مدیر نیست)
                var managedTeamIds = await _context.Team_Tbl
                    .Where(t => t.ManagerUserId == userId && t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                var memberTeamIds = await _context.TeamMember_Tbl
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.TeamId)
                    .ToListAsync();

                var supervisedTeamIds = memberTeamIds.Except(managedTeamIds).ToList();

                if (!supervisedTeamIds.Any())
                    return 0;

                // تسک‌های منتصب شده به این تیم‌ها (Distinct)
                return await _context.TaskAssignment_Tbl
                    .Where(ta => supervisedTeamIds.Contains(ta.AssignedInTeamId ?? 0) &&
                                ta.AssignedUserId != userId &&
                                !ta.Task.IsDeleted)
                    .Select(ta => ta.TaskId)
                    .Distinct()
                    .CountAsync();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// محاسبه آمار قراردادها
        /// </summary>
        public async Task<ContractsStatsViewModel> CalculateContractStatsAsync(string userId)
        {
            try
            {
                // اگر Contract repository در IUnitOfWork موجود باشد
                // در حال حاضر placeholder استفاده می‌کنیم
                return new ContractsStatsViewModel
                {
                    ActiveContracts = 0,
                    ExpiringContracts = 0
                };
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "CalculateContractStats", "خطا در محاسبه آمار قراردادها", ex);
                return new ContractsStatsViewModel();
            }
        }

        /// <summary>
        /// محاسبه آمار طرف حساب‌ها
        /// </summary>
        public async Task<StakeholdersStatsViewModel> CalculateStakeholderStatsAsync(string userId)
        {
            try
            {
                // اگر Stakeholder repository در IUnitOfWork موجود باشد
                // در حال حاضر placeholder استفاده می‌کنیم
                return new StakeholdersStatsViewModel
                {
                    TotalStakeholders = 0,
                    NewThisMonth = 0
                };
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "CalculateStakeholderStats", "خطا در محاسبه آمار طرف حساب‌ها", ex);
                return new StakeholdersStatsViewModel();
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های کاربر
        /// </summary>
        public async Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(string userId)
        {
            try
            {
                // ✅ استفاده از ITaskRepository
                return await _taskRepository.GetRecentTaskActivitiesAsync(userId, 10);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "GetRecentActivities", "خطا در دریافت فعالیت‌های اخیر", ex);
                return new List<RecentActivityViewModel>();
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک - اصلاح شده
        /// </summary>
        public async Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(string userId, int take = 10)
        {
            try
            {
                // ✅ استفاده از ITaskRepository
                return await _taskRepository.GetRecentTaskActivitiesAsync(userId, take);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "GetRecentTaskActivities", "خطا در دریافت فعالیت‌های اخیر", ex);
                return new List<RecentActivityViewModel>();
            }
        }

        /// <summary>
        /// محاسبه تعداد یادآوری‌های فعال - اصلاح شده
        /// </summary>
        public async Task<int> CalculateActiveRemindersCountAsync(string userId)
        {
            try
            {
                // ✅ استفاده از کوئری مستقیم با IQueryable
                var today = DateTime.Now.Date;

                // چون GetAll وجود ندارد، از کوئری مستقیم استفاده می‌کنیم
                var query = from r in _context.TaskReminderEvent_Tbl
                            where r.RecipientUserId == userId &&
                                  r.ScheduledDateTime.Date <= today &&
                                  !r.IsSent
                            select r;

                return query.Count();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// دریافت تسک‌های فوری - اصلاح شده
        /// </summary>
        public async Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5)
        {
            try
            {
                // ✅ استفاده از ITaskRepository
                return await _taskRepository.GetUrgentTasksAsync(userId, take);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "GetUrgentTasks", "خطا در دریافت تسک‌های فوری", ex);
                return new List<TaskSummaryViewModel>();
            }
        }

        /// <summary>
        /// محاسبه زمان گذشته از فعالیت
        /// </summary>
        public string CalculateTimeAgo(DateTime activityDate)
        {
            var timeSpan = DateTime.Now - activityDate;

            if (timeSpan.TotalMinutes < 1)
                return "هم اکنون";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} روز پیش";

            return activityDate.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// دریافت متن وضعیت تسک
        /// </summary>
        public string GetTaskStatusText(byte status)
        {
            return status switch
            {
                0 => "ایجاد شده",
                1 => "در حال انجام",
                2 => "تکمیل شده",
                3 => "تأیید شده",
                4 => "رد شده",
                5 => "در انتظار",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت کلاس badge برای وضعیت تسک
        /// </summary>
        public string GetTaskStatusBadgeClass(byte status)
        {
            return status switch
            {
                0 => "bg-secondary",
                1 => "bg-warning",
                2 => "bg-success",
                3 => "bg-info",
                4 => "bg-danger",
                5 => "bg-primary",
                _ => "bg-dark"
            };
        }
        /// <summary>
        /// دریافت آخرین تسک‌های دریافتی کاربر - با استفاده از Include
        /// </summary>
        public async Task<List<RecentTaskViewModel>> GetRecentReceivedTasksAsync(string userId, int take = 5)
        {
            try
            {
                // ⭐⭐⭐ مرحله 1: دریافت داده‌ها با Include
                var assignments = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignedUserId == userId &&
                                !ta.Task.IsDeleted &&
                                ta.Task.CreatorUserId != userId)
                    .Include(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                    .Include(ta => ta.Task.Creator)
                    .OrderByDescending(ta => ta.AssignmentDate)
                    .Take(take)
                    .ToListAsync(); // ⭐ ابتدا ToListAsync بگیر

                // ⭐⭐⭐ مرحله 2: Projection در حافظه (بدون Expression Tree)
                var recentTasks = assignments.Select(ta => new RecentTaskViewModel
                {
                    Id = ta.TaskId,
                    TaskCode = ta.Task.TaskCode,
                    Title = ta.Task.Title,
                    Description = ta.Task.Description,
                    Priority = ta.Task.Priority,
                    Important = ta.Task.Important,
                    StartDate = ta.Task.StartDate,
                    CreateDate = ta.Task.CreateDate,
                    AssignmentDate = ta.AssignmentDate,

                    // ⭐ حل شده: بدون ?. در Expression Tree
                    CreatorName = ta.Task.Creator != null
                        ? $"{ta.Task.Creator.FirstName} {ta.Task.Creator.LastName}"
                        : "نامشخص",

                    // ⭐ حل شده: جایگزین با ?? در LINQ to Objects
                    CategoryTitle = ta.Task.TaskCategory != null
                        ? ta.Task.TaskCategory.Title ?? "بدون دسته‌بندی"
                        : "بدون دسته‌بندی",

                    IsCompleted = ta.CompletionDate.HasValue,
                    CompletionDate = ta.CompletionDate,
                    DueDate = ta.Task.DueDate,
                    Status = ta.Task.Status,
                    IsOverdue = ta.Task.DueDate.HasValue &&
                               ta.Task.DueDate.Value < DateTime.Now &&
                               !ta.CompletionDate.HasValue
                }).ToList();

                return recentTasks;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository",
                    "GetRecentReceivedTasksAsync", "خطا در دریافت تسک‌های دریافتی", ex);
                return new List<RecentTaskViewModel>();
            }
        }
        /// <summary>
        /// دریافت آخرین تسک‌های واگذار شده توسط کاربر - با استفاده از Include
        /// </summary>
        public async Task<List<RecentAssignedTaskViewModel>> GetRecentAssignedTasksAsync(string userId, int take = 5)
        {
            try
            {
                // ⭐⭐⭐ مرحله 1: دریافت داده‌ها با Include
                var tasks = await _context.Tasks_Tbl
                    .Where(t => t.CreatorUserId == userId &&
                               !t.IsDeleted &&
                               t.TaskAssignments.Any(ta => ta.AssignedUserId != userId))
                    .Include(t => t.TaskCategory)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .OrderByDescending(t => t.CreateDate)
                    .Take(take)
                    .ToListAsync(); // ⭐ ابتدا ToListAsync بگیر

                // ⭐⭐⭐ مرحله 2: Projection در حافظه
                var recentTasks = tasks.Select(t => new RecentAssignedTaskViewModel
                {
                    Id = t.Id,
                    TaskCode = t.TaskCode,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Important = t.Important,
                    StartDate = t.StartDate,
                    CreateDate = t.CreateDate,

                    // ⭐ حل شده: بدون ?. در LINQ to Objects
                    CategoryTitle = t.TaskCategory != null
                        ? t.TaskCategory.Title ?? "بدون دسته‌بندی"
                        : "بدون دسته‌بندی",

                    // تعداد اعضا
                    AssigneesCount = t.TaskAssignments.Count(ta => ta.AssignedUserId != userId),

                    // اولین عضو + تعداد بقیه
                    AssignedToName = GetAssigneesDisplayName(t.TaskAssignments, userId),

                    // ⭐⭐⭐ حل شده: استفاده از fully qualified name برای AssigneeInfo
                   

                    // آمار تکمیل
                    CompletedCount = t.TaskAssignments.Count(ta =>
                        ta.AssignedUserId != userId && ta.CompletionDate.HasValue),
                    TotalAssignees = t.TaskAssignments.Count(ta => ta.AssignedUserId != userId),

                    // وضعیت کلی
                    DueDate = t.DueDate,
                    Status = t.Status,
                    HasOverdueAssignees = t.TaskAssignments.Any(ta =>
                        ta.AssignedUserId != userId &&
                        !ta.CompletionDate.HasValue &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value < DateTime.Now)
                }).ToList();

                return recentTasks;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository",
                    "GetRecentAssignedTasksAsync", "خطا در دریافت تسک‌های واگذار شده", ex);
                return new List<RecentAssignedTaskViewModel>();
            }
        }
        /// <summary>
        /// متد کمکی: ساخت نام نمایشی اعضا
        /// </summary>
        private string GetAssigneesDisplayName(ICollection<TaskAssignment> assignments, string excludeUserId)
        {
            var assignees = assignments
                .Where(ta => ta.AssignedUserId != excludeUserId)
                .ToList();

            if (!assignees.Any())
                return "بدون عضو";

            var firstName = assignees.First().AssignedUser != null
                ? $"{assignees.First().AssignedUser.FirstName} {assignees.First().AssignedUser.LastName}"
                : "نامشخص";

            if (assignees.Count == 1)
                return firstName;

            return $"{firstName} و {assignees.Count - 1} نفر دیگر";
        }
    }
}