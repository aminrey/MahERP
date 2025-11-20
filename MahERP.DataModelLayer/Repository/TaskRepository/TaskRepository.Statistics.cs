using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت آمار و فیلترهای تسک‌ها
    /// شامل: محاسبه آمار، اعمال فیلترها، بررسی فیلترهای فعال
    /// </summary>
    public partial class TaskRepository
    {
        #region Statistics Methods

        /// <summary>
        /// محاسبه آمار ثابت - مستقل از ViewType - اصلاح نهایی
        /// </summary>
        private async Task<TaskStatisticsViewModel> CalculateTaskStatisticsAsync(string userId, List<TaskViewModel> filteredTasks)
        {
            try
            {
                // ⭐⭐⭐ مرحله 1: دریافت همه تسک‌هایی که کاربر مجوز دیدن آن‌ها را دارد
                // شامل: تسک‌های شخصی + قابل مشاهده + نظارتی
                var allAccessibleTasks = await GetTasksByUserWithPermissionsAsync(
                    userId,
                    includeAssigned: true,
                    includeCreated: true,
                    includeDeleted: false,
                    includeSupervisedTasks: true); // ⭐ شامل تسک‌های نظارتی

                // ⭐ اضافه کردن تسک‌های قابل مشاهده از طریق Visibility
                var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
                var visibleTasks = await _context.Tasks_Tbl
                    .Where(t => visibleTaskIds.Contains(t.Id) && !t.IsDeleted)
                    .ToListAsync();

                // ⭐ ترکیب همه تسک‌ها و حذف تکرار
                var combinedTasks = allAccessibleTasks
                    .Concat(visibleTasks)
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                var allAccessibleTaskViewModels = combinedTasks.Select(MapToTaskViewModel).ToList();

                // حذف تکرار از تسک‌های قابل دسترس
                var uniqueAccessibleTasks = allAccessibleTaskViewModels
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // ⭐ مرحله 2: دریافت تسک‌های شخصی کاربر (برای آمار AssignedToMe و AssignedByMe)
                var myTasks = await GetTasksByUserWithPermissionsAsync(
                    userId,
                    includeAssigned: true,
                    includeCreated: true,
                    includeDeleted: false,
                    includeSupervisedTasks: false); // فقط تسک‌های شخصی

                var myTaskViewModels = myTasks.Select(MapToTaskViewModel).ToList();

                // حذف تکرار از تسک‌های شخصی
                var uniqueMyTasks = myTaskViewModels
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // حذف تکرار از تسک‌های فیلتر شده
                var uniqueFilteredTasks = filteredTasks
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // ⭐⭐⭐ محاسبه آمار
                var statistics = new TaskStatisticsViewModel
                {
                    // ⭐⭐⭐ آمار ثابت (از همه تسک‌های قابل دسترس)
                    TotalTasks = uniqueAccessibleTasks.Count, // ⭐ این عدد ثابت می‌ماند و شامل همه تسک‌های قابل دسترس است

                    // ⭐⭐⭐ آمار شخصی (از تسک‌های شخصی کاربر)
                    AssignedToMe = uniqueMyTasks.Count(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId) &&
                        t.CreatorUserId != userId),

                    AssignedByMe = uniqueMyTasks.Count(t =>
                        t.CreatorUserId == userId),

                    // ⭐⭐⭐ آمار متغیر (از تسک‌های فیلتر شده - بسته به ViewType)
                    CompletedTasks = uniqueFilteredTasks.Count(t =>
                        t.CompletionDate.HasValue),

                    OverdueTasks = uniqueFilteredTasks.Count(t =>
                        !t.CompletionDate.HasValue &&
                        t.DueDate.HasValue &&
                        t.DueDate < DateTime.Now),

                    InProgressTasks = uniqueFilteredTasks.Count(t =>
                        !t.CompletionDate.HasValue &&
                        t.IsActive),

                    ImportantTasks = uniqueFilteredTasks.Count(t =>
                        t.Important ||
                        t.Priority == 1),

                    UrgentTasks = uniqueFilteredTasks.Count(t =>
                        t.Priority == 2),

                    TeamTasks = 0,
                    SubTeamTasks = 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در CalculateTaskStatisticsAsync: {ex.Message}");

                return new TaskStatisticsViewModel
                {
                    TotalTasks = 0,
                    AssignedToMe = 0,
                    AssignedByMe = 0,
                    CompletedTasks = 0,
                    OverdueTasks = 0,
                    InProgressTasks = 0,
                    ImportantTasks = 0,
                    UrgentTasks = 0,
                    TeamTasks = 0,
                    SubTeamTasks = 0
                };
            }
        }


        /// <summary>
        /// محاسبه آمار تفصیلی تسک‌های کاربر
        /// </summary>
        private UserTasksStatsViewModel CalculateUserTasksStats(UserTasksComprehensiveViewModel data)
        {
            var today = DateTime.Now.Date;
            var allActiveTasks = data.CreatedTasks
                .Concat(data.AssignedTasks)
                .Concat(data.SupervisedTasks)
                .ToList();

            return new UserTasksStatsViewModel
            {
                CreatedTasksCount = data.CreatedTasks.Count,
                AssignedTasksCount = data.AssignedTasks.Count,
                SupervisedTasksCount = data.SupervisedTasks.Count,
                DeletedTasksCount = data.DeletedTasks.Count,
                CompletedTasksCount = allActiveTasks.Count(t => t.CompletionDate.HasValue),
                OverdueTasksCount = allActiveTasks.Count(t =>
                    !t.CompletionDate.HasValue &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date < today),
                TodayTasksCount = allActiveTasks.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date == today)
            };
        }


        /// <summary>
        /// دریافت آمار تسک‌ها برای کاربر
        /// </summary>
        public async Task<UserTaskStatsViewModel> GetUserTaskStatsAsync(string userId)
        {
            try
            {
                // تسک‌های من
                var myTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

                // تسک‌های واگذار شده
                var assignedByMe = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: false, includeCreated: true);

                // تسک‌های نظارتی
                var supervisedTasks = await GetVisibleTasksForUserAsync(userId);
                supervisedTasks = supervisedTasks.Where(t => t.CreatorUserId != userId).ToList();

                var today = DateTime.Now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var weekEnd = weekStart.AddDays(6);

                return new UserTaskStatsViewModel
                {
                    MyTasksCount = myTasks.Count(t => !t.IsDeleted && t.Status != 2),
                    AssignedByMeCount = assignedByMe.Count(t => !t.IsDeleted),
                    SupervisedTasksCount = supervisedTasks.Count(t => !t.IsDeleted),
                    TodayTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date == today),
                    OverdueTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date < today && t.Status != 2),
                    ThisWeekTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date >= weekStart && t.DueDate.Value.Date <= weekEnd),
                    RemindersCount = await GetActiveRemindersCountAsync(userId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در محاسبه آمار کاربر: {ex.Message}", ex);
            }
        }

        #endregion

        #region Filter Methods
        /// <summary>
        /// اعمال فیلترهای اضافی بر روی لیست تسک‌ها
        /// </summary>
        public async Task<List<TaskViewModel>> ApplyFiltersAsync(List<TaskViewModel> tasks, TaskFilterViewModel filters)
        {
            var filteredTasks = tasks.ToList();

            // فیلتر شعبه
            if (filters.BranchId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.BranchId == filters.BranchId.Value).ToList();
            }

            // فیلتر تیم
            if (filters.TeamId.HasValue)
            {
                var teamUserIds = await GetUsersFromTeamsAsync(new List<int> { filters.TeamId.Value });
                filteredTasks = filteredTasks.Where(t => t.AssignmentsTaskUser != null &&
                                                t.AssignmentsTaskUser.Any(a => teamUserIds.Contains(a.AssignedUserId))).ToList();
            }

            // فیلتر کاربر  
            if (!string.IsNullOrEmpty(filters.UserId))
            {
                filteredTasks = filteredTasks.Where(t =>
                    (t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any(a => a.AssignedUserId == filters.UserId)) ||
                    t.CreatorUserId == filters.UserId).ToList();
            }

            // فیلتر اولویت
            if (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All)
            {
                filteredTasks = filteredTasks.Where(t => t.TaskType == (byte)filters.TaskPriority).ToList();
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.CategoryId == filters.CategoryId.Value).ToList();
            }

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All)
            {
                switch (filters.TaskStatus.Value)
                {
                    case TaskStatusFilter.Completed:
                        filteredTasks = filteredTasks.Where(t => t.CompletionDate.HasValue).ToList();
                        break;
                    case TaskStatusFilter.InProgress:
                        filteredTasks = filteredTasks.Where(t => !t.CompletionDate.HasValue && t.IsActive).ToList();
                        break;
                    case TaskStatusFilter.Overdue:
                        filteredTasks = filteredTasks.Where(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now).ToList();
                        break;
                }
            }

            // فیلتر طرف حساب
            if (filters.StakeholderId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.StakeholderId == filters.StakeholderId.Value).ToList();
            }

            // فیلتر جستجو در متن
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.Title.Contains(filters.SearchTerm) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(filters.SearchTerm)) ||
                    t.TaskCode.Contains(filters.SearchTerm)).ToList();
            }

            return filteredTasks;
        }

        /// <summary>
        /// اعمال فیلترها روی IQueryable تسک‌ها
        /// </summary>
        private IQueryable<Tasks> ApplyFiltersToQuery(IQueryable<Tasks> query, TaskFilterViewModel filters)
        {
            if (filters == null) return query;

            // فیلتر شعبه
            if (filters.BranchId.HasValue)
            {
                query = query.Where(t => t.BranchId == filters.BranchId.Value);
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryId.HasValue)
            {
                query = query.Where(t => t.TaskCategoryId == filters.CategoryId.Value);
            }

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All)
            {
                switch (filters.TaskStatus.Value)
                {

                    case TaskStatusFilter.Created:
                        query = query.Where(t => t.Status == 0);
                        break;
                    case TaskStatusFilter.Approved:
                        query = query.Where(t => t.Status == 3);
                        break;
                    case TaskStatusFilter.Rejected:
                        query = query.Where(t => t.Status == 4);
                        break;
                }
            }

            // فیلتر اولویت
            if (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All)
            {
                switch (filters.TaskPriority.Value)
                {
                    case TaskPriorityFilter.Normal:
                        query = query.Where(t => t.Priority == 0 && !t.Important);
                        break;
                    case TaskPriorityFilter.Important:
                        query = query.Where(t => t.Important || t.Priority == 1);
                        break;
                    case TaskPriorityFilter.Urgent:
                        query = query.Where(t => t.Priority == 2);
                        break;
                }
            }

            // فیلتر طرف حساب
            if (filters.StakeholderId.HasValue)
            {
                query = query.Where(t => t.StakeholderId == filters.StakeholderId.Value);
            }

            // فیلتر جستجو در متن
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(filters.SearchTerm) ||
                    (t.Description != null && t.Description.Contains(filters.SearchTerm)) ||
                    t.TaskCode.Contains(filters.SearchTerm));
            }

            return query;
        }


        /// <summary>
        /// بررسی وجود فیلتر فعال
        /// </summary>
        public bool HasActiveFilters(TaskFilterViewModel filters)
        {
            return filters.BranchId.HasValue ||
                   filters.TeamId.HasValue ||
                   !string.IsNullOrEmpty(filters.UserId) ||
                   filters.CategoryId.HasValue ||
                   filters.StakeholderId.HasValue ||
                   !string.IsNullOrEmpty(filters.SearchTerm) ||
                   (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All) ||
                   (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All);
        }

        #endregion
    }
}
