using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class UserManagerRepository : IUserManagerRepository
    {

        private readonly AppDbContext _Context;


        public UserManagerRepository(AppDbContext Context)
        {
            _Context = Context;

        }

        /// <summary>
        /// دریافت لیست کاربران بر اساس شناسه شعبه - کاربرانی که به شعبه مشخص اختصاص نداده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه - اگر 0 باشد همه کاربران برگردانده می‌شود</param>
        /// <returns>لیست کاربران فعال</returns>
        public List<UserViewModelFull> GetUserListBybranchId(int branchId)
        {
            List<UserViewModelFull> user;

            if (branchId == 0)
            {
                // اگر branchId برابر 0 باشد، همه کاربران فعال را برگردان
                user = _Context.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new UserViewModelFull
                    {
                        Id = u.Id,
                        FullNamesString = u.FirstName + " " + u.LastName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsActive = u.IsActive,
                        UserName = u.UserName
                    })
                    .ToList();
            }
            else
            {
                // کاربرانی که به شعبه مشخص شده اختصاص نداده شده‌اند
                var assignedUserIds = _Context.BranchUser_Tbl
                    .Where(bu => bu.BranchId == branchId && bu.IsActive)
                    .Select(bu => bu.UserId)
                    .ToList();

                user = _Context.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser && !assignedUserIds.Contains(u.Id))
                    .Select(u => new UserViewModelFull
                    {
                        Id = u.Id,
                        FullNamesString = u.FirstName + " " + u.LastName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsActive = u.IsActive,
                        UserName = u.UserName
                    })
                    .ToList();
            }

            return user;
        }

        /// <summary>
        /// دریافت لیست کاربرانی که به شعبه مشخص اختصاص داده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران اختصاص داده شده به شعبه</returns>
        public List<UserViewModelFull> GetAssignedUsersByBranchId(int branchId, bool includeInactive = false)
        {
            var query = from bu in _Context.BranchUser_Tbl
                        join u in _Context.Users on bu.UserId equals u.Id
                        where bu.BranchId == branchId && u.IsActive && !u.IsRemoveUser
                        select new { BranchUser = bu, User = u };

            if (!includeInactive)
            {
                query = query.Where(x => x.BranchUser.IsActive);
            }

            var result = query.Select(x => new UserViewModelFull
            {
                Id = x.User.Id,
                FullNamesString = x.User.FirstName + " " + x.User.LastName,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                IsActive = x.User.IsActive,
                UserName = x.User.UserName
            }).ToList();

            return result;
        }

        /// <summary>
        /// ای دی کاربر بده و اطلاعات کامل بگیر
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public UserViewModelFull GetUserInfoData(string UserId)
        {
            UserViewModelFull? User = (from us in _Context.Users
                                       select new UserViewModelFull
                                       {
                                           Id = us.Id,
                                           CompanyName = us.CompanyName,

                                           Address = us.Address,
                                           City = us.City,
                                           MelliCode = us.MelliCode,
                                           FirstName = us.FirstName,
                                           Email = us.Email,
                                           IsActive = us.IsActive,
                                           FullNamesString = us.FirstName + " " + us.LastName,
                                           PhoneNumber = us.PhoneNumber,
                                           LastName = us.LastName,
                                           UserName = us.UserName,
                                           Gender = us.Gender,
                                           Province = us.Province,
                                           RegisterDate = us.RegisterDate

                                       }).FirstOrDefault();
            return User;





        }
    }
}
