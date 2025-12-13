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
            },

            // 2️⃣ ⭐⭐⭐ ماژول CRM
            new NotificationModuleConfig
            {
                Id = 2,
                ModuleCode = "CRM",
                ModuleNameFa = "ماژول CRM",
                ModuleNameEn = "CRM Module",
                Description = "سیستم مدیریت ارتباط با مشتریان",
                ColorCode = "#4CAF50",
                IsActive = true,
                DisplayOrder = 2
            }
        };

        /// <summary>
        /// ⭐⭐⭐ انواع اعلان‌ها - هر EventType یک TypeConfig جداگانه
        /// </summary>
        public static List<NotificationTypeConfig> NotificationTypes => new()
        {
            #region 📌 ماژول تسکینگ (ModuleConfigId = 1)

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
                RelatedEventTypes = "[13]"
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
                RelatedEventTypes = "[1]"
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
                RelatedEventTypes = "[2]"
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
                RelatedEventTypes = "[3]"
            },

            // 5️⃣ ثبت کامنت جدید
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
                RelatedEventTypes = "[4]"
            },

            // 6️⃣ ویرایش تسک
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
                RelatedEventTypes = "[5]"
            },

            // 7️⃣ تکمیل عملیات
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
                RelatedEventTypes = "[6]"
            },

            // 8️⃣ حذف تسک
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
                RelatedEventTypes = "[7]"
            },

            // 9️⃣ تغییر وضعیت تسک
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
                RelatedEventTypes = "[8]"
            },

            // 🔟 تخصیص عملیات
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
                RelatedEventTypes = "[10]"
            },

            // 1️⃣1️⃣ تغییر اولویت تسک
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
                RelatedEventTypes = "[11]"
            },

            // 1️⃣2️⃣ تخصیص مجدد تسک
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
                RelatedEventTypes = "[12]"
            },

            // 1️⃣3️⃣ ثبت گزارش کار (WorkLog)
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
                RelatedEventTypes = "[14]"
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
                RelatedEventTypes = "[15]"
            },

            #endregion

            #region 📌 ماژول CRM (ModuleConfigId = 2)

            // ═══════════════════════════════════════════════════════════════
            // 🟢 اعلان‌های Lead
            // ═══════════════════════════════════════════════════════════════

            // 1️⃣5️⃣ ایجاد Lead جدید
            new NotificationTypeConfig
            {
                Id = 15,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_CREATED",
                TypeNameFa = "ایجاد Lead جدید",
                Description = "اعلان ایجاد Lead جدید به مدیران فروش",
                CoreNotificationTypeGeneral = 9, // تخصیص/ایجاد
                CoreNotificationTypeSpecific = 101,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 15,
                RelatedEventTypes = "[101]"
            },

            // 1️⃣6️⃣ تخصیص Lead
            new NotificationTypeConfig
            {
                Id = 16,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_ASSIGNED",
                TypeNameFa = "تخصیص Lead",
                Description = "اعلان تخصیص Lead به کاربر مسئول",
                CoreNotificationTypeGeneral = 9, // تخصیص
                CoreNotificationTypeSpecific = 102,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 16,
                RelatedEventTypes = "[102]"
            },

            // 1️⃣7️⃣ تغییر وضعیت Lead
            new NotificationTypeConfig
            {
                Id = 17,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_STATUS_CHANGED",
                TypeNameFa = "تغییر وضعیت Lead",
                Description = "اعلان تغییر وضعیت Lead (سرد، گرم، داغ)",
                CoreNotificationTypeGeneral = 10, // بروزرسانی
                CoreNotificationTypeSpecific = 103,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 17,
                RelatedEventTypes = "[103]"
            },

            // 1️⃣8️⃣ تبدیل Lead به Opportunity
            new NotificationTypeConfig
            {
                Id = 18,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_CONVERTED",
                TypeNameFa = "تبدیل Lead به فرصت",
                Description = "اعلان تبدیل موفق Lead به Opportunity",
                CoreNotificationTypeGeneral = 8, // موفقیت
                CoreNotificationTypeSpecific = 104,
                IsActive = true,
                DefaultPriority = 2,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 18,
                RelatedEventTypes = "[104]"
            },

            // 1️⃣9️⃣ از دست رفتن Lead
            new NotificationTypeConfig
            {
                Id = 19,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_LOST",
                TypeNameFa = "از دست رفتن Lead",
                Description = "اعلان از دست رفتن Lead به مدیران",
                CoreNotificationTypeGeneral = 11, // هشدار
                CoreNotificationTypeSpecific = 105,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 19,
                RelatedEventTypes = "[105]"
            },

            // ═══════════════════════════════════════════════════════════════
            // 🔵 اعلان‌های Opportunity
            // ═══════════════════════════════════════════════════════════════

            // 2️⃣0️⃣ ایجاد Opportunity جدید
            new NotificationTypeConfig
            {
                Id = 20,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_CREATED",
                TypeNameFa = "ایجاد فرصت جدید",
                Description = "اعلان ایجاد فرصت فروش جدید",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 110,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 20,
                RelatedEventTypes = "[110]"
            },

            // 2️⃣1️⃣ تغییر مرحله Pipeline
            new NotificationTypeConfig
            {
                Id = 21,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_STAGE_CHANGED",
                TypeNameFa = "تغییر مرحله فرصت",
                Description = "اعلان انتقال فرصت به مرحله جدید در Pipeline",
                CoreNotificationTypeGeneral = 10,
                CoreNotificationTypeSpecific = 111,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 21,
                RelatedEventTypes = "[111]"
            },

            // 2️⃣2️⃣ برنده شدن فرصت (Won)
            new NotificationTypeConfig
            {
                Id = 22,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_WON",
                TypeNameFa = "برنده شدن فرصت",
                Description = "اعلان موفقیت در بستن فرصت فروش",
                CoreNotificationTypeGeneral = 8, // موفقیت
                CoreNotificationTypeSpecific = 112,
                IsActive = true,
                DefaultPriority = 2,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 22,
                RelatedEventTypes = "[112]"
            },

            // 2️⃣3️⃣ از دست رفتن فرصت (Lost)
            new NotificationTypeConfig
            {
                Id = 23,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_LOST",
                TypeNameFa = "از دست رفتن فرصت",
                Description = "اعلان از دست رفتن فرصت فروش",
                CoreNotificationTypeGeneral = 11, // هشدار
                CoreNotificationTypeSpecific = 113,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 23,
                RelatedEventTypes = "[113]"
            },

            // 2️⃣4️⃣ یادآوری پیگیری فرصت
            new NotificationTypeConfig
            {
                Id = 24,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_FOLLOWUP_REMINDER",
                TypeNameFa = "یادآوری پیگیری فرصت",
                Description = "یادآوری برای پیگیری فرصت‌های باز",
                CoreNotificationTypeGeneral = 6, // یادآوری
                CoreNotificationTypeSpecific = 114,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 24,
                RelatedEventTypes = "[114]"
            },

            // ═══════════════════════════════════════════════════════════════
            // 🟣 اعلان‌های Contact/Organization
            // ═══════════════════════════════════════════════════════════════

            // 2️⃣5️⃣ ایجاد مخاطب جدید
            new NotificationTypeConfig
            {
                Id = 25,
                ModuleConfigId = 2,
                TypeCode = "CRM_CONTACT_CREATED",
                TypeNameFa = "ایجاد مخاطب جدید",
                Description = "اعلان ثبت مخاطب جدید در سیستم",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 120,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 25,
                RelatedEventTypes = "[120]"
            },

            // 2️⃣6️⃣ ایجاد سازمان جدید
            new NotificationTypeConfig
            {
                Id = 26,
                ModuleConfigId = 2,
                TypeCode = "CRM_ORGANIZATION_CREATED",
                TypeNameFa = "ایجاد سازمان جدید",
                Description = "اعلان ثبت سازمان جدید در سیستم",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 121,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 26,
                RelatedEventTypes = "[121]"
            },

            // ═══════════════════════════════════════════════════════════════
            // 🟠 اعلان‌های Activity
            // ═══════════════════════════════════════════════════════════════

            // 2️⃣7️⃣ یادآوری فعالیت
            new NotificationTypeConfig
            {
                Id = 27,
                ModuleConfigId = 2,
                TypeCode = "CRM_ACTIVITY_REMINDER",
                TypeNameFa = "یادآوری فعالیت CRM",
                Description = "یادآوری برای فعالیت‌های برنامه‌ریزی شده",
                CoreNotificationTypeGeneral = 6,
                CoreNotificationTypeSpecific = 130,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 27,
                RelatedEventTypes = "[130]"
            },

            // 2️⃣8️⃣ تخصیص فعالیت
            new NotificationTypeConfig
            {
                Id = 28,
                ModuleConfigId = 2,
                TypeCode = "CRM_ACTIVITY_ASSIGNED",
                TypeNameFa = "تخصیص فعالیت",
                Description = "اعلان تخصیص فعالیت جدید به کاربر",
                CoreNotificationTypeGeneral = 9,
                CoreNotificationTypeSpecific = 131,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 28,
                RelatedEventTypes = "[131]"
            },

            // ═══════════════════════════════════════════════════════════════
            // 🔴 اعلان‌های گزارش و خلاصه
            // ═══════════════════════════════════════════════════════════════

            // 2️⃣9️⃣ گزارش روزانه CRM
            new NotificationTypeConfig
            {
                Id = 29,
                ModuleConfigId = 2,
                TypeCode = "CRM_DAILY_DIGEST",
                TypeNameFa = "گزارش روزانه CRM",
                Description = "خلاصه روزانه فعالیت‌های CRM",
                CoreNotificationTypeGeneral = 0,
                CoreNotificationTypeSpecific = 140,
                IsActive = true,
                DefaultPriority = 0,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 29,
                RelatedEventTypes = "[140]"
            },

            // 3️⃣0️⃣ هشدار عدم فعالیت Lead
            new NotificationTypeConfig
            {
                Id = 30,
                ModuleConfigId = 2,
                TypeCode = "CRM_LEAD_INACTIVE_WARNING",
                TypeNameFa = "هشدار عدم فعالیت Lead",
                Description = "هشدار برای Leadهایی که مدتی بدون پیگیری مانده‌اند",
                CoreNotificationTypeGeneral = 11, // هشدار
                CoreNotificationTypeSpecific = 141,
                IsActive = true,
                DefaultPriority = 1,
                SupportsEmail = true,
                SupportsSms = false,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 30,
                RelatedEventTypes = "[141]"
            },

            // 3️⃣1️⃣ هشدار نزدیک شدن به تاریخ بسته شدن Opportunity
            new NotificationTypeConfig
            {
                Id = 31,
                ModuleConfigId = 2,
                TypeCode = "CRM_OPPORTUNITY_CLOSE_DATE_REMINDER",
                TypeNameFa = "یادآوری تاریخ بسته شدن فرصت",
                Description = "یادآوری نزدیک شدن به Expected Close Date",
                CoreNotificationTypeGeneral = 6,
                CoreNotificationTypeSpecific = 142,
                IsActive = true,
                DefaultPriority = 2,
                SupportsEmail = true,
                SupportsSms = true,
                SupportsTelegram = true,
                AllowUserCustomization = true,
                DisplayOrder = 31,
                RelatedEventTypes = "[142]"
            }

            #endregion
        };
    }
}
