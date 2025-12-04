using MahERP.CommonLayer.Repository;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// ارسال اعلان‌های خارجی (Email, SMS, Telegram)
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 📧 ارسال اعلان‌های خارجی - External Notifications

        /// <summary>
        /// ارسال اعلان ایمیلی
        /// </summary>
        public async Task SendEmailNotificationAsync(
            string userId,
            string subject,
            string body,
            int coreNotificationId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning($"⚠️ ایمیل کاربر {userId} یافت نشد");
                    return;
                }

                if (coreNotificationId > 0)
                {
                    var delivery = new CoreNotificationDelivery
                    {
                        CoreNotificationId = coreNotificationId,
                        DeliveryMethod = 1,
                        DeliveryAddress = user.Email,
                        DeliveryStatus = 0,
                        AttemptCount = 0,
                        CreateDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.CoreNotificationDelivery_Tbl.Add(delivery);
                    await _context.SaveChangesAsync();
                }

                var emailQueue = new EmailQueue
                {
                    ToEmail = user.Email,
                    ToName = $"{user.FirstName} {user.LastName}",
                    Subject = subject,
                    Body = body,
                    IsHtml = true,
                    Priority = 1,
                    Status = 0,
                    CreatedDate = DateTime.Now,
                    RequestedByUserId = "SYSTEM"
                };

                _context.EmailQueue_Tbl.Add(emailQueue);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"📧 ایمیل برای {user.Email} به صف اضافه شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در SendEmailNotificationAsync");
            }
        }

        /// <summary>
        /// ارسال اعلان پیامکی
        /// </summary>
        public async Task SendSmsNotificationAsync(
            string userId,
            string message,
            int coreNotificationId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    _logger.LogWarning($"⚠️ شماره تلفن کاربر {userId} یافت نشد");
                    return;
                }

                if (coreNotificationId > 0)
                {
                    var delivery = new CoreNotificationDelivery
                    {
                        CoreNotificationId = coreNotificationId,
                        DeliveryMethod = 2,
                        DeliveryAddress = user.PhoneNumber,
                        DeliveryStatus = 0,
                        AttemptCount = 0,
                        CreateDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.CoreNotificationDelivery_Tbl.Add(delivery);
                    await _context.SaveChangesAsync();
                }

                var smsQueue = new SmsQueue
                {
                    PhoneNumber = user.PhoneNumber,
                    MessageText = message,
                    RecipientType = 2,
                    Priority = 1,
                    Status = 0,
                    RequestedByUserId = "SYSTEM",
                    CreatedDate = DateTime.Now
                };

                _context.SmsQueue_Tbl.Add(smsQueue);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"📱 پیامک برای {user.PhoneNumber} به صف اضافه شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در SendSmsNotificationAsync");
            }
        }

        /// <summary>
        /// ارسال اعلان تلگرامی
        /// </summary>
        public async Task SendTelegramNotificationAsync(
            string userId,
            string message,
            int coreNotificationId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null || !user.TelegramChatId.HasValue)
                {
                    _logger.LogDebug($"ℹ️ Chat ID تلگرام کاربر {userId} یافت نشد");
                    return;
                }

                var botToken = GetTelegramBotToken();

                if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_DEFAULT_BOT_TOKEN")
                {
                    _logger.LogWarning("⚠️ توکن تلگرام معتبر یافت نشد");
                    return;
                }

                NotificationContext notificationContext = null;
                
                if (coreNotificationId > 0)
                {
                    notificationContext = await BuildNotificationContextAsync(coreNotificationId, userId);
                }
                else
                {
                    notificationContext = new NotificationContext
                    {
                        BaseUrl = "https://resnaco.ir",
                        TaskId = null,
                        EventType = 13,
                        HasPendingTasksList = true,
                        RecipientUserId = userId
                    };
                }

                var telegramQueue = new TelegramNotificationQueue
                {
                    ChatId = user.TelegramChatId.Value,
                    Message = message,
                    BotToken = botToken,
                    ContextJson = notificationContext != null 
                        ? System.Text.Json.JsonSerializer.Serialize(notificationContext) 
                        : null,
                    CoreNotificationId = coreNotificationId > 0 ? coreNotificationId : null,
                    UserId = userId,
                    Priority = 1,
                    Status = 0,
                    RetryCount = 0,
                    MaxRetries = 3,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.TelegramNotificationQueue_Tbl.Add(telegramQueue);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ پیام تلگرام برای {user.UserName} به صف اضافه شد (QueueId: {telegramQueue.Id})");

                if (coreNotificationId > 0)
                {
                    var delivery = new CoreNotificationDelivery
                    {
                        CoreNotificationId = coreNotificationId,
                        DeliveryMethod = 3,
                        DeliveryAddress = user.TelegramChatId.Value.ToString(),
                        DeliveryStatus = 0,
                        AttemptCount = 0,
                        CreateDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.CoreNotificationDelivery_Tbl.Add(delivery);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در افزودن به صف تلگرام");
            }
        }

        /// <summary>
        /// ساخت Context برای دکمه‌های پویای تلگرام
        /// </summary>
        private async Task<NotificationContext> BuildNotificationContextAsync(
            int coreNotificationId,
            string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .Where(n => n.Id == coreNotificationId)
                    .Select(n => new
                    {
                        n.RelatedRecordId,
                        n.RelatedRecordType,
                        n.ActionUrl,
                        n.Message
                    })
                    .FirstOrDefaultAsync();

                if (notification == null)
                    return null;

                byte? eventType = ExtractEventTypeFromNotification(notification.ActionUrl, notification.Message);
                bool hasPendingTasksList = notification.Message?.Contains("📌 تسک‌های در حال انجام شما") == true;

                string taskId = null;
                if (notification.RelatedRecordType == "Task" && !string.IsNullOrEmpty(notification.RelatedRecordId))
                {
                    taskId = notification.RelatedRecordId;
                }

                return new NotificationContext
                {
                    BaseUrl = "https://resnaco.ir",
                    TaskId = taskId,
                    EventType = eventType,
                    HasPendingTasksList = hasPendingTasksList,
                    RecipientUserId = userId
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ خطا در ساخت NotificationContext");
                return null;
            }
        }

        /// <summary>
        /// استخراج نوع رویداد از URL یا محتوای پیام
        /// </summary>
        private byte? ExtractEventTypeFromNotification(string actionUrl, string message)
        {
            if (!string.IsNullOrEmpty(actionUrl))
            {
                if (actionUrl.Contains("/Tasks/Details/")) return 1;
                if (actionUrl.Contains("/Tasks/MyTasks")) return 13;
                if (actionUrl.Contains("CompleteTask")) return 3;
            }

            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("📌 تسک‌های در حال انجام")) return 13;
                if (message.Contains("تسک جدیدی") || message.Contains("اختصاص داده شد")) return 1;
                if (message.Contains("یادآوری سررسید")) return 3;
                if (message.Contains("کامنت جدید")) return 4;
                if (message.Contains("تکمیل شد")) return 2;
                if (message.Contains("ویرایش")) return 5;
                if (message.Contains("تغییر وضعیت")) return 8;
            }

            return null;
        }

        #endregion
    }
}
