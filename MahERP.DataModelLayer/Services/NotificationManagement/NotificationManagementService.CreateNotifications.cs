using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// ثبت و پردازش اعلان‌ها
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 📤 ثبت اعلان - Create Notifications

        /// <summary>
        /// ⭐⭐⭐ ثبت اعلان برای قالب زمان‌بندی شده (Scheduled Template)
        /// </summary>
        public async Task<int> ProcessScheduledNotificationAsync(
            NotificationTemplate template,
            List<string> recipientUserIds)
        {
            if (!recipientUserIds.Any())
            {
                _logger.LogWarning($"⚠️ هیچ کاربری برای قالب {template.TemplateName} یافت نشد");
                return 0;
            }

            try
            {
                int totalNotifications = 0;

                foreach (var recipientUserId in recipientUserIds.Distinct())
                {
                    await ProcessSingleTemplateNotificationAsync(
                        template,
                        recipientUserId,
                        0
                    );
                    
                    totalNotifications++;
                }

                _logger.LogInformation($"✅ {totalNotifications} اعلان برای قالب {template.TemplateName} ارسال شد");
                return totalNotifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ProcessScheduledNotificationAsync");
                return 0;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ پردازش یک قالب خاص برای یک کاربر
        /// ⭐⭐⭐ FIX: برای DailyTaskDigest، چند پیام جداگانه ارسال می‌کند
        /// </summary>
        private async Task ProcessSingleTemplateNotificationAsync(
            NotificationTemplate template,
            string recipientUserId,
            int systemNotificationId)
        {
            try
            {
                _logger.LogInformation($"📤 ارسال قالب {template.TemplateName} به کاربر {recipientUserId} از طریق کانال {template.Channel}");

                var templateData = await BuildTemplateDataAsync(
                    (NotificationEventType)template.NotificationEventType,
                    recipientUserId,
                    template.Subject ?? "اعلان",
                    template.MessageTemplate ?? "",
                    "",
                    systemNotificationId
                );

                // ⭐⭐⭐ FIX: برای Daily Digest، چند پیام جداگانه ارسال کن
                if ((NotificationEventType)template.NotificationEventType == NotificationEventType.DailyTaskDigest)
                {
                    await ProcessDailyDigestMultipleMessagesAsync(
                        template,
                        recipientUserId,
                        templateData,
                        systemNotificationId
                    );
                    return;
                }

                // سایر انواع: رفتار عادی
                var finalMessage = ReplaceAllPlaceholders(template.MessageTemplate, templateData);
                var finalSubject = ReplaceAllPlaceholders(template.Subject ?? "", templateData);

                switch ((NotificationChannel)template.Channel)
                {
                    case NotificationChannel.Email:
                        await SendEmailNotificationAsync(recipientUserId, finalSubject, finalMessage, systemNotificationId);
                        break;
                    case NotificationChannel.Sms:
                        await SendSmsNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                        break;
                    case NotificationChannel.Telegram:
                        await SendTelegramNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                        break;
                    default:
                        _logger.LogWarning($"⚠️ کانال نامعتبر: {template.Channel}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ProcessSingleTemplateNotificationAsync");
            }
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: ارسال چند پیام برای Daily Digest
        /// </summary>
        private async Task ProcessDailyDigestMultipleMessagesAsync(
            NotificationTemplate template,
            string recipientUserId,
            Dictionary<string, string> templateData,
            int systemNotificationId)
        {
            try
            {
                var messages = await BuildPendingTasksMessagesAsync(recipientUserId);

                if (!messages.Any())
                {
                    _logger.LogWarning($"⚠️ هیچ پیامی برای {recipientUserId} ساخته نشد");
                    return;
                }

                _logger.LogInformation($"📨 {messages.Count} پیام برای {recipientUserId} آماده ارسال است");

                for (int i = 0; i < messages.Count; i++)
                {
                    var message = messages[i];
                    
                    var modifiedData = new Dictionary<string, string>(templateData, StringComparer.OrdinalIgnoreCase)
                    {
                        ["PendingTasks"] = message
                    };

                    var finalMessage = ReplaceAllPlaceholders(template.MessageTemplate, modifiedData);

                    switch ((NotificationChannel)template.Channel)
                    {
                        case NotificationChannel.Telegram:
                            await SendTelegramNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                            _logger.LogInformation($"✅ پیام {i + 1}/{messages.Count} برای {recipientUserId} به صف تلگرام اضافه شد");
                            
                            if (i < messages.Count - 1)
                            {
                                await Task.Delay(100);
                            }
                            break;

                        case NotificationChannel.Email:
                            var finalSubject = ReplaceAllPlaceholders(template.Subject ?? "گزارش تسک‌ها", modifiedData);
                            await SendEmailNotificationAsync(recipientUserId, finalSubject, finalMessage, systemNotificationId);
                            break;

                        case NotificationChannel.Sms:
                            await SendSmsNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                            break;
                    }
                }

                _logger.LogInformation($"✅ همه {messages.Count} پیام برای {recipientUserId} ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ProcessDailyDigestMultipleMessagesAsync");
            }
        }

        /// <summary>
        /// ثبت اعلان برای یک رویداد خاص
        /// </summary>
        public async Task<int> ProcessEventNotificationAsync(
            NotificationEventType eventType,
            List<string> recipientUserIds,
            string senderUserId,
            string title,
            string message,
            string actionUrl,
            string relatedRecordId,
            string relatedRecordType,
            byte priority = 1)
        {
            if (!recipientUserIds.Any())
            {
                _logger.LogWarning($"⚠️ هیچ کاربری برای رویداد {eventType} یافت نشد");
                return 0;
            }

            try
            {
                int totalNotifications = 0;

                foreach (var recipientUserId in recipientUserIds.Distinct())
                {
                    var systemNotificationId = await CreateSystemNotificationAsync(
                        eventType, recipientUserId, senderUserId, title, message,
                        actionUrl, relatedRecordId, relatedRecordType, priority
                    );

                    if (systemNotificationId > 0)
                    {
                        totalNotifications++;
                        await ProcessExternalNotificationsAsync(
                            eventType, recipientUserId, title, message, actionUrl, systemNotificationId
                        );
                    }
                }

                _logger.LogInformation($"✅ {totalNotifications} اعلان برای {eventType} ثبت شد");
                return totalNotifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ProcessEventNotificationAsync");
                return 0;
            }
        }

        /// <summary>
        /// ثبت اعلان سیستمی
        /// </summary>
        private async Task<int> CreateSystemNotificationAsync(
            NotificationEventType eventType,
            string recipientUserId,
            string senderUserId,
            string title,
            string message,
            string actionUrl,
            string relatedRecordId,
            string relatedRecordType,
            byte priority)
        {
            try
            {
                string actualSenderId = (senderUserId == "SYSTEM" || string.IsNullOrEmpty(senderUserId)) 
                    ? null 
                    : senderUserId;

                var processedTitle = title;
                var processedMessage = message;
                
                if (IsTaskRelatedEvent(eventType) && !string.IsNullOrEmpty(relatedRecordId) && int.TryParse(relatedRecordId, out int taskId))
                {
                    var task = await _context.Tasks_Tbl
                        .Where(t => t.Id == taskId)
                        .Select(t => new { t.Title, t.TaskCode })
                        .FirstOrDefaultAsync();

                    if (task != null)
                    {
                        processedTitle = processedTitle
                            .Replace("{{TaskTitle}}", task.Title, StringComparison.OrdinalIgnoreCase)
                            .Replace("{{TaskCode}}", task.TaskCode, StringComparison.OrdinalIgnoreCase);
                        
                        processedMessage = processedMessage
                            .Replace("{{TaskTitle}}", task.Title, StringComparison.OrdinalIgnoreCase)
                            .Replace("{{TaskCode}}", task.TaskCode, StringComparison.OrdinalIgnoreCase);
                        
                        // ⭐⭐⭐ FIX: برای TaskCommentAdded، متن کامنت را نمایش نده (حریم خصوصی)
                        if (eventType == NotificationEventType.TaskCommentAdded)
                        {
                            // دریافت اطلاعات فرستنده برای پیام استاتیک
                            string senderName = "کاربر";
                            if (!string.IsNullOrEmpty(senderUserId) && senderUserId != "SYSTEM")
                            {
                                var sender = await _context.Users
                                    .Where(u => u.Id == senderUserId)
                                    .Select(u => new { u.FirstName, u.LastName })
                                    .FirstOrDefaultAsync();
                                
                                if (sender != null)
                                {
                                    senderName = $"{sender.FirstName} {sender.LastName}".Trim();
                                }
                            }
                            
                            // متن استاتیک برای کانال سیستم
                            processedMessage = $"در تسک '{task.Title}' (کد: {task.TaskCode}) توسط {senderName} یک نظر در گفتگو ثبت شده است.";
                        }
                    }
                }

                var notification = new CoreNotification
                {
                    SystemId = 7,
                    SystemName = "مدیریت تسک‌ها",
                    RecipientUserId = recipientUserId,
                    SenderUserId = actualSenderId,
                    NotificationTypeGeneral = MapEventTypeToGeneralType(eventType),
                    Title = processedTitle,
                    Message = processedMessage,
                    ActionUrl = actionUrl,
                    RelatedRecordId = relatedRecordId,
                    RelatedRecordType = relatedRecordType,
                    Priority = priority,
                    IsRead = false,
                    IsClicked = false,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotification_Tbl.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ اعلان سیستمی #{notification.Id} برای {recipientUserId} ثبت شد");
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در CreateSystemNotificationAsync");
                return 0;
            }
        }

        /// <summary>
        /// پردازش و ارسال اعلان‌های خارجی بر اساس قالب‌ها
        /// </summary>
        private async Task ProcessExternalNotificationsAsync(
            NotificationEventType eventType,
            string recipientUserId,
            string title,
            string message,
            string actionUrl,
            int systemNotificationId)
        {
            try
            {
                _logger.LogInformation($"🔍 شروع ProcessExternalNotificationsAsync - EventType: {eventType}, RecipientUserId: {recipientUserId}");

                var templates = await _context.NotificationTemplate_Tbl
                    .Where(t => t.IsActive &&
                               t.NotificationEventType == (byte)eventType &&
                               !t.IsScheduled && // ⭐⭐⭐ FIX: حذف قالب‌های زمان‌بندی شده
                               (
                                   t.RecipientMode == 0 ||
                                   (t.RecipientMode == 1 && t.Recipients.Any(r => r.RecipientType == 2 && r.UserId == recipientUserId)) ||
                                   (t.RecipientMode == 2 && !t.Recipients.Any(r => r.RecipientType == 2 && r.UserId == recipientUserId))
                               ))
                    .ToListAsync();

                _logger.LogInformation($"🔍 یافت شد: {templates.Count} قالب غیر زمان‌بندی برای EventType={eventType}");

                if (!templates.Any())
                {
                    _logger.LogWarning($"⚠️ هیچ قالب خارجی (غیرزمان‌بندی) برای {recipientUserId} و {eventType} یافت نشد");
                    return;
                }

                var templateData = await BuildTemplateDataAsync(eventType, recipientUserId, title, message, actionUrl, systemNotificationId);

                foreach (var template in templates)
                {
                    var finalMessage = ReplaceAllPlaceholders(template.MessageTemplate, templateData);

                    switch ((NotificationChannel)template.Channel)
                    {
                        case NotificationChannel.Email:
                            var finalSubject = ReplaceAllPlaceholders(template.Subject ?? title, templateData);
                            await SendEmailNotificationAsync(recipientUserId, finalSubject, finalMessage, systemNotificationId);
                            break;
                        case NotificationChannel.Sms:
                            await SendSmsNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                            break;
                        case NotificationChannel.Telegram:
                            await SendTelegramNotificationAsync(recipientUserId, finalMessage, systemNotificationId);
                            break;
                    }

                    template.UsageCount++;
                    template.LastUsedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در ProcessExternalNotificationsAsync");
            }
        }

        #endregion
    }
}
