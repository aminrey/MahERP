using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MahERP.CommonLayer.Interface;

namespace MahERP.CommonLayer.Repository
{
    public class TelegramBotSendNotification : ITelegramBotSendNotification
    {
        // ⭐ حذف IHttpClientFactory
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task SendNotificationAsync(string message, long? chatId, string botToken)
        {
            var botClient = new TelegramBotClient(botToken);

            var keyboardMarkup = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithUrl("صفحه وظایف", "https://resnaco.ir/AdminArea/Tasks/IndexTaskS?NotificationId=0"),
                    InlineKeyboardButton.WithUrl("صفحه ناتیفیکیشن سایت", "https://resnaco.ir/AdminArea/Notification"),
                    InlineKeyboardButton.WithUrl("در حال تست- صفحه مبتط ", "http://resnaco.ir")
                }
            });

            if (chatId != null)
                await botClient.SendMessage(chatId.Value, message, replyMarkup: keyboardMarkup);
        }

        public async Task SendLogAsync(string message, long? groupId, string botToken)
        {
            var botClient = new TelegramBotClient(botToken);

            if (groupId != null)
            {
                await botClient.SendMessage(
                   chatId: groupId.Value,
                   text: message,
                   parseMode: ParseMode.Markdown
               );
            }
        }

        /// <summary>
        /// ⭐ هندل کردن دستور /start از کاربران برای ثبت Chat ID
        /// </summary>
        public async Task<bool> HandleStartCommand(long chatId, string startParameter, string botToken, string apiBaseUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(startParameter))
                {
                    // پیام خوش‌آمدگویی عمومی
                    var botClient = new TelegramBotClient(botToken);
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "👋 سلام! به ربات MahERP خوش آمدید.\n\n" +
                              "🔗 برای اتصال حساب خود، لطفاً از طریق نرم‌افزار روی دکمه \"اتصال به تلگرام\" کلیک کنید."
                    );
                    return true;
                }

                // ارسال درخواست به API برای ثبت Chat ID
                var payload = new
                {
                    chatId = chatId,
                    token = startParameter // userId
                };

                // ⭐ استفاده از HttpClient استاتیک
                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var apiUrl = $"{apiBaseUrl}/TaskingArea/UserManager/RegisterTelegramChatId";
                var response = await _httpClient.PostAsync(apiUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var botClientError = new TelegramBotClient(botToken);
                    await botClientError.SendMessage(
                        chatId: chatId,
                        text: "❌ خطا در ارتباط با سرور.\n\n" +
                              "لطفاً دوباره تلاش کنید."
                    );
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TelegramApiResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // ارسال پیام نتیجه به کاربر
                var botClient2 = new TelegramBotClient(botToken);
                
                if (result?.Success == true)
                {
                    await botClient2.SendMessage(
                        chatId: chatId,
                        text: $"✅ سلام {result.UserName}!\n\n" +
                              $"🎉 حساب شما با موفقیت به سیستم MahERP متصل شد.\n\n" +
                              $"📬 از این پس تمام اعلان‌ها و یادآوری‌های شما در این چت نمایش داده می‌شود.\n\n" +
                              $"💡 می‌توانید به نرم‌افزار بازگردید."
                    );
                    return true;
                }
                else if (result?.AlreadyRegistered == true)
                {
                    await botClient2.SendMessage(
                        chatId: chatId,
                        text: $"ℹ️ سلام {result.UserName}!\n\n" +
                              $"✅ حساب شما قبلاً به تلگرام متصل شده است.\n\n" +
                              $"همه چیز آماده است!"
                    );
                    return true;
                }
                else
                {
                    await botClient2.SendMessage(
                        chatId: chatId,
                        text: "❌ خطا در اتصال حساب.\n\n" +
                              "لطفاً دوباره از طریق نرم‌افزار تلاش کنید."
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleStartCommand: {ex.Message}");
                
                try
                {
                    var botClientError = new TelegramBotClient(botToken);
                    await botClientError.SendMessage(
                        chatId: chatId,
                        text: "❌ خطای سیستمی رخ داده است.\n\n" +
                              "لطفاً با پشتیبانی تماس بگیرید."
                    );
                }
                catch
                {
                    // Ignore telegram send error
                }
                
                return false;
            }
        }

        // کلاس کمکی برای دریافت پاسخ API
        private class TelegramApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public bool AlreadyRegistered { get; set; }
            public string UserName { get; set; }
        }
    }
}
