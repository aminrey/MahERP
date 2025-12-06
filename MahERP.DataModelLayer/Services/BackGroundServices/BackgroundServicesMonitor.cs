using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// 🎯 مدیریت و نظارت بر تمام Background Services
    /// این کلاس فقط برای لاگ کردن و مانیتورینگ است
    /// </summary>
    public class BackgroundServicesMonitor : IHostedService
    {
        private readonly ILogger<BackgroundServicesMonitor> _logger;

        public BackgroundServicesMonitor(
            ILogger<BackgroundServicesMonitor> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 Background Services Monitor شروع شد");
            _logger.LogInformation("📊 تمام Background Services در حال اجرا هستند");
            
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("🛑 Background Services Monitor در حال توقف...");
            return Task.CompletedTask;
        }
    }
}
