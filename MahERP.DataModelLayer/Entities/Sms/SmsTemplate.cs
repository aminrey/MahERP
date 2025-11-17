using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Sms
{
    /// <summary>
    /// قالب‌های آماده پیامک
    /// </summary>
    [Table("SmsTemplate_Tbl")]
    public class SmsTemplate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان قالب
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// متن قالب (با placeholder)
        /// مثال: "سلام {Name}، سفارش شما با کد {OrderId} ثبت شد"
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string MessageTemplate { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// نوع قالب
        /// 0=عمومی, 1=خوش‌آمدگویی, 2=یادآوری, 3=تبریک, 4=اطلاع‌رسانی
        /// </summary>
        public byte TemplateType { get; set; } = 0;

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تعداد دفعات استفاده
        /// </summary>
        public int UsageCount { get; set; } = 0;

        // ========== اطلاعات سیستمی ==========

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(450)]
        public string? CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// مخاطبین این قالب
        /// </summary>
        [InverseProperty(nameof(SmsTemplateRecipient.Template))]
        public virtual ICollection<SmsTemplateRecipient> Recipients { get; set; } = new HashSet<SmsTemplateRecipient>();

        // ========== Computed Properties ==========

        [NotMapped]
        public int TotalRecipients => Recipients?.Count ?? 0;

        [NotMapped]
        public int ContactRecipients => Recipients?.Count(r => r.RecipientType == 0) ?? 0;

        [NotMapped]
        public int OrganizationRecipients => Recipients?.Count(r => r.RecipientType == 1) ?? 0;

        [NotMapped]
        public string TemplateTypeText => TemplateType switch
        {
            0 => "عمومی",
            1 => "خوش‌آمدگویی",
            2 => "یادآوری",
            3 => "تبریک",
            4 => "اطلاع‌رسانی",
            _ => "نامشخص"
        };
    }
}