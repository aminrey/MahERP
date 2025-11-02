using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.ModuleAccessViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس مدیریت دسترسی به ماژول‌های سیستم
    /// </summary>
    public interface IModuleAccessService
    {
        #region ✅ بررسی دسترسی - Access Checking

        /// <summary>
        /// بررسی دسترسی کاربر به یک ماژول خاص
        /// اولویت: User > Team > Branch > Default
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="moduleType">نوع ماژول</param>
        /// <returns>نتیجه بررسی دسترسی</returns>
        Task<ModuleAccessResult> CheckUserModuleAccessAsync(string userId, ModuleType moduleType);

        /// <summary>
        /// دریافت لیست ماژول‌های فعال برای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست ماژول‌های قابل دسترسی</returns>
        Task<List<ModuleType>> GetUserEnabledModulesAsync(string userId);

        /// <summary>
        /// دریافت ماژول پیش‌فرض برای ریدایرکت بعد از لاگین
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>ماژول پیش‌فرض یا آخرین استفاده شده</returns>
        Task<ModuleType?> GetDefaultModuleForLoginAsync(string userId);

        #endregion

        #region ✅ مدیریت دسترسی کاربران - User Access Management

        /// <summary>
        /// اعطای دسترسی مستقیم به کاربر
        /// </summary>
        Task<bool> GrantModuleAccessToUserAsync(
            string userId, 
            ModuleType moduleType, 
            string grantedByUserId, 
            string notes = null);

        /// <summary>
        /// لغو دسترسی مستقیم کاربر
        /// </summary>
        Task<bool> RevokeModuleAccessFromUserAsync(string userId, ModuleType moduleType);

        /// <summary>
        /// دریافت لیست دسترسی‌های مستقیم کاربر
        /// </summary>
        Task<List<UserModuleAccessViewModel>> GetUserDirectAccessesAsync(string userId);

        #endregion

        #region ✅ مدیریت دسترسی تیم‌ها - Team Access Management

        /// <summary>
        /// اعطای دسترسی به تیم
        /// </summary>
        Task<bool> GrantModuleAccessToTeamAsync(
            int teamId, 
            ModuleType moduleType, 
            string grantedByUserId, 
            string notes = null);

        /// <summary>
        /// لغو دسترسی تیم
        /// </summary>
        Task<bool> RevokeModuleAccessFromTeamAsync(int teamId, ModuleType moduleType);

        /// <summary>
        /// دریافت لیست دسترسی‌های تیم
        /// </summary>
        Task<List<TeamModuleAccessViewModel>> GetTeamAccessesAsync(int teamId);

        #endregion

        #region ✅ مدیریت دسترسی شعب - Branch Access Management

        /// <summary>
        /// اعطای دسترسی به شعبه
        /// </summary>
        Task<bool> GrantModuleAccessToBranchAsync(
            int branchId, 
            ModuleType moduleType, 
            string grantedByUserId, 
            string notes = null);

        /// <summary>
        /// لغو دسترسی شعبه
        /// </summary>
        Task<bool> RevokeModuleAccessFromBranchAsync(int branchId, ModuleType moduleType);

        /// <summary>
        /// دریافت لیست دسترسی‌های شعبه
        /// </summary>
        Task<List<BranchModuleAccessViewModel>> GetBranchAccessesAsync(int branchId);

        #endregion

        #region ✅ مدیریت تنظیمات کاربر - User Preferences

        /// <summary>
        /// ذخیره آخرین ماژول استفاده شده
        /// </summary>
        Task SaveLastUsedModuleAsync(string userId, ModuleType moduleType);

        /// <summary>
        /// تنظیم ماژول پیش‌فرض کاربر
        /// </summary>
        Task SetUserDefaultModuleAsync(string userId, ModuleType moduleType);

        /// <summary>
        /// دریافت تنظیمات ماژول کاربر
        /// </summary>
        Task<UserModulePreferenceViewModel> GetUserModulePreferenceAsync(string userId);

        #endregion

        #region ✅ گزارشات - Reports

        /// <summary>
        /// دریافت گزارش کامل دسترسی‌ها برای یک ماژول
        /// </summary>
        Task<ModuleAccessReportViewModel> GetModuleAccessReportAsync(ModuleType moduleType);

        /// <summary>
        /// دریافت گزارش کامل تمام ماژول‌ها
        /// </summary>
        Task<AllModulesReportViewModel> GetAllModulesAccessReportAsync();

        /// <summary>
        /// دریافت لیست کاربرانی که به یک ماژول دسترسی دارند
        /// </summary>
        Task<List<UserModuleAccessViewModel>> GetUsersWithModuleAccessAsync(ModuleType moduleType);

        #endregion

        #region ✅ Cache Management

        /// <summary>
        /// پاکسازی کش دسترسی کاربر
        /// </summary>
        void ClearUserAccessCache(string userId);

        /// <summary>
        /// پاکسازی کش تنظیمات کاربر
        /// </summary>
        void ClearUserPreferenceCache(string userId);

        #endregion
    }

    #region ✅ Result & ViewModel Classes

    /// <summary>
    /// نتیجه بررسی دسترسی به ماژول
    /// </summary>
    public class ModuleAccessResult
    {
        /// <summary>
        /// آیا کاربر دسترسی دارد؟
        /// </summary>
        public bool HasAccess { get; set; }

        /// <summary>
        /// نوع ماژول
        /// </summary>
        public ModuleType ModuleType { get; set; }

        /// <summary>
        /// منبع دسترسی: "Direct", "Team", "Branch", "None"
        /// </summary>
        public string AccessSource { get; set; }

        /// <summary>
        /// شناسه منبع (ID از UserModulePermission, TeamModulePermission, یا BranchModulePermission)
        /// </summary>
        public int? SourceId { get; set; }

        /// <summary>
        /// پیام توضیحی
        /// </summary>
        public string Message { get; set; }
    }

    #endregion
}