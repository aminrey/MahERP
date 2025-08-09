using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Repository
{
    public interface IStakeholderRepository
    {
        List<Stakeholder> GetStakeholders(bool includeDeleted = false, int? stakeholderType = null);
        Stakeholder GetStakeholderById(int id, bool includeCRM = false, bool includeContacts = false, bool includeContracts = false, bool includeTasks = false);
        List<Stakeholder> SearchStakeholders(string searchTerm, int? stakeholderType = null);
        bool IsNationalCodeUnique(string nationalCode, int? excludeId = null);
        bool IsEmailUnique(string email, int? excludeId = null);
        StakeholderCRM GetStakeholderCRMById(int stakeholderId);
        
        List<StakeholderContact> GetStakeholderContacts(int stakeholderId, bool includeInactive = false);
        StakeholderContact GetStakeholderContactById(int id);
        List<Stakeholder> SearchAdvanced(StakeholderSearchViewModel model);
        public List<StakeholderViewModel> GetStakeholdersByBranchId(int BranchId);

    }
}