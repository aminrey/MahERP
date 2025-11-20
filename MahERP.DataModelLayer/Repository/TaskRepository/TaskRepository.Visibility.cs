using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت دسترسی‌ها و مجوزهای تسک (Visibility & Permissions)
    /// شامل: تسک‌های قابل مشاهده، مجوزها، نظارت
    /// </summary>
    public partial class TaskRepository 
    {
        #region Visibility Core

        public async Task<List<Tasks>> GetVisibleTasksForUserAsync(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
            
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        public List<Tasks> GetVisibleTasksForUser(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId && tv.IsActive)
                .Where(tv => !tv.StartDate.HasValue || tv.StartDate <= DateTime.Now)
                .Where(tv => !tv.EndDate.HasValue || tv.EndDate >= DateTime.Now)
                .Select(tv => tv.TaskId)
                .ToList();

            var query = _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public async Task<bool> CanUserViewTaskAsync(string userId, int taskId)
        {
            return await _taskVisibilityRepository.CanUserViewTaskAsync(userId, taskId);
        }

        public bool CanUserViewTask(string userId, int taskId)
        {
            var now = DateTime.Now;
            
            return _context.TaskViewer_Tbl
                .Any(tv => tv.UserId == userId && 
                          tv.TaskId == taskId && 
                          tv.IsActive &&
                          (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                          (!tv.EndDate.HasValue || tv.EndDate >= now));
        }

        #endregion

        #region User Tasks with Permissions

        public async Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(
            string userId, 
            bool includeAssigned = true, 
            bool includeCreated = false, 
            bool includeDeleted = false, 
            bool includeSupervisedTasks = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            // تسک‌های نظارتی
            var supervisedTaskIds = new List<int>();
            if (includeSupervisedTasks)
            {
                supervisedTaskIds = await GetSupervisedTaskIdsAsync(userId);
            }

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeCreated)
            {
                query = query.Where(t =>
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeSupervisedTasks)
            {
                query = query.Where(t => supervisedTaskIds.Contains(t.Id));
            }

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        public async Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId, branchId);
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        #endregion

        #region Supervised Tasks

        private async Task<List<int>> GetSupervisedTaskIdsAsync(string userId)
        {
            try
            {
                var supervisedTaskIds = new HashSet<int>();

                // 1. نظارت بر اساس سمت (CanViewSubordinateTasks = true)
                var supervisoryPositions = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.Position != null &&
                                tm.Position.CanViewSubordinateTasks)
                    .ToListAsync();

                foreach (var supervisoryPosition in supervisoryPositions)
                {
                    // دریافت اعضای با سمت پایین‌تر در همان تیم
                    var subordinateMembers = await _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == supervisoryPosition.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.Position != null &&
                                   tm.Position.PowerLevel > supervisoryPosition.Position.PowerLevel)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    // دریافت تسک‌های منتصب شده به افراد تحت نظارت
                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => subordinateMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    // دریافت تسک‌های ایجاد شده توسط افراد تحت نظارت
                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => subordinateMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    // اضافه کردن به مجموعه
                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                // 2. نظارت بر اساس MembershipType = 1 (ناظر هم سطح و زیر دستان)
                var supervisoryMemberships = await _context.TeamMember_Tbl
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.MembershipType == 1) // ناظر
                    .ToListAsync();

                foreach (var supervisoryMembership in supervisoryMemberships)
                {
                    // دریافت اعضای عادی تیم
                    var ordinaryMembers = await _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == supervisoryMembership.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.MembershipType == 0) // عضو عادی
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    // دریافت تسک‌های منتصب شده به اعضای عادی
                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => ordinaryMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    // دریافت تسک‌های ایجاد شده توسط اعضای عادی
                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => ordinaryMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    // اضافه کردن به مجموعه
                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                return supervisedTaskIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSupervisedTaskIdsAsync: {ex.Message}");
                return new List<int>();
            }
        }

        #endregion
    }
}
