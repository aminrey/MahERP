using System;
using System.Threading;
using System.Threading.Tasks;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MahERP.WebApp.Services
{
    /// <summary>
    /// سرویس بروزرسانی خودکار وضعیت تحویل پیامک‌ها
    /// </summary>
    public class SmsDeliveryCheckService : BackgroundService
    {
        private readonly ILogger<SmsDeliveryCheckService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SmsDeliveryCheckService(
            ILogger<SmsDeliveryCheckService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔍 SMS Delivery Check Service شروع به کار کرد");

            // تاخیر اولیه 5 دقیقه
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckPendingDeliveriesAsync();
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // هر 1 ساعت
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در بررسی وضعیت تحویل");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ SMS Delivery Check Service متوقف شد");
        }

        private async Task CheckPendingDeliveriesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var smsService = new SmsService(context);

            _logger.LogInformation("🔍 شروع بررسی وضعیت تحویل پیامک‌ها...");

            int updatedCount = await smsService.UpdatePendingDeliveriesAsync();

            if (updatedCount > 0)
            {
                _logger.LogInformation($"✅ وضعیت {updatedCount} پیامک بروزرسانی شد");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("⚠️ در حال توقف SMS Delivery Check Service...");
            await base.StopAsync(cancellationToken);
        }
    }
}