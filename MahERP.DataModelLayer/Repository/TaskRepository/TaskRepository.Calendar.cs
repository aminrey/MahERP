using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت تقویم و رویدادهای تسک‌ها
    /// شامل: نمایش در تقویم، رنگ‌بندی، رویدادهای شخصی
    /// </summary>
    public partial class TaskRepository
    {
        #region Calendar View Methods

        /// <summary>
        /// دریافت تسک‌های شعبه برای نمایش در تقویم بر اساس تاریخ مهلت انجام
        /// </summary>
        public List<TaskCalendarViewModel> GetTasksForCalendarView(
            string userId,
            int? branchId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            var query = _context.Tasks_Tbl
                .Include(t => t.Creator)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            // فیلتر شعبه
            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            // فیلتر تاریخ
            if (startDate.HasValue)
                query = query.Where(t => t.DueDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.DueDate <= endDate.Value);

            // فیلتر کاربران
            if (assignedUserIds != null && assignedUserIds.Any())
            {
                query = query.Where(t => t.TaskAssignments.Any(ta => assignedUserIds.Contains(ta.AssignedUserId)));
            }

            // فیلتر طرف حساب
            if (stakeholderId.HasValue)
                query = query.Where(t => t.StakeholderId == stakeholderId.Value);

            var tasks = query.ToList();

            return tasks.Select(t => new TaskCalendarViewModel
            {
                Id = t.Id,
                Title = t.Title,
                TaskCode = t.TaskCode,
                Description = t.Description,
                DueDate = t.DueDate,
              
                CategoryTitle = t.TaskCategory?.Title,
                
                IsCompleted = t.Status == 2,
                IsOverdue = t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status != 2
            }).ToList();
        }

        /// <summary>
        /// دریافت رویدادهای تقویم (نسخه Async)
        /// </summary>
        public async Task<List<TaskCalendarViewModel>> GetCalendarEventsAsync(
            string userId,
            DateTime? start = null,
            DateTime? end = null,
            int? branchId = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            var query = _context.Tasks_Tbl
                .Include(t => t.Creator)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Where(t => !t.IsDeleted && t.DueDate.HasValue)
                .AsQueryable();

            // فیلتر بازه زمانی
            if (start.HasValue)
                query = query.Where(t => t.DueDate >= start.Value);

            if (end.HasValue)
                query = query.Where(t => t.DueDate <= end.Value);

            // فیلتر شعبه
            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            // فیلتر کاربران
            if (assignedUserIds != null && assignedUserIds.Any())
            {
                query = query.Where(t => t.TaskAssignments.Any(ta => assignedUserIds.Contains(ta.AssignedUserId)));
            }

            // فیلتر طرف حساب
            if (stakeholderId.HasValue)
                query = query.Where(t => t.StakeholderId == stakeholderId.Value);

            var tasks = await query.ToListAsync();

            var events = new List<TaskCalendarViewModel>();

            foreach (var task in tasks)
            {
                var calendarEvent = new TaskCalendarViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    TaskCode = task.TaskCode,
                    Description = task.Description,
                    DueDate = task.DueDate,
                  
                    CategoryTitle = task.TaskCategory?.Title,
                  
                    IsCompleted = task.Status == 2,
                    IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.Now && task.Status != 2
                };

                events.Add(calendarEvent);
            }

            // ⭐ افزودن رویدادهای شخصی
            var personalEvents = await AddPersonalEventsToCalendarAsync(userId, start, end);
            events.AddRange(personalEvents);

            return events;
        }

        /// <summary>
        /// افزودن رویدادهای شخصی به تقویم (تاریخ‌های شخصی کاربران)
        /// </summary>
        public async Task<List<TaskCalendarViewModel>> AddPersonalEventsToCalendarAsync(
            string userId,
            DateTime? start = null,
            DateTime? end = null)
        {
            var query = _context.TaskAssignment_Tbl
                .Include(ta => ta.Task)
                    .ThenInclude(t => t.Creator)
                .Include(ta => ta.Task.TaskCategory)
                .Where(ta => ta.AssignedUserId == userId &&
                            !ta.Task.IsDeleted &&
                            (ta.PersonalStartDate.HasValue || ta.PersonalEndDate.HasValue))
                .AsQueryable();

            if (start.HasValue)
                query = query.Where(ta => ta.PersonalEndDate >= start.Value || ta.PersonalStartDate >= start.Value);

            if (end.HasValue)
                query = query.Where(ta => ta.PersonalStartDate <= end.Value || ta.PersonalEndDate <= end.Value);

            var assignments = await query.ToListAsync();

            return assignments.Select(ta => new TaskCalendarViewModel
            {
                Id = ta.TaskId,
                Title = $"[شخصی] {ta.Task.Title}",
                TaskCode = ta.Task.TaskCode,
                Description = ta.PersonalTimeNote ?? ta.Task.Description,
                DueDate = ta.PersonalEndDate ?? ta.Task.DueDate,
             
                CategoryTitle = ta.Task.TaskCategory?.Title,
                IsCompleted = ta.CompletionDate.HasValue,
                IsOverdue = ta.PersonalEndDate.HasValue && ta.PersonalEndDate.Value < DateTime.Now && !ta.CompletionDate.HasValue,
            }).ToList();
        }

        #endregion

      
    }
}
