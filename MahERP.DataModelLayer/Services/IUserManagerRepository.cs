using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    public interface IUserManagerRepository
    {
        public List<UserViewModelFull> GetUserListBybranchId(int branchId);

    }
}
