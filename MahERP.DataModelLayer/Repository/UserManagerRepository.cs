using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class UserManagerRepository :IUserManagerRepository
    {

        private readonly AppDbContext _Context;


        public UserManagerRepository(AppDbContext Context)
        {
            _Context = Context;

        }

        public List<UserViewModelFull> GetUserListBybranchId(int branchId)
        {

            return null;

        }


    }
}
