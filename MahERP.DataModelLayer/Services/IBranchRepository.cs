using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface IBranchRepository
    {
        /// <summary>
        /// دریافت لیست شعبه‌هایی که کاربر مشخص در آن‌ها تعریف شده است
        /// </summary>
        /// <param name="UserLoginingid">شناسه کاربر لاگین شده</param>
        /// <returns>لیست شعبه‌هایی که کاربر مجوز اتصال دارد</returns>
        public List<BranchViewModel> GetBrnachListByUserId(string UserLoginingid);
        
        /// <summary>
        /// بررسی یکتا بودن نام شعبه
        /// </summary>
        /// <param name="name">نام شعبه</param>
        /// <param name="excludeId">شناسه شعبه برای حذف از بررسی</param>
        /// <returns>true اگر نام یکتا باشد</returns>
        public bool IsBranchNameUnique(string name, int? excludeId = null);
        
        /// <summary>
        /// دریافت لیست کاربران یک شعبه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران شعبه</returns>
        public List<BranchUser> GetBranchUsers(int branchId, bool includeInactive = false);
        
        /// <summary>
        /// دریافت اطلاعات کاربر شعبه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه کاربر شعبه</param>
        /// <returns>اطلاعات کاربر شعبه</returns>
        public BranchUser GetBranchUserById(int id);

        /// <summary>
        /// بررسی اینکه آیا کاربر مشخص قبلاً به شعبه اختصاص داده شده است
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>true اگر کاربر قبلاً اختصاص داده شده باشد</returns>
        public bool IsUserAssignedToBranch(string userId, int branchId);

        /// <summary>
        /// دریافت تعداد کاربران فعال یک شعبه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>تعداد کاربران فعال</returns>
        public int GetActiveUsersCountByBranch(int branchId);
    }
}