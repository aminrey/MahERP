using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface IBranchRepository
    {
        public BranchViewModel GetBrnachListByUserId(string UserLoginingid);
    }
}