using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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



        public async Task SendNotificationAsync(string message, long? chatId,string botToken)
        {


          //   botToken = "7931841421:AAFna2M4CkkktixVeIxWWE1XRruum9j-kY0"; // توکن ربات تلگرام خود را وارد کنید


            long groupId = 0;//YOUR_GROUP_ID; // آ




            var botClient = new TelegramBotClient(botToken);

           

            var keyboardMarkup = new InlineKeyboardMarkup(new[]
            {
    // سه دکمه در یک ردیف
    new []
    {
        InlineKeyboardButton.WithUrl("صفحه وظایف", "https://resnaco.ir/AdminArea/Tasks/IndexTaskS?NotificationId=0"),
        InlineKeyboardButton.WithUrl("صفحه ناتیفیکیشن سایت", "https://resnaco.ir/AdminArea/Notification"),
        InlineKeyboardButton.WithUrl("در حال تست- صفحه مبتط ", "http://resnaco.ir")
    }
});
            // ارسال پیام به همراه دکمه
       if (chatId != null)
            await botClient.SendMessage(chatId.Value, message, replyMarkup: keyboardMarkup);




        }

        public async Task SendLogAsync(string message, long? groupId, string botToken)
        {
             //botToken = "7714757658:AAHCBmczEfPXRYuusLKf8u8j9nGSlOTGe7g"; // توکن ربات تلگرام خود را وارد کنید
             //botToken = "7931841421:AAFna2M4CkkktixVeIxWWE1XRruum9j-kY0"; // توکن ربات تلگرام خود را وارد کنید

            //groupId = -4512252304;
            //groupId = -4512252304;  
            //groupId = -1002457997534;  

            var botClient = new TelegramBotClient(botToken);

            //    //string message = $"{data.Message}\n{data.Link}";

            //    // ارسال پیام به ربات
            //    await botClient.SendTextMessageAsync(chatId, message);

            //    // ارسال پیام به گروه خصوصی
            //await botClient.SendMediaGroupAsync(groupId, message);
            groupId = null; ///غیر فعال کردن ربات تلگرام

            if (groupId != null && groupId != null)
            {
                // ارسال پیام به گروه خصوصی
                //await botClient.SendTextMessageAsync(groupId, message);
                //await botClient.SendTextMessageAsync(groupId, message, ParseMode.Markdown);

                await botClient.SendMessage(
                   chatId: groupId.Value,
                   text: message,
                   parseMode: ParseMode.Markdown // Optional: Use ParseMode.Html or ParseMode.Markdown if needed
               );
            }
          
            //}





        }


    }
}
