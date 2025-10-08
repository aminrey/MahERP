using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Repository
{
    public interface IStakeholderRepository
    {
        // ========== Stakeholder Methods ==========
        List<Stakeholder> GetStakeholders(bool includeDeleted = false, int? stakeholderType = null, byte? personType = null);
        Stakeholder GetStakeholderById(int id, bool includeCRM = false, bool includeContacts = false, bool includeContracts = false, bool includeTasks = false, bool includeOrganizations = false);
        List<Stakeholder> SearchStakeholders(string searchTerm, int? stakeholderType = null, byte? personType = null);
        bool IsNationalCodeUnique(string nationalCode, int? excludeId = null);
        bool IsEmailUnique(string email, int? excludeId = null);
        bool IsRegistrationNumberUnique(string registrationNumber, int? excludeId = null);
        bool IsEconomicCodeUnique(string economicCode, int? excludeId = null);
        List<StakeholderViewModel> GetStakeholdersByBranchId(int branchId, byte? personType = null);

        // ========== StakeholderContact Methods ==========
        List<StakeholderContact> GetStakeholderContacts(int stakeholderId, bool includeInactive = false);
        StakeholderContact GetStakeholderContactById(int id);
        List<StakeholderContact> GetAvailableContactsForOrganization(int stakeholderId, int? excludeOrganizationId = null);

        // ========== StakeholderOrganization Methods ==========
        List<StakeholderOrganization> GetStakeholderOrganizations(int stakeholderId, bool includeInactive = false);
        StakeholderOrganization GetStakeholderOrganizationById(int id, bool includePositions = false, bool includeMembers = false);
        List<StakeholderOrganization> GetRootOrganizations(int stakeholderId);
        List<StakeholderOrganization> GetChildOrganizations(int parentOrganizationId);
        StakeholderOrganizationViewModel GetOrganizationChartData(int stakeholderId);

        // ========== StakeholderOrganizationPosition Methods ==========
        List<StakeholderOrganizationPosition> GetOrganizationPositions(int organizationId, bool includeInactive = false);
        StakeholderOrganizationPosition GetOrganizationPositionById(int id);
        StakeholderOrganizationPosition GetDefaultPosition(int organizationId);

        // ========== StakeholderOrganizationMember Methods ==========
        List<StakeholderOrganizationMember> GetOrganizationMembers(int organizationId, bool includeInactive = false);
        StakeholderOrganizationMember GetOrganizationMemberById(int id);
        List<StakeholderOrganizationMember> GetContactMemberships(int contactId);
        bool IsContactAlreadyMember(int organizationId, int contactId);

        /// <summary>
        /// ایجاد واحد سازمانی جدید
        /// </summary>
        /// <param name="organization">واحد سازمانی برای ثبت</param>
        /// <returns>شناسه واحد ایجاد شده</returns>
        int CreateOrganization(StakeholderOrganization organization);

        /// <summary>
        /// به‌روزرسانی واحد سازمانی
        /// </summary>
        /// <param name="organization">واحد سازمانی برای به‌روزرسانی</param>
        void UpdateOrganization(StakeholderOrganization organization);
    }
}