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

namespace MahERP.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // چک هر 1 دقیقه

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckUserNotifications();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در Background Service نوتیفیکیشن");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // در صورت خطا 5 دقیقه صبر کن
                }
            }
        }

        private async Task CheckUserNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUsers>>();

            // دریافت همه کاربران فعال
            var activeUsers = await userManager.Users
                .Where(u => u.IsActive && u.EmailConfirmed)
                .ToListAsync();

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

                // 1. چک تسک‌های جدید
                var newTasks = await GetNewTasksForUser(userId, lastCheckTime, taskRepository);
                if (newTasks.Any())
                {
                    await notificationService.CreateTaskNotificationAsync(userId, newTasks);
                }

                // 2. چک یادآوری‌های جدید
                var newReminders = await GetNewRemindersForUser(userId, lastCheckTime, taskRepository);
                if (newReminders.Any())
                {
                    await notificationService.CreateReminderNotificationAsync(userId, newReminders);
                }

                // بروزرسانی آخرین زمان چک
                await UpdateLastCheckTime(userId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در چک نوتیفیکیشن برای کاربر {userId}");
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

            // فقط تسک‌هایی که بعد از آخرین چک ایجاد شده‌اند
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

            // فقط یادآوری‌هایی که بعد از آخرین چک ایجاد شده‌اند یا فعال شده‌اند
            return reminders
                .Where(r => !r.IsRead && 
                           (r.ScheduledDateTime > lastCheckTime || 
                            (r.IsSent && r.ScheduledDateTime <= DateTime.Now)))
                .ToList();
        }

        private async Task<DateTime> GetLastCheckTime(string userId)
        {
            // از کش یا دیتابیس دریافت آخرین زمان چک
            // فعلاً 10 دقیقه قبل را برمی‌گردانیم
            return DateTime.Now.AddMinutes(-10);
        }

        private async Task UpdateLastCheckTime(string userId, DateTime checkTime)
        {
            // ذخیره آخرین زمان چک در کش یا دیتابیس
            // TODO: پیاده‌سازی
        }
    }
}