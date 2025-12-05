using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// دسترسی بر اساس سمت در تیم (Position-Based Visibility)
    /// </summary>
    public partial class TaskRepository
    {
        #region Position-Based Visibility

        /// <summary>
        /// بررسی قابلیت مشاهده بر اساس سمت در تیم
        /// </summary>
        public async Task<bool> CanViewBasedOnPositionAsync(string userId, Tasks task)
        {
            if (!task.TeamId.HasValue) return false;

            // دریافت عضویت کاربر
            var membership = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .FirstOrDefaultAsync(tm => tm.UserId == userId &&
                                          tm.TeamId == task.TeamId &&
                                          tm.IsActive);

            if (membership?.Position == null) return false;

            // بررسی ناظر (MembershipType = 1)
            if (membership.MembershipType == 1)
                return true;

            // بررسی سازنده تسک
            var taskCreatorMembership = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .FirstOrDefaultAsync(tm => tm.UserId == task.CreatorUserId &&
                                          tm.TeamId == task.TeamId &&
                                          tm.IsActive);

            if (taskCreatorMembership?.Position != null)
            {
                // زیردستان
                if (membership.Position.CanViewSubordinateTasks &&
                    membership.Position.PowerLevel < taskCreatorMembership.Position.PowerLevel)
                {
                    return true;
                }

                // همسطح
                if (membership.Position.CanViewPeerTasks &&
                    membership.Position.PowerLevel == taskCreatorMembership.Position.PowerLevel)
                {
                    return true;
                }
            }

            // تسک‌های تیمی (VisibilityLevel >= 2)
            if (task.VisibilityLevel >= 2 && membership.Position.CanViewSubordinateTasks)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده بر اساس سمت
        /// </summary>
        public async Task<List<int>> GetPositionBasedVisibleTasksAsync(string userId, int? branchId = null, int? teamId = null)
        {
            var visibleTasks = new HashSet<int>();

            // دریافت عضویت‌ها
            var memberships = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Where(tm => !branchId.HasValue || tm.Team.BranchId == branchId)
                .Where(tm => !teamId.HasValue || tm.TeamId == teamId)
                .ToListAsync();

            foreach (var membership in memberships)
            {
                // ⭐ ناظر تیم (MembershipType = 1)
                if (membership.MembershipType == 1)
                {
                    var supervisedTasks = await GetSupervisedTasksByMembershipAsync(membership);
                    visibleTasks.UnionWith(supervisedTasks);
                }

                // ⭐ بر اساس سمت
                if (membership.Position != null)
                {
                    if (membership.Position.CanViewSubordinateTasks)
                    {
                        var subordinateTasks = await GetSubordinateTasksAsync(membership);
                        visibleTasks.UnionWith(subordinateTasks);
                    }

                    if (membership.Position.CanViewPeerTasks)
                    {
                        var peerTasks = await GetPeerTasksAsync(membership);
                        visibleTasks.UnionWith(peerTasks);
                    }
                }
            }

            return visibleTasks.ToList();
        }

        /// <summary>
        /// دریافت تسک‌های نظارتی بر اساس MembershipType
        /// </summary>
        private async Task<List<int>> GetSupervisedTasksByMembershipAsync(TeamMember membership)
        {
            var supervisedUserIds = await _context.TeamMember_Tbl
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.UserId != membership.UserId &&
                            tm.MembershipType == 0) // عادی
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!supervisedUserIds.Any()) return new List<int>();

            return await _context.TaskAssignment_Tbl
                .Where(ta => supervisedUserIds.Contains(ta.AssignedUserId) &&
                            ta.AssignedInTeamId == membership.TeamId &&
                            !ta.Task.IsDeleted &&
                            !ta.Task.IsPrivate)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های زیردستان
        /// </summary>
        private async Task<List<int>> GetSubordinateTasksAsync(TeamMember membership)
        {
            var subordinateUserIds = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel > membership.Position.PowerLevel)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!subordinateUserIds.Any()) return new List<int>();

            return await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.TeamId == membership.TeamId &&
                            (
                                subordinateUserIds.Contains(t.CreatorUserId) ||
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    subordinateUserIds.Contains(ta.AssignedUserId))
                            ))
                .Select(t => t.Id)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های همسطح
        /// </summary>
        private async Task<List<int>> GetPeerTasksAsync(TeamMember membership)
        {
            var peerUserIds = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel == membership.Position.PowerLevel &&
                            tm.UserId != membership.UserId)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!peerUserIds.Any()) return new List<int>();

            return await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.TeamId == membership.TeamId &&
                            (
                                peerUserIds.Contains(t.CreatorUserId) ||
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    peerUserIds.Contains(ta.AssignedUserId))
                            ))
                .Select(t => t.Id)
                .Distinct()
                .ToListAsync();
        }

        #endregion
    }
}
