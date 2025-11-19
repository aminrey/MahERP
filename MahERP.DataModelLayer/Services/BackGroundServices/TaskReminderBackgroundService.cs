using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// ⏰ سرویس پس‌زمینه برای پردازش یادآوری‌های تسک
    /// هر دقیقه Schedule های آماده را چک می‌کند و TaskReminderEvent تولید می‌کند
    /// </summary>
    public class TaskReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<TaskReminderBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // ⭐⭐⭐ TimeZone ایران
        private static readonly TimeZoneInfo IranTimeZone = 
            TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        public TaskReminderBackgroundService(
            ILogger<TaskReminderBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⏰ Task Reminder Background Service شروع شد");
            _logger.LogInformation($"🌍 TimeZone: {IranTimeZone.DisplayName}");

            // صبر 30 ثانیه تا سیستم بوت شود
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTaskRemindersAsync(stoppingToken);

                    // ⭐ هر 1 دقیقه چک کن
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در حلقه اصلی پردازش یادآوری‌های تسک");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ Task Reminder Background Service متوقف شد");
        }

        /// <summary>
        /// پردازش یادآوری‌های تسک - تولید Event ها و ارسال Notification
        /// </summary>
        private async Task ProcessTaskRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagementService>();

            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            
            _logger.LogDebug($"🕐 چک یادآوری‌ها - زمان: {nowIran:yyyy-MM-dd HH:mm:ss}");

            // ⭐⭐⭐ دریافت Schedule های فعال و آماده برای اجرا
            var dueSchedules = await context.TaskReminderSchedule_Tbl
                .Include(s => s.Task)
                .Where(s => 
                    s.IsActive &&
                    !s.Task.IsDeleted &&
                    s.Task.Status != 2) // ⭐ Status 2 = تکمیل شده (بر اساس ViewModel)
                .AsNoTracking()
                .ToListAsync(stoppingToken);

            if (!dueSchedules.Any())
            {
                _logger.LogDebug("ℹ️ یادآوری فعالی برای پردازش وجود ندارد");
                return;
            }

            _logger.LogInformation($"📋 {dueSchedules.Count} Schedule فعال یافت شد");

            int processedCount = 0;

            foreach (var schedule in dueSchedules)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    // ⭐⭐⭐ محاسبه زمان بعدی برای این Schedule
                    var nextExecutionTime = CalculateNextExecutionTime(schedule, nowIran);

                    if (!nextExecutionTime.HasValue || nextExecutionTime.Value > nowIran)
                    {
                        // هنوز زمان اجرا نرسیده
                        continue;
                    }

                    // ⭐⭐⭐ چک Double-Execution
                    if (schedule.LastExecuted.HasValue &&
                        (nowIran - schedule.LastExecuted.Value).TotalMinutes < 1)
                    {
                        _logger.LogDebug($"⚠️ یادآوری #{schedule.Id} در کمتر از 1 دقیقه پیش اجرا شده. Skip.");
                        continue;
                    }

                    // ⭐⭐⭐ تولید Event برای هر کاربر مرتبط
                    var recipientUserIds = await GetReminderRecipientsAsync(schedule, context);

                    if (!recipientUserIds.Any())
                    {
                        _logger.LogWarning($"⚠️ هیچ کاربری برای یادآوری #{schedule.Id} یافت نشد");
                        continue;
                    }

                    _logger.LogInformation($"📤 ارسال یادآوری '{schedule.Title}' به {recipientUserIds.Count} کاربر");

                    foreach (var userId in recipientUserIds)
                    {
                        // ⭐ ارسال Notification با استفاده از NotificationManagementService
                        await notificationService.ProcessEventNotificationAsync(
                            NotificationEventType.CustomTaskReminder, // ⭐ استفاده از نوع جدید
                            new List<string> { userId },
                            "SYSTEM", // سیستمی
                            schedule.Title,
                            BuildReminderMessage(schedule),
                            $"/TaskingArea/Tasks/Details/{schedule.TaskId}",
                            schedule.TaskId.ToString(),
                            "Task",
                            priority: 2 // یادآوری‌ها اولویت بالا دارند
                        );
                    }

                    // ⭐⭐⭐ بروزرسانی LastExecuted
                    var scheduleToUpdate = await context.TaskReminderSchedule_Tbl
                        .FirstOrDefaultAsync(s => s.Id == schedule.Id, stoppingToken);

                    if (scheduleToUpdate != null)
                    {
                        scheduleToUpdate.LastExecuted = nowIran;
                        await context.SaveChangesAsync(stoppingToken);
                    }

                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در پردازش یادآوری #{schedule.Id} - {schedule.Title}");
                }
            }

            if (processedCount > 0)
            {
                _logger.LogInformation($"✅ {processedCount} یادآوری با موفقیت پردازش شد");
            }
        }

        /// <summary>
        /// محاسبه زمان بعدی اجرای یادآوری
        /// </summary>
        private DateTime? CalculateNextExecutionTime(
            MahERP.DataModelLayer.Entities.TaskManagement.TaskReminderSchedule schedule,
            DateTime nowIran)
        {
            // ⭐ NotificationTime is TimeSpan, not nullable
            var time = schedule.NotificationTime;

            switch (schedule.ReminderType)
            {
                case 0: // یکبار در زمان مشخص
                    if (!schedule.StartDate.HasValue)
                        return null;

                    var oneTimeExecution = schedule.StartDate.Value.Date.Add(time);
                    
                    // فقط اگر هنوز زمان نرسیده یا امروز است
                    return oneTimeExecution;

                case 1: // تکراری
                    if (!schedule.IntervalDays.HasValue || !schedule.StartDate.HasValue)
                        return null;

                    // محاسبه آخرین اجرای مجاز
                    var lastAllowedExecution = schedule.EndDate?.Date.Add(time);
                    
                    var currentExecution = schedule.StartDate.Value.Date.Add(time);
                    
                    // پیدا کردن اولین زمان اجرا بعد از LastExecuted
                    if (schedule.LastExecuted.HasValue)
                    {
                        currentExecution = schedule.LastExecuted.Value.Date
                            .AddDays(schedule.IntervalDays.Value)
                            .Add(time);
                    }

                    // چک کردن EndDate
                    if (lastAllowedExecution.HasValue && currentExecution > lastAllowedExecution.Value)
                        return null;

                    return currentExecution;

                case 2: // قبل از مهلت
                    if (!schedule.DaysBeforeDeadline.HasValue || schedule.Task.DueDate == null)
                        return null;

                    var deadlineReminder = schedule.Task.DueDate.Value
                        .AddDays(-schedule.DaysBeforeDeadline.Value)
                        .Date
                        .Add(time);

                    return deadlineReminder;

                case 3: // در روز شروع تسک
                    if (schedule.Task.StartDate == null)
                        return null;

                    return schedule.Task.StartDate.Value.Date.Add(time);

                case 4: // در روز پایان مهلت
                    if (schedule.Task.DueDate == null)
                        return null;

                    return schedule.Task.DueDate.Value.Date.Add(time);

                default:
                    return null;
            }
        }

        /// <summary>
        /// دریافت لیست کاربران دریافت‌کننده یادآوری
        /// </summary>
        private async Task<List<string>> GetReminderRecipientsAsync(
            MahERP.DataModelLayer.Entities.TaskManagement.TaskReminderSchedule schedule,
            AppDbContext context)
        {
            // ⭐⭐⭐ فقط کاربران منتصب شده به تسک + سازنده
            var recipients = new List<string>();

            // 1. کاربران منتصب شده
            var assignedUserIds = await context.TaskAssignment_Tbl
                .Where(a => a.TaskId == schedule.TaskId &&
                           !a.CompletionDate.HasValue && // فقط کسانی که تکمیل نکرده‌اند
                           a.AssignedUserId != null)
                .Select(a => a.AssignedUserId)
                .Distinct()
                .ToListAsync();

            recipients.AddRange(assignedUserIds);

            // 2. سازنده تسک (اگر از لیست کاربران منتصب نیست)
            if (!string.IsNullOrEmpty(schedule.Task.CreatorUserId) &&
                !recipients.Contains(schedule.Task.CreatorUserId))
            {
                recipients.Add(schedule.Task.CreatorUserId);
            }

            return recipients.Distinct().ToList();
        }

        /// <summary>
        /// ساخت متن پیام یادآوری
        /// </summary>
        private string BuildReminderMessage(
            MahERP.DataModelLayer.Entities.TaskManagement.TaskReminderSchedule schedule)
        {
            var message = $"یادآوری: {schedule.Title}";

            if (!string.IsNullOrEmpty(schedule.Description))
            {
                message += $"\n\n{schedule.Description}";
            }

            message += $"\n\nتسک: {schedule.Task.Title} ({schedule.Task.TaskCode})";

            if (schedule.Task.DueDate.HasValue)
            {
                var persianDueDate = CommonLayer.PublicClasses.ConvertDateTime
                    .ConvertMiladiToShamsi(schedule.Task.DueDate.Value, "yyyy/MM/dd");
                message += $"\nمهلت: {persianDueDate}";
            }

            return message;
        }
    }
}
