using MahERP.CommonLayer.PublicClasses;
using MahERP.CommonLayer.Repository;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس جامع مدیریت اعلان‌ها
    /// جایگزین CoreNotificationRepository با امکانات کامل
    /// </summary>
    public partial class NotificationManagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationManagementService> _logger;
        private readonly TelegramBotSendNotification _telegramService;

        public NotificationManagementService(
            AppDbContext context,
            ILogger<NotificationManagementService> logger)
        {
            _context = context;
            _logger = logger;
            _telegramService = new TelegramBotSendNotification();
        }
    }
}
