using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.Core;
using System.Text.Json;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// ⭐⭐⭐ Background Service برای پردازش صف تلگرام
    /// این سرویس هر 5 ثانیه صف رو چک می‌کنه و پیام‌های Pending رو ارسال می‌کنه
    /// </summary>
    public class TelegramQueueProcessingBackgroundService : BackgroundService
    {
        private readonly ILogger<TelegramQueueProcessingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TelegramQueueProcessingBackgroundService(
            ILogger<TelegramQueueProcessingBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✈️ Telegram Queue Processing Background Service شروع شد");

            // تأخیر اولیه برای اطمینان از راه‌اندازی کامل سیستم
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTelegramQueueAsync(stoppingToken);
                    
                    // هر 5 ثانیه یک بار چک کن
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در حلقه اصلی پردازش صف تلگرام");
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
            }

            _logger.LogInformation("⛔ Telegram Queue Processing Background Service متوقف شد");
        }

        private async Task ProcessTelegramQueueAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var telegramService = new MahERP.CommonLayer.Repository.TelegramBotSendNotification();

            try
            {
                // ⭐ دریافت پیام‌های Pending که زمان Retry آن‌ها رسیده یا برای اولین بار است
                var pendingMessages = await context.TelegramNotificationQueue_Tbl
                    .Where(q => q.IsActive &&
                               q.Status == 0 && // Pending
                               q.RetryCount < q.MaxRetries &&
                               (!q.NextRetryDate.HasValue || q.NextRetryDate.Value <= DateTime.Now))
                    .OrderBy(q => q.Priority) // اولویت بالاتر اول
                    .ThenBy(q => q.CreatedDate) // قدیمی‌تر اول
                    .Take(20) // حداکثر 20 پیام در هر دور
                    .ToListAsync(stoppingToken);

                if (!pendingMessages.Any())
                {
                    return; // صف خالی است
                }

                _logger.LogInformation($"📤 پردازش {pendingMessages.Count} پیام تلگرام از صف...");

                foreach (var message in pendingMessages)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await ProcessSingleTelegramMessageAsync(
                        context,
                        telegramService,
                        message,
                        stoppingToken
                    );

                    // تأخیر کوتاه بین پیام‌ها (جلوگیری از Rate Limit)
                    await Task.Delay(200, stoppingToken);
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در ProcessTelegramQueueAsync");
            }
        }

        private async Task ProcessSingleTelegramMessageAsync(
            AppDbContext context,
            MahERP.CommonLayer.Repository.TelegramBotSendNotification telegramService,
            TelegramNotificationQueue message,
            CancellationToken stoppingToken)
        {
            try
            {
                message.RetryCount++;
                message.LastAttemptDate = DateTime.Now;

                // ⭐ Deserialize Context
                MahERP.CommonLayer.Repository.NotificationContext? notificationContext = null;
                if (!string.IsNullOrEmpty(message.ContextJson))
                {
                    try
                    {
                        notificationContext = JsonSerializer.Deserialize<MahERP.CommonLayer.Repository.NotificationContext>(
                            message.ContextJson
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ خطا در Deserialize کردن Context - QueueId: {message.Id}");
                    }
                }

                // ⭐ ارسال پیام
                await telegramService.SendNotificationAsync(
                    message.Message,
                    message.ChatId,
                    message.BotToken,
                    notificationContext
                );

                // ✅ موفقیت‌آمیز
                message.Status = 1; // Sent
                message.SentDate = DateTime.Now;
                message.ErrorMessage = null;

                _logger.LogInformation($"✅ پیام تلگرام QueueId: {message.Id} ارسال شد (ChatId: {message.ChatId})");

                // ⭐ بروزرسانی CoreNotificationDelivery
                if (message.CoreNotificationId.HasValue)
                {
                    var delivery = await context.CoreNotificationDelivery_Tbl
                        .Where(d => d.CoreNotificationId == message.CoreNotificationId.Value &&
                                   d.DeliveryMethod == 3 && // Telegram
                                   d.DeliveryAddress == message.ChatId.ToString())
                        .FirstOrDefaultAsync(stoppingToken);

                    if (delivery != null)
                    {
                        delivery.DeliveryStatus = 1; // Sent
                        delivery.DeliveryDate = DateTime.Now;
                        delivery.AttemptCount = message.RetryCount;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ارسال تلگرام QueueId: {message.Id} - Attempt {message.RetryCount}/{message.MaxRetries}");

                message.ErrorMessage = ex.Message;

                // ⭐ اگر تلاش‌ها تمام شد، Failed کن
                if (message.RetryCount >= message.MaxRetries)
                {
                    message.Status = 2; // Failed
                    message.IsActive = false;

                    _logger.LogWarning($"⚠️ پیام QueueId: {message.Id} بعد از {message.RetryCount} تلاش Failed شد");

                    // ⭐ بروزرسانی CoreNotificationDelivery
                    if (message.CoreNotificationId.HasValue)
                    {
                        var delivery = await context.CoreNotificationDelivery_Tbl
                            .Where(d => d.CoreNotificationId == message.CoreNotificationId.Value &&
                                       d.DeliveryMethod == 3 &&
                                       d.DeliveryAddress == message.ChatId.ToString())
                            .FirstOrDefaultAsync(stoppingToken);

                        if (delivery != null)
                        {
                            delivery.DeliveryStatus = 3; // Failed
                            delivery.ErrorMessage = $"خطا در ارسال: {ex.Message}";
                            delivery.AttemptCount = message.RetryCount;
                        }
                    }
                }
                else
                {
                    // ⭐ Exponential Backoff برای Retry بعدی
                    var delayMinutes = Math.Pow(2, message.RetryCount); // 2, 4, 8, 16 minutes
                    message.NextRetryDate = DateTime.Now.AddMinutes(delayMinutes);
                    
                    _logger.LogInformation($"🔄 تلاش مجدد QueueId: {message.Id} در {delayMinutes} دقیقه دیگر");
                }
            }
        }
    }
}
