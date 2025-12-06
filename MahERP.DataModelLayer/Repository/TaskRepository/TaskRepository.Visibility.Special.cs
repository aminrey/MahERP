using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مجوزهای خاص (TaskViewPermission)
    /// </summary>
    public partial class TaskRepository
    {
        #region Special Permissions

        /// <summary>
        /// بررسی مجوزهای خاص برای تسک
        /// </summary>
        public async Task<bool> HasSpecialPermissionAsync(string userId, Tasks task)
        {
            var currentTime = DateTime.Now;

            var permissions = await _context.TaskViewPermission_Tbl
                .Where(tvp => tvp.GranteeUserId == userId &&
                             tvp.IsActive &&
                             (tvp.StartDate == null || tvp.StartDate <= currentTime) &&
                             (tvp.EndDate == null || tvp.EndDate > currentTime))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                switch (permission.PermissionType)
                {
                    case 0: // مشاهده تسک‌های کاربر خاص
                        if (permission.TargetUserId == task.CreatorUserId)
                            return true;
                        
                        var isAssignedToTarget = await _context.TaskAssignment_Tbl
                            .AnyAsync(ta => ta.TaskId == task.Id && 
                                           ta.AssignedUserId == permission.TargetUserId);
                        if (isAssignedToTarget) return true;
                        break;

                    case 1: // مشاهده تسک‌های تیم خاص
                        if (permission.TargetTeamId.HasValue && task.TeamId == permission.TargetTeamId)
                            return true;
                        break;

                    case 2: // مشاهده تسک‌های تیم و زیرتیم‌ها
                        if (permission.TargetTeamId.HasValue)
                        {
                            var allSubTeamIds = await GetAllSubTeamIdsAsync(permission.TargetTeamId.Value);
                            allSubTeamIds.Add(permission.TargetTeamId.Value);

                            if (task.TeamId.HasValue && allSubTeamIds.Contains(task.TeamId.Value))
                                return true;
                        }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// دریافت تسک‌های با مجوز خاص
        /// </summary>
        public async Task<List<int>> GetSpecialPermissionTasksAsync(string userId)
        {
            var currentTime = DateTime.Now;
            var visibleTasks = new HashSet<int>();

            var permissions = await _context.TaskViewPermission_Tbl
                .Where(tvp => tvp.GranteeUserId == userId &&
                             tvp.IsActive &&
                             (tvp.StartDate == null || tvp.StartDate <= currentTime) &&
                             (tvp.EndDate == null || tvp.EndDate > currentTime))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                List<int> permissionTasks = new List<int>();

                switch (permission.PermissionType)
                {
                    case 0: // کاربر خاص
                        if (!string.IsNullOrEmpty(permission.TargetUserId))
                        {
                            permissionTasks = await _context.Tasks_Tbl
                                .Where(t => (t.CreatorUserId == permission.TargetUserId ||
                                            _context.TaskAssignment_Tbl.Any(ta => 
                                                ta.TaskId == t.Id && 
                                                ta.AssignedUserId == permission.TargetUserId)) &&
                                            !t.IsDeleted &&
                                            !t.IsPrivate)
                                .Select(t => t.Id)
                                .ToListAsync();
                        }
                        break;

                    case 1: // تیم خاص
                        if (permission.TargetTeamId.HasValue)
                        {
                            permissionTasks = await _context.Tasks_Tbl
                                .Where(t => t.TeamId == permission.TargetTeamId && 
                                            !t.IsDeleted &&
                                            !t.IsPrivate)
                                .Select(t => t.Id)
                                .ToListAsync();
                        }
                        break;

                    case 2: // تیم و زیرتیم‌ها
                        if (permission.TargetTeamId.HasValue)
                        {
                            var allSubTeamIds = await GetAllSubTeamIdsAsync(permission.TargetTeamId.Value);
                            allSubTeamIds.Add(permission.TargetTeamId.Value);

                            permissionTasks = await _context.Tasks_Tbl
                                .Where(t => t.TeamId.HasValue &&
                                           allSubTeamIds.Contains(t.TeamId.Value) &&
                                           !t.IsDeleted &&
                                           !t.IsPrivate)
                                .Select(t => t.Id)
                                .ToListAsync();
                        }
                        break;
                }

                visibleTasks.UnionWith(permissionTasks);
            }

            return visibleTasks.ToList();
        }

        #endregion
    }
}
