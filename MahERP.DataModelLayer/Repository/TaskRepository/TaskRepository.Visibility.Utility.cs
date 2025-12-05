using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// متدهای Utility: آمار، چارت، SubTeam Tasks
    /// </summary>
    public partial class TaskRepository
    {
        #region Utility Methods

        /// <summary>
        /// دریافت تسک‌های کاربر با مجوزها
        /// </summary>
        public async Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(
            string userId, 
            bool includeAssigned = true, 
            bool includeCreated = false, 
            bool includeDeleted = false, 
            bool includeSupervisedTasks = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId);
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId));
            }
            else if (includeCreated)
            {
                query = query.Where(t => t.CreatorUserId == userId);
            }

            return await query
                .Include(t => t.Creator)
                .Include(t => t.Team)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های شعبه با مجوزها
        /// </summary>
        public async Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId, branchId);
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query
                .Include(t => t.Creator)
                .Include(t => t.Team)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های زیرتیم به صورت گروه‌بندی شده
        /// </summary>
        public async Task<SubTeamTasksGroupedViewModel> GetSubTeamTasksGroupedDetailedAsync(
            string userId,
            int? branchId = null)
        {
            var result = new SubTeamTasksGroupedViewModel();

            var managedTeams = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .ToListAsync();

            if (!managedTeams.Any()) return result;

            foreach (var parentTeam in managedTeams)
            {
                var teamGroup = new SubTeamGroupViewModel
                {
                    ParentTeamId = parentTeam.Id,
                    ParentTeamName = parentTeam.Title
                };

                await LoadSubTeamsRecursiveAsync(teamGroup, parentTeam.Id, 1, userId);

                if (teamGroup.SubTeams.Any())
                {
                    teamGroup.TotalTasks = teamGroup.SubTeams.Values.Sum(st => st.TotalTasks);
                    result.TeamGroups[parentTeam.Id] = teamGroup;
                    result.TotalSubTeamTasks += teamGroup.TotalTasks;
                }
            }

            return result;
        }

        /// <summary>
        /// بارگذاری بازگشتی زیرتیم‌ها
        /// </summary>
        private async Task LoadSubTeamsRecursiveAsync(
            SubTeamGroupViewModel teamGroup,
            int parentTeamId,
            int level,
            string currentUserId = null)
        {
            var subTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .ToListAsync();

            foreach (var subTeam in subTeams)
            {
                var tasks = await _context.Tasks_Tbl
                    .Where(t => t.TeamId == subTeam.Id &&
                               !t.IsDeleted &&
                               !t.IsPrivate)
                    .Include(t => t.Creator)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskCategory)
                    .ToListAsync();

                if (tasks.Any())
                {
                    var subTeamViewModel = new SubTeamTasksViewModel
                    {
                        SubTeamId = subTeam.Id,
                        SubTeamName = subTeam.Title,
                        Level = level,
                        TotalTasks = tasks.Count
                    };

                    var tasksByUser = tasks
                        .GroupBy(t => t.CreatorUserId)
                        .ToDictionary(
                            g => g.Key,
                            g => new UserTasksGroupViewModel
                            {
                                UserId = g.Key,
                                UserFullName = g.First().Creator != null
                                    ? $"{g.First().Creator.FirstName} {g.First().Creator.LastName}"
                                    : "نامشخص",
                                Tasks = g.Select(t => MapTaskToViewModel(t, currentUserId)).ToList()
                            });

                    subTeamViewModel.TasksByUser = tasksByUser;
                    teamGroup.SubTeams[subTeam.Id] = subTeamViewModel;
                }

                await LoadSubTeamsRecursiveAsync(teamGroup, subTeam.Id, level + 1, currentUserId);
            }
        }

        /// <summary>
        /// Map Task به ViewModel
        /// </summary>
        private TaskViewModel MapTaskToViewModel(Tasks task, string currentUserId = null)
        {
            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                CompletionDate = !string.IsNullOrEmpty(currentUserId)
                    ? task.TaskAssignments?
                        .FirstOrDefault(t => t.CompletionDate.HasValue && t.AssignedUserId == currentUserId)
                        ?.CompletionDate
                    : null,
                IsActive = task.IsActive,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,
                CreatorUserId = task.CreatorUserId,
                CategoryId = task.TaskCategoryId,
                CategoryTitle = task.TaskCategory?.Title,
                AssignmentsTaskUser = task.TaskAssignments?
                    .Select(a => new TaskAssignmentViewModel
                    {
                        Id = a.Id,
                        TaskId = a.TaskId,
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null
                            ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                            : "نامشخص",
                        CompletionDate = a.CompletionDate,
                        AssignDate = a.AssignmentDate,
                        Description = a.Description
                    }).ToList() ?? new List<TaskAssignmentViewModel>()
            };
        }

        #endregion

        #region Stats & Chart

        /// <summary>
        /// ایجاد چارت دسترسی
        /// </summary>
        public async Task<TaskVisibilityChartViewModel> GenerateVisibilityChartAsync(int branchId)
        {
            // پیاده‌سازی ساده - می‌توان بعداً توسعه داد
            return new TaskVisibilityChartViewModel
            {
                BranchId = branchId,
                BranchName = (await _context.Branch_Tbl.FindAsync(branchId))?.Name ?? "نامشخص"
            };
        }

        /// <summary>
        /// محاسبه آمار دسترسی
        /// </summary>
        public async Task<TaskVisibilityStatsViewModel> CalculateVisibilityStatsAsync(int branchId)
        {
            return new TaskVisibilityStatsViewModel
            {
                TotalTeams = await _context.Team_Tbl
                    .CountAsync(t => t.BranchId == branchId && t.IsActive),
                
                TotalMembers = await _context.TeamMember_Tbl
                    .Include(tm => tm.Team)
                    .CountAsync(tm => tm.Team.BranchId == branchId && tm.IsActive),
                
                TotalPositions = await _context.TeamPosition_Tbl
                    .Include(tp => tp.Team)
                    .CountAsync(tp => tp.Team.BranchId == branchId && tp.IsActive)
            };
        }

        /// <summary>
        /// دریافت منابع دسترسی کاربر
        /// </summary>
        public async Task<List<string>> GetUserAccessSourcesAsync(string userId)
        {
            var sources = new List<string>();

            var managedTeamsCount = await _context.Team_Tbl
                .CountAsync(t => t.ManagerUserId == userId && t.IsActive);
            
            if (managedTeamsCount > 0)
                sources.Add($"مدیر {managedTeamsCount} تیم");

            var positionMemberships = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.UserId == userId && tm.PositionId.HasValue && tm.IsActive)
                .ToListAsync();

            foreach (var membership in positionMemberships)
            {
                if (membership.Position.CanViewSubordinateTasks)
                    sources.Add($"مشاهده زیردستان - {membership.Position.Title}");
                
                if (membership.Position.CanViewPeerTasks)
                    sources.Add($"مشاهده همسطح - {membership.Position.Title}");
            }

            return sources;
        }

        /// <summary>
        /// دریافت اعضای سمت
        /// </summary>
        public async Task<List<MemberTaskVisibilityInfo>> GetPositionMembersAsync(int positionId)
        {
            var members = await _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Where(tm => tm.PositionId == positionId && tm.IsActive)
                .ToListAsync();

            var memberInfos = new List<MemberTaskVisibilityInfo>();

            foreach (var member in members)
            {
                var visibleTaskIds = await GetVisibleTaskIdsAsync(member.UserId);
                
                memberInfos.Add(new MemberTaskVisibilityInfo
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    UserFullName = $"{member.User.FirstName} {member.User.LastName}",
                    VisibleTasksCount = visibleTaskIds.Count,
                    AccessSources = await GetUserAccessSourcesAsync(member.UserId)
                });
            }

            return memberInfos;
        }

        /// <summary>
        /// دریافت اعضای بدون سمت
        /// </summary>
        public async Task<List<MemberTaskVisibilityInfo>> GetMembersWithoutPositionAsync(int teamId)
        {
            var members = await _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Where(tm => tm.TeamId == teamId && !tm.PositionId.HasValue && tm.IsActive)
                .ToListAsync();

            var memberInfos = new List<MemberTaskVisibilityInfo>();

            foreach (var member in members)
            {
                var visibleTaskIds = await GetVisibleTaskIdsAsync(member.UserId);
                
                memberInfos.Add(new MemberTaskVisibilityInfo
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    UserFullName = $"{member.User.FirstName} {member.User.LastName}",
                    VisibleTasksCount = visibleTaskIds.Count,
                    AccessSources = await GetUserAccessSourcesAsync(member.UserId)
                });
            }

            return memberInfos;
        }

        #endregion
    }
}
