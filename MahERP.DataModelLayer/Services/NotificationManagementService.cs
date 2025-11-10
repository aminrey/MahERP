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
                var templates = await _context.NotificationTemplate_Tbl
                    .Where(t => t.IsActive &&
                               t.NotificationEventType == (byte)eventType &&
                               (
                                   // ⭐ RecipientMode = 0: همه کاربران (بدون چک Recipients)
                                   t.RecipientMode == 0 ||

                                   // ⭐ RecipientMode = 1: فقط کاربران خاص
                                   (t.RecipientMode == 1 && t.Recipients.Any(r => r.RecipientType == 2 && r.UserId == recipientUserId)) ||

                                   // ⭐ RecipientMode = 2: همه به جز...
                                   (t.RecipientMode == 2 && !t.Recipients.Any(r => r.RecipientType == 2 && r.UserId == recipientUserId))
                               ))
                    .ToListAsync();

                if (!templates.Any())
                {
                    _logger.LogDebug($"ℹ️ قالب خارجی برای {recipientUserId} و {eventType} یافت نشد");
                    return;
                }

                _logger.LogInformation($"✅ یافت شد: {templates.Count} قالب برای {eventType} و کاربر {recipientUserId}");

                // ⭐⭐⭐ دریافت اطلاعات کامل برای جایگزینی متغیرها
                var templateData = await BuildTemplateDataAsync(eventType, recipientUserId, title, message, actionUrl, systemNotificationId);

                // 🔄 ارسال از طریق هر کانال
                foreach (var template in templates)
                {
                    // ⭐⭐⭐ جایگزینی کامل متغیرها
                    var finalMessage = ReplaceAllPlaceholders(template.MessageTemplate, templateData);

                    switch ((NotificationChannel)template.Channel)
                    {
                        case NotificationChannel.Email:
                            var finalSubject = ReplaceAllPlaceholders(template.Subject ?? title, templateData);
                            await SendEmailNotificationAsync(
                                recipientUserId,
                                finalSubject,
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
        /// دریافت توکن تلگرام از تنظیمات
        /// </summary>
        private string GetTelegramBotToken()
        {
            try
            {
                // ⭐ استفاده مستقیم از توکن ثابت (در صورتی که جدول تنظیمات نداریم)
                // TODO: بهتر است از appsettings.json یا دیتابیس دریافت شود
                
                return "7931841421:AAFna2M4CkkktixVeIxWWE1XRruum9j-kY0"; // ⭐ توکن پیش‌فرض
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در دریافت توکن تلگرام");
                return "YOUR_DEFAULT_BOT_TOKEN";
            }
        }

        /// <summary>
        /// نگاشت نوع رویداد به نوع عمومی NotificationTypeGeneral
        /// </summary>
        private byte MapEventTypeToGeneralType(NotificationEventType eventType)
        {
            return eventType switch
            {
                NotificationEventType.TaskAssigned => 9,        // اختصاص/انتساب
                NotificationEventType.TaskReassigned => 9,      // اختصاص مجدد
                NotificationEventType.TaskCompleted => 8,       // تکمیل فرآیند
                NotificationEventType.TaskUpdated => 2,         // ویرایش رکورد
                NotificationEventType.TaskDeleted => 3,         // حذف رکورد
                NotificationEventType.TaskCommentAdded => 0,    // اطلاع‌رسانی عمومی
                NotificationEventType.TaskStatusChanged => 10,  // تغییر وضعیت
                NotificationEventType.TaskDeadlineReminder => 6,// یادآوری
                NotificationEventType.CommentMentioned => 0,    // اطلاع‌رسانی عمومی
                NotificationEventType.DailyTaskDigest => 0,     // اطلاع‌رسانی عمومی
                _ => 0 // پیش‌فرض
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
                Priority = notification.Priority,
                IsRead = notification.IsRead,
                IsClicked = notification.IsClicked,
                CreateDate = notification.CreateDate,
                ReadDate = notification.ReadDate,
                ClickDate = notification.ClickDate
                // SenderName و TimeAgo حذف شدند (در ViewModel وجود ندارند)
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
        /// ⭐⭐⭐ ساخت دیکشنری کامل داده‌ها برای جایگزینی در قالب
        /// </summary>
        private async Task<Dictionary<string, string>> BuildTemplateDataAsync(
            NotificationEventType eventType,
            string recipientUserId,
            string title,
            string message,
            string actionUrl,
            int systemNotificationId)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // ⭐ متغیرهای پایه
                { "Title", title },
                { "Message", message },
                { "ActionUrl", actionUrl },
                { "Date", DateTime.Now.ToString("yyyy/MM/dd") },
                { "Time", DateTime.Now.ToString("HH:mm") }
            };

            try
            {
                // ⭐⭐⭐ دریافت اطلاعات کاربر دریافت‌کننده
                var recipient = await _context.Users
                    .Where(u => u.Id == recipientUserId)
                    .Select(u => new
                    {
                        u.FirstName,
                        u.LastName,
                        u.UserName,
                        u.Email,
                        u.PhoneNumber
                    })
                    .FirstOrDefaultAsync();

                if (recipient != null)
                {
                    data["RecipientFirstName"] = recipient.FirstName ?? "";
                    data["RecipientLastName"] = recipient.LastName ?? "";
                    data["RecipientFullName"] = $"{recipient.FirstName} {recipient.LastName}".Trim();
                    data["RecipientUserName"] = recipient.UserName ?? "";
                    data["RecipientEmail"] = recipient.Email ?? "";
                    data["RecipientPhone"] = recipient.PhoneNumber ?? "";
                }

                // ⭐⭐⭐ NEW: دریافت لیست تسک‌های انجام نشده کاربر
                var pendingTasksList = await BuildPendingTasksListAsync(recipientUserId);
                data["PendingTasks"] = pendingTasksList;

                // ⭐⭐⭐ دریافت اطلاعات تسک (اگر مرتبط با تسک باشد)
                if (IsTaskRelatedEvent(eventType))
                {
                    // استخراج TaskId از RelatedRecordId در CoreNotification
                    var coreNotification = await _context.CoreNotification_Tbl
                        .Where(n => n.Id == systemNotificationId)
                        .Select(n => new { n.RelatedRecordId, n.SenderUserId })
                        .FirstOrDefaultAsync();

                    if (coreNotification != null && !string.IsNullOrEmpty(coreNotification.RelatedRecordId))
                    {
                        if (int.TryParse(coreNotification.RelatedRecordId, out int taskId))
                        {
                            // ⭐ دریافت اطلاعات تسک
                            var task = await _context.Tasks_Tbl
                                .Where(t => t.Id == taskId)
                                .Select(t => new
                                {
                                    t.Title,
                                    t.TaskCode,
                                    t.Description,
                                    t.StartDate,
                                    t.DueDate,
                                    t.Priority,
                                    t.CreatorUserId,
                                    CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : "",
                                    // ⭐ حذف Stakeholder (دیگر وجود ندارد)
                                    // استفاده از Contact یا Organization
                                    StakeholderName = t.Contact != null 
                                        ? $"{t.Contact.FirstName} {t.Contact.LastName}" 
                                        : (t.Organization != null ? t.Organization.DisplayName : ""),
                                    BranchName = t.Branch != null ? t.Branch.Name : ""
                                })
                                .FirstOrDefaultAsync();

                            if (task != null)
                            {
                                data["TaskTitle"] = task.Title ?? "";
                                data["TaskCode"] = task.TaskCode ?? "";
                                data["TaskDescription"] = task.Description ?? "";
                                data["TaskStartDate"] = task.StartDate.HasValue 
                                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.StartDate.Value, "yyyy/MM/dd") 
                                    : "";
                                data["TaskDueDate"] = task.DueDate.HasValue 
                                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd") 
                                    : "";
                                data["TaskPriority"] = task.Priority switch
                                {
                                    0 => "عادی",
                                    1 => "متوسط",
                                    2 => "بالا",
                                    3 => "فوری",
                                    _ => "نامشخص"
                                };
                                data["TaskCategory"] = task.CategoryTitle;
                                data["TaskStakeholder"] = task.StakeholderName;
                                data["TaskBranch"] = task.BranchName;

                                // ⭐ دریافت اطلاعات سازنده تسک
                                if (!string.IsNullOrEmpty(task.CreatorUserId))
                                {
                                    var creator = await _context.Users
                                        .Where(u => u.Id == task.CreatorUserId)
                                        .Select(u => new { u.FirstName, u.LastName })
                                        .FirstOrDefaultAsync();

                                    if (creator != null)
                                    {
                                        data["TaskCreatorName"] = $"{creator.FirstName} {creator.LastName}".Trim();
                                    }
                                }

                                // ⭐ دریافت اطلاعات ارسال‌کننده اعلان
                                if (!string.IsNullOrEmpty(coreNotification.SenderUserId))
                                {
                                    var sender = await _context.Users
                                        .Where(u => u.Id == coreNotification.SenderUserId)
                                        .Select(u => new { u.FirstName, u.LastName })
                                        .FirstOrDefaultAsync();

                                    if (sender != null)
                                    {
                                        data["SenderName"] = $"{sender.FirstName} {sender.LastName}".Trim();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ خطا در ساخت داده‌های قالب");
            }

            return data;
        }

        /// <summary>
        /// ⭐⭐⭐ ساخت لیست فرمت‌شده تسک‌های انجام نشده کاربر
        /// </summary>
        private async Task<string> BuildPendingTasksListAsync(string userId)
        {
            try
            {
                // ⭐ دریافت تسک‌های انجام نشده کاربر
                var pendingTasks = await _context.TaskAssignment_Tbl
                    .Where(a => a.AssignedUserId == userId && 
                               !a.CompletionDate.HasValue &&
                               !a.Task.IsDeleted)
                    .Include(a => a.Task)
                        .ThenInclude(t => t.TaskOperations)
                    .Include(a => a.Task.Creator)
                    .OrderBy(a => a.Task.DueDate)
                    .ThenByDescending(a => a.Task.Priority)
                    .Take(10) // ⭐ محدود به 10 تسک اول
                    .Select(a => new
                    {
                        TaskId = a.Task.Id,
                        Title = a.Task.Title,
                        Description = a.Task.Description,
                        StartDate = a.Task.StartDate,
                        DueDate = a.Task.DueDate,
                        Priority = a.Task.Priority,
                        CreatorName = a.Task.Creator != null 
                            ? $"{a.Task.Creator.FirstName} {a.Task.Creator.LastName}" 
                            : "نامشخص",
                        TotalOperations = a.Task.TaskOperations.Count(o => !o.IsDeleted),
                        CompletedOperations = a.Task.TaskOperations.Count(o => !o.IsDeleted && o.IsCompleted)
                    })
                    .ToListAsync();

                if (!pendingTasks.Any())
                {
                    return "✅ همه تسک‌های شما تکمیل شده است!";
                }

                // ⭐ ساخت متن فرمت شده
                var result = new System.Text.StringBuilder();
                result.AppendLine("📌 تسک‌های در حال انجام شما:");
                result.AppendLine();

                int counter = 1;
                foreach (var task in pendingTasks)
                {
                    // ⭐ محاسبه درصد پیشرفت
                    int progressPercentage = task.TotalOperations > 0 
                        ? (task.CompletedOperations * 100) / task.TotalOperations 
                        : 0;

                    // ⭐ تعیین ایموجی اولویت
                    string priorityEmoji = task.Priority switch
                    {
                        3 => "🔴", // فوری
                        2 => "🟠", // بالا
                        1 => "🟡", // متوسط
                        _ => "🟢"  // عادی
                    };

                    string priorityText = task.Priority switch
                    {
                        3 => "فوری",
                        2 => "بالا",
                        1 => "متوسط",
                        _ => "عادی"
                    };

                    // ⭐ توضیح کوتاه (حداکثر 60 کاراکتر)
                    string shortDescription = string.IsNullOrEmpty(task.Description) 
                        ? "بدون توضیحات" 
                        : (task.Description.Length > 60 
                            ? task.Description.Substring(0, 60) + "..." 
                            : task.Description);

                    // ⭐ تاریخ‌ها
                    string startDatePersian = task.StartDate.HasValue 
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.StartDate.Value, "yyyy/MM/dd") 
                        : "---";
                    
                    string dueDatePersian = task.DueDate.HasValue 
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd") 
                        : "---";

                    // ⭐ ساخت متن هر تسک
                    result.AppendLine($"{counter}️⃣ {task.Title}");
                    result.AppendLine($"   📝 {shortDescription}");
                    result.AppendLine($"   📅 شروع: {startDatePersian} | 🔚 پایان: {dueDatePersian}");
                    result.AppendLine($"   👤 سازنده: {task.CreatorName} | {priorityEmoji} اولویت: {priorityText}");
                    result.AppendLine($"   📊 پیشرفت: {progressPercentage}% ({task.CompletedOperations}/{task.TotalOperations} عملیات)");
                    result.AppendLine();

                    counter++;
                }

                // ⭐ اضافه کردن آمار کلی
                result.AppendLine($"📊 جمع کل: {pendingTasks.Count} تسک در حال انجام");

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در ساخت لیست تسک‌های انجام نشده");
                return "⚠️ خطا در دریافت لیست تسک‌ها";
            }
        }

        /// <summary>
        /// بررسی اینکه رویداد مرتبط با تسک است یا خیر
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
                NotificationEventType.DailyTaskDigest => true,
                NotificationEventType.TaskWorkLog => true,
                _ => false
            };
        }

        /// <summary>
        /// ⭐⭐⭐ جایگزینی همه متغیرها در قالب (با پشتیبانی از {{Variable}} و {Variable})
        /// </summary>
        private string ReplaceAllPlaceholders(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template) || data == null || !data.Any())
                return template;

            var result = template;

            foreach (var kvp in data)
            {
                // جایگزینی فرمت {{Variable}}
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
                
                // جایگزینی فرمت {Variable}
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// جایگزینی متغیرهای قالب (متد قدیمی - حفظ شده برای سازگاری)
        /// </summary>
        [Obsolete("از ReplaceAllPlaceholders استفاده کنید")]
        private string ReplaceTemplatePlaceholders(string template, string title, string message, string actionUrl)
        {
            return template
                .Replace("{Title}", title)
                .Replace("{Message}", message)
                .Replace("{ActionUrl}", actionUrl)
                .Replace("{Date}", DateTime.Now.ToString("yyyy/MM/dd"))
                .Replace("{Time}", DateTime.Now.ToString("HH:mm"));
        }

        #endregion
    }
}