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
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagementService>(); // ⭐⭐⭐ اضافه شده

            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            
            _logger.LogDebug($"🕐 چک یادآوری‌ها - زمان: {nowIran:yyyy-MM-dd HH:mm:ss}");

            // ⭐⭐⭐ دریافت Schedule های فعال و آماده برای اجرا
            var dueSchedules = await context.TaskReminderSchedule_Tbl
                .Include(s => s.Task)
                .Where(s => 
                    s.IsActive &&
                    !s.IsExpired && // ⭐⭐⭐ اضافه شده: یادآورهای منقضی نادیده گرفته شوند
                    !s.Task.IsDeleted &&
                    s.Task.Status != 2) // ⭐ Status 2 = تکمیل شده
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
                    // ⭐⭐⭐ FIX: بررسی و تنظیم خودکار MaxSendCount برای یادآوری‌های یکباره قدیمی
                    if (!schedule.MaxSendCount.HasValue && IsOneTimeReminderType(schedule.ReminderType))
                    {
                        var scheduleToFix = await context.TaskReminderSchedule_Tbl
                            .FirstOrDefaultAsync(s => s.Id == schedule.Id, stoppingToken);

                        if (scheduleToFix != null)
                        {
                            scheduleToFix.MaxSendCount = 1;
                            await context.SaveChangesAsync(stoppingToken);
                            
                            _logger.LogInformation($"✅ MaxSendCount برای یادآوری #{schedule.Id} (نوع {schedule.ReminderType}) به 1 تنظیم شد");
                            
                            // بروزرسانی schedule محلی
                            schedule.MaxSendCount = 1;
                        }
                    }

                    // ⭐⭐⭐ بررسی MaxSendCount قبل از محاسبه زمان
                    if (schedule.MaxSendCount.HasValue && schedule.SentCount >= schedule.MaxSendCount.Value)
                    {
                        _logger.LogDebug($"⚠️ یادآوری #{schedule.Id} قبلاً {schedule.SentCount} بار ارسال شده (حداکثر: {schedule.MaxSendCount}). Skip.");
                        
                        // ⭐⭐⭐ منقضی کردن یادآوری به جای غیرفعال کردن
                        if (!schedule.IsExpired)
                        {
                            var scheduleToExpire = await context.TaskReminderSchedule_Tbl
                                .FirstOrDefaultAsync(s => s.Id == schedule.Id, stoppingToken);

                            if (scheduleToExpire != null && !scheduleToExpire.IsExpired)
                            {
                                scheduleToExpire.IsExpired = true;
                                scheduleToExpire.ExpiredReason = $"رسیدن به حداکثر ارسال ({scheduleToExpire.MaxSendCount} بار)";
                                scheduleToExpire.ExpiredDate = nowIran;
                                await context.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation($"🔒 یادآوری #{schedule.Id} به دلیل رسیدن به حداکثر ارسال منقضی شد");
                            }
                        }
                        
                        continue;
                    }

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

                    _logger.LogInformation($"📤 ارسال یادآوری '{schedule.Title}' به {recipientUserIds.Count} کاربر: [{string.Join(", ", recipientUserIds)}]");

                    // ⭐⭐⭐ FIX: ارسال مستقیم به هر کاربر (به جای Enqueue)
                    // این باعث می‌شه قالب‌های خارجی هم ارسال بشن
                    foreach (var recipientUserId in recipientUserIds)
                    {
                        // ⭐ ثبت اعلان سیستمی + ارسال از طریق قالب‌های خارجی
                        await notificationService.ProcessEventNotificationAsync(
                            NotificationEventType.CustomTaskReminder,
                            new List<string> { recipientUserId },
                            "SYSTEM",
                            schedule.Title,
                            schedule.Description ?? schedule.Title,
                            $"/TaskingArea/Tasks/Details/{schedule.TaskId}",
                            schedule.TaskId.ToString(),
                            "Task",
                            priority: 2
                        );
                    }

                    // ⭐⭐⭐ بروزرسانی LastExecuted و افزایش SentCount
                    var scheduleToUpdate = await context.TaskReminderSchedule_Tbl
                        .FirstOrDefaultAsync(s => s.Id == schedule.Id, stoppingToken);

                    if (scheduleToUpdate != null)
                    {
                        scheduleToUpdate.LastExecuted = nowIran;
                        scheduleToUpdate.SentCount++; // ⭐⭐⭐ افزایش تعداد ارسال
                        
                        _logger.LogInformation($"✅ یادآوری #{schedule.Id} ارسال شد. تعداد کل ارسال: {scheduleToUpdate.SentCount}" + 
                            (scheduleToUpdate.MaxSendCount.HasValue ? $"/{scheduleToUpdate.MaxSendCount}" : ""));

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
        /// ⭐⭐⭐ NEW: بررسی اینکه یادآوری از نوع یکباره است یا خیر
        /// </summary>
        private bool IsOneTimeReminderType(byte reminderType)
        {
            return reminderType switch
            {
                0 => true,  // یکبار در زمان مشخص
                2 => true,  // قبل از پایان مهلت (یکبار)
                3 => true,  // در روز شروع تسک (یکبار)
                1 => false, // تکراری
                4 => false, // ⭐⭐⭐ ماهانه - چند روز (تکراری) 🆕
                _ => false
            };
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
                    
                    // ⭐⭐⭐ FIX: اگر قبلاً ارسال شده، دیگر زمان بعدی ندارد
                    if (schedule.SentCount > 0)
                        return null;
                    
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

                    // ⭐⭐⭐ FIX: اگر قبلاً ارسال شده، دیگر زمان بعدی ندارد
                    if (schedule.SentCount > 0)
                        return null;

                    var deadlineReminder = schedule.Task.DueDate.Value
                        .AddDays(-schedule.DaysBeforeDeadline.Value)
                        .Date
                        .Add(time);

                    return deadlineReminder;

                case 3: // در روز شروع تسک
                    if (schedule.Task.StartDate == null)
                        return null;

                    // ⭐⭐⭐ FIX: اگر قبلاً ارسال شده، دیگر زمان بعدی ندارد
                    if (schedule.SentCount > 0)
                        return null;

                    return schedule.Task.StartDate.Value.Date.Add(time);

                case 4: // ⭐⭐⭐ NEW: ماهانه - چند روز 🆕
                    if (string.IsNullOrEmpty(schedule.ScheduledDaysOfMonth))
                        return null;

                    // Parse روزهای ماه
                    var daysOfMonth = schedule.ScheduledDaysOfMonth
                        .Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var day) ? day : (int?)null)
                        .Where(d => d.HasValue && d.Value >= 1 && d.Value <= 31)
                        .Select(d => d.Value)
                        .OrderBy(d => d)
                        .ToList();

                    if (!daysOfMonth.Any())
                        return null;

                    // پیدا کردن اولین روز بعدی در ماه جاری یا ماه‌های بعد
                    var nextExecution = FindNextMonthlyExecution(nowIran, daysOfMonth, time);

                    // چک کردن EndDate
                    if (schedule.EndDate.HasValue && nextExecution > schedule.EndDate.Value.Date.Add(time))
                        return null;

                    return nextExecution;

                default:
                    return null;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: پیدا کردن اولین زمان اجرای ماهانه بعدی
        /// </summary>
        private DateTime FindNextMonthlyExecution(DateTime now, List<int> daysOfMonth, TimeSpan time)
        {
            var currentDay = now.Day;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // ⭐ مرحله 1: بررسی ماه جاری
            var todayExecution = new DateTime(currentYear, currentMonth, Math.Min(currentDay, DateTime.DaysInMonth(currentYear, currentMonth))).Date.Add(time);
            
            // آیا امروز در لیست است و ساعت نگذشته؟
            if (daysOfMonth.Contains(currentDay) && now < todayExecution)
            {
                return todayExecution;
            }

            // پیدا کردن اولین روز بعد از امروز در ماه جاری
            foreach (var day in daysOfMonth.Where(d => d > currentDay))
            {
                var daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                if (day <= daysInCurrentMonth)
                {
                    return new DateTime(currentYear, currentMonth, day).Date.Add(time);
                }
            }

            // ⭐ مرحله 2: اگر در ماه جاری روزی نماند، ماه بعد
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
            var daysInNextMonth = DateTime.DaysInMonth(nextYear, nextMonth);

            // اولین روز موجود در ماه بعد
            var firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextMonth);
            if (firstAvailableDay > 0)
            {
                return new DateTime(nextYear, nextMonth, firstAvailableDay).Date.Add(time);
            }

            // اگر هیچ روزی در ماه بعد موجود نیست (مثلاً روز 31 در فوریه)
            // برو 2 ماه بعد
            var nextNextMonth = nextMonth == 12 ? 1 : nextMonth + 1;
            var nextNextYear = nextMonth == 12 ? nextYear + 1 : nextYear;
            var daysInNextNextMonth = DateTime.DaysInMonth(nextNextYear, nextNextMonth);

            firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextNextMonth);
            if (firstAvailableDay > 0)
            {
                return new DateTime(nextNextYear, nextNextMonth, firstAvailableDay).Date.Add(time);
            }

            // در صورت مشکل، ماه بعد اولین روز از لیست
            return new DateTime(nextYear, nextMonth, daysOfMonth.First()).Date.Add(time);
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

    }
}
