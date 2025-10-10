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
        /// ارسال پیام به یک چت ای دی  یا ایدی خصوصی تلگرام 
        /// </summary>
        /// <param name="message"> پیام</param>
        /// <param name="chatId">چت ای دی شخص مورد نظر</param>
        /// <param name="botToken">توکن ربات </param>
        /// <returns></returns>
        public  Task SendNotificationAsync(string message, long? chatId, string botToken);
        /// <summary>
        /// ارسال پیام به یک گروه تلگرامی 
        /// </summary>
        /// <param name="message">پیام </param>
        /// <param name="groupId">پت ای دی گروه تلگرام</param>
        /// <param name="botToken">توکن ربات </param>
        /// <returns></returns>
        public Task SendLogAsync(string message, long? groupId, string botToken);

    }
}
