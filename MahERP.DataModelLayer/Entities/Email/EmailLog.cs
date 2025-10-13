using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Entities.Email
{
    /// <summary>
    /// لاگ ارسال ایمیل‌ها
    /// </summary>
    [Table("EmailLog_Tbl")]
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ایمیل گیرنده
        /// </summary>
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string ToEmail { get; set; }

        /// <summary>
        /// نام گیرنده
        /// </summary>
        [MaxLength(200)]
        public string? ToName { get; set; }

        /// <summary>
        /// موضوع
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Subject { get; set; }

        /// <summary>
        /// متن ایمیل
        /// </summary>
        [Required]
        public string Body { get; set; }

        /// <summary>
        /// آیا HTML است؟
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// CC
        /// </summary>
        [MaxLength(500)]
        public string? CcEmails { get; set; }

        /// <summary>
        /// BCC
        /// </summary>
        [MaxLength(500)]
        public string? BccEmails { get; set; }

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
        /// کاربر سیستم
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUsers? User { get; set; }

        /// <summary>
        /// وضعیت ارسال
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// زمان ارسال
        /// </summary>
        [Required]
        public DateTime SendDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تعداد پیوست‌ها
        /// </summary>
        public int AttachmentCount { get; set; } = 0;

        /// <summary>
        /// حجم کل پیوست‌ها (KB)
        /// </summary>
        public long AttachmentTotalSizeKB { get; set; } = 0;

        /// <summary>
        /// کاربری که ارسال کرده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SenderUserId { get; set; }

        [ForeignKey(nameof(SenderUserId))]
        public virtual AppUsers? Sender { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string RecipientTypeText => RecipientType switch
        {
            0 => "شخص",
            1 => "سازمان",
            2 => "کاربر سیستم",
            _ => "نامشخص"
        };
    }
}