using MahERP.WebApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// 🏭 Factory برای ثبت خودکار تمام Background Services
    /// </summary>
    public static class BackgroundServicesRegistration
    {
        /// <summary>
        /// ثبت تمام Background Services در یک خط
        /// </summary>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // ⭐⭐⭐ Module Tracking Service (باید قبل از HostedService ثبت شود)
            services.AddSingleton<IModuleTrackingService, ModuleTrackingBackgroundService>();

            // ⭐ Notifications
            services.AddHostedService<NotificationProcessingBackgroundService>();
            services.AddHostedService<ScheduledNotificationBackgroundService>();
            services.AddHostedService<TelegramQueueProcessingBackgroundService>();

            // ⭐ Communications
            services.AddHostedService<EmailBackgroundService>();
            services.AddHostedService<SmsBackgroundService>();
            services.AddHostedService<SmsDeliveryCheckService>();
            services.AddHostedService<TelegramPollingBackgroundService>();

            // ⭐ TaskManagement
            services.AddHostedService<TaskReminderBackgroundService>();
            services.AddHostedService<ScheduledTaskCreationBackgroundService>();

            // ⭐ System
            services.AddHostedService<ExpiredRoleCleanupService>();
            services.AddHostedService<SystemSeedDataBackgroundService>();

            // ⭐⭐⭐ Module Tracking به عنوان HostedService
            services.AddHostedService(sp => (ModuleTrackingBackgroundService)sp.GetRequiredService<IModuleTrackingService>());

            // ⭐⭐⭐ مانیتور
            services.AddHostedService<BackgroundServicesMonitor>();

            return services;
        }

        /// <summary>
        /// ثبت فقط سرویس‌های اعلان
        /// </summary>
        public static IServiceCollection AddNotificationBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<NotificationProcessingBackgroundService>();
            services.AddHostedService<ScheduledNotificationBackgroundService>();
            services.AddHostedService<TelegramQueueProcessingBackgroundService>();
            return services;
        }

        /// <summary>
        /// ثبت فقط سرویس‌های ارتباطی
        /// </summary>
        public static IServiceCollection AddCommunicationBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<EmailBackgroundService>();
            services.AddHostedService<SmsBackgroundService>();
            services.AddHostedService<SmsDeliveryCheckService>();
            services.AddHostedService<TelegramPollingBackgroundService>();
            return services;
        }

        /// <summary>
        /// ثبت فقط سرویس‌های تسک
        /// </summary>
        public static IServiceCollection AddTaskManagementBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<TaskReminderBackgroundService>();
            services.AddHostedService<ScheduledTaskCreationBackgroundService>();
            return services;
        }

        /// <summary>
        /// ثبت فقط سرویس‌های سیستمی
        /// </summary>
        public static IServiceCollection AddSystemBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<ExpiredRoleCleanupService>();
            services.AddHostedService<SystemSeedDataBackgroundService>();
            return services;
        }
    }
}
