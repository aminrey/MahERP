using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;  // ⭐ اضافه شد
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت ناظران و Supervisors
    /// </summary>
    public partial class TaskRepository
    {
        #region Supervisors Management

        /// <summary>
        /// دریافت ناظران یک تسک
        /// </summary>
        public async Task<List<string>> GetTaskSupervisorsAsync(int taskId, bool includeCreator = false)
        {
            var supervisorIds = new HashSet<string>();

            try
            {
                var task = await _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Select(t => new { t.CreatorUserId, t.BranchId, t.TeamId })
                    .FirstOrDefaultAsync();

                if (task == null) return new List<string>();

                if (includeCreator && !string.IsNullOrEmpty(task.CreatorUserId))
                {
                    supervisorIds.Add(task.CreatorUserId);
                }

                // مدیران تیم
                if (task.TeamId.HasValue)
                {
                    var teamManagerId = await _context.Team_Tbl
                        .Where(t => t.Id == task.TeamId.Value)
                        .Select(t => t.ManagerUserId)
                        .FirstOrDefaultAsync();
                    
                    if (!string.IsNullOrEmpty(teamManagerId))
                        supervisorIds.Add(teamManagerId);
                }

                // ناظران اعضا
                var assignments = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.TaskId == taskId && ta.AssignedInTeamId.HasValue)
                    .Select(ta => new { ta.AssignedUserId, ta.AssignedInTeamId })
                    .ToListAsync();

                foreach (var assignment in assignments)
                {
                    if (assignment.AssignedInTeamId.HasValue && task.BranchId.HasValue)
                    {
                        var userSupervisors = await GetUserSupervisorsInTeamAsync(
                            assignment.AssignedUserId,
                            assignment.AssignedInTeamId.Value,
                            task.BranchId.Value
                        );

                        foreach (var supervisorId in userSupervisors)
                        {
                            supervisorIds.Add(supervisorId);
                        }
                    }
                }

                return supervisorIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskSupervisorsAsync: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// دریافت ناظران کاربر در تیم
        /// </summary>
        public async Task<List<string>> GetUserSupervisorsInTeamAsync(string userId, int teamId, int branchId)
        {
            var supervisorIds = new List<string>();

            try
            {
                var userMembership = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .FirstOrDefaultAsync(tm =>
                        tm.UserId == userId &&
                        tm.TeamId == teamId &&
                        tm.IsActive
                    );

                if (userMembership == null) return supervisorIds;

                // مدیر تیم
                var teamManagerId = await _context.Team_Tbl
                    .Where(t => t.Id == teamId)
                    .Select(t => t.ManagerUserId)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(teamManagerId) && teamManagerId != userId)
                {
                    supervisorIds.Add(teamManagerId);
                }

                // ناظران تیم (MembershipType = 1)
                var teamSupervisorIds = await _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId &&
                                tm.UserId != userId &&
                                tm.IsActive &&
                                tm.MembershipType == 1)
                    .Select(tm => tm.UserId)
                    .ToListAsync();

                supervisorIds.AddRange(teamSupervisorIds);

                // بر اساس سمت
                if (userMembership.Position != null)
                {
                    var seniorMembers = await _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == teamId &&
                                    tm.UserId != userId &&
                                    tm.IsActive &&
                                    tm.Position != null &&
                                    tm.Position.PowerLevel < userMembership.Position.PowerLevel &&
                                    tm.Position.CanViewSubordinateTasks)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    supervisorIds.AddRange(seniorMembers);
                }

                return supervisorIds.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserSupervisorsInTeamAsync: {ex.Message}");
                return supervisorIds;
            }
        }

        /// <summary>
        /// دریافت تمام ناظران کاربر
        /// </summary>
        public async Task<List<string>> GetUserAllSupervisorsAsync(string userId, int? branchId = null)
        {
            var supervisorIds = new HashSet<string>();

            try
            {
                var userMemberships = await _context.TeamMember_Tbl
                    .Include(tm => tm.Team)
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Where(tm => !branchId.HasValue || tm.Team.BranchId == branchId)
                    .ToListAsync();

                foreach (var membership in userMemberships)
                {
                    var teamSupervisors = await GetUserSupervisorsInTeamAsync(
                        userId,
                        membership.TeamId,
                        membership.Team.BranchId
                    );

                    foreach (var supervisorId in teamSupervisors)
                    {
                        supervisorIds.Add(supervisorId);
                    }
                }

                return supervisorIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserAllSupervisorsAsync: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// دریافت ناظران با جزئیات
        /// </summary>
        public async Task<List<SupervisorInfoViewModel>> GetTaskSupervisorsWithDetailsAsync(int taskId)
        {
            var supervisors = new List<SupervisorInfoViewModel>();

            try
            {
                var supervisorIds = await GetTaskSupervisorsAsync(taskId, includeCreator: false);

                foreach (var supervisorId in supervisorIds)
                {
                    var user = await _context.Users
                        .Where(u => u.Id == supervisorId)
                        .Select(u => new
                        {
                            u.Id,
                            u.FirstName,
                            u.LastName,
                            u.Email,
                            u.PhoneNumber,
                            u.ProfileImagePath
                        })
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        supervisors.Add(new SupervisorInfoViewModel
                        {
                            UserId = user.Id,
                            FullName = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            ProfileImagePath = user.ProfileImagePath,
                            SupervisionType = "نامشخص" // می‌توان بعداً تکمیل شود
                        });
                    }
                }

                return supervisors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskSupervisorsWithDetailsAsync: {ex.Message}");
                return supervisors;
            }
        }

        #endregion
    }
}
