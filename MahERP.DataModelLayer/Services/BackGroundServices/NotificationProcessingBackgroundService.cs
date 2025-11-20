using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository.Tasking;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// سرویس پس‌زمینه برای پردازش صف اعلان‌ها
    /// تمام عملیات سنگین (دریافت کاربران، ارسال اعلان) اینجا انجام می‌شود
    /// </summary>
    public class NotificationProcessingBackgroundService : BackgroundService
    {
        private readonly ILogger<NotificationProcessingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        // ⭐ صف سبک‌وزن - فقط شناسه‌ها
        private static readonly ConcurrentQueue<NotificationQueueItem> _notificationQueue = new();

        public NotificationProcessingBackgroundService(
            ILogger<NotificationProcessingBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// افزودن اعلان به صف - فوق سبک و بدون Blocking
        /// </summary>
        public static void EnqueueTaskNotification(
            int taskId,
            string senderUserId,
            NotificationEventType eventType,
            byte priority = 1)
        {
            _notificationQueue.Enqueue(new NotificationQueueItem
            {
                TaskId = taskId,
                SenderUserId = senderUserId,
                EventType = eventType,
                Priority = priority,
                EnqueuedAt = DateTime.Now
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔔 Notification Processing Background Service شروع شد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessQueueAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // چک سریع‌تر
               }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در حلقه اصلی پردازش اعلان‌ها");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ Notification Processing Background Service متوقف شد");
        }

        private async Task ProcessQueueAsync(CancellationToken stoppingToken)
        {
            while (_notificationQueue.TryDequeue(out var item))
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    
                    var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagementService>();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // ⭐⭐⭐ دریافت اطلاعات تسک و کاربران
                    var task = await taskRepo.GetTaskByIdAsync(item.TaskId);
                    if (task == null)
                    {
                        _logger.LogWarning($"⚠️ تسک {item.TaskId} یافت نشد");
                        continue;
                    }

                    // ⭐⭐⭐ دریافت لیست دریافت‌کنندگان بر اساس نوع رویداد
                    var recipients = await GetRecipientsForEventAsync(
                        taskRepo, 
                        task, 
                        item.SenderUserId, 
                        item.EventType
                    );

                    if (!recipients.Any())
                    {
                        _logger.LogDebug($"ℹ️ دریافت‌کننده‌ای برای {item.EventType} یافت نشد");
                        continue;
                    }

                    // ⭐⭐⭐ ساخت متن اعلان با جایگزینی پارامترها برای هر کاربر
                    foreach (var recipientUserId in recipients)
                    {
                        var (title, message) = await BuildNotificationContentAsync(
                            context,
                            task, 
                            item.EventType, 
                            recipientUserId,
                            item.SenderUserId
                        );

                        // ⭐ ارسال اعلان برای هر کاربر
                        var count = await notificationService.ProcessEventNotificationAsync(
                            item.EventType,
                            new List<string> { recipientUserId }, // فقط یک کاربر
                            item.SenderUserId,
                            title,
                            message,
                            $"/TaskingArea/Tasks/Details/{task.Id}",
                            task.Id.ToString(),
                            "Task",
                            item.Priority
                        );

                        _logger.LogInformation(
                            $"✅ اعلان {item.EventType} برای تسک #{task.Id} ({task.Title}) به {recipientUserId} ارسال شد");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در پردازش اعلان {item.EventType}");
                    
                    // ⭐ Retry محدود (3 بار)
                    if (item.RetryCount < 3)
                    {
                        item.RetryCount++;
                        _notificationQueue.Enqueue(item);
                        _logger.LogWarning($"🔄 تلاش مجدد {item.RetryCount}/3");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }

                // تأخیر بین پردازش‌ها
                await Task.Delay(200, stoppingToken);
            }
        }

        /// <summary>
        /// دریافت لیست دریافت‌کنندگان بر اساس نوع رویداد
        /// </summary>
        private async Task<List<string>> GetRecipientsForEventAsync(
            ITaskRepository taskRepo,
            MahERP.DataModelLayer.Entities.TaskManagement.Tasks task,
            string senderUserId,
            NotificationEventType eventType)
        {
            var recipients = new List<string>();

            switch (eventType)
            {
                case NotificationEventType.TaskAssigned:
                case NotificationEventType.TaskReassigned:
                    // ⭐ اعضای تسک (بدون سازنده و sender)
                    var assignees = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(assignees.Where(id => id != senderUserId && id != task.CreatorUserId));
                    break;

                case NotificationEventType.TaskCompleted:
                    // ⭐ فقط سازنده تسک (بدون sender)
                    if (!string.IsNullOrEmpty(task.CreatorUserId) && 
                        task.CreatorUserId != senderUserId)
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;

                case NotificationEventType.TaskCommentAdded:
                    // ⭐ همه افراد مرتبط با تسک (بدون sender)
                    var relatedUsers = await taskRepo.GetTaskRelatedUserIdsAsync(task.Id);
                    recipients.AddRange(relatedUsers.Where(id => id != senderUserId));
                    break;

                case NotificationEventType.TaskUpdated:
                    // ⭐ اعضا + سازنده (بدون sender)
                    var assignedUsers = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(assignedUsers.Where(id => id != senderUserId));
                    
                    if (!string.IsNullOrEmpty(task.CreatorUserId) && 
                        task.CreatorUserId != senderUserId)
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;

                case NotificationEventType.TaskDeadlineReminder:
                    // ⭐⭐⭐ EXCEPTION: یادآوری‌های زمان‌بندی شده - همه اعضا (بدون فیلتر sender)
                    // این رویداد از طریق Background Service اجرا می‌شود نه توسط کاربر
                    var allAssignees = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(allAssignees);
                    break;

                case NotificationEventType.TaskDeleted:
                    // ⭐ همه افراد مرتبط (بدون sender)
                    var allRelated = await taskRepo.GetTaskRelatedUserIdsAsync(task.Id);
                    recipients.AddRange(allRelated.Where(id => id != senderUserId));
                    break;

                case NotificationEventType.TaskStatusChanged:
                case NotificationEventType.TaskWorkLog:
                    // ⭐ سازنده + اعضا (بدون sender)
                    var members = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(members.Where(id => id != senderUserId));
                    
                    if (!string.IsNullOrEmpty(task.CreatorUserId) && 
                        task.CreatorUserId != senderUserId)
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;
            }

            // ⭐ حذف تکراری‌ها و برگرداندن لیست یکتا
            return recipients.Distinct().ToList();
        }

        /// <summary>
        /// ⭐⭐⭐ ساخت محتوای اعلان با جایگزینی پارامترها
        /// </summary>
        private async Task<(string title, string message)> BuildNotificationContentAsync(
            AppDbContext context,
            MahERP.DataModelLayer.Entities.TaskManagement.Tasks task,
            NotificationEventType eventType,
            string recipientUserId,
            string senderUserId)
        {
            // ⭐ دریافت اطلاعات کاربر دریافت‌کننده
            var recipient = await context.Users
                .Where(u => u.Id == recipientUserId)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            // ⭐ دریافت اطلاعات کاربر ارسال‌کننده
            var sender = !string.IsNullOrEmpty(senderUserId) 
                ? await context.Users
                    .Where(u => u.Id == senderUserId)
                    .Select(u => new { u.FirstName, u.LastName })
                    .FirstOrDefaultAsync()
                : null;

            // ⭐ متغیرهای مشترک
            var taskTitle = task.Title ?? "تسک";
            var taskCode = task.TaskCode ?? "";
            var recipientName = recipient != null ? $"{recipient.FirstName} {recipient.LastName}".Trim() : "کاربر";
            var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "سیستم";
            var currentDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd");
            var currentTime = DateTime.Now.ToString("HH:mm");
            var dueDate = task.DueDate.HasValue 
                ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd")
                : "نامشخص";

            // ⭐⭐⭐ ساخت عنوان و پیام بر اساس نوع رویداد
            return eventType switch
            {
                NotificationEventType.TaskAssigned => (
                    $"تسک جدید برای {recipientName}",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} به شما اختصاص داده شد.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}\n" +
                    $"⏰ مهلت: {dueDate}"
                ),

                NotificationEventType.TaskCompleted => (
                    $"تسک '{taskTitle}' تکمیل شد",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} تکمیل شده است.\n\n" +
                    $"📅 تاریخ تکمیل: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                NotificationEventType.TaskCommentAdded => (
                    $"کامنت جدید در تسک '{taskTitle}'",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"{senderName} در تسک '{taskTitle}' (کد: {taskCode}) کامنت جدیدی ثبت کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                NotificationEventType.TaskUpdated => (
                    $"تسک '{taskTitle}' بروزرسانی شد",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} ویرایش شده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                NotificationEventType.TaskDeadlineReminder => (
                    $"⏰ یادآوری مهلت تسک '{taskTitle}' برای {recipientName}",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"🔔 یادآوری جهت بررسی تسک '{taskTitle}' (کد: {taskCode})\n\n" +
                    $"⚠️ مهلت این تسک در تاریخ {dueDate} به پایان می‌رسد.\n\n" +
                    $"📅 تاریخ یادآوری: {currentDate}\n" +
                    $"🕐 ساعت یادآوری: {currentTime}\n\n" +
                    $"لطفاً نسبت به انجام آن اقدام فرمایید."
                ),

                NotificationEventType.TaskDeleted => (
                    $"تسک '{taskTitle}' حذف شد",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} حذف شده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                NotificationEventType.TaskStatusChanged => (
                    $"تغییر وضعیت تسک '{taskTitle}'",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"وضعیت تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} تغییر کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                NotificationEventType.TaskReassigned => (
                    $"تسک '{taskTitle}' مجدداً اختصاص داده شد",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) مجدداً توسط {senderName} به شما تخصیص داده شد.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}\n" +
                    $"⏰ مهلت: {dueDate}"
                ),

                NotificationEventType.TaskWorkLog => (
                    $"گزارش کار جدید در تسک '{taskTitle}'",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"{senderName} گزارش کار جدیدی در تسک '{taskTitle}' (کد: {taskCode}) ثبت کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                ),

                _ => (
                    $"اعلان جدید از تسک '{taskTitle}'",
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"رویداد جدیدی در تسک '{taskTitle}' (کد: {taskCode}) رخ داده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
                )
            };
        }

        /// <summary>
        /// ساخت محتوای اعلان بر اساس نوع رویداد (متد قدیمی - حذف شود)
        /// </summary>
        [Obsolete("از BuildNotificationContentAsync استفاده کنید")]
        private (string title, string message) BuildNotificationContent(
            MahERP.DataModelLayer.Entities.TaskManagement.Tasks task,
            NotificationEventType eventType)
        {
            var taskTitle = task.Title ?? "تسک";
            var taskCode = task.TaskCode ?? "";

            return eventType switch
            {
                NotificationEventType.TaskAssigned => (
                    "تسک جدید اختصاص داده شد",
                    $"تسک '{taskTitle}' ({taskCode}) به شما اختصاص داده شده است"
                ),

                NotificationEventType.TaskCompleted => (
                    "تسک تکمیل شد",
                    $"تسک '{taskTitle}' ({taskCode}) توسط یکی از اعضا تکمیل شد"
                ),

                NotificationEventType.TaskCommentAdded => (
                    "کامنت جدید در تسک",
                    $"کامنت جدیدی در تسک '{taskTitle}' ({taskCode}) ثبت شد"
                ),

                NotificationEventType.TaskUpdated => (
                    "تسک بروزرسانی شد",
                    $"تسک '{taskTitle}' ({taskCode}) ویرایش شده است"
                ),

                NotificationEventType.TaskDeadlineReminder => (
                    "یادآوری مهلت تسک",
                    $"مهلت تسک '{taskTitle}' ({taskCode}) نزدیک است"
                ),

                NotificationEventType.TaskDeleted => (
                    "تسک حذف شد",
                    $"تسک '{taskTitle}' ({taskCode}) حذف شده است"
                ),

                NotificationEventType.TaskStatusChanged => (
                    "تغییر وضعیت تسک",
                    $"وضعیت تسک '{taskTitle}' ({taskCode}) تغییر کرد"
                ),

                NotificationEventType.TaskReassigned => (
                    "تسک مجدداً اختصاص داده شد",
                    $"تسک '{taskTitle}' ({taskCode}) به شما تخصیص داده شد"
                ),

                _ => ("اعلان جدید", $"رویداد جدید در تسک '{taskTitle}' ({taskCode})")
            };
        }

        /// <summary>
        /// آیتم صف - سبک‌وزن
        /// </summary>
        private class NotificationQueueItem
        {
            public int TaskId { get; set; }
            public string SenderUserId { get; set; }
            public NotificationEventType EventType { get; set; }
            public byte Priority { get; set; }
            public DateTime EnqueuedAt { get; set; }
            public int RetryCount { get; set; } = 0;
        }
    }
}