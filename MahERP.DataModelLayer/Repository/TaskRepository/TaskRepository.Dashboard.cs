using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت داشبورد تسک‌ها
    /// شامل: داده‌های داشبورد، تسک‌های فوری، فعالیت‌های اخیر
    /// </summary>
    public partial class TaskRepository
    {
        #region Dashboard Methods

        /// <summary>
        /// دریافت داده‌های داشبورد تسک‌ها برای کاربر
        /// </summary>
        public async Task<TaskDashboardViewModel> GetTaskDashboardDataAsync(string userId)
        {
            try
            {
                // دریافت آمار کلی
                var stats = await GetUserTaskStatsAsync(userId);

                // دریافت تسک‌های فوری
                var urgentTasks = await GetUrgentTasksAsync(userId, 10);

                // دریافت فعالیت‌های اخیر
                var recentActivities = await GetRecentTaskActivitiesAsync(userId, 10);

                return new TaskDashboardViewModel
                {
                    UserStats = stats,
                    UrgentTasks = urgentTasks,
                    RecentActivities = recentActivities,

                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت داده‌های داشبورد: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تسک‌های واگذارشده توسط کاربر
        /// </summary>
        public async Task<TasksListViewModel> GetTasksAssignedByUserAsync(string userId, TaskFilterViewModel filters)
        {
            try
            {
                var query = _context.Tasks_Tbl
                    .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskCategory)
    .Include(t => t.Contact)
    .Include(t => t.Organization)
                                    .AsQueryable();

                // اعمال فیلترها
                query = ApplyFiltersToQuery(query, filters);

                var tasks = await query.OrderByDescending(t => t.CreateDate).ToListAsync();
                var taskViewModels = tasks.Select(MapToTaskViewModel).ToList();

                // محاسبه آمار
                var stats = new TasksListStatsViewModel
                {
                    TotalCount = taskViewModels.Count,
                    FilteredCount = taskViewModels.Count,
                    NeedsAttentionCount = taskViewModels.Count(t =>
                        (t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2) ||
                        (t.DueDate.HasValue && t.DueDate.Value.Date <= DateTime.Now.Date.AddDays(1))),
                    OverdueCount = taskViewModels.Count(t =>
                        t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2),
                    CompletedCount = taskViewModels.Count(t => t.Status == 2),
                    InProgressCount = taskViewModels.Count(t => t.Status == 1)
                };

                return new TasksListViewModel
                {
                    Tasks = taskViewModels,
                    Stats = stats,
                    Filters = filters,
                    TotalCount = stats.TotalCount,
                    FilteredCount = stats.FilteredCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تسک‌های واگذار شده: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تسک‌های فوری کاربر
        /// </summary>
        public async Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5)
        {
            try
            {
                var userTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

                var urgentTasks = userTasks.Where(t =>
                    t.Priority == 2 || // فوری
                    t.Important || // مهم
                    (t.DueDate.HasValue && t.DueDate.Value.Date <= DateTime.Now.Date.AddDays(1)) // مهلت امروز یا فردا
                ).OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Take(take)
                .Select(t => new TaskSummaryViewModel
                {
                    Id = t.Id,
                    TaskCode = t.TaskCode,
                    Title = t.Title,
                    Priority = t.Priority,
                    Important = t.Important,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2,
                    StatusText = GetTaskStatusText(t.Status),
                    StatusBadgeClass = GetTaskStatusBadgeClass(t.Status),
                    StakeholderName = GetTaskStakeholderName(t.Id)
                    // DaysUntilDue محاسبه خودکار توسط Property انجام می‌شود
                }).ToList();

                return urgentTasks;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تسک‌های فوری: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک کاربر
        /// </summary>
        public async Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(string userId, int take = 10)
        {
            try
            {
                var recentTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

                var activities = recentTasks
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.LastUpdateDate ?? t.CreateDate)
                    .Take(take)
                    .Select(t => new RecentActivityViewModel
                    {
                        Title = GetActivityTitle(t.Status),
                        Description = $"{t.Title} - {t.TaskCode}",
                        ActivityDate = t.LastUpdateDate ?? t.CreateDate,
                        TimeAgo = CalculateTimeAgo(t.LastUpdateDate ?? t.CreateDate),
                        Url = $"/AdminArea/Tasks/Details/{t.Id}"
                    }).ToList();

                return activities;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت فعالیت‌های اخیر: {ex.Message}", ex);
            }
        }

        #endregion

 
    }
}
