using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface ITeamRepository
    {
        // Team CRUD operations
        List<TeamViewModel> GetTeamsByBranchId(int branchId, bool includeInactive = false);
        TeamViewModel GetTeamById(int teamId, bool includePositions = false, bool includeMembers = false);
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
        List<UserSelectListItem> GetAvailableUsersForBranch(int branchId);

        // Manager assignment
        bool AssignManager(int teamId, string managerUserId, string assignedByUserId);
        bool RemoveManager(int teamId);

        // Utility methods
        bool IsUserTeamManager(string userId, int teamId);
        List<Team> GetTeamsByManagerId(string managerId);
        bool HasChildTeams(int teamId);
        bool CanDeleteTeam(int teamId);

        #region Team Position Methods

        /// <summary>
        /// دریافت سمت‌های یک تیم
        /// </summary>
        List<TeamPosition> GetTeamPositions(int teamId, bool includeInactive = false);

        /// <summary>
        /// دریافت سمت با جزئیات کامل
        /// </summary>
        TeamPosition GetTeamPositionById(int positionId);

        /// <summary>
        /// ایجاد سمت جدید
        /// </summary>
        int CreateTeamPosition(TeamPosition position);

        /// <summary>
        /// بروزرسانی سمت
        /// </summary>
        bool UpdateTeamPosition(TeamPosition position);

        /// <summary>
        /// حذف سمت
        /// </summary>
        bool DeleteTeamPosition(int positionId);

        /// <summary>
        /// بررسی امکان حذف سمت
        /// </summary>
        bool CanDeletePosition(int positionId);

        /// <summary>
        /// دریافت سلسله مراتب تیم‌ها برای چارت سازمانی
        /// </summary>
        List<TeamViewModel> GetTeamHierarchy(int branchId);

        /// <summary>
        /// دریافت درخت سمت‌ها با اعضا برای یک تیم خاص
        /// </summary>
        TeamHierarchyViewModel GetTeamPositionHierarchy(int teamId);

        /// <summary>
        /// بررسی تکراری نبودن عنوان سمت در تیم
        /// </summary>
        bool IsPositionTitleUnique(int teamId, string title, int? excludePositionId = null);

        /// <summary>
        /// بررسی تکراری نبودن سمت پیش‌فرض در تیم
        /// فقط یک سمت در هر تیم می‌تواند پیش‌فرض باشد
        /// </summary>
        bool IsDefaultPositionUnique(int teamId, int? excludePositionId = null);

        /// <summary>
        /// ایجاد یا دریافت سمت "مدیریت تیم" برای یک تیم
        /// </summary>
        TeamPosition GetOrCreateManagementPosition(int teamId, string creatorUserId);

        /// <summary>
        /// دریافت سمت مدیریت تیم
        /// </summary>
        TeamPosition GetManagementPosition(int teamId);

        /// <summary>
        /// اضافه کردن مدیر تیم به سمت مدیریت
        /// </summary>
        bool AddManagerToManagementPosition(int teamId, string managerUserId, string assignedByUserId);

        /// <summary>
        /// حذف مدیر از سمت مدیریت (بدون حذف سمت)
        /// </summary>
        bool RemoveManagerFromManagementPosition(int teamId);

        /// <summary>
        /// دریافت سمت‌های پیشنهادی با سطوح قدرت استاندارد
        /// </summary>
        static List<SuggestedPosition> GetSuggestedPositions()
        {
            return new List<SuggestedPosition>
            {
                new() { Title = "مدیر تیم", PowerLevel = 0, Description = "مسئول کل تیم و تصمیم‌گیری‌های راهبردی" },
                new() { Title = "معاون", PowerLevel = 1, Description = "کمک به مدیر و جانشینی در غیاب وی" },
                new() { Title = "سرپرست", PowerLevel = 2, Description = "نظارت بر بخش خاص از کارهای تیم" },
                new() { Title = "کارشناس ارشد", PowerLevel = 3, Description = "انجام کارهای تخصصی پیچیده" },
                new() { Title = "کارشناس", PowerLevel = 4, Description = "انجام کارهای تخصصی معمولی" },
                new() { Title = "کارشناس مبتدی", PowerLevel = 5, Description = "یادگیری و انجام کارهای ساده" },
                new() { Title = "کارمند", PowerLevel = 6, Description = "انجام کارهای عمومی و پشتیبانی" },
                new() { Title = "منشی", PowerLevel = 7, Description = "پشتیبانی اداری و ارتباطات" }
            };
        }

        /// <summary>
        /// دریافت پایین‌ترین سطح موجود در تیم + 1
        /// </summary>
        int GetNextLowestPowerLevel(int teamId);

        #endregion

        /// <summary>
        /// خارج کردن عضو از سمت (بدون حذف از تیم)
        /// </summary>
        bool RemoveMemberFromPosition(int memberId);

        /// <summary>
        /// تخصیص عضو به سمت
        /// </summary>
        bool AssignMemberToPosition(int memberId, int positionId);

        #region Advanced Team Member Management
        TeamMemberManagementData PrepareTeamMemberData(int branchId, int teamId);
        CreateMemberResult CreateTeamMemberWithValidation(TeamMemberViewModel model, string addedByUserId);
        UpdateMemberResult UpdateTeamMemberWithValidation(TeamMemberViewModel model);
        DeleteMemberResult DeleteTeamMemberWithValidation(int memberId);
        ToggleMemberStatusResult ToggleMemberStatus(int memberId);
        List<TeamMemberViewModel> SearchTeamMembers(int teamId, TeamMemberSearchFilter filter);
        #endregion

        #region ViewBag Preparation Methods
        /// <summary>
        /// آماده‌سازی داده‌های ViewBag برای تیم
        /// </summary>
        TeamViewBagData PrepareTeamViewBags(int branchId);

        /// <summary>
        /// آماده‌سازی داده‌های ViewBag برای اعضای تیم
        /// </summary>
        TeamMemberViewBagData PrepareTeamMemberViewBags(int branchId, int teamId);
        #endregion
    }
}