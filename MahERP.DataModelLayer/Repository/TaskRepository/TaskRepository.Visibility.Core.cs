using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// ⭐⭐⭐ منطق اصلی دسترسی و بررسی قابلیت مشاهده تسک
    /// </summary>
    public partial class TaskRepository
    {
        #region Core Visibility Checks

        /// <summary>
        /// ⭐⭐⭐ بررسی اینکه آیا کاربر می‌تواند تسک را مشاهده کند (نسخه بروز شده)
        /// </summary>
        public async Task<bool> CanUserViewTaskAsync(string userId, int taskId)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .Include(t => t.Team)
                    .FirstOrDefaultAsync(t => t.Id == taskId);
                
                if (task == null || task.IsDeleted) return false;

                // ⭐ 1. تسک خصوصی: فقط سازنده و اعضا
                if (task.IsPrivate || task.VisibilityLevel == 1)
                {
                    if (task.CreatorUserId == userId) return true;
                    
                    return await _context.TaskAssignment_Tbl
                        .AnyAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);
                }

                // ⭐ 2. سازنده تسک
                if (task.CreatorUserId == userId) return true;

                // ⭐ 3. عضو تسک
                var isAssigned = await _context.TaskAssignment_Tbl
                    .AnyAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);
                
                if (isAssigned) return true;

                // ⭐⭐⭐ 4. مدیر بالاسری (HierarchyManager)
                if (task.TeamId.HasValue)
                {
                    var isHierarchyManager = await IsUserHierarchyManagerOfTaskAsync(userId, task.TeamId.Value);
                    if (isHierarchyManager) return true;
                }

                // ⭐ 5. مدیر تیم مستقیم
                if (task.TeamId.HasValue && task.Team?.ManagerUserId == userId)
                {
                    return true;
                }

                // ⭐ 6. بر اساس سمت
                if (task.TeamId.HasValue)
                {
                    var canViewBasedOnPosition = await CanViewBasedOnPositionAsync(userId, task);
                    if (canViewBasedOnPosition) return true;
                }

                // ⭐ 7. مجوزهای خاص
                var hasSpecialPermission = await HasSpecialPermissionAsync(userId, task);
                if (hasSpecialPermission) return true;

                // ⭐ 8. تسک عمومی (VisibilityLevel >= 3)
                if (task.VisibilityLevel >= 3) return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CanUserViewTaskAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست شناسه تسک‌های قابل مشاهده (بروز شده با HierarchyManager)
        /// </summary>
        public async Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId = null, int? teamId = null)
        {
            var visibleTaskIds = new HashSet<int>();
            var currentTime = DateTime.Now;

            Console.WriteLine($"🔍 GetVisibleTaskIdsAsync - User: {userId}, Branch: {branchId}, Team: {teamId}");

            // ⭐ 0. دریافت شعبه‌های کاربر
            List<int> userBranchIds;
            if (branchId.HasValue)
            {
                userBranchIds = new List<int> { branchId.Value };
            }
            else
            {
                userBranchIds = await _context.BranchUser_Tbl
                    .Where(bu => bu.UserId == userId && bu.IsActive)
                    .Select(bu => bu.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!userBranchIds.Any())
                {
                    Console.WriteLine($"⚠️ کاربر در هیچ شعبه‌ای عضو نیست");
                    return new List<int>();
                }
            }

            // ⭐ 1. تسک‌های ساخته شده
            var createdTasks = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId &&
                            userBranchIds.Contains(t.BranchId ?? 0) &&
                            !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(createdTasks);
            Console.WriteLine($"   ✅ Created: {createdTasks.Count}");

            // ⭐ 2. تسک‌های منتصب شده
            var assignedTasks = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            userBranchIds.Contains(ta.Task.BranchId ?? 0) &&
                            !ta.Task.IsDeleted)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();
            visibleTaskIds.UnionWith(assignedTasks);
            Console.WriteLine($"   ✅ Assigned: {assignedTasks.Count}");

            // ⭐⭐⭐ 3. تسک‌های سلسله مراتبی (HierarchyManager + Manager)
            foreach (var branchIdItem in userBranchIds)
            {
                var hierarchyTasks = await GetHierarchyVisibleTasksAsync(userId, branchIdItem);
                visibleTaskIds.UnionWith(hierarchyTasks);
                Console.WriteLine($"   ✅ Hierarchy (Branch {branchIdItem}): {hierarchyTasks.Count}");
            }

            // ⭐ 4. تسک‌های بر اساس سمت
            var positionTasks = await GetPositionBasedVisibleTasksAsync(userId, branchId, teamId);
            visibleTaskIds.UnionWith(positionTasks);
            Console.WriteLine($"   ✅ Position-based: {positionTasks.Count}");

            // ⭐ 5. مجوزهای خاص
            var specialTasks = await GetSpecialPermissionTasksAsync(userId);
            visibleTaskIds.UnionWith(specialTasks);
            Console.WriteLine($"   ✅ Special permissions: {specialTasks.Count}");

            // ⭐ 6. مجوزهای مستقیم (TaskViewer)
            var directPermissionTasks = await _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId &&
                            tv.IsActive &&
                            (tv.StartDate == null || tv.StartDate <= currentTime) &&
                            (tv.EndDate == null || tv.EndDate > currentTime) &&
                            userBranchIds.Contains(tv.Task.BranchId ?? 0) &&
                            !tv.Task.IsDeleted)
                .Select(tv => tv.TaskId)
                .ToListAsync();
            visibleTaskIds.UnionWith(directPermissionTasks);
            Console.WriteLine($"   ✅ Direct permissions: {directPermissionTasks.Count}");

            Console.WriteLine($"📊 Total visible tasks: {visibleTaskIds.Count}");
            return visibleTaskIds.ToList();
        }

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده (Entity)
        /// </summary>
        public async Task<List<Tasks>> GetVisibleTasksForUserAsync(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);

            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query
                .Include(t => t.Creator)
                .Include(t => t.Team)
                .Include(t => t.TaskCategory)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        #endregion
    }
}
