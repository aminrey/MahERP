using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// انتیتی تنظیمات نوتیفیکیشن کاربران
    /// برای مدیریت تنظیمات دریافت نوتیفیکیشن برای هر کاربر
    /// </summary>
    public class CoreNotificationSetting
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// شناسه سیستم اصلی ERP
        /// </summary>
        [Required]
        public byte SystemId { get; set; }

        /// <summary>
        /// نوع کلی نوتیفیکیشن
        /// </summary>
        [Required]
        public byte NotificationTypeGeneral { get; set; }

        /// <summary>
        /// آیا نوتیفیکیشن سیستمی فعال است؟
        /// </summary>
        public bool IsSystemEnabled { get; set; } = true;

        /// <summary>
        /// آیا ارسال ایمیل فعال است؟
        /// </summary>
        public bool IsEmailEnabled { get; set; } = false;

        /// <summary>
        /// آیا ارسال پیامک فعال است؟
        /// </summary>
        public bool IsSmsEnabled { get; set; } = false;

        /// <summary>
        /// آیا ارسال تلگرام فعال است؟
        /// </summary>
        public bool IsTelegramEnabled { get; set; } = false;

        /// <summary>
        /// ساعت شروع دریافت نوتیفیکیشن (24 ساعته)
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// ساعت پایان دریافت نوتیفیکیشن (24 ساعته)
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// آیا در روزهای تعطیل نوتیفیکیشن ارسال شود؟
        /// </summary>
        public bool SendOnHolidays { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد تنظیمات
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// آیا این تنظیمات فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}