using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository مسئول فیلتر کردن تسک‌ها
    /// </summary>
    public class TaskFilteringRepository : ITaskFilteringRepository
    {
        private readonly AppDbContext _context;
        private readonly ITaskVisibilityRepository _visibilityRepository;

        public TaskFilteringRepository(
            AppDbContext context,
            ITaskVisibilityRepository visibilityRepository)
        {
            _context = context;
            _visibilityRepository = visibilityRepository;
        }

        /// <summary>
        /// دریافت تسک‌های من
        /// </summary>
        public async Task<List<Tasks>> GetMyTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetMyTasksAsync - User: {userId}");

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);

            var myAssignmentTaskIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var relevantTaskIds = visibleTaskIds.Intersect(myAssignmentTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط من
        /// </summary>
        public async Task<List<Tasks>> GetAssignedByMeTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetAssignedByMeTasksAsync - User: {userId}");

            var myCreatedTaskIds = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = myCreatedTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// دریافت تسک‌های نظارتی - ⭐⭐⭐ اصلاح شده برای شامل شدن رونوشت‌ها
        /// </summary>
        public async Task<List<Tasks>> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetSupervisedTasksAsync - User: {userId}");

            // ⭐⭐⭐ 1. تسک‌های نظارتی سیستمی (بر اساس visibility)
            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);
            Console.WriteLine($"   📋 Total Visible TaskIds from Visibility: {visibleTaskIds.Count}");

            // ⭐⭐⭐ DEBUG: چک کردن تسک خاص
            var debugTaskId = await _context.Tasks_Tbl
                .Where(t => t.Title.Contains("ثبت بانک شرکت"))
                .Select(t => new { t.Id, t.Title, t.CreatorUserId })
                .FirstOrDefaultAsync();

            if (debugTaskId != null)
            {
                Console.WriteLine($"   🐛 DEBUG Task Found: Id={debugTaskId.Id}, Title={debugTaskId.Title}, Creator={debugTaskId.CreatorUserId}");
                Console.WriteLine($"   🐛 Is in visibleTaskIds? {visibleTaskIds.Contains(debugTaskId.Id)}");
                Console.WriteLine($"   🐛 Creator == userId? {debugTaskId.CreatorUserId == userId}");
                
                var hasAssignment = await _context.TaskAssignment_Tbl
                    .AnyAsync(ta => ta.TaskId == debugTaskId.Id && ta.AssignedUserId == userId);
                Console.WriteLine($"   🐛 Has Assignment to user? {hasAssignment}");
            }

            var systemSupervisedTaskIds = await _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id) &&
                           t.CreatorUserId != userId && // تسک‌هایی که خودم نساخته‌ام
                           !t.IsDeleted)
                .Where(t => !_context.TaskAssignment_Tbl.Any(ta => ta.TaskId == t.Id && ta.AssignedUserId == userId)) // به من منتصب نشده
                .Select(t => t.Id)
                .ToListAsync();

            Console.WriteLine($"   ✅ System Supervised (Filtered): {systemSupervisedTaskIds.Count}");
            if (debugTaskId != null && systemSupervisedTaskIds.Contains(debugTaskId.Id))
            {
                Console.WriteLine($"   🐛 DEBUG Task is in System Supervised list!");
            }

            // ⭐⭐⭐ 2. تسک‌های رونوشت شده (از TaskViewer)
            var carbonCopyTaskIds = await _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId &&
                            tv.IsActive &&
                            (tv.StartDate == null || tv.StartDate <= DateTime.Now) &&
                            (tv.EndDate == null || tv.EndDate > DateTime.Now))
                .Select(tv => tv.TaskId)
                .ToListAsync();

            Console.WriteLine($"   ✅ Carbon Copy: {carbonCopyTaskIds.Count}");
            if (debugTaskId != null && carbonCopyTaskIds.Contains(debugTaskId.Id))
            {
                Console.WriteLine($"   🐛 DEBUG Task is in Carbon Copy list!");
            }

            // ⭐⭐⭐ 3. ترکیب هر دو نوع
            var allSupervisedTaskIds = systemSupervisedTaskIds.Union(carbonCopyTaskIds).Distinct().ToList();

            Console.WriteLine($"   📊 System Supervised: {systemSupervisedTaskIds.Count}, Carbon Copy: {carbonCopyTaskIds.Count}, Total: {allSupervisedTaskIds.Count}");

            // ⭐⭐⭐ 4. دریافت تسک‌ها با اطلاعات نوع نظارت
            var tasks = await _context.Tasks_Tbl
                .Where(t => allSupervisedTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// دریافت همه تسک‌های قابل مشاهده
        /// </summary>
        public async Task<List<Tasks>> GetAllVisibleTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetAllVisibleTasksAsync - User: {userId}");

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);

            var tasks = await _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// دریافت تسک‌های منتصب شده به من
        /// </summary>
        public async Task<List<Tasks>> GetAssignedToMeTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetAssignedToMeTasksAsync - User: {userId}");

            var assignedTaskIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            ta.Task.CreatorUserId != userId)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = assignedTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// دریافت تسک‌های تیمی
        /// </summary>
        public async Task<List<Tasks>> GetTeamTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetTeamTasksAsync - User: {userId}");

            var userTeamIds = await _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            if (!userTeamIds.Any())
                return new List<Tasks>();

            var teamTaskIds = await _context.Tasks_Tbl
                .Where(t => t.TeamId.HasValue && userTeamIds.Contains(t.TeamId.Value) && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = teamTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters);
        }

        /// <summary>
        /// اعمال فیلترها
        /// </summary>
        public List<Tasks> ApplyFilters(List<Tasks> tasks, TaskFilterViewModel filters)
        {
            if (filters == null) return tasks;

            if (filters.BranchId.HasValue)
                tasks = tasks.Where(t => t.BranchId == filters.BranchId).ToList();

            if (filters.CategoryId.HasValue)
                tasks = tasks.Where(t => t.TaskCategoryId == filters.CategoryId).ToList();

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                tasks = tasks.Where(t =>
                    t.Title.ToLower().Contains(search) ||
                    t.TaskCode.ToLower().Contains(search) ||
                    (t.Description != null && t.Description.ToLower().Contains(search))
                ).ToList();
            }

            return tasks;
        }

        /// <summary>
        /// محاسبه آمار لیست
        /// </summary>
        public TaskListStatsViewModel CalculateStats(List<Tasks> tasks, string userId)
        {
            return new TaskListStatsViewModel
            {
                TotalPending = tasks.Count(t => !IsTaskCompletedForUser(t.Id, userId)),
                TotalCompleted = tasks.Count(t => IsTaskCompletedForUser(t.Id, userId)),
                TotalOverdue = tasks.Count(t => t.DueDate.HasValue &&
                                                t.DueDate.Value < DateTime.Now &&
                                                !IsTaskCompletedForUser(t.Id, userId)),
                TotalUrgent = tasks.Count(t => t.Priority == 2),
                TotalImportant = tasks.Count(t => t.Important || t.Priority == 1)
            };
        }

        /// <summary>
        /// ⭐⭐⭐ اعمال فیلتر سریع وضعیت (Quick Status Filter)
        /// </summary>
        public List<Tasks> ApplyQuickStatusFilter(List<Tasks> tasks, QuickStatusFilter filter, string userId)
        {
            return filter switch
            {
                QuickStatusFilter.Pending => tasks.Where(t => !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Completed => tasks.Where(t => IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Overdue => tasks.Where(t => 
                    t.DueDate.HasValue && 
                    t.DueDate.Value < DateTime.Now && 
                    !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Urgent => tasks.Where(t => 
                    t.Priority == 2 && 
                    !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                _ => tasks // QuickStatusFilter.All
            };
        }

        #region Helper Methods

        private bool IsTaskCompletedForUser(int taskId, string userId)
        {
            return _context.TaskAssignment_Tbl
                .Any(a => a.TaskId == taskId &&
                         a.AssignedUserId == userId &&
                         a.CompletionDate.HasValue);
        }

        #endregion
    }
}