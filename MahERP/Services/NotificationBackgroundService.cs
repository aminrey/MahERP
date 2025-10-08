using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Identity;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MahERP.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        
        // ⭐ صف thread-safe برای نوتیفیکیشن‌های فوری
        private readonly ConcurrentQueue<TaskNotificationJob> _urgentQueue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// اضافه کردن نوتیفیکیشن فوری به صف
        /// </summary>
        public void EnqueueTaskCreation(int taskId, string creatorUserId, List<string> assignedUserIds)
        {
            _urgentQueue.Enqueue(new TaskNotificationJob
            {
                TaskId = taskId,
                CreatorUserId = creatorUserId,
                AssignedUserIds = assignedUserIds,
                EnqueuedAt = DateTime.Now
            });
            
            _signal.Release(); // سیگنال به worker thread
            _logger.LogInformation($"نوتیفیکیشن تسک {taskId} به صف اضافه شد - تعداد در صف: {_urgentQueue.Count}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Notification Background Service شروع شد");

            // ⭐ دو task موازی: urgent notifications + scheduled checks
            var urgentTask = ProcessUrgentNotificationsAsync(stoppingToken);
            var scheduledTask = ProcessScheduledNotificationsAsync(stoppingToken);

            await Task.WhenAll(urgentTask, scheduledTask);
            
            _logger.LogInformation("⛔ Notification Background Service متوقف شد");
        }

        /// <summary>
        /// پردازش نوتیفیکیشن‌های فوری
        /// </summary>
        private async Task ProcessUrgentNotificationsAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("▶️ Urgent Notifications Processor شروع شد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // صبر برای سیگنال یا timeout
                    await _signal.WaitAsync(TimeSpan.FromSeconds(30), stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<TaskNotificationService>();

                    int processedCount = 0;
                    while (_urgentQueue.TryDequeue(out var job))
                    {
                        try
                        {
                            await notificationService.NotifyTaskCreatedAsync(
                                job.TaskId, job.CreatorUserId, job.AssignedUserIds);
                            
                            processedCount++;
                            _logger.LogInformation(
                                $"✅ نوتیفیکیشن فوری تسک {job.TaskId} ارسال شد - تعداد کاربران: {job.AssignedUserIds.Count}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ خطا در ارسال نوتیفیکیشن فوری تسک {job.TaskId}");
                        }
                    }

                    if (processedCount > 0)
                    {
                        _logger.LogInformation($"📨 {processedCount} نوتیفیکیشن فوری پردازش شد");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("⛔ Urgent Notifications Processor متوقف شد");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در ProcessUrgentNotificationsAsync");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        /// <summary>
        /// پردازش نوتیفیکیشن‌های زمان‌بندی شده
        /// </summary>
        private async Task ProcessScheduledNotificationsAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("▶️ Scheduled Notifications Processor شروع شد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("🔍 شروع چک نوتیفیکیشن‌های زمان‌بندی شده...");
                    await CheckUserNotifications();
                    _logger.LogDebug("✅ چک نوتیفیکیشن‌های زمان‌بندی شده کامل شد");
                    
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در ProcessScheduledNotificationsAsync");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
            
            _logger.LogInformation("⛔ Scheduled Notifications Processor متوقف شد");
        }

        private async Task CheckUserNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUsers>>();

            var activeUsers = await userManager.Users
                .Where(u => u.IsActive && u.EmailConfirmed)
                .ToListAsync();

            _logger.LogDebug($"📋 چک نوتیفیکیشن برای {activeUsers.Count} کاربر فعال");

            foreach (var user in activeUsers)
            {
                await CheckUserTasksAndReminders(user.Id, taskRepository, notificationService);
            }
        }

        private async Task CheckUserTasksAndReminders(
            string userId, 
            ITaskRepository taskRepository, 
            INotificationService notificationService)
        {
            try
            {
                var now = DateTime.Now;
                var lastCheckTime = await GetLastCheckTime(userId);

                // چک تسک‌های جدید
                var newTasks = await GetNewTasksForUser(userId, lastCheckTime, taskRepository);
                if (newTasks.Any())
                {
                    await notificationService.CreateTaskNotificationAsync(userId, newTasks);
                    _logger.LogInformation($"📬 {newTasks.Count} تسک جدید برای کاربر {userId}");
                }

                // چک یادآوری‌های جدید
                var newReminders = await GetNewRemindersForUser(userId, lastCheckTime, taskRepository);
                if (newReminders.Any())
                {
                    await notificationService.CreateReminderNotificationAsync(userId, newReminders);
                    _logger.LogInformation($"🔔 {newReminders.Count} یادآوری جدید برای کاربر {userId}");
                }

                await UpdateLastCheckTime(userId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در چک نوتیفیکیشن برای کاربر {userId}");
            }
        }

        private async Task<List<TaskViewModel>> GetNewTasksForUser(
            string userId, 
            DateTime lastCheckTime, 
            ITaskRepository taskRepository)
        {
            var userTasks = await taskRepository.GetUserTasksComprehensiveAsync(
                userId,
                includeCreatedTasks: false,
                includeAssignedTasks: true,
                includeSupervisedTasks: false,
                includeDeletedTasks: false
            );

            return userTasks.AssignedTasks
                .Where(t => t.CreateDate > lastCheckTime && t.CreatorUserId != userId)
                .ToList();
        }

        private async Task<List<TaskReminderItemViewModel>> GetNewRemindersForUser(
            string userId, 
            DateTime lastCheckTime, 
            ITaskRepository taskRepository)
        {
            var reminders = await taskRepository.GetDashboardRemindersAsync(userId, maxResults: 50, daysAhead: 7);

            return reminders
                .Where(r => !r.IsRead && 
                           (r.ScheduledDateTime > lastCheckTime || 
                            (r.IsSent && r.ScheduledDateTime <= DateTime.Now)))
                .ToList();
        }

        private async Task<DateTime> GetLastCheckTime(string userId)
        {
            // TODO: پیاده‌سازی با Cache یا Database
            return DateTime.Now.AddMinutes(-10);
        }

        private async Task UpdateLastCheckTime(string userId, DateTime checkTime)
        {
            // TODO: پیاده‌سازی ذخیره در Cache یا Database
        }

        /// <summary>
        /// کلاس کمکی برای نگهداری اطلاعات نوتیفیکیشن
        /// </summary>
        private class TaskNotificationJob
        {
            public int TaskId { get; set; }
            public string CreatorUserId { get; set; }
            public List<string> AssignedUserIds { get; set; }
            public DateTime EnqueuedAt { get; set; }
        }
    }
}