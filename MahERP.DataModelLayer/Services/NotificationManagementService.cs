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
        /// ⭐⭐⭐ ثبت اعلان برای قالب زمان‌بندی شده (Scheduled Template)
        /// این متد مستقیماً از قالب استفاده می‌کند و دوباره Query نمی‌زند
        /// ⚠️ برای اعلان‌های زمان‌بندی شده، اعلان سیستمی ثبت نمی‌شود
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
                    // ⭐⭐⭐ برای اعلان‌های زمان‌بندی شده، فقط ارسال مستقیم از طریق کانال
                    // بدون ثبت اعلان سیستمی
                    await ProcessSingleTemplateNotificationAsync(
                        template,
                        recipientUserId,
                        0 // ⭐ systemNotificationId = 0 (بدون اعلان سیستمی)
                    );
                    
                    totalNotifications++;
                }

                _logger.LogInformation($"✅ {totalNotifications} اعلان برای قالب {template.TemplateName} ارسال شد (بدون ثبت سیستمی)");
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
        /// </summary>
        private async Task ProcessSingleTemplateNotificationAsync(
            NotificationTemplate template,
            string recipientUserId,
            int systemNotificationId)
        {
            try
            {
                _logger.LogInformation($"📤 ارسال قالب {template.TemplateName} به کاربر {recipientUserId} از طریق کانال {template.Channel}");

                // ⭐⭐⭐ دریافت اطلاعات کامل برای جایگزینی متغیرها
                var templateData = await BuildTemplateDataAsync(
                    (NotificationEventType)template.NotificationEventType,
                    recipientUserId,
                    template.Subject ?? "اعلان",
                    template.MessageTemplate ?? "",
                    "",
                    systemNotificationId
                );

                // ⭐⭐⭐ جایگزینی متغیرها
                var finalMessage = ReplaceAllPlaceholders(template.MessageTemplate, templateData);
                var finalSubject = ReplaceAllPlaceholders(template.Subject ?? "", templateData);

                // ⭐⭐⭐ ارسال بر اساس کانال
                switch ((NotificationChannel)template.Channel)
                {
                    case NotificationChannel.Email:
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

                    default:
                        _logger.LogWarning($"⚠️ کانال نامعتبر: {template.Channel}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ProcessSingleTemplateNotificationAsync برای کاربر {recipientUserId}");
            }
        }

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
                // ⭐⭐⭐ FIX: اگر SenderUserId برابر "SYSTEM" است، آن را null کن
                string actualSenderId = (senderUserId == "SYSTEM" || string.IsNullOrEmpty(senderUserId)) 
                    ? null 
                    : senderUserId;

                var notification = new CoreNotification
                {
                    SystemId = 7, // Tasking
                    SystemName = "مدیریت تسک‌ها",
                    RecipientUserId = recipientUserId,
                    SenderUserId = actualSenderId, // ⭐ می‌تواند null باشد
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

                _logger.LogInformation($"✅ اعلان سیستمی #{notification.Id} برای {recipientUserId} ثبت شد");

                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در CreateSystemNotificationAsync برای {recipientUserId}");
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

                // ⭐⭐⭐ فقط اگر systemNotificationId مشخص شده، CoreNotificationDelivery ثبت کن
                if (coreNotificationId > 0)
                {
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
                }

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

                // ⭐⭐⭐ فقط اگر systemNotificationId مشخص شده، CoreNotificationDelivery ثبت کن
                if (coreNotificationId > 0)
                {
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
                }

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
        /// ارسال اعلان تلگرامی با دکمه‌های پویا
        /// </summary>
        public async Task SendTelegramNotificationAsync(
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

                // ⭐⭐⭐ فقط اگر systemNotificationId مشخص شده، CoreNotificationDelivery ثبت کن
                CoreNotificationDelivery delivery = null;
                if (coreNotificationId > 0)
                {
                    // ✅ ایجاد رکورد Delivery
                    delivery = new CoreNotificationDelivery
                    {
                        CoreNotificationId = coreNotificationId,
                        DeliveryMethod = 3, // Telegram
                        DeliveryAddress = user.TelegramChatId.Value.ToString(),
                        DeliveryStatus = 0,
                        AttemptCount = 0,
                        CreateDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.CoreNotificationDelivery_Tbl.Add(delivery);
                    await _context.SaveChangesAsync();
                }

                // ✅ ارسال مستقیم تلگرام
                var botToken = GetTelegramBotToken();

                if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_DEFAULT_BOT_TOKEN")
                {
                    _logger.LogWarning("⚠️ توکن تلگرام معتبر یافت نشد");
                    if (delivery != null)
                    {
                        delivery.DeliveryStatus = 3; // خطا
                        delivery.ErrorMessage = "توکن تلگرام تنظیم نشده است";
                        await _context.SaveChangesAsync();
                    }
                    return;
                }

                // ⭐⭐⭐ ساخت NotificationContext برای دکمه‌های پویا
                var notificationContext = await BuildNotificationContextAsync(coreNotificationId, userId);

                try
                {
                    // ⭐ ارسال با Context
                    await _telegramService.SendNotificationAsync(
                        message,
                        user.TelegramChatId.Value,
                        botToken,
                        notificationContext // ⭐ پارامتر جدید
                    );

                    // ✅ بروزرسانی وضعیت موفق
                    if (delivery != null)
                    {
                        delivery.DeliveryStatus = 1; // ارسال شده
                        delivery.DeliveryDate = DateTime.Now;
                    }

                    _logger.LogInformation($"✈️ پیام تلگرام با دکمه‌های پویا برای {user.UserName} (ChatId: {user.TelegramChatId.Value}) ارسال شد");
                }
                catch (Exception sendEx)
                {
                    // ✅ ثبت خطای ارسال
                    if (delivery != null)
                    {
                        delivery.DeliveryStatus = 3; // خطا
                        delivery.ErrorMessage = $"خطا در ارسال: {sendEx.Message}";
                    }

                    _logger.LogError(sendEx, $"❌ خطا در ارسال تلگرام به ChatId: {user.TelegramChatId.Value}");
                }

                if (delivery != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در SendTelegramNotificationAsync");
            }
        }
        /// <summary>
        /// ⭐⭐⭐ ساخت Context برای دکمه‌های پویای تلگرام
        /// </summary>
        private async Task<MahERP.CommonLayer.Repository.NotificationContext> BuildNotificationContextAsync(
            int coreNotificationId,
            string userId)
        {
            try
            {
                // دریافت اطلاعات اعلان
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

                // ⭐ تعیین نوع رویداد از ActionUrl یا Message
                byte? eventType = ExtractEventTypeFromNotification(notification.ActionUrl, notification.Message);

                // ⭐ بررسی اینکه آیا پیام شامل لیست تسک‌ها است
                bool hasPendingTasksList = notification.Message?.Contains("📌 تسک‌های در حال انجام شما") == true;

                // ⭐ استخراج TaskId اگر مرتبط با Task باشد
                string taskId = null;
                if (notification.RelatedRecordType == "Task" && !string.IsNullOrEmpty(notification.RelatedRecordId))
                {
                    taskId = notification.RelatedRecordId;
                }

                // ⭐ دریافت BaseUrl از تنظیمات
                // TODO: بهتر است از Configuration یا Settings دریافت شود
                string baseUrl = "https://resnaco.ir"; // ⭐ URL سایت شما

                return new MahERP.CommonLayer.Repository.NotificationContext
                {
                    BaseUrl = baseUrl,
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
        /// ⭐ استخراج نوع رویداد از URL یا محتوای پیام
        /// </summary>
        private byte? ExtractEventTypeFromNotification(string actionUrl, string message)
        {
            // ⭐ روش 1: تشخیص از URL
            if (!string.IsNullOrEmpty(actionUrl))
            {
                if (actionUrl.Contains("/Tasks/Details/"))
                    return 1; // احتمالاً TaskAssigned

                if (actionUrl.Contains("/Tasks/MyTasks"))
                    return 13; // احتمالاً DailyTaskDigest

                if (actionUrl.Contains("CompleteTask"))
                    return 3; // TaskDeadlineReminder
            }

            // ⭐ روش 2: تشخیص از محتوای پیام
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("📌 تسک‌های در حال انجام"))
                    return 13; // DailyTaskDigest

                if (message.Contains("تسک جدیدی") || message.Contains("اختصاص داده شد"))
                    return 1; // TaskAssigned

                if (message.Contains("یادآوری سررسید") || message.Contains("سررسید تسک"))
                    return 3; // TaskDeadlineReminder

                if (message.Contains("کامنت جدید"))
                    return 4; // TaskCommentAdded

                if (message.Contains("تکمیل شد"))
                    return 2; // TaskCompleted

                if (message.Contains("ویرایش"))
                    return 5; // TaskUpdated

                if (message.Contains("تغییر وضعیت"))
                    return 8; // TaskStatusChanged
            }

            return null; // نامشخص - دکمه‌های پیش‌فرض
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

        #region 🎨 متدهای عمومی - Public Utilities

        /// <summary>
        /// ⭐⭐⭐ رندر کردن قالب برای ارسال دستی (بدون ثبت اعلان سیستمی)
        /// این متد برای استفاده در Controller ها و ارسال دستی پیام است
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

                // ⭐ ساخت Dictionary داده‌ها
                var templateData = await BuildTemplateDataAsync(
                    (NotificationEventType)template.NotificationEventType,
                    recipientUserId,
                    defaultSubject,
                    defaultMessage,
                    "",
                    0 // بدون systemNotificationId
                );

                // ⭐ جایگزینی متغیرها
                var renderedSubject = ReplaceAllPlaceholders(defaultSubject ?? template.Subject ?? "", templateData);
                var renderedMessage = ReplaceAllPlaceholders(defaultMessage ?? template.MessageTemplate ?? "", templateData);

                return (renderedSubject, renderedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در رندر کردن قالب برای ارسال دستی");
                return (defaultSubject, defaultMessage);
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
                var telegramToken = _context.Settings_Tbl.FirstOrDefault().TelegramBotToken;
                // ⭐ استفاده مستقیم از توکن ثابت (در صورتی که جدول تنظیمات نداریم)
                // TODO: بهتر است از appsettings.json یا دیتابیس دریافت شود
                
                return telegramToken; // ⭐ توکن پیش‌فرض
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
                RecipientUserId = notification.RecipientUserId,
                SenderUserId = notification.SenderUserId,
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
                    var fullName = $"{recipient.FirstName} {recipient.LastName}".Trim();
                    
                    // ⭐ متغیرهای جدید
                    data["RecipientFirstName"] = recipient.FirstName ?? "";
                    data["RecipientLastName"] = recipient.LastName ?? "";
                    data["RecipientFullName"] = fullName;
                    data["RecipientUserName"] = recipient.UserName ?? "";
                    data["RecipientEmail"] = recipient.Email ?? "";
                    data["RecipientPhone"] = recipient.PhoneNumber ?? "";
                    
                    // ⭐ متغیرهای قدیمی (backward compatibility)
                    data["FirstName"] = recipient.FirstName ?? "";
                    data["LastName"] = recipient.LastName ?? "";
                    data["UserName"] = fullName;
                    data["Email"] = recipient.Email ?? "";
                    data["PhoneNumber"] = recipient.PhoneNumber ?? "";
                }

                // ⭐⭐⭐ برای اعلان‌های دوره‌ای (DailyTaskDigest): فقط لیست تسک‌ها
                if (eventType == NotificationEventType.DailyTaskDigest)
                {
                    var pendingTasksList = await BuildPendingTasksListAsync(recipientUserId);
                    data["PendingTasks"] = pendingTasksList;
                    
                    _logger.LogDebug($"✅ ساخت PendingTasks برای {recipientUserId}: {pendingTasksList.Length} کاراکتر");
                    
                    // برای اعلان دوره‌ای، اطلاعات تسک خاص نداریم
                    return data;
                }

                // ⭐⭐⭐ برای اعلان‌های مرتبط با تسک خاص
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
                            await PopulateTaskDataAsync(data, taskId, coreNotification.SenderUserId);
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
        /// ⭐⭐⭐ پر کردن اطلاعات تسک در Dictionary
        /// </summary>
        private async Task PopulateTaskDataAsync(
            Dictionary<string, string> data, 
            int taskId, 
            string senderUserId)
        {
            try
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
                        StakeholderName = t.Contact != null 
                            ? $"{t.Contact.FirstName} {t.Contact.LastName}" 
                            : (t.Organization != null ? t.Organization.DisplayName : ""),
                        BranchName = t.Branch != null ? t.Branch.Name : ""
                    })
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    _logger.LogWarning($"⚠️ تسک #{taskId} یافت نشد");
                    return;
                }

                // ⭐ پر کردن متغیرهای تسک
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
                
                // ⭐ Backward compatibility
                data["DueDate"] = data["TaskDueDate"];

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
                if (!string.IsNullOrEmpty(senderUserId) && senderUserId != "SYSTEM")
                {
                    var sender = await _context.Users
                        .Where(u => u.Id == senderUserId)
                        .Select(u => new { u.FirstName, u.LastName })
                        .FirstOrDefaultAsync();

                    if (sender != null)
                    {
                        data["SenderName"] = $"{sender.FirstName} {sender.LastName}".Trim();
                    }
                    else
                    {
                        data["SenderName"] = "سیستم";
                    }
                }
                else
                {
                    data["SenderName"] = "سیستم";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ خطا در دریافت اطلاعات تسک #{taskId}");
            }
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

                    // ⭐ توضیح کوتاه (حداقل 60 کاراکتر)
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
                return "خطا در دریافت لیست تسک‌ها";
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
                NotificationEventType.DailyTaskDigest => false, // این یک اعلان دوره‌ای است
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

        #endregion
    }
}