using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.CommonLayer.Interface
{
    public interface ITelegramBotSendNotification
    {
        public  Task SendNotificationAsync(string message, long? chatId, string botToken);
        public Task SendLogAsync(string message, long? groupId, string botToken);

    }
}
