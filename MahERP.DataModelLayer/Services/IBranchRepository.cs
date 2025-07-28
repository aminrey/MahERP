using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface IBranchRepository
    {
        public List<BranchViewModel> GetBrnachListByUserId(string UserLoginingid);
        public bool IsBranchNameUnique(string name, int? excludeId = null);
        public List<BranchUser> GetBranchUsers(int branchId, bool includeInactive = false);
        public BranchUser GetBranchUserById(int id);



    }
}