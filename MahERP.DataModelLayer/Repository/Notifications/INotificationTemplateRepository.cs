using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.ViewModels.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Notifications
{
    /// <summary>
    /// Repository برای مدیریت الگوهای پیام
    /// </summary>
    public interface INotificationTemplateRepository
    {
        // ==================== Template CRUD ====================
        
        /// <summary>
        /// دریافت لیست تمام الگوها
        /// </summary>
        /// <param name="notificationTypeId">فیلتر بر اساس نوع اعلان (اختیاری)</param>
        /// <param name="channelType">فیلتر بر اساس کانال (اختیاری)</param>
        Task<List<NotificationTemplate>> GetAllTemplatesAsync(int? notificationTypeId = null, byte? channelType = null);

        /// <summary>
        /// دریافت یک الگو با شناسه
        /// </summary>
        Task<NotificationTemplate> GetTemplateByIdAsync(int templateId);

        /// <summary>
        /// دریافت الگو با کد
        /// </summary>
        Task<NotificationTemplate> GetTemplateByCodeAsync(string templateCode);

        /// <summary>
        /// ایجاد الگوی جدید
        /// </summary>
        Task<int> CreateTemplateAsync(NotificationTemplate template, string userId);

        /// <summary>
        /// بروزرسانی الگو
        /// </summary>
        Task<bool> UpdateTemplateAsync(NotificationTemplate template, string userId, string changeNote = null);

        /// <summary>
        /// حذف الگو (فقط غیرسیستمی)
        /// </summary>
        Task<bool> DeleteTemplateAsync(int templateId);

        /// <summary>
        /// فعال/غیرفعال کردن الگو
        /// </summary>
        Task<bool> ToggleTemplateStatusAsync(int templateId, bool isActive);

        // ==================== Recipients Management ====================

        /// <summary>
        /// حذف دریافت‌کننده از الگو
        /// </summary>
        Task<bool> RemoveRecipientAsync(int recipientId);

        /// <summary>
        /// بروزرسانی دسته‌ای دریافت‌کنندگان
        /// </summary>
        Task<bool> UpdateRecipientsAsync(int templateId, List<string> userIds, string currentUserId);

        // ==================== Template Variables ====================
        
        /// <summary>
        /// دریافت متغیرهای یک الگو
        /// </summary>
        Task<List<NotificationTemplateVariable>> GetTemplateVariablesAsync(int templateId);

        /// <summary>
        /// دریافت متغیرهای پیش‌فرض سیستم
        /// </summary>
        Task<List<SystemVariableViewModel>> GetSystemVariablesAsync();

        /// <summary>
        /// ⭐⭐⭐ دریافت متغیرهای فیلتر شده بر اساس نوع اعلان
        /// </summary>
        Task<List<SystemVariableViewModel>> GetVariablesForEventTypeAsync(byte eventType);

        /// <summary>
        /// افزودن متغیر به الگو
        /// </summary>
        Task<bool> AddVariableToTemplateAsync(NotificationTemplateVariable variable);

        /// <summary>
        /// حذف متغیر از الگو
        /// </summary>
        Task<bool> RemoveVariableFromTemplateAsync(int variableId);

        // ==================== Template History ====================
        
        /// <summary>
        /// دریافت تاریخچه تغییرات الگو
        /// </summary>
        Task<List<NotificationTemplateHistory>> GetTemplateHistoryAsync(int templateId);

        /// <summary>
        /// بازگرداندن الگو به نسخه قبلی
        /// </summary>
        Task<bool> RestoreTemplateVersionAsync(int templateId, int version, string userId);

        // ==================== Template Preview ====================
        
        /// <summary>
        /// پیش‌نمایش الگو با داده‌های نمونه
        /// </summary>
        Task<string> PreviewTemplateAsync(int templateId, Dictionary<string, string> sampleData = null);

        /// <summary>
        /// جایگزینی متغیرها در متن الگو
        /// </summary>
        string ReplaceTemplateVariables(string content, Dictionary<string, string> variables);

        // ==================== ViewModels ====================
        
        /// <summary>
        /// دریافت ViewModel لیست الگوها
        /// </summary>
        /// <param name="notificationTypeId">فیلتر بر اساس نوع اعلان</param>
        /// <param name="channelType">فیلتر بر اساس کانال</param>
        Task<NotificationTemplateListViewModel> GetTemplateListViewModelAsync(int? notificationTypeId = null, byte? channelType = null);

        /// <summary>
        /// دریافت ViewModel ایجاد/ویرایش الگو
        /// </summary>
        /// <param name="templateId">شناسه الگو (برای ویرایش)</param>
        /// <param name="eventType">نوع رویداد (برای فیلتر متغیرها)</param>
        Task<NotificationTemplateFormViewModel> GetTemplateFormViewModelAsync(int? templateId = null, byte? eventType = null);
        Task<bool> AddRecipientAsync(
           int templateId,
           byte recipientType,
           int? contactId,
           int? organizationId,
           string userId,
           string currentUserId);
    }
}