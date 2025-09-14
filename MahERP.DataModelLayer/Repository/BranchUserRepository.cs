using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class BranchUserRepository : IBranchUserRepository
    {
        private readonly AppDbContext _context;

        public BranchUserRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ?????? ???? ??????? ???? ?? ???? ????? ????
        /// </summary>
        public List<BranchUser> GetBranchUsersByBranchId(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .Include(bu => bu.AssignedByUser)
                .Where(bu => bu.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(bu => bu.IsActive && bu.User.IsActive && !bu.User.IsRemoveUser);

            return query.OrderBy(bu => bu.User.LastName)
                       .ThenBy(bu => bu.User.FirstName)
                       .ToList();
        }

        /// <summary>
        /// ?????? ????? ???? ?? ???? ?????
        /// </summary>
        public BranchUser GetBranchUserById(int id)
        {
            return _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .Include(bu => bu.AssignedByUser)
                .FirstOrDefault(bu => bu.Id == id);
        }

        /// <summary>
        /// ?????? ????? ???? ?? ???? ????? ????? ? ????
        /// </summary>
        public BranchUser GetBranchUserByUserAndBranch(string userId, int branchId)
        {
            return _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .Include(bu => bu.AssignedByUser)
                .FirstOrDefault(bu => bu.UserId == userId && bu.BranchId == branchId);
        }

        /// <summary>
        /// ????? ???? ????? ?? ????
        /// </summary>
        public bool UserExistsInBranch(string userId, int branchId)
        {
            return _context.BranchUser_Tbl
                .Any(bu => bu.UserId == userId && bu.BranchId == branchId);
        }

        /// <summary>
        /// ?????? ???? ????????? ?? ????? ?? ????? ??? ???
        /// </summary>
        public List<BranchUserViewModel> GetUserBranches(string userId, bool includeInactive = false)
        {
            var query = from bu in _context.BranchUser_Tbl
                        join b in _context.Branch_Tbl on bu.BranchId equals b.Id
                        where bu.UserId == userId
                        select new { BranchUser = bu, Branch = b };

            if (!includeInactive)
                query = query.Where(x => x.BranchUser.IsActive && x.Branch.IsActive);

            return query.Select(x => new BranchUserViewModel
            {
                Id = x.BranchUser.Id,
                BranchId = x.BranchUser.BranchId,
                UserId = x.BranchUser.UserId,
                Role = x.BranchUser.Role,
                IsActive = x.BranchUser.IsActive,
                AssignDate = x.BranchUser.AssignDate,
                AssignedByUserId = x.BranchUser.AssignedByUserId,
                BranchName = x.Branch.Name
            }).OrderBy(x => x.BranchName).ToList();
        }

        /// <summary>
        /// ?????? ????? ??????? ???? ?? ???? ?? ???? ???
        /// </summary>
        public int GetUsersCountByRole(int branchId, byte? role = null)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Where(bu => bu.BranchId == branchId && 
                            bu.IsActive && 
                            bu.User.IsActive && 
                            !bu.User.IsRemoveUser);

            if (role.HasValue)
                query = query.Where(bu => bu.Role == role.Value);

            return query.Count();
        }

        /// <summary>
        /// ?????? ??????? ????
        /// </summary>
        public List<BranchUser> SearchBranchUsers(int branchId, string searchTerm = null, byte? role = null, bool? isActive = null)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .Include(bu => bu.AssignedByUser)
                .Where(bu => bu.BranchId == branchId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(bu => 
                    bu.User.FirstName.Contains(searchTerm) ||
                    bu.User.LastName.Contains(searchTerm) ||
                    bu.User.Email.Contains(searchTerm) ||
                    bu.User.UserName.Contains(searchTerm));
            }

            if (role.HasValue)
                query = query.Where(bu => bu.Role == role.Value);

            if (isActive.HasValue)
                query = query.Where(bu => bu.IsActive == isActive.Value);

            return query.OrderBy(bu => bu.User.LastName)
                       .ThenBy(bu => bu.User.FirstName)
                       .ToList();
        }

        /// <summary>
        /// ?????? ?????? ????
        /// </summary>
        public List<BranchUser> GetBranchManagers(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .Include(bu => bu.AssignedByUser)
                .Where(bu => bu.BranchId == branchId && bu.Role == 2); // Role 2 = ????

            if (!includeInactive)
                query = query.Where(bu => bu.IsActive && bu.User.IsActive && !bu.User.IsRemoveUser);

            return query.OrderBy(bu => bu.AssignDate).ToList();
        }

        /// <summary>
        /// ????? ????? ??? ????? ???? ???? ???
        /// </summary>
        public bool IsUserBranchManager(string userId, int branchId)
        {
            return _context.BranchUser_Tbl
                .Any(bu => bu.UserId == userId && 
                          bu.BranchId == branchId && 
                          bu.Role == 2 && 
                          bu.IsActive);
        }

        /// <summary>
        /// ????? ????? ???? ????
        /// </summary>
        public int CreateBranchUser(BranchUser branchUser)
        {
            try
            {
                _context.BranchUser_Tbl.Add(branchUser);
                _context.SaveChanges();
                return branchUser.Id;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ??????????? ??????? ????? ????
        /// </summary>
        public bool UpdateBranchUser(BranchUser branchUser)
        {
            try
            {
                _context.BranchUser_Tbl.Update(branchUser);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ??? ????? ?? ????
        /// </summary>
        public bool DeleteBranchUser(int id)
        {
            try
            {
                var branchUser = _context.BranchUser_Tbl.Find(id);
                if (branchUser != null)
                {
                    _context.BranchUser_Tbl.Remove(branchUser);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ????/??????? ???? ????? ????
        /// </summary>
        public bool ToggleBranchUserStatus(int id, bool isActive)
        {
            try
            {
                var branchUser = _context.BranchUser_Tbl.Find(id);
                if (branchUser != null)
                {
                    branchUser.IsActive = isActive;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}