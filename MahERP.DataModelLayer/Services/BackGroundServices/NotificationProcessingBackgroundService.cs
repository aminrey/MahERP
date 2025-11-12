using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository.Tasking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

                    // ⭐⭐⭐ ساخت متن اعلان
                    var (title, message) = BuildNotificationContent(task, item.EventType);

                    // ⭐ ارسال اعلان
                    var count = await notificationService.ProcessEventNotificationAsync(
                        item.EventType,
                        recipients,
                        item.SenderUserId,
                        title,
                        message,
                        $"/TaskingArea/Tasks/Details/{task.Id}",
                        task.Id.ToString(),
                        "Task",
                        item.Priority
                    );

                    _logger.LogInformation(
                        $"✅ اعلان {item.EventType} برای تسک #{task.Id} به {count} کاربر ارسال شد");
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
                    // ⭐ اعضای تسک (بدون سازنده)
                    var assignees = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(assignees.Where(id => id != senderUserId));
                    break;

                case NotificationEventType.TaskCompleted:
                    // ⭐ فقط سازنده تسک
                    if (!string.IsNullOrEmpty(task.CreatorUserId) && task.CreatorUserId != senderUserId)
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;

                case NotificationEventType.TaskCommentAdded:
                    // ⭐ همه افراد مرتبط با تسک
                    var relatedUsers = await taskRepo.GetTaskRelatedUserIdsAsync(task.Id);
                    recipients.AddRange(relatedUsers.Where(id => id != senderUserId));
                    break;

                case NotificationEventType.TaskUpdated:
                    // ⭐ اعضا + سازنده
                    var assignedUsers = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(assignedUsers);
                    
                    if (!string.IsNullOrEmpty(task.CreatorUserId) && task.CreatorUserId != senderUserId)
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;

                case NotificationEventType.TaskDeadlineReminder:
                    // ⭐ همه اعضا (بدون فیلتر سازنده)
                    var allAssignees = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(allAssignees);
                    break;

                case NotificationEventType.TaskDeleted:
                    // ⭐ همه افراد مرتبط
                    var allRelated = await taskRepo.GetTaskRelatedUserIdsAsync(task.Id);
                    recipients.AddRange(allRelated.Where(id => id != senderUserId));
                    break;

                case NotificationEventType.TaskStatusChanged:
                    // ⭐ سازنده + اعضا
                    var members = await taskRepo.GetTaskAssignedUserIdsAsync(task.Id);
                    recipients.AddRange(members);
                    
                    if (!string.IsNullOrEmpty(task.CreatorUserId))
                    {
                        recipients.Add(task.CreatorUserId);
                    }
                    break;
            }

            return recipients.Distinct().ToList();
        }

        /// <summary>
        /// ساخت محتوای اعلان بر اساس نوع رویداد
        /// </summary>
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