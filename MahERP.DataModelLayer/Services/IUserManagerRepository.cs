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
        /// <summary>
        /// دریافت لیست کاربران بر اساس شناسه شعبه
        /// </summary>
        /// <param name="branchId">شناسه شعبه - اگر 0 باشد همه کاربران برگردانده می‌شود</param>
        /// <returns>لیست کاربران فعال</returns>
        public List<UserViewModelFull> GetUserListBybranchId(int branchId);

        /// <summary>
        /// دریافت لیست کاربرانی که به شعبه مشخص اختصاص داده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران اختصاص داده شده به شعبه</returns>
        public List<UserViewModelFull> GetAssignedUsersByBranchId(int branchId, bool includeInactive = false);
        /// <summary>
        /// ای دی کاربر بده و بهت مشخصات کامل کاربر و میده
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public UserViewModelFull GetUserInfoData(string UserId);

    }
}
