using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MahERP.CommonLayer.Interface;
using MahERP.DataModelLayer.Repository;
using Microsoft.Extensions.Configuration;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    public class TelegramPollingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramPollingBackgroundService> _logger;
        private TelegramBotClient _botClient;
        private string _botToken;

        public TelegramPollingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TelegramPollingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🤖 Telegram Polling Background Service starting...");

            // انتظار تا سرویس‌های دیگر راه‌اندازی شوند
            await Task.Delay(5000, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var settings = context.SettingsUW.Get().FirstOrDefault();

            if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken) || !settings.IsTelegramEnabled)
            {
                _logger.LogWarning("⚠️ Telegram bot not configured or disabled");
                return;
            }

            _botToken = settings.TelegramBotToken;
            _botClient = new TelegramBotClient(_botToken);

            try
            {
                // دریافت اطلاعات ربات
                var me = await _botClient.GetMe(stoppingToken);
                _logger.LogInformation($"✅ Telegram bot started: @{me.Username} ({me.FirstName})");

                // ⭐⭐⭐ تنظیمات Polling - اصلاح شده
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = new[] { UpdateType.Message }, // فقط پیام‌ها
                    DropPendingUpdates = true // ⭐ تغییر از ThrowPendingUpdates به DropPendingUpdates
                };

                // شروع Polling
                await _botClient.ReceiveAsync(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting Telegram polling");
            }
        }

        /// <summary>
        /// پردازش پیام‌های دریافتی از تلگرام
        /// </summary>
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"📨 Received update: Type={update.Type}, ID={update.Id}");

                // فقط پیام‌های text
                if (update.Type != UpdateType.Message || update.Message?.Text == null)
                {
                    return;
                }

                var message = update.Message;
                var chatId = message.Chat.Id;
                var text = message.Text;

                _logger.LogInformation($"💬 Message from {chatId}: {text}");

                // پردازش دستور /start
                if (text.StartsWith("/start"))
                {
                    await HandleStartCommand(chatId, text, cancellationToken);
                }
                else if (text.StartsWith("/help"))
                {
                    await HandleHelpCommand(chatId, cancellationToken);
                }
                else if (text.StartsWith("/status"))
                {
                    await HandleStatusCommand(chatId, cancellationToken);
                }
                else
                {
                    // پیام پیش‌فرض
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "❓ دستور نامعتبر.\n\n" +
                              "دستورات موجود:\n" +
                              "/start - شروع و اتصال حساب\n" +
                              "/help - راهنما\n" +
                              "/status - وضعیت اتصال",
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling update");
            }
        }

        /// <summary>
        /// پردازش دستور /start
        /// </summary>
        private async Task HandleStartCommand(long chatId, string text, CancellationToken cancellationToken)
        {
            try
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var startParameter = parts.Length > 1 ? parts[1] : null;

                _logger.LogInformation($"🚀 /start command: chatId={chatId}, parameter={startParameter}");

                if (string.IsNullOrEmpty(startParameter))
                {
                    // پیام خوش‌آمدگویی
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "👋 سلام! به ربات MahERP خوش آمدید.\n\n" +
                              "🔗 برای اتصال حساب خود، لطفاً از طریق نرم‌افزار روی دکمه \"اتصال به تلگرام\" کلیک کنید.",
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                // فراخوانی سرویس برای ثبت Chat ID
                using var scope = _serviceProvider.CreateScope();
                var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramBotSendNotification>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                
                var baseUrl = configuration["AppSettings:BaseUrl"] ?? "http://localhost";
                
                var result = await telegramService.HandleStartCommand(
                    chatId, 
                    startParameter, 
                    _botToken,
                    baseUrl
                );

                _logger.LogInformation($"✅ HandleStartCommand result: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in /start command");
                
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ خطا در پردازش درخواست.\n\nلطفاً دوباره تلاش کنید.",
                    cancellationToken: cancellationToken
                );
            }
        }

        /// <summary>
        /// پردازش دستور /help
        /// </summary>
        private async Task HandleHelpCommand(long chatId, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "📚 راهنمای ربات MahERP:\n\n" +
                      "🔗 /start - اتصال حساب\n" +
                      "📊 /status - وضعیت اتصال\n" +
                      "❓ /help - نمایش این راهنما\n\n" +
                      "💡 برای اتصال حساب خود، از داخل نرم‌افزار روی دکمه \"اتصال به تلگرام\" کلیک کنید.",
                cancellationToken: cancellationToken
            );
        }

        /// <summary>
        /// پردازش دستور /status
        /// </summary>
        private async Task HandleStatusCommand(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                // جستجوی کاربر با این Chat ID
                var user = context.UserManagerUW.Get(u => u.TelegramChatId == chatId).FirstOrDefault();
                
                if (user != null)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"✅ حساب شما متصل است!\n\n" +
                              $"👤 نام: {user.FirstName} {user.LastName}\n" +
                              $"📧 ایمیل: {user.Email ?? "ثبت نشده"}\n" +
                              $"📱 موبایل: {user.PhoneNumber}\n\n" +
                              $"📬 شما تمام اعلان‌های تسک و یادآوری‌ها را در این چت دریافت می‌کنید.",
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "⚠️ حساب شما متصل نیست.\n\n" +
                              "🔗 برای اتصال، از نرم‌افزار روی دکمه \"اتصال به تلگرام\" کلیک کنید.",
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in /status command");
                
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ خطا در بررسی وضعیت.",
                    cancellationToken: cancellationToken
                );
            }
        }

        /// <summary>
        /// پردازش خطاهای Polling
        /// </summary>
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "❌ Telegram polling error");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Telegram Polling Background Service stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}