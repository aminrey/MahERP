using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Repository
{
    public interface ITeamRepository
    {
        // Team CRUD operations
        List<TeamViewModel> GetTeamsByBranchId(int branchId, bool includeInactive = false);
        TeamViewModel GetTeamById(int teamId);
        Team GetTeamEntityById(int teamId);
        int CreateTeam(Team team);
        bool UpdateTeam(Team team);
        bool DeleteTeam(int teamId);
        bool ToggleTeamStatus(int teamId);

        // TeamMember CRUD operations
        List<TeamMemberViewModel> GetTeamMembers(int teamId, bool includeInactive = false);
        TeamMemberViewModel GetTeamMemberById(int memberId);
        int AddTeamMember(TeamMember member);
        bool UpdateTeamMember(TeamMember member);
        bool RemoveTeamMember(int memberId);

        // Organizational Chart
        OrganizationalChartViewModel GetOrganizationalChart(int branchId);
        List<TeamViewModel> GetTeamHierarchy(int branchId);
        List<UserSelectListItem> GetAvailableUsersForBranch(int branchId);

        // Manager assignment
        bool AssignManager(int teamId, string managerUserId, string assignedByUserId);
        bool RemoveManager(int teamId);

        // Utility methods
        bool IsUserTeamManager(string userId, int teamId);
        List<Team> GetTeamsByManagerId(string managerId);
        bool HasChildTeams(int teamId);
        bool CanDeleteTeam(int teamId);
    }
}