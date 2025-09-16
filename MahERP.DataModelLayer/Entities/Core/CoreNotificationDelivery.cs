using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// انتیتی تحویل نوتیفیکیشن - برای مدیریت ارسال از طریق روش‌های مختلف
    /// </summary>
    public class CoreNotificationDelivery
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه نوتیفیکیشن اصلی
        /// </summary>
        [Required]
        public int CoreNotificationId { get; set; }
        [ForeignKey("CoreNotificationId")]
        public virtual CoreNotification CoreNotification { get; set; }

        /// <summary>
        /// روش تحویل
        /// 0 = سیستم داخلی (UI)
        /// 1 = ایمیل
        /// 2 = پیامک
        /// 3 = تلگرام
        /// 4 = واتساپ
        /// 5 = پوش نوتیفیکیشن موبایل
        /// 6 = دسکتاپ نوتیفیکیشن
        /// </summary>
        [Required]
        public byte DeliveryMethod { get; set; }

        /// <summary>
        /// آدرس مقصد (ایمیل، شماره تلفن، chat_id و...)
        /// </summary>
        [MaxLength(200)]
        public string? DeliveryAddress { get; set; }

        /// <summary>
        /// وضعیت تحویل
        /// 0 = در انتظار ارسال
        /// 1 = ارسال شده
        /// 2 = تحویل داده شده
        /// 3 = خطا در ارسال
        /// 4 = لغو شده
        /// 5 = مسدود شده
        /// </summary>
        [Required]
        public byte DeliveryStatus { get; set; } = 0;

        /// <summary>
        /// تاریخ تلاش برای ارسال
        /// </summary>
        public DateTime? AttemptDate { get; set; }

        /// <summary>
        /// تاریخ تحویل موفق
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ارسال
        /// </summary>
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// حداکثر تعداد تلاش مجاز
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// متن خطا (در صورت وجود)
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// شناسه خارجی (message_id، tracking_id و...)
        /// </summary>
        [MaxLength(100)]
        public string? ExternalId { get; set; }

        /// <summary>
        /// تاریخ بعدی تلاش (برای retry)
        /// </summary>
        public DateTime? NextRetryDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// آیا این رکورد فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}