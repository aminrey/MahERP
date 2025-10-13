using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MahERP.WebApp.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EmailBackgroundService(
            ILogger<EmailBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📧 Email Background Service شروع به کار کرد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailQueueAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // هر 1 دقیقه
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در پردازش صف ایمیل");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ Email Background Service متوقف شد");
        }

        private async Task ProcessEmailQueueAsync()
        {
            // TODO: پیاده‌سازی در آینده
            _logger.LogDebug("📧 بررسی صف ایمیل...");
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("⚠️ در حال توقف Email Background Service...");
            await base.StopAsync(cancellationToken);
        }
    }
}