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

        // ⭐⭐⭐ تنظیم TimeZone ایران
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

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
            _logger.LogInformation($"🌍 TimeZone: {IranTimeZone.DisplayName}");

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

            // ⭐⭐⭐ استفاده از زمان ایران به جای UTC
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);

            _logger.LogInformation($"🕐 زمان فعلی ایران: {nowIran:yyyy-MM-dd HH:mm:ss}");

            // ⭐⭐⭐ FIX: اضافه کردن شرط‌های جدید برای جلوگیری از اجرای مکرر
            var dueTemplates = await context.NotificationTemplate_Tbl
                .Where(t =>
                    t.IsScheduled &&
                    t.IsScheduleEnabled &&
                    t.IsActive &&
                    t.NextExecutionDate.HasValue &&
                    t.NextExecutionDate.Value <= nowIran &&
                    // ⭐⭐⭐ شرط اصلی: اگر قبلاً اجرا شده، حداقل یک دقیقه فاصله باشد
                    (!t.LastExecutionDate.HasValue || 
                     EF.Functions.DateDiffMinute(t.LastExecutionDate.Value, nowIran) >= 1))
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
                    // ⭐⭐⭐ FIX: چک مجدد در حافظه (Double-check)
                    if (template.LastExecutionDate.HasValue &&
                        (nowIran - template.LastExecutionDate.Value).TotalMinutes < 1)
                    {
                        _logger.LogWarning($"⚠️ قالب {template.TemplateName} در کمتر از 1 دقیقه پیش اجرا شده است. Skip.");
                        continue;
                    }

                    _logger.LogInformation($"📤 اجرای قالب: {template.TemplateName} (NextExecution: {template.NextExecutionDate:yyyy-MM-dd HH:mm})");
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

            // ⭐⭐⭐ استفاده از متد جدید که مستقیماً با قالب کار می‌کند
            var count = await notificationService.ProcessScheduledNotificationAsync(
                template,
                recipients
            );

            _logger.LogInformation($"✅ قالب {template.TemplateName} به {count} کاربر ارسال شد");

            // ⭐⭐⭐ بروزرسانی اطلاعات اجرا با زمان ایران (بدون Include)
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);

            // ⭐⭐⭐ FIX: بارگذاری مجدد از دیتابیس برای اطمینان از آخرین وضعیت
            var templateToUpdate = await context.NotificationTemplate_Tbl
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            if (templateToUpdate != null)
            {
                templateToUpdate.LastExecutionDate = nowIran;
                templateToUpdate.UsageCount++;
                templateToUpdate.LastUsedDate = nowIran;

                // ⭐⭐⭐ محاسبه زمان بعدی (حتماً بعد از زمان فعلی باشد)
                var nextExecution = CalculateNextExecutionDate(templateToUpdate);
                templateToUpdate.NextExecutionDate = nextExecution;

                // ⭐⭐⭐ لاگ برای دیباگ
                _logger.LogInformation($"📅 بروزرسانی زمان‌ها:");
                _logger.LogInformation($"   LastExecutionDate: {templateToUpdate.LastExecutionDate:yyyy-MM-dd HH:mm:ss}");
                _logger.LogInformation($"   NextExecutionDate: {templateToUpdate.NextExecutionDate:yyyy-MM-dd HH:mm:ss}");

                // ⭐⭐⭐ FIX: استفاده از Update به جای Attach
                // چون AsNoTracking استفاده کردیم، باید با Update کل entity رو بروزرسانی کنیم
                 context.NotificationTemplate_Tbl.Update(templateToUpdate);
                await context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ اطلاعات زمان‌بندی برای {template.TemplateName} بروزرسانی شد");
            }
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

                // ⭐⭐⭐ FIX: فقط فیلد NextExecutionDate را بروزرسانی کن (بدون Include)
                var templateToUpdate = await context.NotificationTemplate_Tbl
                    .AsNoTracking() // ⭐ عدم Track کردن Entity
                    .FirstOrDefaultAsync(t => t.Id == template.Id);

                if (templateToUpdate != null)
                {
                    templateToUpdate.NextExecutionDate = nextExecution;

                    // ⭐ Attach کردن و فقط NextExecutionDate را Modified کن
                    context.NotificationTemplate_Tbl.Attach(templateToUpdate);
                    context.Entry(templateToUpdate).Property(t => t.NextExecutionDate).IsModified = true;

                    await context.SaveChangesAsync();

                    _logger.LogInformation($"📅 زمان بعدی اجرا برای {template.TemplateName}: {nextExecution:yyyy-MM-dd HH:mm}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در محاسبه زمان بعدی برای قالب {template.TemplateName}");
            }
        }

        /// <summary>
        /// محاسبه زمان اجرای بعدی بر اساس نوع زمان‌بندی - با استفاده از TimeZone ایران
        /// </summary>
        private DateTime? CalculateNextExecutionDate(
            MahERP.DataModelLayer.Entities.Notifications.NotificationTemplate template)
        {
            if (string.IsNullOrEmpty(template.ScheduledTime))
                return null;

            // ⭐⭐⭐ استفاده از زمان ایران
            var nowUtc = DateTime.UtcNow;
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, IranTimeZone);
            
            var timeParts = template.ScheduledTime.Split(':');

            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out int hour) ||
                !int.TryParse(timeParts[1], out int minute))
            {
                _logger.LogWarning($"⚠️ فرمت ساعت نامعتبر برای قالب {template.TemplateName}: {template.ScheduledTime}");
                return null;
            }

            DateTime? nextExecutionIran = null;

            switch (template.ScheduleType)
            {
                case 1: // روزانه
                    // ⭐⭐⭐ ساخت DateTime با Iran TimeZone
                    nextExecutionIran = new DateTime(nowIran.Year, nowIran.Month, nowIran.Day, hour, minute, 0, DateTimeKind.Unspecified);
                    
                    // ⭐⭐⭐ FIX: اگر زمان امروز گذشته، حتماً یک روز اضافه کن
                    if (nextExecutionIran <= nowIran)
                    {
                        nextExecutionIran = nextExecutionIran.Value.AddDays(1);
                    }
                    
                    _logger.LogInformation($"📅 روزانه: NextExecution (Iran) = {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                case 2: // هفتگی
                    if (string.IsNullOrEmpty(template.ScheduledDaysOfWeek))
                        return null;

                    var daysOfWeek = template.ScheduledDaysOfWeek
                        .Split(',')
                        .Select(d => int.Parse(d.Trim()))
                        .OrderBy(d => d)
                        .ToList();

                    nextExecutionIran = FindNextWeeklyExecution(nowIran, hour, minute, daysOfWeek);
                    _logger.LogInformation($"📅 هفتگی: NextExecution (Iran) = {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                case 3: // ماهانه (یک روز)
                    if (!template.ScheduledDayOfMonth.HasValue)
                        return null;

                    nextExecutionIran = FindNextMonthlyExecution(nowIran, hour, minute, template.ScheduledDayOfMonth.Value);
                    _logger.LogInformation($"📅 ماهانه (یک روز): NextExecution (Iran) = {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                case 4: // ⭐⭐⭐ ماهانه (چند روز) 🆕
                    if (string.IsNullOrEmpty(template.ScheduledDaysOfMonth))
                        return null;

                    var daysOfMonth = template.ScheduledDaysOfMonth
                        .Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var day) ? day : (int?)null)
                        .Where(d => d.HasValue && d.Value >= 1 && d.Value <= 31)
                        .Select(d => d.Value)
                        .OrderBy(d => d)
                        .ToList();

                    if (!daysOfMonth.Any())
                        return null;

                    nextExecutionIran = FindNextMonthlyMultipleDaysExecution(nowIran, daysOfMonth, hour, minute);
                    _logger.LogInformation($"📅 ماهانه (چند روز): NextExecution (Iran) = {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                default:
                    // ⭐⭐⭐ نوع زمان‌بندی نامعتبر
                    _logger.LogWarning($"⚠️ نوع زمان‌بندی نامعتبر برای قالب {template.TemplateName}: {template.ScheduleType}");
                    return null;
            }

            // ⭐⭐⭐ اگر nextExecutionIran هنوز null است
            if (!nextExecutionIran.HasValue)
            {
                _logger.LogWarning($"⚠️ محاسبه زمان بعدی برای قالب {template.TemplateName} ناموفق بود");
                return null;
            }

            // ⭐⭐⭐ تبدیل Iran Time به UTC
            try
            {
                var nextExecutionUtc = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran.Value, IranTimeZone);
                _logger.LogInformation($"✅ Converted to UTC: {nextExecutionUtc:yyyy-MM-dd HH:mm:ss}");
                return nextExecutionUtc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در تبدیل به UTC: {nextExecutionIran}");
                return null;
            }
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی هفتگی
        /// </summary>
        private DateTime FindNextWeeklyExecution(DateTime nowIran, int hour, int minute, List<int> daysOfWeek)
        {
            var currentDayOfWeek = (int)nowIran.DayOfWeek;

            // ⭐ چک کردن امروز
            var todayExecution = new DateTime(nowIran.Year, nowIran.Month, nowIran.Day, hour, minute, 0, DateTimeKind.Unspecified);
            if (daysOfWeek.Contains(currentDayOfWeek) && todayExecution > nowIran)
            {
                return todayExecution;
            }

            // ⭐ پیدا کردن روز بعدی
            for (int i = 1; i <= 7; i++)
            {
                var nextDate = nowIran.AddDays(i);
                var nextDayOfWeek = (int)nextDate.DayOfWeek;

                if (daysOfWeek.Contains(nextDayOfWeek))
                {
                    return new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, hour, minute, 0, DateTimeKind.Unspecified);
                }
            }

            // پیش‌فرض (نباید به اینجا برسد)
            return nowIran.AddDays(7);
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی ماهانه
        /// </summary>
        private DateTime FindNextMonthlyExecution(DateTime nowIran, int hour, int minute, int dayOfMonth)
        {
            // ⭐ چک کردن این ماه
            var daysInMonth = DateTime.DaysInMonth(nowIran.Year, nowIran.Month);
            var targetDay = Math.Min(dayOfMonth, daysInMonth);

            var thisMonthExecution = new DateTime(nowIran.Year, nowIran.Month, targetDay, hour, minute, 0, DateTimeKind.Unspecified);
            if (thisMonthExecution > nowIran)
            {
                return thisMonthExecution;
            }

            // ⭐ ماه بعد
            var nextMonth = nowIran.AddMonths(1);
            daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            targetDay = Math.Min(dayOfMonth, daysInMonth);

            return new DateTime(nextMonth.Year, nextMonth.Month, targetDay, hour, minute, 0, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: پیدا کردن اولین زمان اجرای ماهانه بعدی (با چند روز انتخابی)
        /// </summary>
        private DateTime FindNextMonthlyMultipleDaysExecution(DateTime now, List<int> daysOfMonth, int hour, int minute)
        {
            var currentDay = now.Day;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // ⭐ مرحله 1: بررسی ماه جاری
            var todayExecution = new DateTime(currentYear, currentMonth, Math.Min(currentDay, DateTime.DaysInMonth(currentYear, currentMonth)), hour, minute, 0, DateTimeKind.Unspecified);
            
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
                    return new DateTime(currentYear, currentMonth, day, hour, minute, 0, DateTimeKind.Unspecified);
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
                return new DateTime(nextYear, nextMonth, firstAvailableDay, hour, minute, 0, DateTimeKind.Unspecified);
            }

            // اگر هیچ روزی در ماه بعد موجود نیست (مثلاً روز 31 در فوریه)
            // برو 2 ماه بعد
            var nextNextMonth = nextMonth == 12 ? 1 : nextMonth + 1;
            var nextNextYear = nextMonth == 12 ? nextYear + 1 : nextYear;
            var daysInNextNextMonth = DateTime.DaysInMonth(nextNextYear, nextNextMonth);

            firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextNextMonth);
            if (firstAvailableDay > 0)
            {
                return new DateTime(nextNextYear, nextNextMonth, firstAvailableDay, hour, minute, 0, DateTimeKind.Unspecified);
            }

            // در صورت مشکل، ماه بعد اولین روز از لیست
            return new DateTime(nextYear, nextMonth, daysOfMonth.First(), hour, minute, 0, DateTimeKind.Unspecified);
        }
    }
}
