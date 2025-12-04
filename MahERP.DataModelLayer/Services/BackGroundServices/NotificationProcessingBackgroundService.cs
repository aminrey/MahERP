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
                    // ⭐⭐⭐ FIX: یادآوری‌های زمان‌بندی شده - همه اعضا + سازنده (بدون فیلتر sender)
                    // این رویداد از طریق Background Service اجرا می‌شود نه توسط کاربر
                    var allAssignees = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(allAssignees);
                    
                    // ⭐ اضافه کردن سازنده تسک (اگر از لیست کاربران منتصب نیست)
                    if (!string.IsNullOrEmpty(task.CreatorUserId) &&
                        !recipients.Contains(task.CreatorUserId))
                    {
                        recipients.Add(task.CreatorUserId);
                    }
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
        /// این متد محتوا رو از قالب می‌گیره یا از محتوای پیش‌فرض استفاده می‌کنه
        /// </summary>
        private async Task<(string title, string message)> BuildNotificationContentAsync(
            AppDbContext context,
            MahERP.DataModelLayer.Entities.TaskManagement.Tasks task,
            NotificationEventType eventType,
            string recipientUserId,
            string senderUserId)
        {
            // ⭐⭐⭐ ساخت Dictionary داده‌ها برای جایگزینی
            var templateData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // ⭐ اطلاعات پایه
                templateData["Date"] = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd");
                templateData["Time"] = DateTime.Now.ToString("HH:mm");

                // ⭐ دریافت اطلاعات کاربر دریافت‌کننده
                var recipient = await context.Users
                    .Where(u => u.Id == recipientUserId)
                    .Select(u => new { u.FirstName, u.LastName, u.UserName, u.Email, u.PhoneNumber })
                    .FirstOrDefaultAsync();

                if (recipient != null)
                {
                    var fullName = $"{recipient.FirstName} {recipient.LastName}".Trim();
                    
                    templateData["RecipientFirstName"] = recipient.FirstName ?? "";
                    templateData["RecipientLastName"] = recipient.LastName ?? "";
                    templateData["RecipientFullName"] = fullName;
                    templateData["RecipientUserName"] = recipient.UserName ?? "";
                    templateData["RecipientEmail"] = recipient.Email ?? "";
                    templateData["RecipientPhone"] = recipient.PhoneNumber ?? "";
                    
                    // Backward compatibility
                    templateData["FirstName"] = recipient.FirstName ?? "";
                    templateData["LastName"] = recipient.LastName ?? "";
                    templateData["UserName"] = fullName;
                    templateData["Email"] = recipient.Email ?? "";
                    templateData["PhoneNumber"] = recipient.PhoneNumber ?? "";
                }

                // ⭐ اطلاعات تسک
                templateData["TaskTitle"] = task.Title ?? "";
                templateData["TaskCode"] = task.TaskCode ?? "";
                templateData["TaskDescription"] = task.Description ?? "";
                templateData["TaskStartDate"] = task.StartDate.HasValue 
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.StartDate.Value, "yyyy/MM/dd") 
                    : "";
                templateData["TaskDueDate"] = task.DueDate.HasValue 
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd") 
                    : "";
                templateData["TaskPriority"] = task.Priority switch { 
                    0 => "عادی", 
                    1 => "متوسط", 
                    2 => "بالا", 
                    3 => "فوری", 
                    _ => "نامشخص" 
                };
                
                // Backward compatibility
                templateData["DueDate"] = templateData["TaskDueDate"];

                // ⭐ دریافت اطلاعات کاربر ارسال‌کننده
                if (!string.IsNullOrEmpty(senderUserId) && senderUserId != "SYSTEM")
                {
                    var sender = await context.Users
                        .Where(u => u.Id == senderUserId)
                        .Select(u => new { u.FirstName, u.LastName })
                        .FirstOrDefaultAsync();

                    templateData["SenderName"] = sender != null 
                        ? $"{sender.FirstName} {sender.LastName}".Trim() 
                        : "سیستم";
                }
                else
                {
                    templateData["SenderName"] = "سیستم";
                }

                // ⭐⭐⭐ SPECIAL CASE: برای TaskDeadlineReminder از TaskReminderSchedule استفاده کن
                if (eventType == NotificationEventType.TaskDeadlineReminder || 
                    eventType == NotificationEventType.CustomTaskReminder) // ⭐⭐⭐ اضافه شد
                {
                    var reminderSchedule = await context.TaskReminderSchedule_Tbl
                        .Where(s => s.TaskId == task.Id && s.IsActive)
                        .OrderByDescending(s => s.LastExecuted)
                        .FirstOrDefaultAsync();

                    if (reminderSchedule != null && !string.IsNullOrWhiteSpace(reminderSchedule.Title))
                    {
                        // ⭐ استفاده از عنوان و توضیحات از Schedule
                        string title = ReplaceVariables(reminderSchedule.Title, templateData);
                        string message = reminderSchedule.Description ?? "";
                        
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            // پیام پیش‌فرض
                            message = $"🔔 یادآوری برای تسک {{{{TaskTitle}}}} (کد: {{{{TaskCode}}}})\n\n" +
                                     $"⚠️ مهلت پایان: {{{{TaskDueDate}}}}\n\n" +
                                     $"لطفاً نسبت به انجام آن اقدام فرمایید.";
                        }
                        
                        message = ReplaceVariables(message, templateData);
                        templateData["Title"] = title;
                        templateData["Message"] = message;
                        
                        return (title, message);
                    }
                }

                // ⭐⭐⭐ سعی کن قالب مربوط به این رویداد رو پیدا کنی (فقط غیرزمان‌بندی)
                var template = await context.NotificationTemplate_Tbl
                    .Where(t => t.IsActive && 
                               t.NotificationEventType == (byte)eventType &&
                               !t.IsScheduled) // ⭐⭐⭐ FIX: فقط قالب‌های غیرزمان‌بندی
                    .OrderByDescending(t => t.UsageCount)
                    .FirstOrDefaultAsync();

                if (template != null)
                {
                    // ⭐⭐⭐ استفاده از قالب
                    string title = ReplaceVariables(template.Subject ?? GetDefaultTitle(eventType, templateData), templateData);
                    string message = ReplaceVariables(template.MessageTemplate ?? GetDefaultMessage(eventType, templateData), templateData);
                    
                    templateData["Title"] = title;
                    templateData["Message"] = message;
                    
                    return (title, message);
                }

                // ⭐ پیش‌فرض
                var defaultTitle = GetDefaultTitle(eventType, templateData);
                var defaultMessage = GetDefaultMessage(eventType, templateData);
                
                templateData["Title"] = defaultTitle;
                templateData["Message"] = defaultMessage;
                
                return (defaultTitle, defaultMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در BuildNotificationContentAsync");
                
                // Fallback
                return ($"اعلان مربوط به {task?.Title ?? "تسک"}", 
                       $"یک رویداد جدید در تسک {task?.TaskCode ?? ""} رخ داده است.");
            }
        }

        /// <summary>
        /// ⭐⭐⭐ جایگزینی متغیرها با مقادیر واقعی
        /// </summary>
        private string ReplaceVariables(string text, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(text) || data == null || !data.Any())
                return text;

            var result = text;

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
        /// ⭐⭐⭐ دریافت عنوان پیش‌فرض بر اساس نوع رویداد
        /// </summary>
        private string GetDefaultTitle(NotificationEventType eventType, Dictionary<string, string> data)
        {
            return eventType switch
            {
                NotificationEventType.TaskAssigned => $"تسک جدید برای {data["RecipientFullName"]}",
                NotificationEventType.TaskCompleted => $"تسک '{data["TaskTitle"]}' تکمیل شد",
                NotificationEventType.TaskCommentAdded => $"کامنت جدید در تسک '{data["TaskTitle"]}'",
                NotificationEventType.TaskUpdated => $"تسک '{data["TaskTitle"]}' بروزرسانی شد",
                NotificationEventType.TaskDeadlineReminder => $"⏰ یادآوری مهلت تسک '{data["TaskTitle"]}'",
                NotificationEventType.TaskDeleted => $"تسک '{data["TaskTitle"]}' حذف شد",
                NotificationEventType.TaskStatusChanged => $"تغییر وضعیت تسک '{data["TaskTitle"]}'",
                NotificationEventType.TaskReassigned => $"تسک '{data["TaskTitle"]}' مجدداً اختصاص داده شد",
                NotificationEventType.TaskWorkLog => $"گزارش کار جدید در تسک '{data["TaskTitle"]}'",
                _ => $"اعلان جدید از تسک '{data["TaskTitle"]}'"
            };
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت پیام پیش‌فرض بر اساس نوع رویداد
        /// </summary>
        private string GetDefaultMessage(NotificationEventType eventType, Dictionary<string, string> data)
        {
            var recipientName = data["RecipientFullName"];
            var taskTitle = data["TaskTitle"];
            var taskCode = data["TaskCode"];
            var senderName = data["SenderName"];
            var currentDate = data["Date"];
            var currentTime = data["Time"];
            var dueDate = data["TaskDueDate"];

            return eventType switch
            {
                NotificationEventType.TaskAssigned => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} به شما اختصاص داده شد.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}\n" +
                    $"⏰ مهلت: {dueDate}",

                NotificationEventType.TaskCompleted => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} تکمیل شده است.\n\n" +
                    $"📅 تاریخ تکمیل: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                NotificationEventType.TaskCommentAdded => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"{senderName} در تسک '{taskTitle}' (کد: {taskCode}) کامنت جدیدی ثبت کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                NotificationEventType.TaskUpdated => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} ویرایش شده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                NotificationEventType.TaskDeadlineReminder => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"🔔 یادآوری جهت بررسی تسک '{taskTitle}' (کد: {taskCode})\n\n" +
                    $"⚠️ مهلت این تسک در تاریخ {dueDate} به پایان می‌رسد.\n\n" +
                    $"📅 تاریخ یادآوری: {currentDate}\n" +
                    $"🕐 ساعت یادآوری: {currentTime}\n\n" +
                    $"لطفاً نسبت به انجام آن اقدام فرمایید.",

                NotificationEventType.TaskDeleted => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} حذف شده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                NotificationEventType.TaskStatusChanged => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"وضعیت تسک '{taskTitle}' (کد: {taskCode}) توسط {senderName} تغییر کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                NotificationEventType.TaskReassigned => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"تسک '{taskTitle}' (کد: {taskCode}) مجدداً توسط {senderName} به شما تخصیص داده شد.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}\n" +
                    $"⏰ مهلت: {dueDate}",

                NotificationEventType.TaskWorkLog => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"{senderName} گزارش کار جدیدی در تسک '{taskTitle}' (کد: {taskCode}) ثبت کرده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}",

                _ => 
                    $"سلام {recipientName} عزیز،\n\n" +
                    $"رویداد جدیدی در تسک '{taskTitle}' (کد: {taskCode}) رخ داده است.\n\n" +
                    $"📅 تاریخ: {currentDate}\n" +
                    $"🕐 ساعت: {currentTime}"
            };
        }

        /// <summary>
        /// ⭐⭐⭐ ساخت محتوای یادآوری مهلت تسک با استفاده از TaskReminderSchedule
        /// [DEPRECATED] - این متد دیگه استفاده نمیشه، محتوا از BuildNotificationContentAsync میاد
        /// </summary>
        [Obsolete("از BuildNotificationContentAsync استفاده کنید")]
        private async Task<(string title, string message)> BuildTaskDeadlineReminderAsync(
            AppDbContext context,
            MahERP.DataModelLayer.Entities.TaskManagement.Tasks task,
            string recipientName,
            string taskTitle,
            string taskCode,
            string dueDate,
            string currentDate,
            string currentTime)
        {
            // این متد دیگه استفاده نمیشه
            return ("", "");
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