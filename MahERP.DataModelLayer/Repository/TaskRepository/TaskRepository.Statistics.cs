using MahERP.DataModelLayer.Entities.TaskManagement;
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
        /// محاسبه آمار تسک‌ها
        /// </summary>
        public async Task<TaskStatisticsViewModel> CalculateTaskStatisticsAsync(List<TaskViewModel> tasks)
        {
            var stats = new TaskStatisticsViewModel
            {
                TotalPending = tasks.Count(t => t.Status != 2),
                TotalCompleted = tasks.Count(t => t.Status == 2),
                TotalOverdue = tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.Now && t.Status != 2),
                TotalUrgent = tasks.Count(t => (t.Priority == 2 || t.Important) && t.Status != 2)
            };

            return await Task.FromResult(stats);
        }

        /// <summary>
        /// محاسبه آمار کاربر
        /// </summary>
        private TaskStatisticsViewModel CalculateUserTasksStats(List<TaskViewModel> tasks)
        {
            return new TaskStatisticsViewModel
            {
                TotalPending = tasks.Count(t => t.Status != 2),
                TotalCompleted = tasks.Count(t => t.Status == 2),
                TotalOverdue = tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.Now && t.Status != 2),
                TotalUrgent = tasks.Count(t => (t.Priority == 2 || t.Important) && t.Status != 2)
            };
        }

        /// <summary>
        /// دریافت آمار تسک‌ها برای کاربر
        /// </summary>
        public async Task<UserTaskStatsViewModel> GetUserTaskStatsAsync(string userId)
        {
            var stats = new UserTaskStatsViewModel
            {
                UserId = userId
            };

            // تسک‌های منتصب شده
            var assignedTasks = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId)
                .Include(ta => ta.Task)
                .ToListAsync();

            stats.TotalAssignedTasks = assignedTasks.Count;
            stats.CompletedTasks = assignedTasks.Count(ta => ta.CompletionDate.HasValue);
            stats.PendingTasks = assignedTasks.Count(ta => !ta.CompletionDate.HasValue);
            stats.OverdueTasks = assignedTasks.Count(ta => 
                !ta.CompletionDate.HasValue && 
                ta.Task.DueDate.HasValue && 
                ta.Task.DueDate < DateTime.Now);

            // تسک‌های ایجاد شده
            stats.TotalCreatedTasks = await _context.Tasks_Tbl
                .CountAsync(t => t.CreatorUserId == userId && !t.IsDeleted);

            return stats;
        }

        #endregion

        #region Filter Methods

        /// <summary>
        /// اعمال فیلترهای اضافی بر روی لیست تسک‌ها
        /// </summary>
        public async Task<List<TaskViewModel>> ApplyFiltersAsync(
            List<TaskViewModel> tasks,
            TaskFilterViewModel filters)
        {
            if (filters == null || !HasActiveFilters(filters))
                return tasks;

            var filteredTasks = tasks.AsQueryable();

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue)
            {
                filteredTasks = filters.TaskStatus.Value switch
                {
                    TaskStatusFilter.InProgress => filteredTasks.Where(t => t.Status == 1),
                    TaskStatusFilter.Completed => filteredTasks.Where(t => t.Status == 2),
                    TaskStatusFilter.Overdue => filteredTasks.Where(t => 
                        t.DueDate.HasValue && 
                        t.DueDate < DateTime.Now && 
                        t.Status != 2),
                    _ => filteredTasks
                };
            }

            // فیلتر اولویت
            if (filters.Priority.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == filters.Priority.Value);
            }

            // فیلتر مهم
            if (filters.IsImportant.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Important == filters.IsImportant.Value);
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryIds != null && filters.CategoryIds.Any())
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.CategoryId.HasValue && 
                    filters.CategoryIds.Contains(t.CategoryId.Value));
            }

            // فیلتر کاربران منتصب شده
            if (filters.AssignedUserIds != null && filters.AssignedUserIds.Any())
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.AssignmentsTaskUser != null &&
                    t.AssignmentsTaskUser.Any(a => filters.AssignedUserIds.Contains(a.AssignedUserId)));
            }

            // فیلتر تیم‌ها
            if (filters.TeamIds != null && filters.TeamIds.Any())
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.TeamId.HasValue &&
                    filters.TeamIds.Contains(t.TeamId.Value));
            }

            // فیلتر طرف حساب
            if (filters.StakeholderIds != null && filters.StakeholderIds.Any())
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.StakeholderId.HasValue &&
                    filters.StakeholderIds.Contains(t.StakeholderId.Value));
            }

            // فیلتر تاریخ ایجاد
            if (filters.CreateDateFrom.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.CreateDate >= filters.CreateDateFrom.Value);
            }

            if (filters.CreateDateTo.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.CreateDate <= filters.CreateDateTo.Value);
            }

            // فیلتر تاریخ مهلت
            if (filters.DueDateFrom.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.DueDate.HasValue && 
                    t.DueDate >= filters.DueDateFrom.Value);
            }

            if (filters.DueDateTo.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.DueDate.HasValue && 
                    t.DueDate <= filters.DueDateTo.Value);
            }

            // فیلتر جستجو در عنوان/توضیحات
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                filteredTasks = filteredTasks.Where(t =>
                    t.Title.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                    t.TaskCode.ToLower().Contains(searchTerm));
            }

            return await Task.FromResult(filteredTasks.ToList());
        }

        /// <summary>
        /// اعمال فیلترها بر روی Query اصلی (برای عملکرد بهتر)
        /// </summary>
        private IQueryable<Tasks> ApplyFiltersToQuery(
            IQueryable<Tasks> query,
            TaskFilterViewModel filters)
        {
            if (filters == null || !HasActiveFilters(filters))
                return query;

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue)
            {
                query = filters.TaskStatus.Value switch
                {
                    TaskStatusFilter.InProgress => query.Where(t => t.Status == 1),
                    TaskStatusFilter.Completed => query.Where(t => t.Status == 2),
                    TaskStatusFilter.Overdue => query.Where(t =>
                        t.DueDate.HasValue &&
                        t.DueDate < DateTime.Now &&
                        t.Status != 2),
                    _ => query
                };
            }

            // فیلتر اولویت
            if (filters.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filters.Priority.Value);
            }

            // فیلتر مهم
            if (filters.IsImportant.HasValue)
            {
                query = query.Where(t => t.Important == filters.IsImportant.Value);
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryIds != null && filters.CategoryIds.Any())
            {
                query = query.Where(t =>
                    t.TaskCategoryId.HasValue &&
                    filters.CategoryIds.Contains(t.TaskCategoryId.Value));
            }

            // فیلتر تیم‌ها
            if (filters.TeamIds != null && filters.TeamIds.Any())
            {
                query = query.Where(t =>
                    t.TeamId.HasValue &&
                    filters.TeamIds.Contains(t.TeamId.Value));
            }

            // فیلتر طرف حساب
            if (filters.StakeholderIds != null && filters.StakeholderIds.Any())
            {
                query = query.Where(t =>
                    t.StakeholderId.HasValue &&
                    filters.StakeholderIds.Contains(t.StakeholderId.Value));
            }

            // فیلتر تاریخ ایجاد
            if (filters.CreateDateFrom.HasValue)
            {
                query = query.Where(t => t.CreateDate >= filters.CreateDateFrom.Value);
            }

            if (filters.CreateDateTo.HasValue)
            {
                query = query.Where(t => t.CreateDate <= filters.CreateDateTo.Value);
            }

            // فیلتر تاریخ مهلت
            if (filters.DueDateFrom.HasValue)
            {
                query = query.Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate >= filters.DueDateFrom.Value);
            }

            if (filters.DueDateTo.HasValue)
            {
                query = query.Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate <= filters.DueDateTo.Value);
            }

            // فیلتر جستجو
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                    t.TaskCode.ToLower().Contains(searchTerm));
            }

            return query;
        }

        /// <summary>
        /// بررسی وجود فیلتر فعال
        /// </summary>
        public bool HasActiveFilters(TaskFilterViewModel filters)
        {
            if (filters == null)
                return false;

            return filters.TaskStatus.HasValue ||
                   filters.Priority.HasValue ||
                   filters.IsImportant.HasValue ||
                   (filters.CategoryIds != null && filters.CategoryIds.Any()) ||
                   (filters.AssignedUserIds != null && filters.AssignedUserIds.Any()) ||
                   (filters.TeamIds != null && filters.TeamIds.Any()) ||
                   (filters.StakeholderIds != null && filters.StakeholderIds.Any()) ||
                   filters.CreateDateFrom.HasValue ||
                   filters.CreateDateTo.HasValue ||
                   filters.DueDateFrom.HasValue ||
                   filters.DueDateTo.HasValue ||
                   !string.IsNullOrWhiteSpace(filters.SearchTerm);
        }

        #endregion
    }
}
