using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Entities.Sms
{
    /// <summary>
    /// صف ارسال پیامک - برای پردازش Background
    /// </summary>
    [Table("SmsQueue_Tbl")]
    public class SmsQueue
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شماره گیرنده
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// متن پیام
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; }

        /// <summary>
        /// نوع گیرنده: 0=شخص, 1=سازمان, 2=کاربر سیستم
        /// </summary>
        public byte RecipientType { get; set; }

        /// <summary>
        /// شناسه شخص (Contact)
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// شناسه سازمان (Organization)
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// کاربر سیستم (AppUser)
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUsers? User { get; set; }

        /// <summary>
        /// Provider مورد نظر (اختیاری - null = پیش‌فرض)
        /// </summary>
        public int? ProviderId { get; set; }

        [ForeignKey(nameof(ProviderId))]
        public virtual SmsProvider? Provider { get; set; }

        /// <summary>
        /// وضعیت صف
        /// 0=در صف, 1=در حال پردازش, 2=ارسال شده, 3=خطا, 4=لغو شده
        /// </summary>
        public byte Status { get; set; } = 0;

        /// <summary>
        /// اولویت ارسال (0=عادی, 1=متوسط, 2=بالا, 3=فوری)
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
        /// زمان برنامه‌ریزی شده برای ارسال (اختیاری)
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
        /// شناسه لاگ ارسال (پس از موفقیت)
        /// </summary>
        public int? SmsLogId { get; set; }

        [ForeignKey(nameof(SmsLogId))]
        public virtual SmsLog? SmsLog { get; set; }

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
        public string PriorityText => Priority switch
        {
            0 => "عادی",
            1 => "متوسط",
            2 => "بالا",
            3 => "فوری",
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