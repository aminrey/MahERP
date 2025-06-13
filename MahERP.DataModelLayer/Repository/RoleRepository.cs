using MahERP.DataModelLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MahERP.DataModelLayer.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext db)
        {
            _context = db;
        }

        public string GetRoleId(string userId)
        {
            //دارای یک نکته مهم
            var getRoleId = _context.UserRoles.Where(ur => ur.UserId == userId ).ToList();

            string getRollString = "";
            for (int i = 0; i < getRoleId.Count; i++)
            {
                getRollString += getRoleId[i].RoleId.ToString() + ",";
            }

            return getRollString;
        }
        public string GetRolePatternId(int RolePatternID)
        {
            //دارای یک نکته مهم
            var getRoleId = _context.RolePatternDetails_Tbl.Where(rp => rp.RolePatternID == RolePatternID).ToList();
            string getRollString = "";
            for (int i = 0; i < getRoleId.Count; i++)
            {
                getRollString += getRoleId[i].RoleID.ToString() + ",";
            }

            return getRollString;
        }
    }

}
