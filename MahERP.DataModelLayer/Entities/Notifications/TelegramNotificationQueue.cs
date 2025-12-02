using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// صف پیام‌های تلگرام برای ارسال در Background
    /// </summary>
    [Table("TelegramNotificationQueue_Tbl")]
    public class TelegramNotificationQueue
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Chat ID تلگرام
        /// </summary>
        [Required]
        public long ChatId { get; set; }

        /// <summary>
        /// متن پیام
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// توکن ربات
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string BotToken { get; set; }

        /// <summary>
        /// Context برای دکمه‌های پویا (JSON)
        /// </summary>
        public string? ContextJson { get; set; }

        /// <summary>
        /// شناسه CoreNotification مرتبط (nullable)
        /// </summary>
        public int? CoreNotificationId { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        /// <summary>
        /// اولویت (1=عادی, 2=متوسط, 3=بالا)
        /// </summary>
        public byte Priority { get; set; } = 1;

        /// <summary>
        /// وضعیت: 0=Pending, 1=Sent, 2=Failed
        /// </summary>
        public byte Status { get; set; } = 0;

        /// <summary>
        /// تعداد تلاش‌های انجام شده
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// حداکثر تعداد تلاش
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// پیام خطا (در صورت Failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ ارسال موفق
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// آخرین تلاش
        /// </summary>
        public DateTime? LastAttemptDate { get; set; }

        /// <summary>
        /// زمان بعدی تلاش (برای Exponential Backoff)
        /// </summary>
        public DateTime? NextRetryDate { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
