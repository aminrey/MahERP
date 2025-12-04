using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// متدهای کمکی عمومی
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 🛠️ متدهای کمکی - Helper Methods

        /// <summary>
        /// دریافت توکن تلگرام از تنظیمات
        /// </summary>
        private string GetTelegramBotToken()
        {
            try
            {
                var telegramToken = _context.Settings_Tbl.FirstOrDefault()?.TelegramBotToken;
                return telegramToken ?? "YOUR_DEFAULT_BOT_TOKEN";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در دریافت توکن تلگرام");
                return "YOUR_DEFAULT_BOT_TOKEN";
            }
        }

        /// <summary>
        /// نگاشت نوع رویداد به نوع عمومی
        /// </summary>
        private byte MapEventTypeToGeneralType(NotificationEventType eventType)
        {
            return eventType switch
            {
                NotificationEventType.TaskAssigned => 9,
                NotificationEventType.TaskReassigned => 9,
                NotificationEventType.TaskCompleted => 8,
                NotificationEventType.TaskUpdated => 2,
                NotificationEventType.TaskDeleted => 3,
                NotificationEventType.TaskCommentAdded => 0,
                NotificationEventType.TaskStatusChanged => 10,
                NotificationEventType.TaskDeadlineReminder => 6,
                NotificationEventType.CustomTaskReminder => 6,
                NotificationEventType.CommentMentioned => 0,
                NotificationEventType.DailyTaskDigest => 0,
                _ => 0
            };
        }

        /// <summary>
        /// تبدیل CoreNotification به ViewModel
        /// </summary>
        private CoreNotificationViewModel MapToViewModel(CoreNotification notification)
        {
            return new CoreNotificationViewModel
            {
                Id = notification.Id,
                SystemId = notification.SystemId,
                SystemName = notification.SystemName,
                Title = notification.Title,
                Message = notification.Message,
                NotificationTypeGeneral = notification.NotificationTypeGeneral,
                ActionUrl = notification.ActionUrl,
                RelatedRecordId = notification.RelatedRecordId,
                RelatedRecordType = notification.RelatedRecordType,
                RelatedRecordTitle = notification.RelatedRecordTitle,
                RecipientUserId = notification.RecipientUserId,
                SenderUserId = notification.SenderUserId,
                Priority = notification.Priority,
                IsRead = notification.IsRead,
                IsClicked = notification.IsClicked,
                CreateDate = notification.CreateDate,
                ReadDate = notification.ReadDate,
                ClickDate = notification.ClickDate
            };
        }

        /// <summary>
        /// محاسبه زمان گذشته به صورت متنی
        /// </summary>
        private string CalculateTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "اکنون";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} روز پیش";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} ماه پیش";
            
            return $"{(int)(timeSpan.TotalDays / 365)} سال پیش";
        }

        /// <summary>
        /// جایگزینی همه متغیرها در قالب
        /// </summary>
        private string ReplaceAllPlaceholders(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template) || data == null || !data.Any())
                return template;

            var result = template;

            foreach (var kvp in data)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// بررسی اینکه رویداد مرتبط با تسک است
        /// </summary>
        private bool IsTaskRelatedEvent(NotificationEventType eventType)
        {
            return eventType switch
            {
                NotificationEventType.TaskAssigned => true,
                NotificationEventType.TaskCompleted => true,
                NotificationEventType.TaskCommentAdded => true,
                NotificationEventType.TaskUpdated => true,
                NotificationEventType.TaskDeleted => true,
                NotificationEventType.TaskStatusChanged => true,
                NotificationEventType.TaskReassigned => true,
                NotificationEventType.TaskDeadlineReminder => true,
                NotificationEventType.TaskOperationCompleted => true,
                NotificationEventType.OperationAssigned => true,
                NotificationEventType.CommentMentioned => true,
                NotificationEventType.TaskPriorityChanged => true,
                NotificationEventType.CustomTaskReminder => true,
                NotificationEventType.TaskWorkLog => true,
                NotificationEventType.DailyTaskDigest => false,
                _ => false
            };
        }

        #endregion
    }
}
