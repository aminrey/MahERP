using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.CommonLayer.Interface
{
    public interface ITelegramBotSendNotification 
    { 
        /// <summary>
        /// ارسال پیام به یک چت ای دی یا ایدی خصوصی تلگرام 
        /// </summary>
        public Task SendNotificationAsync(string message, long? chatId, string botToken);
        
        /// <summary>
        /// ارسال پیام به یک گروه تلگرامی 
        /// </summary>
        public Task SendLogAsync(string message, long? groupId, string botToken);
        
        /// <summary>
        /// هندل کردن دستور /start برای ثبت Chat ID کاربران
        /// </summary>
        public Task<bool> HandleStartCommand(long chatId, string startParameter, string botToken, string apiBaseUrl);
    }
}
