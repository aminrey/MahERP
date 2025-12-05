using MahERP.DataModelLayer.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// سرویس Background برای اطمینان از وجود Seed Data های پایه سیستم
    /// این سرویس فقط یک بار در ابتدای اجرای برنامه اجرا می‌شود
    /// </summary>
    public class SystemSeedDataBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SystemSeedDataBackgroundService> _logger;

        public SystemSeedDataBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SystemSeedDataBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("🚀 SystemSeedDataBackgroundService started.");

                // صبر 5 ثانیه تا برنامه به طور کامل بالا بیاید
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var seedDataRepository = scope.ServiceProvider
                    .GetRequiredService<ISystemSeedDataRepository>();

                _logger.LogInformation("📦 Checking system seed data...");

                // اطمینان از وجود Seed Data های سیستم اعلان
                await seedDataRepository.EnsureNotificationSeedDataAsync();

                // اطمینان از وجود Seed Data های تنظیمات تسک
                await seedDataRepository.EnsureTaskSettingsSeedDataAsync();

                _logger.LogInformation("✅ System seed data check completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SystemSeedDataBackgroundService");
            }
        }
    }
}
