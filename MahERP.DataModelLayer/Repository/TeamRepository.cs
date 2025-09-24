using AutoMapper;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        // Constant برای نام سمت مدیریت
        private const string MANAGEMENT_POSITION_TITLE = "مدیریت تیم";

        public TeamRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Team CRUD Operations

        public List<TeamViewModel> GetTeamsByBranchId(int branchId, bool includeInactive = false)
        {
            var query = _context.Team_Tbl
                .Include(t => t.Manager)
                .Include(t => t.ParentTeam)
                .Include(t => t.Branch)
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Where(t => t.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(t => t.IsActive);

            var teams = query.ToList();
            return _mapper.Map<List<TeamViewModel>>(teams);
        }
        public TeamViewModel GetTeamById(int teamId, bool includePositions = false, bool includeMembers = false)
        {
            var query = _context.Team_Tbl
                .Include(t => t.Manager)
                .Include(t => t.ParentTeam)
                .Include(t => t.Branch)
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Include(t => t.ChildTeams)
                .AsQueryable();

            // شامل کردن سمت‌ها در صورت درخواست
            if (includePositions)
            {
                query = query.Include(t => t.TeamPositions.Where(tp => tp.IsActive))
                    .ThenInclude(tp => tp.TeamMembers.Where(tm => tm.IsActive))
                        .ThenInclude(tm => tm.User);
            }

            // شامل کردن اعضا در صورت درخواست
            if (includeMembers)
            {
                query = query.Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .ThenInclude(tm => tm.User)
                    .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .ThenInclude(tm => tm.Position);
            }

            var team = query.FirstOrDefault(t => t.Id == teamId);

            if (team == null) return null;

            var teamViewModel = _mapper.Map<TeamViewModel>(team);

            // تنظیم اطلاعات اضافی در صورت نیاز
            if (includePositions && team.TeamPositions != null)
            {
                teamViewModel.TeamPositions = team.TeamPositions
                    .Where(tp => tp.IsActive)
                    .OrderBy(tp => tp.PowerLevel)
                    .Select(tp => new TeamPositionViewModel
                    {
                        Id = tp.Id,
                        TeamId = tp.TeamId,
                        TeamTitle = team.Title,
                        Title = tp.Title,
                        Description = tp.Description,
                        PowerLevel = tp.PowerLevel,
                        CanViewSubordinateTasks = tp.CanViewSubordinateTasks,
                        CanViewPeerTasks = tp.CanViewPeerTasks,
                        MaxMembers = tp.MaxMembers,
                        IsDefault = tp.IsDefault,
                        IsActive = tp.IsActive,
                        DisplayOrder = tp.DisplayOrder,
                        CreateDate = tp.CreateDate,
                        CreatorName = tp.Creator != null ? $"{tp.Creator.FirstName} {tp.Creator.LastName}" : "",
                        LastUpdateDate = tp.LastUpdateDate,
                        LastUpdaterName = tp.LastUpdater != null ? $"{tp.LastUpdater.FirstName} {tp.LastUpdater.LastName}" : "",
                        Members = tp.TeamMembers?.Where(tm => tm.IsActive).Select(tm => new TeamMemberViewModel
                        {
                            Id = tm.Id,
                            TeamId = tm.TeamId,
                            UserId = tm.UserId,
                            UserFullName = $"{tm.User.FirstName} {tm.User.LastName}",
                            PositionId = tm.PositionId,
                            PositionTitle = tp.Title,
                            PowerLevel = tp.PowerLevel,
                            RoleDescription = tm.RoleDescription,
                            MembershipType = tm.MembershipType,
                            StartDate = tm.StartDate,
                            EndDate = tm.EndDate,
                            IsActive = tm.IsActive
                        }).ToList() ?? new List<TeamMemberViewModel>()
                    }).ToList();
            }

            if (includeMembers && team.TeamMembers != null)
            {
                teamViewModel.TeamMembers = team.TeamMembers
                    .Where(tm => tm.IsActive)
                    .Select(tm => new TeamMemberViewModel
                    {
                        Id = tm.Id,
                        TeamId = tm.TeamId,
                        TeamTitle = team.Title,
                        UserId = tm.UserId,
                        UserFullName = $"{tm.User.FirstName} {tm.User.LastName}",
                        PositionId = tm.PositionId,
                        PositionTitle = tm.Position?.Title ?? null,
                        PowerLevel = tm.Position?.PowerLevel,
                        Position = tm.Position?.Title ?? "عضو", // فیلد legacy برای سازگاری
                        RoleDescription = tm.RoleDescription,
                        MembershipType = tm.MembershipType,
                        MembershipTypeText = tm.MembershipType switch
                        {
                            0 => "عضو عادی",
                            1 => "عضو ویژه",
                            2 => "مدیر تیم",
                            _ => "نامشخص"
                        },
                        StartDate = tm.StartDate,
                        EndDate = tm.EndDate,
                        AddedByUserId = tm.AddedByUserId,
                        AddedByUserName = tm.AddedByUser != null ? $"{tm.AddedByUser.FirstName} {tm.AddedByUser.LastName}" : "",
                        IsActive = tm.IsActive,
                        CreateDate = tm.CreateDate,
                        LastUpdateDate = tm.LastUpdateDate
                    }).ToList();
            }

            return teamViewModel;
        }
        public Team GetTeamEntityById(int teamId)
        {
            return _context.Team_Tbl
                .Include(t => t.ChildTeams)
                .Include(t => t.TeamMembers)
                .FirstOrDefault(t => t.Id == teamId);
        }

        public int CreateTeam(Team team)
        {
            team.CreateDate = DateTime.Now;
            _context.Team_Tbl.Add(team);
            _context.SaveChanges();
            return team.Id;
        }

        public bool UpdateTeam(Team team)
        {
            try
            {
                team.LastUpdateDate = DateTime.Now;
                _context.Team_Tbl.Update(team);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTeam(int teamId)
        {
            try
            {
                var team = GetTeamEntityById(teamId);
                if (team == null || !CanDeleteTeam(teamId))
                    return false;

                // ??? ????? ???
                _context.TeamMember_Tbl.RemoveRange(team.TeamMembers);

                // ??? ???
                _context.Team_Tbl.Remove(team);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToggleTeamStatus(int teamId)
        {
            try
            {
                var team = _context.Team_Tbl.Find(teamId);
                if (team == null) return false;

                team.IsActive = !team.IsActive;
                team.LastUpdateDate = DateTime.Now;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region TeamMember CRUD Operations

        public List<TeamMemberViewModel> GetTeamMembers(int teamId, bool includeInactive = false)
        {
            var query = _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Include(tm => tm.Team)
                .Include(tm => tm.AddedByUser)
                .Where(tm => tm.TeamId == teamId);

            if (!includeInactive)
                query = query.Where(tm => tm.IsActive);

            var members = query.ToList();
            return _mapper.Map<List<TeamMemberViewModel>>(members);
        }

        public TeamMemberViewModel GetTeamMemberById(int memberId)
        {
            var member = _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Include(tm => tm.Team)
                .Include(tm => tm.AddedByUser)
                .FirstOrDefault(tm => tm.Id == memberId);

            return member != null ? _mapper.Map<TeamMemberViewModel>(member) : null;
        }

        public int AddTeamMember(TeamMember member)
        {
            member.CreateDate = DateTime.Now;
            member.StartDate = DateTime.Now;
            _context.TeamMember_Tbl.Add(member);
            _context.SaveChanges();
            return member.Id;
        }

        public bool UpdateTeamMember(TeamMember member)
        {
            try
            {
                member.LastUpdateDate = DateTime.Now;
                _context.TeamMember_Tbl.Update(member);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveTeamMember(int memberId)
        {
            try
            {
                var member = _context.TeamMember_Tbl.Find(memberId);
                if (member == null) return false;

                _context.TeamMember_Tbl.Remove(member);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Organizational Chart

        public OrganizationalChartViewModel GetOrganizationalChart(int branchId)
        {
            var teams = GetTeamHierarchy(branchId); // اصلاح شده
            var availableUsers = GetAvailableUsersForBranch(branchId);

            var branchName = _context.Branch_Tbl
                .Where(b => b.Id == branchId)
                .Select(b => b.Name)
                .FirstOrDefault();

            return new OrganizationalChartViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                RootTeams = teams.Where(t => !t.ParentTeamId.HasValue).ToList(),
                AllTeams = teams,
                AvailableUsers = availableUsers
            };
        }

        /// <summary>
        /// دریافت سلسله مراتب تیم‌ها برای چارت سازمانی
        /// </summary>
        public List<TeamViewModel> GetTeamHierarchy(int branchId)
        {
            var teams = GetTeamsByBranchId(branchId, false);

            // محاسبه سطح هر تیم در سلسله مراتب
            foreach (var team in teams)
            {
                team.Level = CalculateTeamLevel(team, teams);
                team.ChildTeams = teams.Where(t => t.ParentTeamId == team.Id).ToList();
            }

            return teams;
        }

        private int CalculateTeamLevel(TeamViewModel team, List<TeamViewModel> allTeams)
        {
            if (!team.ParentTeamId.HasValue)
                return 0;

            var parent = allTeams.FirstOrDefault(t => t.Id == team.ParentTeamId.Value);
            return parent != null ? CalculateTeamLevel(parent, allTeams) + 1 : 0;
        }

        public List<UserSelectListItem> GetAvailableUsersForBranch(int branchId)
        {
            var users = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Where(bu => bu.BranchId == branchId && bu.User.IsActive)
                .Select(bu => new UserSelectListItem
                {
                    UserId = bu.UserId,
                    FullName = $"{bu.User.FirstName} {bu.User.LastName}",
                    Position = bu.User.PositionName
                })
                .Distinct()
                .ToList();

            return users;
        }

        #endregion

        #region Manager Assignment

        public bool AssignManager(int teamId, string managerUserId, string assignedByUserId)
        {
            try
            {
                var team = _context.Team_Tbl.Find(teamId);
                if (team == null) return false;

                // تنظیم مدیر در جدول Team
                team.ManagerUserId = managerUserId;
                team.LastUpdaterUserId = assignedByUserId;
                team.LastUpdateDate = DateTime.Now;

                // اضافه کردن مدیر به سمت مدیریت
                var success = AddManagerToManagementPosition(teamId, managerUserId, assignedByUserId);
                if (!success) return false;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveManager(int teamId)
        {
            try
            {
                var team = _context.Team_Tbl.Find(teamId);
                if (team == null) return false;

                // حذف مدیر از سمت مدیریت
                RemoveManagerFromManagementPosition(teamId);

                // حذف مدیر از جدول Team
                team.ManagerUserId = null;
                team.LastUpdateDate = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Utility Methods

        public bool IsUserTeamManager(string userId, int teamId)
        {
            return _context.Team_Tbl
                .Any(t => t.Id == teamId && t.ManagerUserId == userId);
        }

        public List<Team> GetTeamsByManagerId(string managerId)
        {
            return _context.Team_Tbl
                .Where(t => t.ManagerUserId == managerId && t.IsActive)
                .ToList();
        }

        public bool HasChildTeams(int teamId)
        {
            return _context.Team_Tbl
                .Any(t => t.ParentTeamId == teamId && t.IsActive);
        }

        public bool CanDeleteTeam(int teamId)
        {
            // ????? ???? ??????? ????? ?? ????? ????
            return !HasChildTeams(teamId) &&
                   !_context.TeamMember_Tbl.Any(tm => tm.TeamId == teamId && tm.IsActive);
        }

        #endregion

        #region Team Position Methods

        public List<TeamPosition> GetTeamPositions(int teamId, bool includeInactive = false)
        {
            var query = _context.TeamPosition_Tbl
                .Include(p => p.TeamMembers.Where(tm => tm.IsActive))
                    .ThenInclude(tm => tm.User)
                .Where(p => p.TeamId == teamId);

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query.OrderBy(p => p.PowerLevel).ToList();
        }

        public TeamPosition GetTeamPositionById(int positionId)
        {
            return _context.TeamPosition_Tbl
                .Include(p => p.Team)
                .Include(p => p.TeamMembers.Where(tm => tm.IsActive))
                    .ThenInclude(tm => tm.User)
                .Include(p => p.Creator)
                .Include(p => p.LastUpdater)
                .FirstOrDefault(p => p.Id == positionId);
        }

        public int CreateTeamPosition(TeamPosition position)
        {
            position.CreateDate = DateTime.Now;
            _context.TeamPosition_Tbl.Add(position);
            _context.SaveChanges();
            return position.Id;
        }

        public bool UpdateTeamPosition(TeamPosition position)
        {
            try
            {
                position.LastUpdateDate = DateTime.Now;
                _context.TeamPosition_Tbl.Update(position);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTeamPosition(int positionId)
        {
            try
            {
                var position = GetTeamPositionById(positionId);
                if (position == null || !CanDeletePosition(positionId))
                    return false;

                _context.TeamPosition_Tbl.Remove(position);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CanDeletePosition(int positionId)
        {
            return !_context.TeamMember_Tbl
                .Any(tm => tm.PositionId == positionId && tm.IsActive);
        }

        /// <summary>
        /// دریافت درخت سمت‌ها با اعضا برای یک تیم خاص
        /// </summary>
        public TeamHierarchyViewModel GetTeamPositionHierarchy(int teamId)
        {
            var team = GetTeamById(teamId);
            if (team == null) return null;

            var positions = GetTeamPositions(teamId);

            var hierarchy = new TeamHierarchyViewModel
            {
                TeamId = teamId,
                TeamTitle = team.Title,
                Positions = positions.Select(p => new TeamPositionHierarchyViewModel
                {
                    Id = p.Id,
                    TeamId = teamId,
                    Title = p.Title,
                    Description = p.Description,
                    PowerLevel = p.PowerLevel,
                    CurrentMembers = p.TeamMembers.Count(tm => tm.IsActive),
                    MaxMembers = p.MaxMembers,
                    CanAddMember = !p.MaxMembers.HasValue || p.TeamMembers.Count(tm => tm.IsActive) < p.MaxMembers.Value,
                    PowerLevelText = GetPowerLevelText(p.PowerLevel), // استفاده از متد جدید
                    Members = p.TeamMembers.Where(tm => tm.IsActive).Select(tm => new TeamMemberViewModel
                    {
                        Id = tm.Id,
                        UserId = tm.UserId,
                        UserFullName = $"{tm.User.FirstName} {tm.User.LastName}",
                        PositionId = tm.PositionId,
                        PositionTitle = p.Title,
                        PowerLevel = p.PowerLevel,
                        RoleDescription = tm.RoleDescription,
                        IsActive = tm.IsActive
                    }).ToList()
                }).ToList(),
                MembersWithoutPosition = GetTeamMembers(teamId)
                    .Where(tm => tm.IsActive && !tm.PositionId.HasValue).ToList()
            };

            return hierarchy;
        }

        public bool IsPositionTitleUnique(int teamId, string title, int? excludePositionId = null)
        {
            var query = _context.TeamPosition_Tbl
                .Where(p => p.TeamId == teamId && p.Title == title && p.IsActive);

            if (excludePositionId.HasValue)
                query = query.Where(p => p.Id != excludePositionId.Value);

            return !query.Any();
        }

        /// <summary>
        /// بررسی تکراری نبودن سطح قدرت در تیم - حذف شده چون سطوح تکراری مجاز است
        /// </summary>
        [Obsolete("سطوح قدرت تکراری مجاز است - همیشه true برمی‌گرداند", false)]
        public bool IsPowerLevelUnique(int teamId, int powerLevel, int? excludePositionId = null)
        {
            
            var query = _context.TeamPosition_Tbl
                .Where(p => p.TeamId == teamId && p.PowerLevel == powerLevel && p.IsActive);

            if (excludePositionId.HasValue)
                query = query.Where(p => p.Id != excludePositionId.Value);

            return !query.Any();
        }

        /// <summary>
        /// بررسی تکراری نبودن سمت پیش‌فرض در تیم
        /// فقط یک سمت در هر تیم می‌تواند پیش‌فرض باشد
        /// </summary>
        public bool IsDefaultPositionUnique(int teamId, int? excludePositionId = null)
        {
            var query = _context.TeamPosition_Tbl
                .Where(p => p.TeamId == teamId && p.IsDefault && p.IsActive);

            if (excludePositionId.HasValue)
                query = query.Where(p => p.Id != excludePositionId.Value);

            return !query.Any();
        }

        /// <summary>
        /// ایجاد یا دریافت سمت "مدیریت تیم" برای یک تیم
        /// </summary>
        public TeamPosition GetOrCreateManagementPosition(int teamId, string creatorUserId)
        {
            // ابتدا بررسی می‌کنیم که سمت مدیریت وجود دارد یا نه
            var existingPosition = GetManagementPosition(teamId);
            if (existingPosition != null)
                return existingPosition;

            // ایجاد سمت مدیریت جدید
            var managementPosition = new TeamPosition
            {
                TeamId = teamId,
                Title = "مدیریت تیم",
                Description = "سمت مدیریت تیم - ایجاد شده به صورت خودکار",
                PowerLevel = 1, // بالاترین سطح قدرت
                CanViewSubordinateTasks = true,
                CanViewPeerTasks = true,
                MaxMembers = 1, // فقط یک نفر می‌تواند مدیر باشد
                DisplayOrder = 0,
                IsDefault = false,
                IsActive = true,
                CreatorUserId = creatorUserId,
                CreateDate = DateTime.Now
            };

            CreateTeamPosition(managementPosition);
            return managementPosition;
        }

        /// <summary>
        /// دریافت سمت مدیریت تیم
        /// </summary>
        public TeamPosition GetManagementPosition(int teamId)
        {
            return _context.TeamPosition_Tbl
                .Include(p => p.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .FirstOrDefault(p => p.TeamId == teamId &&
                                   p.Title == MANAGEMENT_POSITION_TITLE &&
                                   p.PowerLevel == 0 && // تغییر از 999 به 0
                                   p.IsActive);
        }

        /// <summary>
        /// اضافه کردن مدیر تیم به سمت مدیریت
        /// </summary>
        public bool AddManagerToManagementPosition(int teamId, string managerUserId, string assignedByUserId)
        {
            try
            {
                // دریافت یا ایجاد سمت مدیریت
                var managementPosition = GetOrCreateManagementPosition(teamId, assignedByUserId);

                // حذف مدیر قبلی از سمت مدیریت (اگر وجود دارد)
                RemoveManagerFromManagementPosition(teamId);

                // بررسی اینکه کاربر قبلاً عضو تیم است یا نه
                var existingMember = _context.TeamMember_Tbl
                    .FirstOrDefault(tm => tm.TeamId == teamId && tm.UserId == managerUserId && tm.IsActive);

                if (existingMember != null)
                {
                    // اگر قبلاً عضو است، فقط سمتش را به مدیریت تغییر می‌دهیم
                    existingMember.PositionId = managementPosition.Id;
                    existingMember.MembershipType = 2; // مدیر تیم
                    existingMember.RoleDescription = "مدیر تیم";
                    existingMember.LastUpdateDate = DateTime.Now;
                    _context.TeamMember_Tbl.Update(existingMember);
                }
                else
                {
                    // اضافه کردن به عنوان عضو جدید در سمت مدیریت
                    var teamMember = new TeamMember
                    {
                        TeamId = teamId,
                        UserId = managerUserId,
                        PositionId = managementPosition.Id,
                        MembershipType = 2, // مدیر تیم
                        RoleDescription = "مدیر تیم",
                        StartDate = DateTime.Now,
                        AddedByUserId = assignedByUserId,

                        IsActive = true,
                        CreateDate = DateTime.Now
                    };

                    _context.TeamMember_Tbl.Add(teamMember);
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// حذف مدیر از سمت مدیریت (بدون حذف سمت)
        /// </summary>
        public bool RemoveManagerFromManagementPosition(int teamId)
        {
            try
            {
                var managementPosition = GetManagementPosition(teamId);
                if (managementPosition == null) return true;

                // حذف تمام اعضای سمت مدیریت
                var managementMembers = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.PositionId == managementPosition.Id && tm.IsActive)
                    .ToList();

                foreach (var member in managementMembers)
                {
                    // به جای حذف کامل، می‌توانید سمت را null کنید یا عضویت را غیرفعال کنید
                    member.PositionId = null;
                    member.MembershipType = 0; // عضو عادی
                    member.RoleDescription = "عضو";
                    member.LastUpdateDate = DateTime.Now;
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// خارج کردن عضو از سمت (بدون حذف از تیم)
        /// </summary>
        public bool RemoveMemberFromPosition(int memberId)
        {
            try
            {
                var member = _context.TeamMember_Tbl.Find(memberId);
                if (member == null) return false;

                // فقط سمت را null می‌کنیم، عضو در تیم باقی می‌ماند
                member.PositionId = null;
                member.MembershipType = 0; // تبدیل به عضو عادی
                member.RoleDescription = "عضو";
                member.LastUpdateDate = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تخصیص عضو به سمت
        /// </summary>
        public bool AssignMemberToPosition(int memberId, int positionId)
        {
            try
            {
                var member = _context.TeamMember_Tbl.Find(memberId);
                var position = _context.TeamPosition_Tbl.Find(positionId);

                if (member == null || position == null) return false;

                // بررسی ظرفیت سمت
                if (position.MaxMembers.HasValue)
                {
                    var currentCount = _context.TeamMember_Tbl
                        .Count(tm => tm.PositionId == positionId && tm.IsActive);

                    if (currentCount >= position.MaxMembers.Value)
                        return false;
                }

                member.PositionId = positionId;
                member.LastUpdateDate = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region Advanced Team Member Management Methods

        /// <summary>
        /// آماده‌سازی داده‌های ViewBag برای مدیریت اعضای تیم
        /// </summary>
        public TeamMemberManagementData PrepareTeamMemberData(int branchId, int teamId)
        {
            var availableUsers = GetAvailableUsersForBranch(branchId);
            var availablePositions = GetTeamPositions(teamId)
                .Where(p => p.IsActive && p.CanAddMember)
                .ToList();

            return new TeamMemberManagementData
            {
                AvailableUsers = availableUsers,
                AvailablePositions = availablePositions,
                BranchId = branchId,
                TeamId = teamId
            };
        }

        /// <summary>
        /// ایجاد عضو جدید با validation کامل
        /// </summary>
        public CreateMemberResult CreateTeamMemberWithValidation(TeamMemberViewModel model, string addedByUserId)
        {
            try
            {
                // بررسی وجود کاربر
                var user = _context.Users.Find(model.UserId);
                if (user == null)
                    return CreateMemberResult.Failed("کاربر مورد نظر یافت نشد.");

                // بررسی عدم تکراری بودن عضویت
                var existingMember = _context.TeamMember_Tbl
                    .FirstOrDefault(tm => tm.TeamId == model.TeamId && tm.UserId == model.UserId && tm.IsActive);

                if (existingMember != null)
                    return CreateMemberResult.Failed("این کاربر قبلاً عضو این تیم است.");

                // بررسی ظرفیت سمت (اگر انتخاب شده)
                if (model.PositionId.HasValue)
                {
                    var position = GetTeamPositionById(model.PositionId.Value);
                    if (position == null)
                        return CreateMemberResult.Failed("سمت انتخاب شده یافت نشد.");

                    if (!position.CanAddMember)
                        return CreateMemberResult.Failed($"ظرفیت سمت '{position.Title}' تکمیل شده است.");
                }

                // ایجاد عضو جدید
                var member = new TeamMember
                {
                    TeamId = model.TeamId,
                    UserId = model.UserId,
                    PositionId = model.PositionId,
                    RoleDescription = model.RoleDescription,
                    MembershipType = model.MembershipType,
                    IsActive = model.IsActive,
                    AddedByUserId = addedByUserId,
                    StartDate = DateTime.Now,
                    CreateDate = DateTime.Now
                };

                var memberId = AddTeamMember(member);

                return CreateMemberResult.Success(memberId, "عضو با موفقیت به تیم اضافه شد.");
            }
            catch (Exception ex)
            {
                return CreateMemberResult.Failed($"خطا در اضافه کردن عضو: {ex.Message}");
            }
        }

        /// <summary>
        /// ویرایش عضو با validation
        /// </summary>
        public UpdateMemberResult UpdateTeamMemberWithValidation(TeamMemberViewModel model)
        {
            try
            {
                var member = _context.TeamMember_Tbl.Find(model.Id);
                if (member == null)
                    return UpdateMemberResult.Failed("عضو مورد نظر یافت نشد.");

                // بررسی تغییر سمت و ظرفیت
                if (model.PositionId.HasValue && model.PositionId != member.PositionId)
                {
                    var newPosition = GetTeamPositionById(model.PositionId.Value);
                    if (newPosition == null)
                        return UpdateMemberResult.Failed("سمت انتخاب شده یافت نشد.");

                    if (!newPosition.CanAddMember)
                        return UpdateMemberResult.Failed($"ظرفیت سمت '{newPosition.Title}' تکمیل شده است.");
                }

                // بروزرسانی اطلاعات
                member.PositionId = model.PositionId;
                member.RoleDescription = model.RoleDescription;
                member.MembershipType = model.MembershipType;
                member.IsActive = model.IsActive;
                member.LastUpdateDate = DateTime.Now;

                var success = UpdateTeamMember(member);

                return success
                    ? UpdateMemberResult.Success("اطلاعات عضو با موفقیت بروزرسانی شد.")
                    : UpdateMemberResult.Failed("خطا در بروزرسانی اطلاعات عضو.");
            }
            catch (Exception ex)
            {
                return UpdateMemberResult.Failed($"خطا: {ex.Message}");
            }
        }

        /// <summary>
        /// حذف عضو با بررسی‌های امنیتی
        /// </summary>
        public DeleteMemberResult DeleteTeamMemberWithValidation(int memberId)
        {
            try
            {
                var member = _context.TeamMember_Tbl
                    .Include(tm => tm.User)
                    .Include(tm => tm.Team)
                    .FirstOrDefault(tm => tm.Id == memberId);

                if (member == null)
                    return DeleteMemberResult.Failed("عضو مورد نظر یافت نشد.");

                // بررسی اینکه آیا عضو مدیر تیم است
                if (member.Team.ManagerUserId == member.UserId)
                    return DeleteMemberResult.Failed("نمی‌توان مدیر تیم را حذف کرد. ابتدا مدیر جدید انتخاب کنید.");

                var success = RemoveTeamMember(memberId);

                return success
                    ? DeleteMemberResult.Success("عضو با موفقیت از تیم حذف شد.")
                    : DeleteMemberResult.Failed("خطا در حذف عضو از تیم.");
            }
            catch (Exception ex)
            {
                return DeleteMemberResult.Failed($"خطا: {ex.Message}");
            }
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال عضو
        /// </summary>
        public ToggleMemberStatusResult ToggleMemberStatus(int memberId)
        {
            try
            {
                var member = _context.TeamMember_Tbl.Find(memberId);
                if (member == null)
                    return ToggleMemberStatusResult.Failed("عضو مورد نظر یافت نشد.");

                member.IsActive = !member.IsActive;
                member.LastUpdateDate = DateTime.Now;

                var success = UpdateTeamMember(member);

                return success
                    ? ToggleMemberStatusResult.Success($"وضعیت عضو به '{(member.IsActive ? "فعال" : "غیرفعال")}' تغییر یافت.")
                    : ToggleMemberStatusResult.Failed("خطا در تغییر وضعیت عضو.");
            }
            catch (Exception ex)
            {
                return ToggleMemberStatusResult.Failed($"خطا: {ex.Message}");
            }
        }

        /// <summary>
        /// جستجو و فیلترینگ اعضای تیم
        /// </summary>
        public List<TeamMemberViewModel> SearchTeamMembers(int teamId, TeamMemberSearchFilter filter)
        {
            var query = _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Include(tm => tm.Position)
                .Include(tm => tm.AddedByUser)
                .Where(tm => tm.TeamId == teamId);

            // فیلتر وضعیت فعال بودن
            if (filter.IsActive.HasValue)
                query = query.Where(tm => tm.IsActive == filter.IsActive.Value);

            // فیلتر سمت
            if (filter.PositionId.HasValue)
                query = query.Where(tm => tm.PositionId == filter.PositionId.Value);

            // فیلتر نوع عضویت
            if (filter.MembershipType.HasValue)
                query = query.Where(tm => tm.MembershipType == filter.MembershipType.Value);

            // فیلتر متنی
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                var searchText = filter.SearchText.ToLower();
                query = query.Where(tm =>
                    tm.User.FirstName.ToLower().Contains(searchText) ||
                    tm.User.LastName.ToLower().Contains(searchText) ||
                    tm.RoleDescription.ToLower().Contains(searchText));
            }

            var members = query.ToList();
            return _mapper.Map<List<TeamMemberViewModel>>(members);
        }

        #endregion


        /// <summary>
        /// دریافت متن سطح قدرت بر اساس PowerLevel
        /// </summary>
        public static string GetPowerLevelText(int powerLevel)
        {
            return powerLevel switch
            {
                0 => "مدیر تیم",
                1 => "معاون/سرپرست",
                2 => "کارشناس ارشد",
                3 => "کارشناس",
                4 => "کارشناس مبتدی",
                5 => "منشی/دستیار",
                6 => "کارمند",
                _ => $"سطح {powerLevel}"
            };
        }

        /// <summary>
        /// دریافت عنوان پیش‌فرض سمت بر اساس PowerLevel
        /// </summary>
        public static string GetDefaultPositionTitle(int powerLevel)
        {
            return powerLevel switch
            {
                0 => "مدیر تیم",
                1 => "معاون",
                2 => "کارشناس ارشد",
                3 => "کارشناس",
                4 => "کارشناس مبتدی",
                5 => "منشی",
                6 => "کارمند",
                _ => $"سمت سطح {powerLevel}"
            };
        }


        #region ViewBag Preparation Methods

        /// <summary>
        /// آماده‌سازی داده‌های ViewBag برای تیم
        /// </summary>
        public TeamViewBagData PrepareTeamViewBags(int branchId)
        {
            var data = new TeamViewBagData();

            // دریافت تیم‌های والد ممکن
            var parentTeams = GetTeamsByBranchId(branchId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Title
                }).ToList();

            parentTeams.Insert(0, new SelectListItem { Value = "", Text = "-- بدون تیم والد --" });
            data.ParentTeams = parentTeams;

            // دریافت کاربران موجود برای انتخاب مدیر
            var availableUsers = GetAvailableUsersForBranch(branchId)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId,
                    Text = u.FullName + (string.IsNullOrEmpty(u.Position) ? "" : $" ({u.Position})")
                }).ToList();

            availableUsers.Insert(0, new SelectListItem { Value = "", Text = "-- انتخاب مدیر --" });
            data.AvailableUsers = availableUsers;

            // سطوح دسترسی
            data.AccessLevels = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "عمومی" },
            new SelectListItem { Value = "1", Text = "محدود" }
        };

            return data;
        }

        /// <summary>
        /// آماده‌سازی داده‌های ViewBag برای اعضای تیم
        /// </summary>
        public TeamMemberViewBagData PrepareTeamMemberViewBags(int branchId, int teamId)
        {
            var data = new TeamMemberViewBagData();
            var memberData = PrepareTeamMemberData(branchId, teamId);

            // کاربران موجود
            data.AvailableUsers = memberData.AvailableUsers.Select(u => new SelectListItem
            {
                Value = u.UserId,
                Text = u.FullName + (string.IsNullOrEmpty(u.Position) ? "" : $" ({u.Position})")
            }).ToList();

            // سمت‌های موجود
            data.AvailablePositions = memberData.AvailablePositions.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Title} (سطح قدرت: {p.PowerLevel})"
            }).ToList();

            data.AvailablePositions.Insert(0, new SelectListItem { Value = "", Text = "-- انتخاب سمت (اختیاری) --" });

            // انواع عضویت
            data.MembershipTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "عضو عادی" },
            new SelectListItem { Value = "1", Text = "عضو ویژه" },
            new SelectListItem { Value = "2", Text = "مدیر تیم" }
        };

            return data;
        }

        #endregion

        /// <summary>
        /// دریافت سمت‌های پیشنهادی
        /// </summary>
        public static List<SuggestedPosition> GetSuggestedPositions()
        {
            return new List<SuggestedPosition>
            {
                new SuggestedPosition { Title = "مدیر تیم", PowerLevel = 0, Description = "مسئول کل تیم" },
                new SuggestedPosition { Title = "معاون", PowerLevel = 1, Description = "کمک به مدیر" },
                new SuggestedPosition { Title = "سرپرست", PowerLevel = 2, Description = "نظارت بر بخش خاص" },
                new SuggestedPosition { Title = "کارشناس ارشد", PowerLevel = 3, Description = "کارهای تخصصی پیچیده" },
                new SuggestedPosition { Title = "کارشناس", PowerLevel = 4, Description = "کارهای تخصصی معمولی" },
                new SuggestedPosition { Title = "کارشناس مبتدی", PowerLevel = 5, Description = "یادگیری و کارهای ساده" },
                new SuggestedPosition { Title = "کارمند", PowerLevel = 6, Description = "کارهای عمومی" },
                new SuggestedPosition { Title = "منشی", PowerLevel = 7, Description = "پشتیبانی اداری" }
            };
        }

        /// <summary>
        /// دریافت پایین‌ترین سطح موجود در تیم + 1
        /// </summary>
        public int GetNextLowestPowerLevel(int teamId)
        {
            var maxPowerLevel = _context.TeamPosition_Tbl
                .Where(p => p.TeamId == teamId && p.IsActive)
                .Max(p => (int?)p.PowerLevel);

            return maxPowerLevel.HasValue ? maxPowerLevel.Value + 1 : 0;
        }
    }
    }