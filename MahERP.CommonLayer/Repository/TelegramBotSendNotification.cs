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

        /// <summary>
        /// ⭐⭐⭐ ارسال اعلان با دکمه‌های پویا بر اساس نوع و محتوا
        /// </summary>
        /// <param name="message">متن پیام</param>
        /// <param name="chatId">شناسه چت</param>
        /// <param name="botToken">توکن ربات</param>
        /// <param name="notificationContext">اطلاعات اضافی برای تولید دکمه‌ها (nullable)</param>
        public async Task SendNotificationAsync(
            string message, 
            long? chatId, 
            string botToken, 
            NotificationContext notificationContext = null)
        {
            if (chatId == null) return;

            var botClient = new TelegramBotClient(botToken);

            // ⭐ تولید دکمه‌های پویا
            var keyboard = BuildDynamicKeyboard(notificationContext);

            await botClient.SendMessage(
                chatId: chatId.Value, 
                text: message, 
                replyMarkup: keyboard,
                parseMode: ParseMode.Html // ⭐ پشتیبانی از HTML formatting
            );
        }

        /// <summary>
        /// ⭐⭐⭐ متد overload برای سازگاری با کد قدیمی
        /// </summary>
        public async Task SendNotificationAsync(string message, long? chatId, string botToken)
        {
            await SendNotificationAsync(message, chatId, botToken, null);
        }

        /// <summary>
        /// ⭐⭐⭐ ساخت دکمه‌های پویا بر اساس Context
        /// </summary>
        private InlineKeyboardMarkup BuildDynamicKeyboard(NotificationContext context)
        {
            var buttons = new List<List<InlineKeyboardButton>>();

            if (context == null || string.IsNullOrEmpty(context.BaseUrl))
            {
                // ⭐ دکمه‌های پیش‌فرض اگر context نداشتیم
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithUrl("📋 لیست تسک‌ها", "https://resnaco.ir/TaskingArea/Tasks/Index")
                });
                return new InlineKeyboardMarkup(buttons);
            }

            // ⭐⭐⭐ سناریو 1: اعلان‌های مرتبط با تسک خاص
            if (!string.IsNullOrEmpty(context.TaskId) && 
                context.EventType != null && 
                context.EventType != 13) // نه DailyTaskDigest
            {
                // ⭐ دکمه مشاهده تسک
                var taskUrl = $"{context.BaseUrl}/TaskingArea/Tasks/Details/{context.TaskId}";
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithUrl("📄 مشاهده تسک", taskUrl)
                });

                // ⭐ دکمه ثبت کامنت (برای اعلان‌های خاص)
                if (IsCommentableEvent(context.EventType.Value))
                {
                    var commentUrl = $"{context.BaseUrl}/TaskingArea/Tasks/Details/{context.TaskId}#comments";
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithUrl("💬 ثبت کامنت", commentUrl)
                    });
                }
            }

            // ⭐⭐⭐ سناریو 2: اعلان روزانه تسک‌های انجام نشده (DailyTaskDigest)
            if (context.EventType == 13 || context.HasPendingTasksList)
            {
                // ⭐ دکمه لیست تمام تسک‌های در حال انجام
                var myTasksUrl = $"{context.BaseUrl}/TaskingArea/Tasks/MyTasks";
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithUrl("📋 مشاهده لیست تسک‌ها", myTasksUrl)
                });

                // ⭐ دکمه تسک‌های امروز
                var todayTasksUrl = $"{context.BaseUrl}/TaskingArea/Tasks/MyTasks?filter=today";
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithUrl("📅 تسک‌های امروز", todayTasksUrl)
                });
            }

            // ⭐⭐⭐ سناریو 3: اعلان‌های یادآوری سررسید
            if (context.EventType == 3) // TaskDeadlineReminder
            {
                if (!string.IsNullOrEmpty(context.TaskId))
                {
                    var taskUrl = $"{context.BaseUrl}/TaskingArea/Tasks/Details/{context.TaskId}";
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithUrl("⚠️ مشاهده تسک", taskUrl),
                        InlineKeyboardButton.WithUrl("✅ تکمیل شد", $"{context.BaseUrl}/TaskingArea/Tasks/CompleteTask/{context.TaskId}")
                    });
                }
            }

            // ⭐ دکمه پیش‌فرض برای دسترسی سریع
            if (!buttons.Any())
            {
                var dashboardUrl = $"{context.BaseUrl}/TaskingArea/Dashboard";
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithUrl("🏠 داشبورد", dashboardUrl)
                });
            }

            // ⭐ همیشه دکمه لیست اعلان‌ها را اضافه کن
            var notificationsUrl = $"{context.BaseUrl}/AppCoreArea/Notification";
            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithUrl("🔔 همه اعلان‌ها", notificationsUrl)
            });

            return new InlineKeyboardMarkup(buttons);
        }

        /// <summary>
        /// ⭐ بررسی اینکه رویداد قابل کامنت است یا خیر
        /// </summary>
        private bool IsCommentableEvent(byte eventType)
        {
            return eventType switch
            {
                1 => true,  // TaskAssigned
                5 => true,  // TaskUpdated
                8 => true,  // TaskStatusChanged
                11 => true, // TaskPriorityChanged
                12 => true, // TaskReassigned
                _ => false
            };
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

    /// <summary>
    /// ⭐⭐⭐ کلاس Context برای تولید دکمه‌های پویا
    /// </summary>
    public class NotificationContext
    {
        /// <summary>
        /// URL پایه سیستم (مثل: https://yourdomain.com)
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// شناسه تسک (اگر مرتبط با تسک باشد)
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// نوع رویداد از Enum NotificationEventType
        /// </summary>
        public byte? EventType { get; set; }

        /// <summary>
        /// آیا پیام شامل لیست تسک‌های انجام نشده است؟
        /// </summary>
        public bool HasPendingTasksList { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده (برای لینک‌های شخصی‌سازی شده)
        /// </summary>
        public string? RecipientUserId { get; set; }

        /// <summary>
        /// عنوان تسک (برای متن دکمه)
        /// </summary>
        public string? TaskTitle { get; set; }

        /// <summary>
        /// اولویت تسک (برای نمایش ایموجی مناسب)
        /// </summary>
        public byte? TaskPriority { get; set; }
    }
}
