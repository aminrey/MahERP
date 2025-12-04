using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// آمار و گزارش اعلان‌ها
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 📊 آمار و گزارش - Statistics

        /// <summary>
        /// آمار نوتیفیکیشن‌های کاربر
        /// </summary>
        public async Task<CoreNotificationStatsViewModel> GetUserNotificationStatsAsync(
            string userId,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive);

                if (fromDate.HasValue)
                    query = query.Where(n => n.CreateDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(n => n.CreateDate <= toDate.Value);

                return new CoreNotificationStatsViewModel
                {
                    TotalNotifications = await query.CountAsync(),
                    ReadNotifications = await query.CountAsync(n => n.IsRead),
                    UnreadNotifications = await query.CountAsync(n => !n.IsRead),
                    ClickedNotifications = await query.CountAsync(n => n.IsClicked)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در GetUserNotificationStatsAsync");
                return new CoreNotificationStatsViewModel();
            }
        }

        /// <summary>
        /// رندر کردن قالب برای ارسال دستی
        /// </summary>
        public async Task<(string RenderedSubject, string RenderedMessage)> RenderTemplateForManualSendAsync(
            int templateId,
            string recipientUserId,
            string senderUserId,
            string defaultSubject,
            string defaultMessage)
        {
            try
            {
                var template = await _context.NotificationTemplate_Tbl
                    .FirstOrDefaultAsync(t => t.Id == templateId);

                if (template == null)
                {
                    _logger.LogWarning($"⚠️ قالب {templateId} یافت نشد");
                    return (defaultSubject, defaultMessage);
                }

                var templateData = await BuildTemplateDataAsync(
                    (Enums.NotificationEventType)template.NotificationEventType,
                    recipientUserId,
                    defaultSubject,
                    defaultMessage,
                    "",
                    0
                );

                var renderedSubject = ReplaceAllPlaceholders(defaultSubject ?? template.Subject ?? "", templateData);
                var renderedMessage = ReplaceAllPlaceholders(defaultMessage ?? template.MessageTemplate ?? "", templateData);

                return (renderedSubject, renderedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در رندر کردن قالب");
                return (defaultSubject, defaultMessage);
            }
        }

        #endregion
    }
}
