using MahERP.DataModelLayer.Entities.Notifications;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// داده‌های پایه (Seed Data) برای سیستم اعلان‌ها
    /// ⭐⭐⭐ این کلاس توسط SystemSeedDataBackgroundService استفاده می‌شود
    /// </summary>
    public static class StaticNotificationSeedData
    {
        /// <summary>
        /// ⭐⭐⭐ ماژول‌های اعلان
        /// </summary>
        public static List<NotificationModuleConfig> NotificationModules => new()
        {
            // 1️⃣ ماژول تسکینگ
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
        };

        /// <summary>
        /// ⭐⭐⭐ انواع اعلان‌ها - هر EventType یک TypeConfig جداگانه
        /// </summary>
        public static List<NotificationTypeConfig> NotificationTypes => new()
        {
            // 1️⃣ اعلان روزانه (زمان‌بندی شده)
            new NotificationTypeConfig
            {
                Id = 1,
                ModuleConfigId = 1,
                TypeCode = "TASK_DAILY_DIGEST",
                TypeNameFa = "گزارش روزانه تسک‌ها",
                Description = "ارسال خودکار گزارش تسک‌های در حال انجام",
                CoreNotificationTypeGeneral = 0,
                CoreNotificationTypeSpecific = 13,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 1,
                RelatedEventTypes = "[13]" // ⭐ DailyTaskDigest
            },

            // 2️⃣ تخصیص تسک جدید
            new NotificationTypeConfig
            {
                Id = 2,
                ModuleConfigId = 1,
                TypeCode = "TASK_ASSIGNED",
                TypeNameFa = "تخصیص تسک جدید",
                Description = "اعلان هنگام تخصیص تسک جدید به کاربر",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 1,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 2,
                RelatedEventTypes = "[1]" // ⭐ TaskAssigned
            },

            // 3️⃣ تکمیل تسک
            new NotificationTypeConfig
            {
                Id = 3,
                ModuleConfigId = 1,
                TypeCode = "TASK_COMPLETED",
                TypeNameFa = "تکمیل تسک",
                Description = "اعلان تکمیل تسک به سازنده و ناظرین",
                CoreNotificationTypeGeneral = 8,
                CoreNotificationTypeSpecific = 2,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 3,
                RelatedEventTypes = "[2]" // ⭐ TaskCompleted
            },

            // 4️⃣ یادآوری سررسید
            new NotificationTypeConfig
            {
                Id = 4,
                ModuleConfigId = 1,
                TypeCode = "TASK_DEADLINE_REMINDER",
                TypeNameFa = "یادآوری سررسید تسک",
                Description = "یادآوری خودکار تسک‌های نزدیک به سررسید",
                CoreNotificationTypeGeneral = 6,
                CoreNotificationTypeSpecific = 3,
                IsActive = true,
                DefaultPriority = 2,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 4,
                RelatedEventTypes = "[3]" // ⭐ TaskDeadlineReminder
            },

            // 5️⃣ ⭐⭐⭐ NEW: ثبت کامنت جدید
            new NotificationTypeConfig
            {
                Id = 5,
                ModuleConfigId = 1,
                TypeCode = "TASK_COMMENT_ADDED",
                TypeNameFa = "ثبت کامنت جدید",
                Description = "اعلان ثبت کامنت جدید در تسک به اعضا",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 4,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 5,
                RelatedEventTypes = "[4]" // ⭐ TaskCommentAdded
            },

            // 6️⃣ ⭐⭐⭐ NEW: ویرایش تسک
            new NotificationTypeConfig
            {
                Id = 6,
                ModuleConfigId = 1,
                TypeCode = "TASK_UPDATED",
                TypeNameFa = "ویرایش تسک",
                Description = "اعلان ویرایش اطلاعات تسک به اعضا",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 5,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 6,
                RelatedEventTypes = "[5]" // ⭐ TaskUpdated
            },

            // 7️⃣ ⭐⭐⭐ NEW: تکمیل عملیات
            new NotificationTypeConfig
            {
                Id = 7,
                ModuleConfigId = 1,
                TypeCode = "TASK_OPERATION_COMPLETED",
                TypeNameFa = "تکمیل عملیات تسک",
                Description = "اعلان تکمیل یک عملیات خاص در تسک",
                CoreNotificationTypeGeneral = 8,
                CoreNotificationTypeSpecific = 6,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 7,
                RelatedEventTypes = "[6]" // ⭐ TaskOperationCompleted
            },

            // 8️⃣ ⭐⭐⭐ NEW: حذف تسک
            new NotificationTypeConfig
            {
                Id = 8,
                ModuleConfigId = 1,
                TypeCode = "TASK_DELETED",
                TypeNameFa = "حذف تسک",
                Description = "اعلان حذف تسک به اعضای مرتبط",
                CoreNotificationTypeGeneral = 11,
                CoreNotificationTypeSpecific = 7,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 8,
                RelatedEventTypes = "[7]" // ⭐ TaskDeleted
            },

            // 9️⃣ ⭐⭐⭐ NEW: تغییر وضعیت تسک
            new NotificationTypeConfig
            {
                Id = 9,
                ModuleConfigId = 1,
                TypeCode = "TASK_STATUS_CHANGED",
                TypeNameFa = "تغییر وضعیت تسک",
                Description = "اعلان تغییر وضعیت تسک (تایید، رد، در حال انجام، ...)",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 8,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 9,
                RelatedEventTypes = "[8]" // ⭐ TaskStatusChanged
            },

            // 🔟 ⭐⭐⭐ NEW: تخصیص عملیات
            new NotificationTypeConfig
            {
                Id = 10,
                ModuleConfigId = 1,
                TypeCode = "OPERATION_ASSIGNED",
                TypeNameFa = "تخصیص عملیات",
                Description = "اعلان تخصیص عملیات خاص به کاربر",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 9,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 10,
                RelatedEventTypes = "[10]" // ⭐ OperationAssigned (فعلاً استفاده نمیشه)
            },

            // 1️⃣1️⃣ ⭐⭐⭐ NEW: تغییر اولویت تسک
            new NotificationTypeConfig
            {
                Id = 11,
                ModuleConfigId = 1,
                TypeCode = "TASK_PRIORITY_CHANGED",
                TypeNameFa = "تغییر اولویت تسک",
                Description = "اعلان تغییر اولویت تسک به اعضا",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 11,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 11,
                RelatedEventTypes = "[11]" // ⭐ TaskPriorityChanged
            },

            // 1️⃣2️⃣ ⭐⭐⭐ NEW: تخصیص مجدد تسک
            new NotificationTypeConfig
            {
                Id = 12,
                ModuleConfigId = 1,
                TypeCode = "TASK_REASSIGNED",
                TypeNameFa = "تخصیص مجدد تسک",
                Description = "اعلان تخصیص مجدد تسک به کاربر جدید",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 12,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 12,
                RelatedEventTypes = "[12]" // ⭐ TaskReassigned
            },

            // 1️⃣3️⃣ ⭐⭐⭐ NEW: ثبت گزارش کار (WorkLog)
            new NotificationTypeConfig
            {
                Id = 13,
                ModuleConfigId = 1,
                TypeCode = "TASK_WORKLOG_ADDED",
                TypeNameFa = "ثبت گزارش کار",
                Description = "اعلان ثبت گزارش کار در تسک به سازنده و ناظرین",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 14,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 13,
                RelatedEventTypes = "[14]" // ⭐ TaskWorkLog
            },

            // 1️⃣4️⃣ یادآوری سفارشی تسک
            new NotificationTypeConfig
            {
                Id = 14,
                ModuleConfigId = 1,
                TypeCode = "TASK_CUSTOM_REMINDER",
                TypeNameFa = "یادآوری سفارشی تسک",
                Description = "یادآوری‌های سفارشی که کاربر برای تسک‌ها تنظیم می‌کند",
                CoreNotificationTypeGeneral = 6,
                CoreNotificationTypeSpecific = 15,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 14,
                RelatedEventTypes = "[15]" // ⭐ CustomTaskReminder
            }
        };
    }
}
