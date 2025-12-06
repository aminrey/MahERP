using MahERP.DataModelLayer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// 🎯 Interface برای ردیابی ماژول‌های استفاده شده توسط کاربران
    /// </summary>
    public interface IModuleTrackingService
    {
        /// <summary>
        /// اضافه کردن درخواست ردیابی ماژول به صف (Non-blocking)
        /// </summary>
        void EnqueueModuleTracking(string userId, ModuleType moduleType);
    }

    /// <summary>
    /// Background Service برای ذخیره آخرین ماژول استفاده شده کاربران
    /// </summary>
    public class ModuleTrackingBackgroundService : BackgroundService, IModuleTrackingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleTrackingBackgroundService> _logger;

        // صف Thread-Safe برای ذخیره درخواست‌ها
        private readonly ConcurrentQueue<ModuleTrackingRequest> _queue = new();

        // Event برای اطلاع از وجود آیتم جدید در صف
        private readonly SemaphoreSlim _signal = new(0);

        public ModuleTrackingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ModuleTrackingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// اضافه کردن درخواست جدید به صف (Non-blocking)
        /// </summary>
        public void EnqueueModuleTracking(string userId, ModuleType moduleType)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("⚠️ ModuleTracking: UserId is null or empty");
                return;
            }

            var request = new ModuleTrackingRequest
            {
                UserId = userId,
                ModuleType = moduleType,
                Timestamp = DateTime.Now
            };

            _queue.Enqueue(request);
            _signal.Release(); // اعلام وجود آیتم جدید

            _logger.LogDebug("✅ ModuleTracking queued: User={UserId}, Module={ModuleType}",
                userId, moduleType);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 ModuleTrackingBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // منتظر بمان تا آیتم جدیدی در صف باشد
                    await _signal.WaitAsync(stoppingToken);

                    // پردازش تمام آیتم‌های موجود در صف
                    while (_queue.TryDequeue(out var request))
                    {
                        await ProcessModuleTrackingAsync(request, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service در حال متوقف شدن است
                    _logger.LogInformation("⏹️ ModuleTrackingBackgroundService stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in ModuleTrackingBackgroundService main loop");

                    // تاخیر کوتاه قبل از تلاش مجدد
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("🛑 ModuleTrackingBackgroundService stopped");
        }

        /// <summary>
        /// پردازش درخواست ذخیره ماژول
        /// </summary>
        private async Task ProcessModuleTrackingAsync(
            ModuleTrackingRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ایجاد Scope جدید برای دسترسی به Scoped Services
                using var scope = _serviceProvider.CreateScope();
                var moduleAccessService = scope.ServiceProvider
                    .GetRequiredService<IModuleAccessService>();

                // ذخیره آخرین ماژول
                await moduleAccessService.SaveLastUsedModuleAsync(
                    request.UserId,
                    request.ModuleType);

                _logger.LogInformation(
                    "✅ ModuleTracking saved: User={UserId}, Module={ModuleType}, ProcessTime={ProcessTime}ms",
                    request.UserId,
                    request.ModuleType,
                    (DateTime.Now - request.Timestamp).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to save module tracking: User={UserId}, Module={ModuleType}",
                    request.UserId,
                    request.ModuleType);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏸️ ModuleTrackingBackgroundService stopping - processing remaining items...");

            // پردازش آیتم‌های باقی‌مانده در صف
            while (_queue.TryDequeue(out var request))
            {
                await ProcessModuleTrackingAsync(request, cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// مدل درخواست ذخیره ماژول
    /// </summary>
    internal class ModuleTrackingRequest
    {
        public string UserId { get; set; }
        public ModuleType ModuleType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}