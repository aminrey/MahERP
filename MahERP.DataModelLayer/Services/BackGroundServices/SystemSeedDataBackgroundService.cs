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
            
            // ⭐ لاگ در Constructor برای اطمینان از ساخته شدن سرویس
            _logger.LogInformation("📦 SystemSeedDataBackgroundService CONSTRUCTED");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 SystemSeedDataBackgroundService ExecuteAsync STARTED");
            
            try
            {
                // ⭐ صبر کوتاه‌تر برای تست
                _logger.LogInformation("⏳ Waiting 3 seconds before seeding data...");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

                _logger.LogInformation("🔧 Creating service scope...");
                using var scope = _serviceProvider.CreateScope();
                
                _logger.LogInformation("🔍 Getting ISystemSeedDataRepository...");
                var seedDataRepository = scope.ServiceProvider
                    .GetRequiredService<ISystemSeedDataRepository>();

                _logger.LogInformation("📦 Checking system seed data...");

                // 1️⃣ اطمینان از وجود Seed Data های سیستم اعلان
                _logger.LogInformation("📢 Ensuring Notification Seed Data...");
                await seedDataRepository.EnsureNotificationSeedDataAsync();

                // 2️⃣ اطمینان از وجود Seed Data های تنظیمات تسک
                _logger.LogInformation("📋 Ensuring Task Settings Seed Data...");
                await seedDataRepository.EnsureTaskSettingsSeedDataAsync();

                // 3️⃣ اطمینان از وجود سمت‌های رایج سازمانی
                _logger.LogInformation("👔 Ensuring Organization Position Seed Data...");
                await seedDataRepository.EnsurePositionSeedDataAsync();

                // 4️⃣ ⭐⭐⭐ اطمینان از وجود وضعیت‌های سرنخ CRM
                _logger.LogInformation("🎯 Ensuring CRM Lead Status Seed Data...");
                await seedDataRepository.EnsureCrmLeadStatusSeedDataAsync();

                _logger.LogInformation("✅ System seed data check completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ SystemSeedDataBackgroundService was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SystemSeedDataBackgroundService: {Message}", ex.Message);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("▶️ SystemSeedDataBackgroundService StartAsync called");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏹️ SystemSeedDataBackgroundService StopAsync called");
            return base.StopAsync(cancellationToken);
        }
    }
}
