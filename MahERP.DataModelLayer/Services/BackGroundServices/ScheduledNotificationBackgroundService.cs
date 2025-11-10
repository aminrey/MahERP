using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository.Notifications;
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
    /// ⏰ سرویس پس‌زمینه برای اجرای قالب‌های اعلان زمان‌بندی شده
    /// هر دقیقه چک می‌کند آیا قالبی برای اجرا آماده است یا خیر
    /// </summary>
    public class ScheduledNotificationBackgroundService : BackgroundService
    {
        private readonly ILogger<ScheduledNotificationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ScheduledNotificationBackgroundService(
            ILogger<ScheduledNotificationBackgroundService> _logger,
            IServiceProvider serviceProvider)
        {
            this._logger = _logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⏰ Scheduled Notification Background Service شروع شد");

            // ⭐ صبر 30 ثانیه تا سیستم بوت شود
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndExecuteScheduledTemplatesAsync(stoppingToken);

                    // ⭐ هر 1 دقیقه چک کن
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در حلقه اصلی زمان‌بندی اعلان‌ها");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ Scheduled Notification Background Service متوقف شد");
        }

        /// <summary>
        /// چک کردن و اجرای قالب‌های آماده برای ارسال
        /// </summary>
        private async Task CheckAndExecuteScheduledTemplatesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ⭐ دریافت قالب‌های آماده برای اجرا
            var dueTemplates = await context.NotificationTemplate_Tbl
                .Where(t =>
                    t.IsScheduled &&
                    t.IsScheduleEnabled &&
                    t.IsActive &&
                    t.NextExecutionDate.HasValue &&
                    t.NextExecutionDate.Value <= DateTime.Now)
                .ToListAsync(stoppingToken);

            if (!dueTemplates.Any())
            {
                _logger.LogDebug("ℹ️ قالب زمان‌بندی شده‌ای برای اجرا وجود ندارد");
                return;
            }

            _logger.LogInformation($"⏰ {dueTemplates.Count} قالب زمان‌بندی شده آماده اجرا است");

            foreach (var template in dueTemplates)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await ExecuteScheduledTemplateAsync(template, scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در اجرای قالب زمان‌بندی شده #{template.Id} - {template.TemplateName}");
                }
            }
        }

        /// <summary>
        /// اجرای یک قالب زمان‌بندی شده
        /// </summary>
        private async Task ExecuteScheduledTemplateAsync(
            MahERP.DataModelLayer.Entities.Notifications.NotificationTemplate template,
            IServiceProvider serviceProvider)
        {
            var notificationService = serviceProvider.GetRequiredService<NotificationManagementService>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation($"📤 شروع ارسال قالب زمان‌بندی شده: {template.TemplateName}");

            // ⭐⭐⭐ دریافت لیست دریافت‌کنندگان
            var recipients = await GetScheduledTemplateRecipientsAsync(template, context);

            if (!recipients.Any())
            {
                _logger.LogWarning($"⚠️ دریافت‌کننده‌ای برای قالب {template.TemplateName} یافت نشد");
                
                // ⭐ بروزرسانی زمان بعدی
                await UpdateNextExecutionDateAsync(template, context);
                return;
            }

            // ⭐⭐⭐ ارسال اعلان به تمام دریافت‌کنندگان
            var count = await notificationService.ProcessEventNotificationAsync(
                (NotificationEventType)template.NotificationEventType,
                recipients,
                "SYSTEM", // ارسال‌کننده: سیستم
                "اعلان روزانه", // عنوان (از متغیرهای قالب پر می‌شود)
                template.MessageTemplate ?? "",
                "", // بدون لینک
                null, // بدون رکورد مرتبط
                null,
                0 // اولویت عادی
            );

            _logger.LogInformation($"✅ قالب {template.TemplateName} به {count} کاربر ارسال شد");

            // ⭐ بروزرسانی اطلاعات اجرا
            template.LastExecutionDate = DateTime.Now;
            template.UsageCount++;
            template.LastUsedDate = DateTime.Now;

            // ⭐ محاسبه زمان بعدی
            await UpdateNextExecutionDateAsync(template, context);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// دریافت لیست دریافت‌کنندگان برای قالب زمان‌بندی شده
        /// </summary>
        private async Task<List<string>> GetScheduledTemplateRecipientsAsync(
            MahERP.DataModelLayer.Entities.Notifications.NotificationTemplate template,
            AppDbContext context)
        {
            // ⭐ بر اساس RecipientMode
            switch (template.RecipientMode)
            {
                case 0: // همه کاربران
                    return await context.Users
                        .Where(u => u.IsActive && !u.IsRemoveUser)
                        .Select(u => u.Id)
                        .ToListAsync();

                case 1: // فقط کاربران مشخص
                    return await context.NotificationTemplateRecipient_Tbl
                        .Where(r => r.NotificationTemplateId == template.Id &&
                                   r.IsActive &&
                                   r.RecipientType == 2) // User
                        .Select(r => r.UserId)
                        .ToListAsync();

                case 2: // همه به جز کاربران مشخص
                    var excludedUsers = await context.NotificationTemplateRecipient_Tbl
                        .Where(r => r.NotificationTemplateId == template.Id &&
                                   r.IsActive &&
                                   r.RecipientType == 2)
                        .Select(r => r.UserId)
                        .ToListAsync();

                    return await context.Users
                        .Where(u => u.IsActive &&
                                   !u.IsRemoveUser &&
                                   !excludedUsers.Contains(u.Id))
                        .Select(u => u.Id)
                        .ToListAsync();

                default:
                    return new List<string>();
            }
        }

        /// <summary>
        /// محاسبه و بروزرسانی زمان اجرای بعدی
        /// </summary>
        private async Task UpdateNextExecutionDateAsync(
            MahERP.DataModelLayer.Entities.Notifications.NotificationTemplate template,
            AppDbContext context)
        {
            try
            {
                var nextExecution = CalculateNextExecutionDate(template);

                template.NextExecutionDate = nextExecution;

                context.NotificationTemplate_Tbl.Update(template);
                await context.SaveChangesAsync();

                _logger.LogInformation($"📅 زمان بعدی اجرا برای {template.TemplateName}: {nextExecution:yyyy-MM-dd HH:mm}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در محاسبه زمان بعدی برای قالب {template.TemplateName}");
            }
        }

        /// <summary>
        /// محاسبه زمان اجرای بعدی بر اساس نوع زمان‌بندی
        /// </summary>
        private DateTime? CalculateNextExecutionDate(
            MahERP.DataModelLayer.Entities.Notifications.NotificationTemplate template)
        {
            if (string.IsNullOrEmpty(template.ScheduledTime))
                return null;

            var now = DateTime.Now;
            var timeParts = template.ScheduledTime.Split(':');

            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out int hour) ||
                !int.TryParse(timeParts[1], out int minute))
            {
                _logger.LogWarning($"⚠️ فرمت ساعت نامعتبر برای قالب {template.TemplateName}: {template.ScheduledTime}");
                return null;
            }

            DateTime nextExecution;

            switch (template.ScheduleType)
            {
                case 1: // روزانه
                    nextExecution = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
                    if (nextExecution <= now)
                    {
                        nextExecution = nextExecution.AddDays(1);
                    }
                    break;

                case 2: // هفتگی
                    if (string.IsNullOrEmpty(template.ScheduledDaysOfWeek))
                        return null;

                    var daysOfWeek = template.ScheduledDaysOfWeek
                        .Split(',')
                        .Select(d => int.Parse(d.Trim()))
                        .OrderBy(d => d)
                        .ToList();

                    nextExecution = FindNextWeeklyExecution(now, hour, minute, daysOfWeek);
                    break;

                case 3: // ماهانه
                    if (!template.ScheduledDayOfMonth.HasValue)
                        return null;

                    nextExecution = FindNextMonthlyExecution(now, hour, minute, template.ScheduledDayOfMonth.Value);
                    break;

                case 4: // Cron Expression
                    // ⭐ TODO: پیاده‌سازی Cron Parser (نیاز به کتابخانه NCrontab)
                    _logger.LogWarning("⚠️ Cron Expression هنوز پیاده‌سازی نشده است");
                    return null;

                default:
                    return null;
            }

            return nextExecution;
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی هفتگی
        /// </summary>
        private DateTime FindNextWeeklyExecution(DateTime now, int hour, int minute, List<int> daysOfWeek)
        {
            var currentDayOfWeek = (int)now.DayOfWeek;

            // ⭐ چک کردن امروز
            var todayExecution = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (daysOfWeek.Contains(currentDayOfWeek) && todayExecution > now)
            {
                return todayExecution;
            }

            // ⭐ پیدا کردن روز بعدی
            for (int i = 1; i <= 7; i++)
            {
                var nextDate = now.AddDays(i);
                var nextDayOfWeek = (int)nextDate.DayOfWeek;

                if (daysOfWeek.Contains(nextDayOfWeek))
                {
                    return new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, hour, minute, 0);
                }
            }

            // پیش‌فرض (نباید به اینجا برسد)
            return now.AddDays(7);
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی ماهانه
        /// </summary>
        private DateTime FindNextMonthlyExecution(DateTime now, int hour, int minute, int dayOfMonth)
        {
            // ⭐ چک کردن این ماه
            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var targetDay = Math.Min(dayOfMonth, daysInMonth);

            var thisMonthExecution = new DateTime(now.Year, now.Month, targetDay, hour, minute, 0);
            if (thisMonthExecution > now)
            {
                return thisMonthExecution;
            }

            // ⭐ ماه بعد
            var nextMonth = now.AddMonths(1);
            daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            targetDay = Math.Min(dayOfMonth, daysInMonth);

            return new DateTime(nextMonth.Year, nextMonth.Month, targetDay, hour, minute, 0);
        }
    }
}
