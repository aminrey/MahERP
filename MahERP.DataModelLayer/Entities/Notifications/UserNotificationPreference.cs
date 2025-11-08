using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// تنظیمات شخصی کاربر - فقط برای انواعی که ادمین اجازه داده
    /// </summary>
    public class UserNotificationPreference
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        public int NotificationTypeConfigId { get; set; }
        [ForeignKey("NotificationTypeConfigId")]
        public virtual NotificationTypeConfig NotificationTypeConfig { get; set; }

        /// <summary>
        /// آیا این اعلان برای کاربر فعال است؟
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// دریافت از طریق سیستم (اعلان درون‌برنامه)
        /// </summary>
        public bool ReceiveBySystem { get; set; } = true;

        /// <summary>
        /// دریافت از طریق ایمیل
        /// </summary>
        public bool ReceiveByEmail { get; set; } = true;

        /// <summary>
        /// دریافت از طریق پیامک
        /// </summary>
        public bool ReceiveBySms { get; set; } = false;

        /// <summary>
        /// دریافت از طریق تلگرام
        /// </summary>
        public bool ReceiveByTelegram { get; set; } = true;

        /// <summary>
        /// نوع ارسال (0=فوری، 1=تجمیعی روزانه)
        /// </summary>
        public byte DeliveryMode { get; set; } = 0;

        /// <summary>
        /// زمان ترجیحی ارسال (برای حالت تجمیعی)
        /// </summary>
        public TimeSpan? PreferredDeliveryTime { get; set; }

        /// <summary>
        /// ساعات سکوت - شروع
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// ساعات سکوت - پایان
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// فقط اعلان‌های فوری
        /// </summary>
        public bool OnlyUrgentNotifications { get; set; } = false;

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastModifiedAt { get; set; }
    }
}