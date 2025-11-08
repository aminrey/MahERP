using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.ViewModels.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Notifications
{
    /// <summary>
    /// Repository برای مدیریت تنظیمات اعلان‌ها
    /// </summary>
    public interface INotificationSettingsRepository
    {
        // ==================== Module & Type Config ====================
        
        /// <summary>
        /// دریافت لیست تمام ماژول‌ها
        /// </summary>
        Task<List<NotificationModuleConfig>> GetAllModulesAsync();

        /// <summary>
        /// دریافت یک ماژول با انواع اعلان‌هایش
        /// </summary>
        Task<NotificationModuleConfig> GetModuleWithTypesAsync(int moduleId);

        /// <summary>
        /// دریافت انواع اعلان یک ماژول
        /// </summary>
        Task<List<NotificationTypeConfig>> GetModuleNotificationTypesAsync(int moduleId);

        /// <summary>
        /// دریافت یک نوع اعلان
        /// </summary>
        Task<NotificationTypeConfig> GetNotificationTypeByIdAsync(int typeId);

        /// <summary>
        /// دریافت نوع اعلان با کد
        /// </summary>
        Task<NotificationTypeConfig> GetNotificationTypeByCodeAsync(string typeCode);

        /// <summary>
        /// فعال/غیرفعال کردن نوع اعلان
        /// </summary>
        Task<bool> ToggleNotificationTypeAsync(int typeId, bool isActive);

        /// <summary>
        /// بروزرسانی تنظیمات نوع اعلان
        /// </summary>
        Task<bool> UpdateNotificationTypeConfigAsync(NotificationTypeConfig config);

        // ==================== User Preferences ====================
        
        /// <summary>
        /// دریافت تنظیمات شخصی کاربر
        /// </summary>
        Task<List<UserNotificationPreference>> GetUserPreferencesAsync(string userId);

        /// <summary>
        /// دریافت تنظیمات کاربر برای یک نوع اعلان خاص
        /// </summary>
        Task<UserNotificationPreference> GetUserPreferenceAsync(string userId, int typeId);

        /// <summary>
        /// ذخیره تنظیمات شخصی کاربر
        /// </summary>
        Task<bool> SaveUserPreferenceAsync(UserNotificationPreference preference);

        /// <summary>
        /// بروزرسانی تنظیمات شخصی کاربر
        /// </summary>
        Task<bool> UpdateUserPreferenceAsync(UserNotificationPreference preference);

        /// <summary>
        /// ایجاد تنظیمات پیش‌فرض برای کاربر جدید
        /// </summary>
        Task<bool> CreateDefaultUserPreferencesAsync(string userId);

        // ==================== Blacklist ====================
        
        /// <summary>
        /// دریافت لیست سیاه
        /// </summary>
        Task<List<NotificationBlacklist>> GetBlacklistAsync();

        /// <summary>
        /// افزودن کاربر به لیست سیاه
        /// </summary>
        Task<bool> AddToBlacklistAsync(NotificationBlacklist blacklist);

        /// <summary>
        /// حذف از لیست سیاه
        /// </summary>
        Task<bool> RemoveFromBlacklistAsync(int blacklistId);

        /// <summary>
        /// بررسی اینکه کاربر در لیست سیاه است یا نه
        /// </summary>
        Task<bool> IsUserBlacklistedAsync(string userId, int? typeId = null);

        // ==================== Validation ====================
        
        /// <summary>
        /// بررسی اینکه آیا کاربر می‌تواند اعلان دریافت کند
        /// </summary>
        Task<bool> CanUserReceiveNotificationAsync(string userId, int typeId, byte channel);

        /// <summary>
        /// دریافت ViewModel برای صفحه تنظیمات
        /// </summary>
        Task<NotificationSettingsViewModel> GetSettingsViewModelAsync();

        /// <summary>
        /// دریافت ViewModel برای تنظیمات شخصی کاربر
        /// </summary>
        Task<UserNotificationSettingsViewModel> GetUserSettingsViewModelAsync(string userId);

        /// <summary>
        /// دریافت ViewModel برای ویرایش نوع اعلان
        /// </summary>
        Task<NotificationTypeEditViewModel> GetEditTypeViewModelAsync(int typeId);

        /// <summary>
        /// دریافت تمام انواع اعلان (برای Dropdown ها)
        /// </summary>
        Task<List<NotificationTypeConfig>> GetAllNotificationTypesAsync();

        #region Recipient Management (سیستم جدید)

        /// <summary>
        /// دریافت ViewModel برای مدیریت دریافت‌کنندگان یک نوع اعلان
        /// </summary>
        Task<ManageRecipientsViewModel> GetManageRecipientsViewModelAsync(int typeId);

        /// <summary>
        /// بروزرسانی حالت ارسال (SendMode)
        /// </summary>
        Task<bool> UpdateSendModeAsync(int typeId, byte sendMode);

        /// <summary>
        /// افزودن کاربر به لیست دریافت‌کنندگان
        /// </summary>
        Task<bool> AddRecipientAsync(int typeId, string userId, string reason, string createdByUserId);

        /// <summary>
        /// حذف کاربر از لیست دریافت‌کنندگان
        /// </summary>
        Task<bool> RemoveRecipientAsync(int recipientId);

        /// <summary>
        /// بررسی اینکه آیا کاربر باید این اعلان را دریافت کند
        /// </summary>
        Task<bool> ShouldReceiveNotificationAsync(int typeId, string userId);

        #endregion

    }
}