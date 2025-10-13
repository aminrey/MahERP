using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var queueRepo = new EmailQueueRepository(context);
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            var pendingItems = await queueRepo.GetPendingItemsAsync(5);

            foreach (var item in pendingItems)
            {
                try
                {
                    _logger.LogInformation($"📤 در حال ارسال ایمیل به {item.ToEmail}");

                    await queueRepo.MarkAsProcessingAsync(item.Id);

                    // Parse attachments if any
                    var attachments = string.IsNullOrEmpty(item.Attachments)
                        ? null
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(item.Attachments);

                    var result = await emailService.SendEmailAsync(
                        item.ToEmail,
                        item.Subject,
                        item.Body,
                        item.ToName,
                        item.IsHtml,
                        attachments,
                        item.CcEmails,
                        item.BccEmails
                    );

                    if (result.Success)
                    {
                        await queueRepo.MarkAsSuccessAsync(item.Id);
                        _logger.LogInformation($"✅ ایمیل با موفقیت ارسال شد: {item.ToEmail}");
                    }
                    else
                    {
                        await queueRepo.MarkAsFailedAsync(item.Id, result.ErrorMessage);
                        _logger.LogWarning($"⚠️ خطا در ارسال ایمیل: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ خطا در پردازش آیتم {item.Id}");
                    await queueRepo.MarkAsFailedAsync(item.Id, ex.Message);
                }

                await Task.Delay(1000); // 1 second delay
            }

            var stats = await queueRepo.GetStatisticsAsync();
            if (stats.Pending > 0 || stats.Processing > 0)
            {
                _logger.LogInformation(
                    $"📊 آمار صف ایمیل - در صف: {stats.Pending}, موفق: {stats.Completed}, خطا: {stats.Failed}"
                );
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("⚠️ در حال توقف Email Background Service...");
            await base.StopAsync(cancellationToken);
        }
    }
}