using Microsoft.AspNetCore.Mvc;
using MahERP.CommonLayer.Interface;
using MahERP.DataModelLayer.Repository;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MahERP.DataModelLayer.Services;
using Telegram.Bot;

namespace MahERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly ITelegramBotSendNotification _telegramService;
        private readonly IUnitOfWork _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramWebhookController> _logger;

        public TelegramWebhookController(
            ITelegramBotSendNotification telegramService,
            IUnitOfWork context,
            IConfiguration configuration,
            ILogger<TelegramWebhookController> logger)
        {
            _telegramService = telegramService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// ⭐⭐⭐ این endpoint توسط تلگرام فراخوانی می‌شود
        /// URL: https://yourdomain.com/api/TelegramWebhook
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            // ⭐⭐⭐ لاگ کامل request
            _logger.LogInformation($"=== TELEGRAM WEBHOOK RECEIVED ===");
            _logger.LogInformation($"Update ID: {update.Id}");
            _logger.LogInformation($"Update Type: {update.Type}");
            
            try
            {
                if (update.Message != null)
                {
                    _logger.LogInformation($"Message ID: {update.Message.MessageId}");
                    _logger.LogInformation($"Chat ID: {update.Message.Chat.Id}");
                    _logger.LogInformation($"Text: {update.Message.Text}");
                    _logger.LogInformation($"From: {update.Message.From?.Username ?? update.Message.From?.FirstName}");
                }

                // دریافت تنظیمات
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    _logger.LogWarning("⚠️ Telegram bot token not configured");
                    return Ok();
                }

                // فقط پیام‌های text را پردازش می‌کنیم
                if (update.Type != UpdateType.Message || update.Message?.Text == null)
                {
                    _logger.LogInformation("ℹ️ Not a text message, ignoring");
                    return Ok();
                }

                var message = update.Message;
                var chatId = message.Chat.Id;
                var text = message.Text;

                // بررسی دستور /start
                if (text.StartsWith("/start"))
                {
                    var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var startParameter = parts.Length > 1 ? parts[1] : null;

                    _logger.LogInformation($"🚀 /start command detected");
                    _logger.LogInformation($"   Chat ID: {chatId}");
                    _logger.LogInformation($"   Parameter: {startParameter ?? "NONE"}");

                    // دریافت Base URL
                    var baseUrl = _configuration["AppSettings:BaseUrl"];
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        baseUrl = $"{Request.Scheme}://{Request.Host}";
                    }

                    _logger.LogInformation($"🌐 API Base URL: {baseUrl}");

                    var result = await _telegramService.HandleStartCommand(
                        chatId, 
                        startParameter, 
                        settings.TelegramBotToken,
                        baseUrl
                    );

                    _logger.LogInformation($"✅ HandleStartCommand completed with result: {result}");
                }

                _logger.LogInformation($"=== TELEGRAM WEBHOOK PROCESSING COMPLETE ===");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in Telegram webhook processing");
                _logger.LogError($"   Message: {ex.Message}");
                _logger.LogError($"   Stack: {ex.StackTrace}");
                return Ok();
            }
        }

        /// <summary>
        /// GET endpoint برای بررسی وضعیت Webhook
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Ok(new 
                    { 
                        status = "not_configured", 
                        message = "Telegram bot not configured",
                        timestamp = DateTime.UtcNow 
                    });
                }

                var botClient = new Telegram.Bot.TelegramBotClient(settings.TelegramBotToken);
                var webhookInfo = await botClient.GetWebhookInfo();

                return Ok(new 
                { 
                    status = "active", 
                    webhookUrl = webhookInfo.Url,
                    pendingUpdates = webhookInfo.PendingUpdateCount,
                    lastErrorDate = webhookInfo.LastErrorDate,
                    lastErrorMessage = webhookInfo.LastErrorMessage,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    status = "error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}