using MahERP.DataModelLayer.Entities.Notifications;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// داده‌های پایه (Seed Data) برای سیستم اعلان‌ها
    /// </summary>
    public static class StaticNotificationSeedData
    {

        /// <summary>
        /// ماژول‌های سیستم اعلان
        /// </summary>
        public static List<NotificationModuleConfig> NotificationModules => new()
        {
            new NotificationModuleConfig
            {
                Id = 1,
                ModuleCode = "TASKING",
                ModuleNameFa = "ماژول تسکینگ",
                ModuleNameEn = "Tasking Module",
                Description = "سیستم مدیریت تسک‌ها و پروژه‌ها",
                ColorCode = "#2196F3",
                IsActive = true,
                DisplayOrder = 1
            }
            // می‌توانید ماژول‌های دیگر را اینجا اضافه کنید
        };

        /// <summary>
        /// انواع اعلان‌ها
        /// </summary>
        public static List<NotificationTypeConfig> NotificationTypes => new()
        {
           

            // 6️⃣ یادآوری سفارشی تسک
            new NotificationTypeConfig
            {
                Id = 6,
                ModuleConfigId = 1,
                TypeCode = "TASK_CUSTOM_REMINDER",
                TypeNameFa = "یادآوری سفارشی تسک",
                Description = "یادآوری‌های سفارشی که کاربر برای تسک‌های خود یا اعضای تیم تنظیم می‌کند",
                CoreNotificationTypeGeneral = 6,
                CoreNotificationTypeSpecific = 15,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 6,
                RelatedEventTypes = "[15]"
            }
        };
    }
}
