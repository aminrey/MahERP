using Microsoft.AspNetCore.Mvc;
using MahERP.DataModelLayer.Repository;
using Telegram.Bot;
using MahERP.DataModelLayer.Services;

namespace MahERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramDebugController : ControllerBase
    {
        private readonly IUnitOfWork _context;
        private readonly ILogger<TelegramDebugController> _logger;

        public TelegramDebugController(IUnitOfWork context, ILogger<TelegramDebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// بررسی وضعیت Webhook
        /// GET: api/TelegramDebug/webhook-status
        /// </summary>
        [HttpGet("webhook-status")]
        public async Task<IActionResult> GetWebhookStatus()
        {
            try
            {
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Ok(new 
                    { 
                        status = "error",
                        message = "Bot token not configured in database" 
                    });
                }

                var botClient = new TelegramBotClient(settings.TelegramBotToken);
                
                // دریافت اطلاعات ربات
                var me = await botClient.GetMe();
                
                // دریافت وضعیت webhook
                var webhookInfo = await botClient.GetWebhookInfo();

                return Ok(new 
                { 
                    status = "success",
                    botInfo = new
                    {
                        id = me.Id,
                        username = me.Username,
                        firstName = me.FirstName
                    },
                    webhookInfo = new
                    {
                        url = webhookInfo.Url,
                        hasCustomCertificate = webhookInfo.HasCustomCertificate,
                        pendingUpdateCount = webhookInfo.PendingUpdateCount,
                        lastErrorDate = webhookInfo.LastErrorDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        lastErrorMessage = webhookInfo.LastErrorMessage,
                        maxConnections = webhookInfo.MaxConnections,
                        allowedUpdates = webhookInfo.AllowedUpdates
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webhook status");
                return Ok(new 
                { 
                    status = "error",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// تنظیم مجدد Webhook
        /// POST: api/TelegramDebug/set-webhook
        /// </summary>
        [HttpPost("set-webhook")]
        public async Task<IActionResult> SetWebhook([FromBody] SetWebhookRequest request)
        {
            try
            {
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Ok(new 
                    { 
                        status = "error",
                        message = "Bot token not configured" 
                    });
                }

                var botClient = new TelegramBotClient(settings.TelegramBotToken);
                
                var webhookUrl = request.WebhookUrl ?? $"{Request.Scheme}://{Request.Host}/api/TelegramWebhook";
                
                await botClient.SetWebhook(webhookUrl);
                
                // بررسی وضعیت جدید
                var webhookInfo = await botClient.GetWebhookInfo();

                return Ok(new 
                { 
                    status = "success",
                    message = "Webhook set successfully",
                    webhookUrl = webhookInfo.Url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting webhook");
                return Ok(new 
                { 
                    status = "error",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// حذف Webhook (برای تست local)
        /// POST: api/TelegramDebug/delete-webhook
        /// </summary>
        [HttpPost("delete-webhook")]
        public async Task<IActionResult> DeleteWebhook()
        {
            try
            {
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Ok(new { status = "error", message = "Bot token not configured" });
                }

                var botClient = new TelegramBotClient(settings.TelegramBotToken);
                await botClient.DeleteWebhook();

                return Ok(new { status = "success", message = "Webhook deleted" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "error", message = ex.Message });
            }
        }

        /// <summary>
        /// تست ارسال پیام به Chat ID
        /// POST: api/TelegramDebug/send-test-message
        /// </summary>
        [HttpPost("send-test-message")]
        public async Task<IActionResult> SendTestMessage([FromBody] SendTestMessageRequest request)
        {
            try
            {
                var settings = _context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Ok(new { status = "error", message = "Bot token not configured" });
                }

                var botClient = new TelegramBotClient(settings.TelegramBotToken);
                
                var message = await botClient.SendMessage(
                    chatId: request.ChatId,
                    text: request.Message ?? "🧪 Test message from MahERP"
                );

                return Ok(new 
                { 
                    status = "success",
                    message = "Message sent",
                    messageId = message.MessageId
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "error", message = ex.Message });
            }
        }
    }

    public class SetWebhookRequest
    {
        public string? WebhookUrl { get; set; }
    }

    public class SendTestMessageRequest
    {
        public long ChatId { get; set; }
        public string? Message { get; set; }
    }
}