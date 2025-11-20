using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت تیم‌ها و کاربران
    /// شامل: دریافت تیم‌ها، کاربران تیم، شعبه‌های کاربر
    /// </summary>
    public partial class TaskRepository
    {
        #region Team Operations

        public async Task<List<string>> GetUsersFromTeamsAsync(List<int> teamIds)
        {
            if (teamIds == null || !teamIds.Any())
                return new List<string>();

            var teamUserIds = new List<string>();

            foreach (var teamId in teamIds)
            {
                // دریافت اعضای تیم
                var teamMembers = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.UserId)
                    .ToList();

                teamUserIds.AddRange(teamMembers);

                // اضافه کردن مدیر تیم
                var team = _context.Team_Tbl.FirstOrDefault(t => t.Id == teamId);
                if (team != null && !string.IsNullOrEmpty(team.ManagerUserId))
                {
                    teamUserIds.Add(team.ManagerUserId);
                }
            }

            return teamUserIds.Distinct().ToList();
        }

        public async Task<List<TeamViewModel>> GetUserRelatedTeamsAsync(string userId)
        {
            var teams = new List<TeamViewModel>();

            // تیم‌هایی که کاربر مدیر آن‌هاست
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive);

            // تیم‌هایی که کاربر عضو آن‌هاست
            var memberTeams = _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Team)
                .Where(t => t != null && t.IsActive);

            // ترکیب و حذف تکرار
            var allTeams = managedTeams.Union(memberTeams).Distinct().ToList();

            foreach (var team in allTeams)
            {
                var manager = _context.Users.FirstOrDefault(u => u.Id == team.ManagerUserId);

                teams.Add(new TeamViewModel
                {
                    Id = team.Id,
                    Title = team.Title,
                    Description = team.Description,
                    BranchId = team.BranchId,
                    IsActive = team.IsActive,
                    ManagerFullName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "ندارد"
                });
            }

            return teams.OrderBy(t => t.Title).ToList();
        }

        public async Task<List<TeamViewModel>> GetUserTeamsByBranchAsync(string userId, int branchId)
        {
            try
            {
                Console.WriteLine($"🔍 GetUserTeamsByBranchAsync: UserId={userId}, BranchId={branchId}");

                var userTeams = await _context.TeamMember_Tbl
                    .Where(tm =>
                        tm.UserId == userId &&
                        tm.IsActive &&
                        tm.Team.BranchId == branchId &&
                        tm.Team.IsActive)
                    .Include(tm => tm.Team)
                        .ThenInclude(t => t.Manager)
                    .Include(tm => tm.Team.TeamMembers.Where(m => m.IsActive))
                    .Select(tm => new TeamViewModel
                    {
                        Id = tm.Team.Id,
                        Title = tm.Team.Title,
                        ManagerUserId = tm.Team.ManagerUserId,
                        ManagerName = tm.Team.Manager != null
                            ? $"{tm.Team.Manager.FirstName} {tm.Team.Manager.LastName}"
                            : null,
                        MemberCount = tm.Team.TeamMembers.Count(m => m.IsActive),
                        BranchId = tm.Team.BranchId,
                        IsActive = tm.Team.IsActive
                    })
                    .Distinct()
                    .OrderBy(t => t.Title)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {userTeams.Count} teams");

                // اگر هیچ تیمی نیافت، گزینه "بدون تیم" برگردان
                if (!userTeams.Any())
                {
                    Console.WriteLine("⚠️ No teams found, returning 'بدون تیم' option");
                    return new List<TeamViewModel>
                    {
                        new TeamViewModel
                        {
                            Id = 0,
                            Title = "بدون تیم",
                            ManagerName = null,
                            MemberCount = 0,
                            BranchId = branchId,
                            IsActive = true
                        }
                    };
                }

                return userTeams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserTeamsByBranchAsync: {ex.Message}");
                
                return new List<TeamViewModel>
                {
                    new TeamViewModel
                    {
                        Id = 0,
                        Title = "بدون تیم (خطا)",
                        ManagerName = null,
                        MemberCount = 0
                    }
                };
            }
        }

        public async Task<List<TeamViewModel>> GetBranchTeamsWithManagersAsync(int branchId)
        {
            try
            {
                Console.WriteLine($"🔍 Fetching teams for branch: {branchId}");

                var teams = await _context.Team_Tbl
                    .Where(t => t.BranchId == branchId && t.IsActive)
                    .Include(t => t.Manager)
                    .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        ManagerUserId = t.ManagerUserId,
                        ManagerName = t.Manager != null
                            ? $"{t.Manager.FirstName} {t.Manager.LastName}"
                            : "بدون مدیر",
                        MemberCount = t.TeamMembers.Count(tm => tm.IsActive)
                    })
                    .OrderBy(t => t.Title)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {teams.Count} teams");

                return teams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region User Operations

        public async Task<List<UserViewModelFull>> GetUserRelatedUsersAsync(string userId)
        {
            var relatedUserIds = new HashSet<string>();

            // اعضای تیم‌هایی که کاربر مدیر آن‌هاست
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive);
            foreach (var team in managedTeams)
            {
                var memberIds = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == team.Id && tm.IsActive)
                    .Select(tm => tm.UserId);
                foreach (var memberId in memberIds)
                    relatedUserIds.Add(memberId);
            }

            // همکاران در تیم‌هایی که کاربر عضو آن‌هاست
            var memberTeamIds = _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId);

            foreach (var teamId in memberTeamIds)
            {
                var teammateIds = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.UserId);
                foreach (var teammateId in teammateIds)
                    relatedUserIds.Add(teammateId);
            }

            // تبدیل به UserViewModelFull
            var users = new List<UserViewModelFull>();
            foreach (var relatedUserId in relatedUserIds)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == relatedUserId);
                if (user != null)
                {
                    users.Add(new UserViewModelFull
                    {
                        Id = user.Id,
                        FullNamesString = $"{user.FirstName} {user.LastName}",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        PositionName = user.PositionName,
                        IsActive = user.IsActive
                    });
                }
            }

            return users.OrderBy(u => u.FullNamesString).ToList();
        }

        #endregion

        #region Branch Operations

        public int GetUserBranchId(string userId)
        {
            var branchIds = GetUserBranchIds(userId);
            return branchIds.FirstOrDefault(); // اولین شعبه
        }

        public List<int> GetUserBranchIds(string userId)
        {
            var branchIds = _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Select(bu => bu.BranchId)
                .Distinct()
                .ToList();

            // اگر کاربر در هیچ شعبه‌ای نیست، شعبه پیش‌فرض
            return branchIds.Any() ? branchIds : new List<int> { 1 };
        }

        #endregion
    }
}
