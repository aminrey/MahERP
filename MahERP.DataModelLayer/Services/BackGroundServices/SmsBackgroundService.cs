using System;
using System.Threading;
using System.Threading.Tasks;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MahERP.WebApp.Services
{
    public class SmsBackgroundService : BackgroundService
    {
        private readonly ILogger<SmsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public SmsBackgroundService(
            ILogger<SmsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 SMS Background Service شروع به کار کرد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSmsQueueAsync();
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // هر 30 ثانیه
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در پردازش صف پیامک");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ SMS Background Service متوقف شد");
        }

        private async Task ProcessSmsQueueAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var queueRepo = new SmsQueueRepository(context);
            var smsService = new SmsService(context);

            // پردازش پیامک‌های جدید
            var pendingItems = await queueRepo.GetPendingItemsAsync(10);

            foreach (var item in pendingItems)
            {
                try
                {
                    _logger.LogInformation($"📤 در حال ارسال پیامک به {item.PhoneNumber}");

                    // علامت‌گذاری به عنوان "در حال پردازش"
                    await queueRepo.MarkAsProcessingAsync(item.Id);

                    // ارسال پیامک
                    var log = await smsService.SendToContactAsync(
                        item.ContactId ?? 0,
                        item.MessageText,
                        item.RequestedByUserId,
                        item.ProviderId
                    );

                    if (log.IsSuccess)
                    {
                        await queueRepo.MarkAsSuccessAsync(item.Id, log.Id);
                        _logger.LogInformation($"✅ پیامک با موفقیت ارسال شد: {item.PhoneNumber}");
                    }
                    else
                    {
                        await queueRepo.MarkAsFailedAsync(item.Id, log.ErrorMessage);
                        _logger.LogWarning($"⚠️ خطا در ارسال پیامک: {log.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در پردازش آیتم {item.Id}");
                    await queueRepo.MarkAsFailedAsync(item.Id, ex.Message);
                }

                // تاخیر کوتاه بین ارسال‌ها
                await Task.Delay(500);
            }

            // پردازش تلاش‌های مجدد
            var retryItems = await queueRepo.GetRetryableItemsAsync(5);

            foreach (var item in retryItems)
            {
                try
                {
                    _logger.LogInformation($"🔄 تلاش مجدد برای ارسال به {item.PhoneNumber} (تلاش {item.RetryCount + 1})");

                    await queueRepo.MarkAsProcessingAsync(item.Id);

                    var log = await smsService.SendToContactAsync(
                        item.ContactId ?? 0,
                        item.MessageText,
                        item.RequestedByUserId,
                        item.ProviderId
                    );

                    if (log.IsSuccess)
                    {
                        await queueRepo.MarkAsSuccessAsync(item.Id, log.Id);
                        _logger.LogInformation($"✅ تلاش مجدد موفق بود");
                    }
                    else
                    {
                        await queueRepo.MarkAsFailedAsync(item.Id, log.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در تلاش مجدد");
                    await queueRepo.MarkAsFailedAsync(item.Id, ex.Message);
                }

                await Task.Delay(500);
            }

            // لاگ آمار
            var stats = await queueRepo.GetStatisticsAsync();
            if (stats.Pending > 0 || stats.Processing > 0)
            {
                _logger.LogInformation(
                    $"📊 آمار صف - در صف: {stats.Pending}, در حال پردازش: {stats.Processing}, موفق: {stats.Completed}, خطا: {stats.Failed}"
                );
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("⚠️ در حال توقف SMS Background Service...");
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}