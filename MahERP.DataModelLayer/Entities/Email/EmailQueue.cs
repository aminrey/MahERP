using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Email
{
    /// <summary>
    /// صف ارسال ایمیل - برای پردازش Background
    /// </summary>
    [Table("EmailQueue_Tbl")]
    public class EmailQueue
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// آدرس ایمیل گیرنده
        /// </summary>
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string ToEmail { get; set; }

        /// <summary>
        /// نام گیرنده (اختیاری)
        /// </summary>
        [MaxLength(200)]
        public string? ToName { get; set; }

        /// <summary>
        /// موضوع ایمیل
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Subject { get; set; }

        /// <summary>
        /// بدنه ایمیل (HTML یا Plain Text)
        /// </summary>
        [Required]
        public string Body { get; set; }

        /// <summary>
        /// آیا بدنه HTML است؟
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// CC (کپی)
        /// </summary>
        [MaxLength(500)]
        public string? CcEmails { get; set; }

        /// <summary>
        /// BCC (کپی مخفی)
        /// </summary>
        [MaxLength(500)]
        public string? BccEmails { get; set; }

        /// <summary>
        /// مسیر فایل‌های پیوست (JSON Array)
        /// </summary>
        public string? Attachments { get; set; }

        /// <summary>
        /// وضعیت صف
        /// 0=در صف, 1=در حال پردازش, 2=ارسال شده, 3=خطا, 4=لغو شده
        /// </summary>
        public byte Status { get; set; } = 0;

        /// <summary>
        /// اولویت ارسال
        /// </summary>
        public byte Priority { get; set; } = 0;

        /// <summary>
        /// تعداد تلاش‌های انجام شده
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// حداکثر تعداد تلاش
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// زمان ایجاد
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// زمان برنامه‌ریزی شده
        /// </summary>
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// آخرین زمان تلاش
        /// </summary>
        public DateTime? LastAttemptDate { get; set; }

        /// <summary>
        /// زمان پردازش موفق
        /// </summary>
        public DateTime? ProcessedDate { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// کاربری که درخواست ارسال کرده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string RequestedByUserId { get; set; }

        [ForeignKey(nameof(RequestedByUserId))]
        public virtual AppUsers? RequestedBy { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "در صف",
            1 => "در حال پردازش",
            2 => "ارسال شده",
            3 => "خطا",
            4 => "لغو شده",
            _ => "نامشخص"
        };

        [NotMapped]
        public bool CanRetry => RetryCount < MaxRetryCount && Status == 3;

        [NotMapped]
        public bool IsReadyToProcess =>
            Status == 0 &&
            (!ScheduledDate.HasValue || ScheduledDate.Value <= DateTime.Now);
    }
}