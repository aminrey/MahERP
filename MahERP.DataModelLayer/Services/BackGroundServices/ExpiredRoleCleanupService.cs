using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    /// ⭐⭐⭐ Background Service برای پاکسازی خودکار نقش‌های منقضی شده
    /// 
    /// 📖 توضیحات عملکرد:
    /// این سرویس به صورت خودکار و مستمر در پس‌زمینه اجرا می‌شود و کارهای زیر را انجام می‌دهد:
    /// 
    /// 1️⃣ **شناسایی نقش‌های منقضی شده:**
    ///    - UserRole هایی که EndDate < DateTime.Now
    ///    - UserRole هایی که IsActive = true
    /// 
    /// 2️⃣ **غیرفعال کردن نقش‌ها:**
    ///    - IsActive را false می‌کند
    ///    - LastUpdateDate را به‌روزرسانی می‌کند
    /// 
    /// 3️⃣ **همگام‌سازی دسترسی‌ها:**
    ///    - UserPermission های مربوط به آن نقش را حذف می‌کند
    ///    - دسترسی‌های دستی کاربر را حفظ می‌کند
    /// 
    /// 4️⃣ **لاگ کردن:**
    ///    - تمام عملیات را لاگ می‌کند
    ///    - در صورت خطا، Error لاگ می‌شود
    /// 
    /// ⚙️ تنظیمات:
    /// - **چک اولیه:** 1 دقیقه بعد از شروع برنامه
    /// - **دوره تکرار:** هر 1 ساعت یک بار
    /// - **زمان اجرا:** قابل تنظیم از طریق appsettings.json
    /// 
    /// 🔧 توسعه آینده:
    /// - اضافه کردن Notification برای کاربران (X روز قبل از انقضا)
    /// - ارسال Email به مدیران سیستم
    /// - ایجاد گزارش خلاصه از نقش‌های حذف شده
    /// - قابلیت Restore برای نقش‌های حذف شده
    /// - تنظیم زمان‌بندی پیشرفته (مثلاً فقط شب‌ها اجرا شود)
    /// 
    /// 📝 نکات مهم:
    /// - از Scoped Service Pool استفاده می‌کند (IServiceScopeFactory)
    /// - DbContext در هر دوره جدید ایجاد و Dispose می‌شود
    /// - در صورت خطا، سرویس متوقف نمی‌شود و به چرخه بعدی می‌رود
    /// - تمام تغییرات در Transaction انجام می‌شود
    /// 
    /// 🚀 مثال استفاده:
    /// این سرویس به صورت خودکار در Program.cs ثبت می‌شود:
    /// <code>
    /// services.AddHostedService<ExpiredRoleCleanupService>();
    /// </code>
    /// </summary>
    public class ExpiredRoleCleanupService : BackgroundService
    {
        private readonly ILogger<ExpiredRoleCleanupService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // ⚙️ تنظیمات زمان‌بندی
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _checkInterval;

        /// <summary>
        /// سازنده سرویس
        /// 
        /// از IServiceScopeFactory استفاده می‌کند چون:
        /// - BackgroundService یک Singleton است
        /// - اما DbContext باید Scoped باشد
        /// - پس در هر دوره، یک Scope جدید ایجاد می‌کنیم
        /// </summary>
        public ExpiredRoleCleanupService(
            ILogger<ExpiredRoleCleanupService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            // ⭐ خواندن تنظیمات از appsettings.json
            _initialDelay = TimeSpan.FromMinutes(
                configuration.GetValue<int>("BackgroundServices:ExpiredRoleCleanup:InitialDelayMinutes", 1)
            );

            _checkInterval = TimeSpan.FromHours(
                configuration.GetValue<int>("BackgroundServices:ExpiredRoleCleanup:CheckIntervalHours", 1)
            );

            _logger.LogInformation(
                "⚙️ ExpiredRoleCleanupService تنظیمات: تاخیر اولیه={InitialDelay} دقیقه، چک هر {CheckInterval} ساعت",
                _initialDelay.TotalMinutes,
                _checkInterval.TotalHours
            );
        }

        /// <summary>
        /// ⭐ متد اصلی Background Service
        /// این متد به صورت مداوم اجرا می‌شود تا زمانی که برنامه متوقف شود
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 ExpiredRoleCleanupService شروع به کار کرد");

            // ⏳ تاخیر اولیه - صبر کن تا برنامه کامل بالا بیاید
            await Task.Delay(_initialDelay, stoppingToken);

            // 🔄 حلقه اصلی - تا زمانی که برنامه بسته نشده
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔍 شروع چک کردن نقش‌های منقضی شده... ({Time})", DateTime.Now);

                    // ⭐ اجرای عملیات پاکسازی
                    var cleanedCount = await CleanupExpiredRolesAsync();

                    if (cleanedCount > 0)
                    {
                        _logger.LogInformation("✅ {Count} نقش منقضی شده غیرفعال شد", cleanedCount);
                    }
                    else
                    {
                        _logger.LogInformation("✅ هیچ نقش منقضی شده‌ای یافت نشد");
                    }
                }
                catch (Exception ex)
                {
                    // ⚠️ در صورت خطا، لاگ کن اما سرویس را متوقف نکن
                    _logger.LogError(ex, "❌ خطا در پاکسازی نقش‌های منقضی شده");
                }

                // ⏳ صبر کن تا دوره بعدی
                _logger.LogInformation("⏳ چک بعدی در {Minutes} دقیقه دیگر...", _checkInterval.TotalMinutes);
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("🛑 ExpiredRoleCleanupService متوقف شد");
        }

        /// <summary>
        /// ⭐⭐⭐ پاکسازی نقش‌های منقضی شده
        /// 
        /// مراحل:
        /// 1. ایجاد Scope جدید
        /// 2. دریافت DbContext
        /// 3. یافتن نقش‌های منقضی شده
        /// 4. غیرفعال کردن آن‌ها
        /// 5. حذف دسترسی‌های مربوطه
        /// 6. ذخیره تغییرات
        /// </summary>
        /// <returns>تعداد نقش‌های غیرفعال شده</returns>
        private async Task<int> CleanupExpiredRolesAsync()
        {
            // ⭐ ایجاد Scope جدید برای دسترسی به DbContext
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.Now;
            var cleanedCount = 0;

            try
            {
                // 🔍 یافتن UserRole های منقضی شده که هنوز فعال هستند
                var expiredRoles = await context.UserRole_Tbl
                    .Where(ur =>
                        ur.IsActive &&                          // فعال باشد
                        ur.EndDate.HasValue &&                  // تاریخ پایان داشته باشد
                        ur.EndDate.Value < now)                 // تاریخ پایان گذشته باشد
                    .Include(ur => ur.User)                     // برای لاگ کردن نام کاربر
                    .Include(ur => ur.Role)                     // برای لاگ کردن نام نقش
                    .ToListAsync();

                if (!expiredRoles.Any())
                {
                    return 0; // هیچ نقش منقضی شده‌ای نیست
                }

                _logger.LogInformation("📋 {Count} نقش منقضی شده یافت شد", expiredRoles.Count);

                // 🔄 پردازش هر UserRole منقضی شده
                foreach (var userRole in expiredRoles)
                {
                    try
                    {
                        _logger.LogInformation(
                            "🔸 غیرفعال کردن نقش '{RoleName}' برای کاربر '{UserName}' (تاریخ انقضا: {EndDate})",
                            userRole.Role?.NameFa,
                            $"{userRole.User?.FirstName} {userRole.User?.LastName}",
                            userRole.EndDate
                        );

                        // 1️⃣ غیرفعال کردن UserRole
                        userRole.IsActive = false;
                        userRole.LastUpdateDate = DateTime.Now;
                        userRole.LastUpdaterUserId = "SYSTEM"; // نشان‌دهنده عملیات خودکار

                        // 2️⃣ حذف UserPermission های مربوط به این نقش
                        //    فقط دسترسی‌هایی که از طریق Role بوده و دستی نیست
                        var rolePermissions = await context.UserPermission_Tbl
                            .Where(up =>
                                up.UserId == userRole.UserId &&
                                up.SourceRoleId == userRole.RoleId &&
                                up.SourceType == 1 &&                   // از نقش آمده
                                !up.IsManuallyModified)                 // دستی نیست
                            .ToListAsync();

                        if (rolePermissions.Any())
                        {
                            context.UserPermission_Tbl.RemoveRange(rolePermissions);
                            _logger.LogInformation(
                                "  ↳ {Count} دسترسی حذف شد",
                                rolePermissions.Count
                            );
                        }

                        cleanedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "❌ خطا در پردازش UserRole با Id={UserRoleId}",
                            userRole.Id
                        );
                        // ادامه به UserRole بعدی
                    }
                }

                // ✅ ذخیره تمام تغییرات
                await context.SaveChangesAsync();

                _logger.LogInformation("✅ {Count} نقش با موفقیت غیرفعال شد", cleanedCount);

                return cleanedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطای کلی در پاکسازی نقش‌های منقضی شده");
                throw;
            }
        }

        /// <summary>
        /// ⭐ متد اضافی: ارسال Notification به کاربران
        /// (برای توسعه آینده)
        /// 
        /// می‌تواند X روز قبل از انقضا به کاربر اطلاع دهد
        /// </summary>
        private async Task SendExpirationNotificationsAsync(AppDbContext context)
        {
            // TODO: پیاده‌سازی Notification برای نقش‌هایی که نزدیک انقضا هستند
            // مثلاً 7 روز قبل از انقضا یک پیام ارسال کن

            var warningDate = DateTime.Now.AddDays(7);

            var upcomingExpired = await context.UserRole_Tbl
                .Where(ur =>
                    ur.IsActive &&
                    ur.EndDate.HasValue &&
                    ur.EndDate.Value > DateTime.Now &&
                    ur.EndDate.Value <= warningDate)
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();

            foreach (var userRole in upcomingExpired)
            {
                // ارسال Notification یا Email
                _logger.LogInformation(
                    "⚠️ هشدار: نقش '{RoleName}' کاربر '{UserName}' در تاریخ {EndDate} منقضی می‌شود",
                    userRole.Role?.NameFa,
                    $"{userRole.User?.FirstName} {userRole.User?.LastName}",
                    userRole.EndDate
                );
            }
        }
    }
}