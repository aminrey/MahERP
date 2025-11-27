using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// قالب‌های اعلان برای ارسال از طریق کانال‌های مختلف
    /// </summary>
    [Table("NotificationTemplate_Tbl")]
    public class NotificationTemplate
    {
        [Key]
        public int Id { get; set; }

        #region 🔹 اطلاعات اصلی

        /// <summary>
        /// نام قالب
        /// </summary>
        [Required(ErrorMessage = "نام قالب الزامی است")]
        [MaxLength(100)]
        public string TemplateName { get; set; }

        /// <summary>
        /// کد یکتای قالب (برای استفاده برنامه‌نویسی)
        /// </summary>
        [MaxLength(50)]
        public string? TemplateCode { get; set; }

        /// <summary>
        /// توضیحات قالب
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        #endregion

        #region 🔹 نوع رویداد و کانال

        /// <summary>
        /// نوع رویداد (از enum NotificationEventType)
        /// مثال: 1=TaskAssigned, 2=TaskCompleted, 3=TaskDeadlineReminder
        /// </summary>
        [Required(ErrorMessage = "نوع رویداد الزامی است")]
        public byte NotificationEventType { get; set; }

        /// <summary>
        /// کانال ارسال (از enum NotificationChannel)
        /// 0 = System (داخل سیستم - همیشه ارسال می‌شود)
        /// 1 = Email
        /// 2 = SMS
        /// 3 = Telegram
        /// </summary>
        [Required(ErrorMessage = "کانال ارسال الزامی است")]
        public byte Channel { get; set; }

        #endregion

        #region 🔹 محتوای قالب

        /// <summary>
        /// موضوع پیام (فقط برای Email)
        /// </summary>
        [MaxLength(200)]
        public string? Subject { get; set; }

        /// <summary>
        /// محتوای متنی قالب
        /// پشتیبانی از متغیرها: {Title}, {Message}, {ActionUrl}, {Date}, {Time}
        /// </summary>
        [Required(ErrorMessage = "محتوای قالب الزامی است")]
        public string? MessageTemplate { get; set; }

        /// <summary>
        /// محتوای HTML (فقط برای Email)
        /// استفاده از ویرایشگر TinyMCE
        /// </summary>
        public string? BodyHtml { get; set; }

        #endregion

        #region 🔹 تنظیمات دریافت‌کنندگان

        /// <summary>
        /// حالت انتخاب دریافت‌کنندگان
        /// 0 = AllUsers (همه کاربران - پیش‌فرض)
        /// 1 = SpecificUsers (فقط کاربران خاص)
        /// 2 = AllExceptUsers (همه به جز کاربران خاص)
        /// </summary>
        [Required]
        public byte RecipientMode { get; set; } = 0;

        #endregion

        #region 🔹 تنظیمات زمان‌بندی (برای اعلان‌های دوره‌ای)

        /// <summary>
        /// آیا این قالب زمان‌بندی شده است؟
        /// (برای اعلان‌های دوره‌ای مثل اعلان روزانه)
        /// </summary>
        public bool IsScheduled { get; set; } = false;

        /// <summary>
        /// نوع زمان‌بندی
        /// 0 = Manual (دستی - بدون زمان‌بندی)
        /// 1 = Daily (روزانه)
        /// 2 = Weekly (هفتگی)
        /// 3 = Monthly (ماهانه)
        /// 4 = Custom (سفارشی با Cron Expression)
        /// </summary>
        public byte ScheduleType { get; set; } = 0;

        /// <summary>
        /// ساعت اجرا (24 ساعته) - مثال: "08:00"
        /// </summary>
        [MaxLength(5)]
        public string? ScheduledTime { get; set; }

        /// <summary>
        /// روزهای هفته برای اجرا (برای Weekly)
        /// مثال: "1,3,5" = دوشنبه، چهارشنبه، جمعه
        /// 0=یکشنبه، 1=دوشنبه، ..., 6=شنبه
        /// </summary>
        [MaxLength(50)]
        public string? ScheduledDaysOfWeek { get; set; }

        /// <summary>
        /// روز ماه برای اجرا (برای Monthly)
        /// مثال: 1, 15, 30
        /// </summary>
        public int? ScheduledDayOfMonth { get; set; }

        /// <summary>
        /// Cron Expression برای زمان‌بندی پیشرفته
        /// مثال: "0 8 * * 1-5" = هر روز کاری ساعت 8 صبح
        /// </summary>
        [MaxLength(100)]
        public string? CronExpression { get; set; }

        /// <summary>
        /// آخرین زمان اجرای موفق
        /// </summary>
        public DateTime? LastExecutionDate { get; set; }

        /// <summary>
        /// زمان بعدی اجرا (محاسبه شده)
        /// ⚠️ ذخیره در UTC - نمایش با تبدیل به Iran TimeZone
        /// </summary>
        public DateTime? NextExecutionDate { get; set; }

        /// <summary>
        /// آیا زمان‌بندی فعال است؟
        /// </summary>
        public bool IsScheduleEnabled { get; set; } = true;

        #endregion

        #region 🔹 متادیتا و آمار

        /// <summary>
        /// آیا این قالب پیش‌فرض سیستم است؟
        /// قالب‌های سیستمی قابل حذف نیستند
        /// </summary>
        public bool IsSystemTemplate { get; set; } = false;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// نسخه قالب (برای تاریخچه)
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// تعداد دفعات استفاده از این قالب
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// آخرین زمان استفاده
        /// </summary>
        public DateTime? LastUsedDate { get; set; }

        #endregion

        #region 🔹 Audit Fields

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر سازنده
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// شناسه آخرین ویرایش‌کننده
        /// </summary>
        [MaxLength(450)]
        public string? LastModifiedByUserId { get; set; }

        #endregion

        #region 🔹 Navigation Properties

        /// <summary>
        /// کاربر سازنده
        /// </summary>
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers? CreatedBy { get; set; }

        /// <summary>
        /// آخرین ویرایش‌کننده
        /// </summary>
        [ForeignKey(nameof(LastModifiedByUserId))]
        public virtual AppUsers? LastModifiedBy { get; set; }

        /// <summary>
        /// لیست دریافت‌کنندگان این قالب
        /// (زمانی که RecipientMode = 1 یا 2)
        /// </summary>
        public virtual ICollection<NotificationTemplateRecipient> Recipients { get; set; } 
            = new List<NotificationTemplateRecipient>();

        /// <summary>
        /// تاریخچه تغییرات قالب
        /// </summary>
        public virtual ICollection<NotificationTemplateHistory> History { get; set; } 
            = new List<NotificationTemplateHistory>();

        #endregion

        #region 🔹 Helper Properties

        /// <summary>
        /// نام نوع رویداد (برای نمایش)
        /// </summary>
        [NotMapped]
        public string EventTypeName => ((NotificationEventType)NotificationEventType).ToString();

        /// <summary>
        /// نام کانال (برای نمایش)
        /// </summary>
        [NotMapped]
        public string ChannelName => ((NotificationChannel)Channel) switch
        {
            Enums.NotificationChannel.System => "سیستم داخلی",
            Enums.NotificationChannel.Email => "ایمیل",
            Enums.NotificationChannel.Sms => "پیامک",
            Enums.NotificationChannel.Telegram => "تلگرام",
            _ => "نامشخص"
        };

        /// <summary>
        /// نام حالت دریافت‌کنندگان (برای نمایش)
        /// </summary>
        [NotMapped]
        public string RecipientModeName => RecipientMode switch
        {
            0 => "همه کاربران",
            1 => "کاربران خاص",
            2 => "همه به جز...",
            _ => "نامشخص"
        };

        /// <summary>
        /// آیکون کانال
        /// </summary>
        [NotMapped]
        public string ChannelIcon => Channel switch
        {
            0 => "fa-desktop",      // System
            1 => "fa-envelope",     // Email
            2 => "fa-sms",          // SMS
            3 => "fa-telegram",     // Telegram
            _ => "fa-bell"
        };

        /// <summary>
        /// نام نوع زمان‌بندی (برای نمایش)
        /// </summary>
        [NotMapped]
        public string ScheduleTypeName => ScheduleType switch
        {
            0 => "دستی",
            1 => "روزانه",
            2 => "هفتگی",
            3 => "ماهانه",
            4 => "سفارشی",
            _ => "نامشخص"
        };

        /// <summary>
        /// آیا زمان اجرای بعدی رسیده است؟
        /// </summary>
        [NotMapped]
        public bool IsDueForExecution =>
            IsScheduled &&
            IsScheduleEnabled &&
            IsActive &&
            NextExecutionDate.HasValue &&
            NextExecutionDate.Value <= DateTime.Now;

        #endregion
    }
}