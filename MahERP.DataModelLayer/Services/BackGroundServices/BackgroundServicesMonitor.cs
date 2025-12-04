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
        private readonly IEnumerable<IHostedService> _services;

        public BackgroundServicesMonitor(
            ILogger<BackgroundServicesMonitor> logger,
            IEnumerable<IHostedService> services)
        {
            _logger = logger;
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 Background Services Monitor شروع شد");
            _logger.LogInformation($"📊 تعداد سرویس‌های ثبت شده: {_services.Count()}");
            
            foreach (var service in _services)
            {
                var serviceName = service.GetType().Name;
                _logger.LogInformation($"   ✅ {serviceName}");
            }

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("🛑 Background Services Monitor در حال توقف...");
            return Task.CompletedTask;
        }
    }
}
