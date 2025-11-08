using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس جامع مدیریت اعلان‌ها
    /// جایگزین CoreNotificationRepository با امکانات کامل
    /// </summary>
    public class NotificationManagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationManagementService> _logger;
        private readonly MahERP.CommonLayer.Repository.TelegramBotSendNotification _telegramService;

        public NotificationManagementService(
            AppDbContext context,
            ILogger<NotificationManagementService> logger)
        {
            _context = context;
            _logger = logger;
            _telegramService = new MahERP.CommonLayer.Repository.TelegramBotSendNotification();
        }

        #region 🔔 عملیات پایه - Basic Operations (جایگزین CoreNotificationRepository)

        /// <summary>
        /// دریافت نوتیفیکیشن‌های کاربر با فیلتر و صفحه‌بندی
        /// </summary>
        public async Task<CoreNotificationListViewModel> GetUserNotificationsAsync(
            string userId,
            byte? systemId = null,
            bool unreadOnly = false,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive)
                    .AsQueryable();

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                if (unreadOnly)
                    query = query.Where(n => !n.IsRead);

                var totalCount = await query.CountAsync();
                var unreadCount = await _context.CoreNotification_Tbl
                    .CountAsync(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                var notifications = await query
                    .OrderByDescending(n => n.CreateDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(n => n.Sender)
                    .Include(n => n.Details)
                    .Include(n => n.Deliveries)
                    .ToListAsync();

                return new CoreNotificationListViewModel
                {
                    Notifications = notifications.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت نوتیفیکیشن‌های کاربر");
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد خوانده نشده");
                return 0;
            }
        }

        /// <summary>
        /// دریافت جزئیات یک نوتیفیکیشن
        /// </summary>
        public async Task<CoreNotificationViewModel> GetNotificationByIdAsync(int notificationId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .Include(n => n.Sender)
                    .Include(n => n.Recipient)
                    .Include(n => n.Details)
                    .Include(n => n.Deliveries)
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.IsActive);

                return notification != null ? MapToViewModel(notification) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در دریافت نوتیفیکیشن {notificationId}");
                return null;
            }
        }

        #endregion

        #region 📤 ثبت اعلان - Create Notifications

        /// <summary>
        /// ثبت اعلان برای یک رویداد خاص با ارسال خودکار اعلان‌های خارجی
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
                    // 🔸 1. ثبت اعلان سیستمی (همیشه ثبت می‌شود)
                    var systemNotificationId = await CreateSystemNotificationAsync(
                        eventType,
                        recipientUserId,
                        senderUserId,
                        title,
                        message,
                        actionUrl,
                        relatedRecordId,
                        relatedRecordType,
                        priority
                    );

                    if (systemNotificationId > 0)
                    {
                        totalNotifications++;

                        // 🔸 2. بررسی و ارسال اعلان‌های خارجی (Email/SMS/Telegram)
                        await ProcessExternalNotificationsAsync(
                            eventType,
                            recipientUserId,
                            title,
                            message,
                            actionUrl,
                            systemNotificationId
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
                var notification = new CoreNotification
                {
                    SystemId = 7, // Tasking
                    SystemName = "مدیریت تسک‌ها",
                    RecipientUserId = recipientUserId,
                    SenderUserId = senderUserId,
                    NotificationTypeGeneral = MapEventTypeToGeneralType(eventType),
                    Title = title,
                    Message = message,
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

                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در CreateSystemNotificationAsync");
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
                // 🔍 دریافت قالب‌های فعال برای این کاربر و این رویداد
                var templates = await _context.NotificationTemplate_Tbl
                    .Include(t => t.Recipients.Where(r => r.RecipientType == 2 && r.UserId == recipientUserId))
                    .Where(t => t.IsActive &&
                               t.NotificationEventType == (byte)eventType &&
                               t.Recipients.Any(r => r.RecipientType == 2 && r.UserId == recipientUserId))
                    .ToListAsync();

                if (!templates.Any())
                {
                    _logger.LogDebug($"ℹ️ قالب خارجی برای {recipientUserId} و {eventType} یافت نشد");
                    return;
                }

                // 🔄 ارسال از طریق هر کانال
                foreach (var template in templates)
                {
                    var finalMessage = ReplaceTemplatePlaceholders(
                        template.MessageTemplate,
                        title,
                        message,
                        actionUrl
                    );

                    switch ((NotificationChannel)template.Channel)
                    {
                        case NotificationChannel.Email:
                            await SendEmailNotificationAsync(
                                recipientUserId,
                                template.Subject,
                                finalMessage,
                                systemNotificationId
                            );
                            break;

                        case NotificationChannel.Sms:
                            await SendSmsNotificationAsync(
                                recipientUserId,
                                finalMessage,
                                systemNotificationId
                            );
                            break;

                        case NotificationChannel.Telegram:
                            await SendTelegramNotificationAsync(
                                recipientUserId,
                                finalMessage,
                                systemNotificationId
                            );
                            break;
                    }

                    // بروزرسانی تعداد استفاده
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

        #region 📧 ارسال اعلان‌های خارجی - External Notifications

        /// <summary>
        /// ارسال اعلان ایمیلی
        /// </summary>
        private async Task SendEmailNotificationAsync(
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

                // ✅ ایجاد رکورد Delivery
                var delivery = new CoreNotificationDelivery
                {
                    CoreNotificationId = coreNotificationId,
                    DeliveryMethod = 1, // Email
                    DeliveryAddress = user.Email,
                    DeliveryStatus = 0, // Pending
                    AttemptCount = 0,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotificationDelivery_Tbl.Add(delivery);
                await _context.SaveChangesAsync();

                // ✅ افزودن به صف ایمیل
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
        private async Task SendSmsNotificationAsync(
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

                // ✅ ایجاد رکورد Delivery
                var delivery = new CoreNotificationDelivery
                {
                    CoreNotificationId = coreNotificationId,
                    DeliveryMethod = 2, // SMS
                    DeliveryAddress = user.PhoneNumber,
                    DeliveryStatus = 0,
                    AttemptCount = 0,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotificationDelivery_Tbl.Add(delivery);
                await _context.SaveChangesAsync();

                // ✅ افزودن به صف پیامک
                var smsQueue = new MahERP.DataModelLayer.Entities.Sms.SmsQueue
                {
                    PhoneNumber = user.PhoneNumber,
                    MessageText = message,
                    RecipientType = 2, // User
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
        private async Task SendTelegramNotificationAsync(
            string userId,
            string message,
            int coreNotificationId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                
                // ✅ اصلاح: بررسی long? به جای string
                if (user == null || !user.TelegramChatId.HasValue)
                {
                    _logger.LogDebug($"ℹ️ Chat ID تلگرام کاربر {userId} یافت نشد");
                    return;
                }

                // ✅ ایجاد رکورد Delivery
                var delivery = new CoreNotificationDelivery
                {
                    CoreNotificationId = coreNotificationId,
                    DeliveryMethod = 3, // Telegram
                    DeliveryAddress = user.TelegramChatId.Value.ToString(), // ✅ تبدیل long به string
                    DeliveryStatus = 0,
                    AttemptCount = 0,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotificationDelivery_Tbl.Add(delivery);
                await _context.SaveChangesAsync();

                // ✅ ارسال مستقیم تلگرام
                var botToken = GetTelegramBotToken();
                
                if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_DEFAULT_BOT_TOKEN")
                {
                    _logger.LogWarning("⚠️ توکن تلگرام معتبر یافت نشد");
                    delivery.DeliveryStatus = 3; // خطا
                    delivery.ErrorMessage = "توکن تلگرام تنظیم نشده است";
                    await _context.SaveChangesAsync();
                    return;
                }

                try
                {
                    await _telegramService.SendNotificationAsync(
                        message,
                        user.TelegramChatId.Value, // ✅ استفاده مستقیم از long
                        botToken
                    );

                    // ✅ بروزرسانی وضعیت موفق
                    delivery.DeliveryStatus = 1; // ارسال شده
                    delivery.DeliveryDate = DateTime.Now;
                    
                    _logger.LogInformation($"✈️ پیام تلگرام برای {user.UserName} (ChatId: {user.TelegramChatId.Value}) ارسال شد");
                }
                catch (Exception sendEx)
                {
                    // ✅ ثبت خطای ارسال
                    delivery.DeliveryStatus = 3; // خطا
                    delivery.ErrorMessage = $"خطا در ارسال: {sendEx.Message}";
                    
                    _logger.LogError(sendEx, $"❌ خطا در ارسال تلگرام به ChatId: {user.TelegramChatId.Value}");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در SendTelegramNotificationAsync");
            }
        }

        #endregion

        #region 🔧 مدیریت وضعیت - Status Management

        /// <summary>
        /// علامت‌گذاری به عنوان خوانده شده
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null) return false;

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAsReadAsync");
                return false;
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان کلیک شده
        /// </summary>
        public async Task<bool> MarkAsClickedAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null) return false;

                notification.IsClicked = true;
                notification.ClickDate = DateTime.Now;

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAsClickedAsync");
                return false;
            }
        }

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های مرتبط با یک رکورد
        /// </summary>
        public async Task<int> MarkRelatedNotificationsAsReadAsync(string userId, byte systemId, string relatedRecordId)
        {
            try
            {
                var notifications = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId &&
                               n.SystemId == systemId &&
                               n.RelatedRecordId == relatedRecordId &&
                               n.IsActive &&
                               !n.IsRead)
                    .ToListAsync();

                var readDate = DateTime.Now;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadDate = readDate;
                }

                await _context.SaveChangesAsync();
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkRelatedNotificationsAsReadAsync");
                return 0;
            }
        }

        /// <summary>
        /// علامت‌گذاری همه به عنوان خوانده شده
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                var notifications = await query.ToListAsync();

                var readDate = DateTime.Now;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadDate = readDate;
                }

                await _context.SaveChangesAsync();
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAllAsReadAsync");
                return 0;
            }
        }

        #endregion

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

        #endregion

        #region 🛠️ متدهای کمکی - Helper Methods

        /// <summary>
        /// جایگزینی متغیرهای قالب
        /// </summary>
        private string ReplaceTemplatePlaceholders(string template, string title, string message, string actionUrl)
        {
            return template
                .Replace("{Title}", title)
                .Replace("{Message}", message)
                .Replace("{ActionUrl}", actionUrl)
                .Replace("{Date}", DateTime.Now.ToString("yyyy/MM/dd"))
                .Replace("{Time}", DateTime.Now.ToString("HH:mm"));
        }

        /// <summary>
        /// تبدیل EventType به NotificationTypeGeneral
        /// </summary>
        private byte MapEventTypeToGeneralType(NotificationEventType eventType)
        {
            return eventType switch
            {
                NotificationEventType.TaskAssigned => 9, // اختصاص
                NotificationEventType.TaskCompleted => 8, // تکمیل
                NotificationEventType.TaskDeadlineReminder => 6, // یادآوری
                NotificationEventType.TaskCommentAdded => 1, // ایجاد
                NotificationEventType.TaskUpdated => 2, // ویرایش
                NotificationEventType.TaskDeleted => 3, // حذف
                NotificationEventType.TaskStatusChanged => 10, // تغییر وضعیت
                NotificationEventType.TaskReassigned => 9, // اختصاص مجدد
                _ => 0 // عمومی
            };
        }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        private CoreNotificationViewModel MapToViewModel(CoreNotification notification)
        {
            return new CoreNotificationViewModel
            {
                Id = notification.Id,
                SystemId = notification.SystemId,
                SystemName = notification.SystemName,
                RecipientUserId = notification.RecipientUserId,
                RecipientUserName = notification.Recipient?.UserName,
                SenderUserId = notification.SenderUserId,
                SenderUserName = notification.Sender?.UserName,
                NotificationTypeGeneral = notification.NotificationTypeGeneral,
                NotificationTypeName = GetNotificationTypeName(notification.NotificationTypeGeneral),
                Title = notification.Title,
                Message = notification.Message,
                CreateDate = notification.CreateDate,
                CreateDatePersian = ConvertDateTime.ConvertMiladiToShamsi(notification.CreateDate, "yyyy/MM/dd"),
                CreateTime = notification.CreateDate.ToString("HH:mm"),
                IsRead = notification.IsRead,
                ReadDate = notification.ReadDate,
                Priority = notification.Priority,
                PriorityName = GetPriorityName(notification.Priority),
                ActionUrl = notification.ActionUrl,
                RelatedRecordId = notification.RelatedRecordId,
                RelatedRecordType = notification.RelatedRecordType,
                IsActive = notification.IsActive,
                RelativeTime = GetRelativeTime(notification.CreateDate),
                Icon = GetNotificationIcon(notification.NotificationTypeGeneral)
            };
        }

        private string GetNotificationTypeName(byte type) => type switch
        {
            0 => "عمومی", 1 => "ایجاد", 2 => "ویرایش", 3 => "حذف",
            4 => "تایید/رد", 5 => "هشدار", 6 => "یادآوری", 7 => "خطا",
            8 => "تکمیل", 9 => "اختصاص", 10 => "تغییر وضعیت",
            _ => "نامشخص"
        };

        private string GetPriorityName(byte priority) => priority switch
        {
            0 => "عادی", 1 => "مهم", 2 => "فوری", 3 => "بحرانی", _ => "نامشخص"
        };

        private string GetNotificationIcon(byte type) => type switch
        {
            0 => "fa-info-circle", 1 => "fa-plus-circle", 2 => "fa-edit",
            3 => "fa-trash", 4 => "fa-check-circle", 5 => "fa-exclamation-triangle",
            6 => "fa-clock", 7 => "fa-times-circle", 8 => "fa-flag-checkered",
            9 => "fa-user-plus", 10 => "fa-exchange-alt", _ => "fa-bell"
        };

        private string GetRelativeTime(DateTime createDate)
        {
            var timeSpan = DateTime.Now - createDate;

            if (timeSpan.TotalMinutes < 1) return "الان";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} روز پیش";

            return ConvertDateTime.ConvertMiladiToShamsi(createDate, "yyyy/MM/dd");
        }

        private string GetTelegramBotToken()
        {
            try
            {
                // دریافت تنظیمات از دیتابیس
                var settings = _context.Settings_Tbl.FirstOrDefault();

                // بررسی فعال بودن تلگرام
                if (settings != null && settings.IsTelegramEnabled && !string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return settings.TelegramBotToken;
                }

                // در صورت عدم وجود تنظیمات، از appsettings استفاده شود
                _logger.LogWarning("⚠️ توکن تلگرام در تنظیمات یافت نشد یا تلگرام غیرفعال است");
                return "YOUR_DEFAULT_BOT_TOKEN"; // یا می‌توانید از IConfiguration استفاده کنید
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در دریافت توکن تلگرام");
                return "YOUR_DEFAULT_BOT_TOKEN";
            }
        }

        #endregion
    }
}