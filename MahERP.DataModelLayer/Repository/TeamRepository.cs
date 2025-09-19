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
            var teams = GetTeamHierarchy(branchId);
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

        public List<TeamViewModel> GetTeamHierarchy(int branchId)
        {
            var teams = GetTeamsByBranchId(branchId, false);
            
            // ?????? ??? ?? ??? ?? ????? ?????
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
            // ?????? ??????? ????? ?? ????
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
    }
}