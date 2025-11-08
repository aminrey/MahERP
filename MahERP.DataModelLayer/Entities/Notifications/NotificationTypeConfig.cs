using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// تنظیمات هر نوع اعلان
    /// </summary>
    public class NotificationTypeConfig
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ارتباط با ماژول
        /// </summary>
        public int ModuleConfigId { get; set; }
        [ForeignKey("ModuleConfigId")]
        public virtual NotificationModuleConfig ModuleConfig { get; set; }

        /// <summary>
        /// کد یکتای نوع اعلان
        /// </summary>
        [Required, MaxLength(100)]
        public string TypeCode { get; set; } // "TASK_ASSIGNED", "TASK_COMPLETED"

        /// <summary>
        /// نام فارسی
        /// </summary>
        [Required, MaxLength(200)]
        public string TypeNameFa { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// نوع اعلان در CoreNotification (NotificationTypeGeneral)
        /// مثال: 9 = اختصاص/انتساب برای TASK_ASSIGNED
        /// </summary>
        public byte CoreNotificationTypeGeneral { get; set; }

        /// <summary>
        /// نوع تخصصی در CoreNotificationDetail
        /// </summary>
        public byte CoreNotificationTypeSpecific { get; set; }

        /// <summary>
        /// آیا این نوع اعلان فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// اولویت پیش‌فرض (0=عادی، 1=مهم، 2=فوری، 3=بحرانی)
        /// </summary>
        public byte DefaultPriority { get; set; } = 0;

        /// <summary>
        /// آیا از ایمیل پشتیبانی می‌کند؟
        /// </summary>
        public bool SupportsEmail { get; set; } = true;

        /// <summary>
        /// آیا از پیامک پشتیبانی می‌کند؟
        /// </summary>
        public bool SupportsSms { get; set; } = true;

        /// <summary>
        /// آیا از تلگرام پشتیبانی می‌کند؟
        /// </summary>
        public bool SupportsTelegram { get; set; } = true;

        /// <summary>
        /// الگوی پیش‌فرض اعلان سیستمی
        /// </summary>
        public int? DefaultSystemNotificationTemplateId { get; set; }

        /// <summary>
        /// الگوی پیش‌فرض ایمیل
        /// </summary>
        public int? DefaultEmailTemplateId { get; set; }

        /// <summary>
        /// الگوی پیش‌فرض پیامک
        /// </summary>
        public int? DefaultSmsTemplateId { get; set; }

        /// <summary>
        /// الگوی پیش‌فرض تلگرام
        /// </summary>
        public int? DefaultTelegramTemplateId { get; set; }

        /// <summary>
        /// ⭐ آیا کاربران می‌توانند این اعلان را در تنظیمات شخصی خود تغییر دهند؟
        /// </summary>
        public bool AllowUserCustomization { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// حالت ارسال اعلان
        /// 0 = AllUsers (همه کاربران - پیش‌فرض)
        /// 1 = SpecificUsers (فقط کاربران مشخص شده)
        /// 2 = AllExceptUsers (همه به جز کاربران مشخص شده)
        /// </summary>
        public byte SendMode { get; set; } = 0;

        /// <summary>
        /// رابطه با جدول دریافت‌کنندگان
        /// </summary>
        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();

    }
}