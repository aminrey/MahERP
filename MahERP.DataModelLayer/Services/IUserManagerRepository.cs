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
        List<UserViewModelFull> GetUserListBybranchId(int branchId);

        /// <summary>
        /// دریافت لیست کاربرانی که به شعبه مشخص اختصاص داده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران اختصاص داده شده به شعبه</returns>
        List<UserViewModelFull> GetAssignedUsersByBranchId(int branchId, bool includeInactive = false);

        /// <summary>
        /// ای دی کاربر بده و بهت مشخصات کامل کاربر و میده
        /// </summary>
        /// <param name="UserId">شناسه کاربر</param>
        /// <returns>اطلاعات کامل کاربر</returns>
        UserViewModelFull GetUserInfoData(string UserId);

        /// <summary>
        /// بایگانی کاربر - کاربر از تمام شعبه‌ها و تیم‌ها خارج می‌شود و بایگانی می‌شود
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>true در صورت موفقیت</returns>
        Task<bool> ArchiveUserAsync(string userId);

        /// <summary>
        /// حذف کامل کاربر - کاربر از همه جا حذف می‌شود و اطلاعاتش در تسک‌ها ثبت می‌شود
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>true در صورت موفقیت</returns>
        Task<bool> CompletelyDeleteUserAsync(string userId);

        /// <summary>
        /// بررسی وجود یوزرنیم (حتی در کاربران حذف شده کامل)
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="excludeUserId">شناسه کاربر جهت استثنا (برای ویرایش)</param>
        /// <returns>true اگر یوزرنیم موجود باشد</returns>
        Task<bool> IsUsernameExistsAsync(string username, string excludeUserId = null);
    }
}
