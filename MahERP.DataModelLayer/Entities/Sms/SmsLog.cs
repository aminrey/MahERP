using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Entities.Sms
{
    /// <summary>
    /// لاگ ارسال پیامک‌ها
    /// </summary>
    [Table("SmsLog_Tbl")]
    public class SmsLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Provider استفاده شده
        /// </summary>
        [Required]
        public int ProviderId { get; set; }

        [ForeignKey(nameof(ProviderId))]
        public virtual SmsProvider? Provider { get; set; }

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
        /// شناسه پیام در سرویس Provider
        /// </summary>
        public long? ProviderMessageId { get; set; }

        /// <summary>
        /// وضعیت ارسال
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام خطا (در صورت عدم موفقیت)
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// وضعیت تحویل
        /// 0=در صف ارسال, 1=ارسال شده, 2=تحویل داده شده, 3=خطا
        /// </summary>
        public int DeliveryStatus { get; set; } = 0;

        /// <summary>
        /// آیا تحویل داده شده؟
        /// </summary>
        public bool IsDelivered { get; set; } = false;

        /// <summary>
        /// زمان ارسال
        /// </summary>
        [Required]
        public DateTime SendDate { get; set; } = DateTime.Now;

        /// <summary>
        /// زمان تحویل (اگر موفق شده باشد)
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// آخرین بررسی وضعیت
        /// </summary>
        public DateTime? LastStatusCheckDate { get; set; }

        /// <summary>
        /// کاربری که ارسال کرده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SenderUserId { get; set; }

        [ForeignKey(nameof(SenderUserId))]
        public virtual AppUsers? Sender { get; set; }

        /// <summary>
        /// شناسه قالب استفاده شده (اختیاری)
        /// </summary>
        public int? TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual SmsTemplate? Template { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string RecipientTypeText => RecipientType switch
        {
            0 => "شخص",
            1 => "سازمان",
            2 => "کاربر سیستم",
            _ => "نامشخص"
        };

        [NotMapped]
        public string DeliveryStatusText => DeliveryStatus switch
        {
            0 => "در صف ارسال",
            1 => "ارسال شده",
            2 => "تحویل داده شده",
            3 => "خطا",
            _ => "نامشخص"
        };
    }
}