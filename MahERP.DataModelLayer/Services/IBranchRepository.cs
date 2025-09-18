using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using System;
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

        /// <summary>
        /// دریافت جزئیات کامل یک شعبه شامل کاربران، طرف حساب‌ها و شعبه‌های زیرمجموعه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="userId">شناسه کاربر جهت بررسی دسترسی</param>
        /// <param name="includeInactiveUsers">شامل کاربران غیرفعال</param>
        /// <param name="includeInactiveStakeholders">شامل طرف حساب‌های غیرفعال</param>
        /// <param name="includeInactiveChildBranches">شامل شعبه‌های زیرمجموعه غیرفعال</param>
        /// <returns>جزئیات کامل شعبه</returns>
        public BranchDetailsViewModel GetBranchDetailsById(int branchId, string userId = null, 
            bool includeInactiveUsers = false, bool includeInactiveStakeholders = false, 
            bool includeInactiveChildBranches = false);

        /// <summary>
        /// دریافت اطلاعات کامل برای فرم افزودن کاربر به شعبه
        /// شامل اطلاعات شعبه و لیست کاربران قابل انتساب
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>ViewModel کامل برای فرم افزودن کاربر</returns>
        public BranchUserViewModel GetAddUserToBranchViewModel(int branchId);
        
        /// <summary>
        /// دریافت لیست کاربران شعبه با جزئیات کامل
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران شعبه</returns>
        public List<BranchUserViewModel> GetBranchUsersByBranchId(int branchId, bool includeInactive = false);

        #region متدهای مدیریت دسته‌بندی تسک شعبه با طرف حساب

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه و طرف حساب مشخص
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <param name="activeOnly">فقط موارد فعال</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه و طرف حساب</returns>
        public List<BranchTaskCategoryStakeholder> GetTaskCategoriesByBranchAndStakeholder(int branchId, int? stakeholderId = null, bool activeOnly = true);

        /// <summary>
        /// دریافت اطلاعات کامل انتصاب دسته‌بندی به شعبه و طرف حساب
        /// </summary>
        /// <param name="id">شناسه انتصاب</param>
        /// <returns>اطلاعات انتصاب</returns>
        public BranchTaskCategoryStakeholder GetBranchTaskCategoryStakeholderById(int id);

        /// <summary>
        /// دریافت ViewModel برای افزودن دسته‌بندی به شعبه با طرف حساب
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>ViewModel شامل اطلاعات لازم</returns>
        public BranchTaskCategoryStakeholderViewModel GetAddTaskCategoryToBranchStakeholderViewModel(int branchId, int? stakeholderId = null);

        /// <summary>
        /// بررسی اینکه آیا دسته‌بندی قبلاً به شعبه و طرف حساب اضافه شده یا نه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="taskCategoryId">شناسه دسته‌بندی</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>true اگر قبلاً اضافه شده باشد</returns>
        public bool IsTaskCategoryAssignedToBranchStakeholder(int branchId, int taskCategoryId, int stakeholderId);

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک برای شعبه مشخص (برای استفاده در فرم‌های دیگر)
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب (اختیاری)</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه</returns>
        List<TaskCategoryItemViewModel> GetTaskCategoriesForBranchStakeholder(int branchId, int? stakeholderId = null);

        /// <summary>
        /// دریافت دسته‌بندی‌های تسک بر اساس شعبه و طرف حساب انتخاب شده (برای cascade)
        /// این متد زمانی فراخوانی می‌شود که طرف حساب تغییر کند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>لیست دسته‌بندی‌های تسک قابل انتخاب</returns>
        List<TaskCategoryItemViewModel> GetTaskCategoriesForStakeholderChange(int branchId, int stakeholderId);

        #endregion

        
    }
}