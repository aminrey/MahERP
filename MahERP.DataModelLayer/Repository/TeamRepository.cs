using AutoMapper;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
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

        public TeamViewModel GetTeamById(int teamId)
        {
            var team = _context.Team_Tbl
                .Include(t => t.Manager)
                .Include(t => t.ParentTeam)
                .Include(t => t.Branch)
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Include(t => t.ChildTeams)
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .ThenInclude(tm => tm.User)
                .FirstOrDefault(t => t.Id == teamId);

            return team != null ? _mapper.Map<TeamViewModel>(team) : null;
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

                team.ManagerUserId = managerUserId;
                team.LastUpdaterUserId = assignedByUserId;
                team.LastUpdateDate = DateTime.Now;

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
            Title = p.Title,
            Description = p.Description,
            PowerLevel = p.PowerLevel,
            CurrentMembers = p.TeamMembers.Count(tm => tm.IsActive),
            MaxMembers = p.MaxMembers,
            CanAddMember = !p.MaxMembers.HasValue || p.TeamMembers.Count(tm => tm.IsActive) < p.MaxMembers.Value,
            PowerLevelText = p.PowerLevel switch
            {
                0 => "مدیر تیم",
                1 => "معاون/سرپرست",
                2 => "کارشناس ارشد",
                3 => "کارشناس",
                _ => $"سطح {p.PowerLevel}"
            },
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

public bool IsPowerLevelUnique(int teamId, int powerLevel, int? excludePositionId = null)
{
    var query = _context.TeamPosition_Tbl
        .Where(p => p.TeamId == teamId && p.PowerLevel == powerLevel && p.IsActive);

    if (excludePositionId.HasValue)
        query = query.Where(p => p.Id != excludePositionId.Value);

    return !query.Any();
}

#endregion
    }
}