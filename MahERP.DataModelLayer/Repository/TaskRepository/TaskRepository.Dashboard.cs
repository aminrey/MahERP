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
            var dashboard = new TaskDashboardViewModel();
         
            // دریافت تسک‌های فوری
            dashboard.UrgentTasks = await GetUrgentTasksAsync(userId, 5);

            // دریافت فعالیت‌های اخیر
            dashboard.RecentActivities = await GetRecentTaskActivitiesAsync(userId, 10);

            // آمار کلی
            dashboard.UserStats = await GetUserTaskStatsAsync(userId);


            return dashboard;
        }

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط کاربر
        /// </summary>
        public async Task<TasksListViewModel> GetTasksAssignedByUserAsync(
            string userId,
            TaskFilterViewModel filters)
        {
            var query = _context.Tasks_Tbl
                .Include(t => t.Creator)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.TaskOperations)
                .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                .AsQueryable();

            // اعمال فیلترها
            query = ApplyFiltersToQuery(query, filters);

            var tasks = await query
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = _mapper.Map<List<TaskViewModel>>(tasks);

            var stats = CalculateUserTasksStats(taskViewModels);

            return new TasksListViewModel
            {
                Tasks = taskViewModels,
                Stats = stats,
                TotalCount = taskViewModels.Count
            };
        }

        /// <summary>
        /// دریافت تسک‌های تحت نظارت کاربر
        /// </summary>
        public async Task<TasksListViewModel> GetSupervisedTasksAsync(
            string userId,
            TaskFilterViewModel filters)
        {
            // دریافت شناسه تسک‌های نظارتی
            var supervisedTaskIds = await GetSupervisedTaskIdsAsync(userId);

            if (!supervisedTaskIds.Any())
            {
                return new TasksListViewModel
                {
                    Tasks = new List<TaskViewModel>(),
                    Stats = new TaskStatisticsViewModel(),
                    TotalCount = 0
                };
            }

            var query = _context.Tasks_Tbl
                .Include(t => t.Creator)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.TaskOperations)
                .Where(t => supervisedTaskIds.Contains(t.Id) && !t.IsDeleted)
                .AsQueryable();

            // اعمال فیلترها
            query = ApplyFiltersToQuery(query, filters);

            var tasks = await query
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = _mapper.Map<List<TaskViewModel>>(tasks);

            var stats = CalculateUserTasksStats(taskViewModels);

            return new TasksListViewModel
            {
                Tasks = taskViewModels,
                Stats = stats,
                TotalCount = taskViewModels.Count
            };
        }

        /// <summary>
        /// دریافت تسک‌های فوری کاربر
        /// </summary>
        public async Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5)
        {
            var tasks = await _context.TaskAssignment_Tbl
                .Include(ta => ta.Task)
                    .ThenInclude(t => t.TaskCategory)
                .Where(ta => ta.AssignedUserId == userId &&
                            !ta.CompletionDate.HasValue &&
                            !ta.Task.IsDeleted &&
                            (ta.Task.Priority == 2 || ta.Task.Important))
                .OrderBy(ta => ta.Task.DueDate)
                .Take(take)
                .Select(ta => new TaskSummaryViewModel
                {
                    TaskId = ta.TaskId,
                    TaskCode = ta.Task.TaskCode,
                    Title = ta.Task.Title,
                    DueDate = ta.Task.DueDate,
                    DueDatePersian = ta.Task.DueDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(ta.Task.DueDate, "yyyy/MM/dd")
                        : null,
                    Priority = ta.Task.Priority,
                    Important = ta.Task.Important,
                    CategoryTitle = ta.Task.TaskCategory != null ? ta.Task.TaskCategory.Title : null,
                    IsOverdue = ta.Task.DueDate.HasValue && ta.Task.DueDate < DateTime.Now
                })
                .ToListAsync();

            return tasks;
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک کاربر
        /// </summary>
        public async Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(
            string userId,
            int take = 10)
        {
            var activities = new List<RecentActivityViewModel>();

            // تسک‌های ایجاد شده اخیر
            var createdTasks = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.CreateDate)
                .Take(take / 2)
                .ToListAsync();

            foreach (var task in createdTasks)
            {
                activities.Add(new RecentActivityViewModel
                {
                    ActivityType = "TaskCreated",
                    Title = "ایجاد تسک",
                    Description = $"تسک «{task.Title}» ایجاد شد",
                    TaskId = task.Id,
                    TaskCode = task.TaskCode,
                    ActivityDate = task.CreateDate,
                    ActivityDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.CreateDate, "yyyy/MM/dd HH:mm"),
                    Icon = "fa-plus-circle",
                    BadgeClass = "bg-primary"
                });
            }

            // تسک‌های تکمیل شده اخیر
            var completedAssignments = await _context.TaskAssignment_Tbl
                .Include(ta => ta.Task)
                .Where(ta => ta.AssignedUserId == userId &&
                            ta.CompletionDate.HasValue)
                .OrderByDescending(ta => ta.CompletionDate)
                .Take(take / 2)
                .ToListAsync();

            foreach (var assignment in completedAssignments)
            {
                activities.Add(new RecentActivityViewModel
                {
                    ActivityType = "TaskCompleted",
                    Title = "تکمیل تسک",
                    Description = $"تسک «{assignment.Task.Title}» تکمیل شد",
                    TaskId = assignment.TaskId,
                    TaskCode = assignment.Task.TaskCode,
                    ActivityDate = assignment.CompletionDate.Value,
                    ActivityDatePersian = ConvertDateTime.ConvertMiladiToShamsi(assignment.CompletionDate, "yyyy/MM/dd HH:mm"),
                    Icon = "fa-check-circle",
                    BadgeClass = "bg-success"
                });
            }

            return activities
                .OrderByDescending(a => a.ActivityDate)
                .Take(take)
                .ToList();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// دریافت شناسه تسک‌های نظارتی
        /// </summary>
        private async Task<List<int>> GetSupervisedTaskIdsAsync(string userId)
        {
            var supervisedIds = new HashSet<int>();

            // 1. تسک‌های تیم‌های تحت مدیریت
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            if (managedTeamIds.Any())
            {
                var teamTaskIds = await _context.Tasks_Tbl
                    .Where(t => t.TeamId.HasValue &&
                               managedTeamIds.Contains(t.TeamId.Value) &&
                               !t.IsDeleted &&
                               !t.IsPrivate)
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var id in teamTaskIds)
                    supervisedIds.Add(id);
            }

            // 2. تسک‌های بر اساس سمت
            var positionBasedTasks = await _taskVisibilityRepository.GetPositionBasedVisibleTasksAsync(userId);
            foreach (var id in positionBasedTasks)
                supervisedIds.Add(id);

            // 3. تسک‌های با مجوز خاص
            var specialPermissionTasks = await _taskVisibilityRepository.GetSpecialPermissionTasksAsync(userId);
            foreach (var id in specialPermissionTasks)
                supervisedIds.Add(id);

            // حذف تسک‌هایی که کاربر خودش در آن‌ها عضو است
            var userOwnTasks = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId)
                .Select(ta => ta.TaskId)
                .ToListAsync();

            var userCreatedTasks = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            supervisedIds.ExceptWith(userOwnTasks);
            supervisedIds.ExceptWith(userCreatedTasks);

            return supervisedIds.ToList();
        }

        #endregion
    }
}
