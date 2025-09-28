using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// محاسبه آمار تسک‌ها - اصلاح شده
        /// </summary>
        public async Task<TasksStatsViewModel> CalculateTaskStatsAsync(string userId)
        {
            try
            {
                // ✅ استفاده از ITaskRepository
                var myTasks = await _taskRepository.GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: false);
                var assignedByMeTasks = await _taskRepository.GetTasksByUserWithPermissionsAsync(userId, includeAssigned: false, includeCreated: true);
                var allVisibleTasks = await _taskRepository.GetVisibleTasksForUserAsync(userId);

                var today = DateTime.Now.Date;

                return new TasksStatsViewModel
                {
                    // تسک‌های من (منتصب شده به من)
                    MyTasksCount = myTasks?.Count(t => !t.IsDeleted && t.Status != 2) ?? 0,

                    // تسک‌هایی که من ایجاد کرده‌ام
                    AssignedByMeCount = assignedByMeTasks?.Count(t => !t.IsDeleted) ?? 0,

                    // تسک‌های تحت نظارت (تسک‌هایی که من سازنده نیستم ولی می‌توانم ببینم)
                    SupervisedTasksCount = allVisibleTasks?.Count(t =>
                        !t.IsDeleted &&
                        t.CreatorUserId != userId &&
                        !myTasks.Any(mt => mt.Id == t.Id)) ?? 0,

                    // تسک‌های امروز
                    TodayTasksCount = myTasks?.Count(t =>
                        !t.IsDeleted &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value.Date == today) ?? 0,

                    // تسک‌های عقب افتاده
                    OverdueTasksCount = myTasks?.Count(t =>
                        !t.IsDeleted &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value.Date < today &&
                        t.Status != 2) ?? 0,

                    // یادآوری‌ها
                    RemindersCount = await CalculateActiveRemindersCountAsync(userId)
                };
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("MainDashboardRepository", "CalculateTaskStats", "خطا در محاسبه آمار تسک‌ها", ex);
                return new TasksStatsViewModel();
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
    }
}