using MahERP.DataModelLayer.Entities.AcControl;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface IBranchRepository
    {
        List<Branch> GetBranches(bool includeInactive = false);
        Branch GetBranchById(int id, bool includeUsers = false, bool includeStakeholders = false, bool includeTasks = false, bool includeChildBranches = false);
        List<Branch> GetMainBranches(bool includeInactive = false);
        List<Branch> GetChildBranches(int parentId, bool includeInactive = false);
        List<BranchUser> GetBranchUsers(int branchId, bool includeInactive = false);
        BranchUser GetBranchUserById(int id);
        bool IsBranchNameUnique(string name, int? excludeId = null);
        List<Branch> SearchBranches(string searchTerm);
        List<Stakeholder> GetBranchStakeholders(int branchId, bool includeInactive = false);
    }
}