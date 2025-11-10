using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahERP.CommonLayer.Repository;

namespace MahERP.CommonLayer.Interface
{
    public interface ITelegramBotSendNotification 
    { 
        /// <summary>
        /// ارسال پیام به یک چت ای دی یا ایدی خصوصی تلگرام با دکمه‌های پویا
        /// </summary>
        Task SendNotificationAsync(
            string message, 
            long? chatId, 
            string botToken, 
            NotificationContext notificationContext = null);

        /// <summary>
        /// ⭐ Overload برای سازگاری با کد قدیمی
        /// </summary>
        Task SendNotificationAsync(string message, long? chatId, string botToken);
        
        /// <summary>
        /// ارسال پیام به یک گروه تلگرامی 
        /// </summary>
        Task SendLogAsync(string message, long? groupId, string botToken);
        
        /// <summary>
        /// هندل کردن دستور /start برای ثبت Chat ID کاربران
        /// </summary>
        Task<bool> HandleStartCommand(long chatId, string startParameter, string botToken, string apiBaseUrl);
    }
}
