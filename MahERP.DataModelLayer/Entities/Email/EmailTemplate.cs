using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Email
{
    /// <summary>
    /// قالب‌های ایمیل HTML
    /// </summary>
    [Table("EmailTemplate_Tbl")]
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان قالب
        /// </summary>
        [Required(ErrorMessage = "عنوان قالب الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// موضوع ایمیل (با placeholder)
        /// </summary>
        [Required(ErrorMessage = "موضوع ایمیل الزامی است")]
        [MaxLength(500)]
        public string SubjectTemplate { get; set; }

        /// <summary>
        /// متن HTML قالب (با placeholder و استایل inline)
        /// </summary>
        [Required(ErrorMessage = "محتوای HTML الزامی است")]
        public string BodyHtml { get; set; }

        /// <summary>
        /// متن Plain Text برای ایمیل‌هایی که HTML را پشتیبانی نمی‌کنند
        /// </summary>
        public string BodyPlainText { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// دسته‌بندی قالب
        /// 0=عمومی, 1=خوش‌آمدگویی, 2=یادآوری, 3=تبریک, 4=اطلاع‌رسانی, 5=فاکتور
        /// </summary>
        public byte Category { get; set; } = 0;

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تعداد دفعات استفاده
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// پیش‌نمایش تصویر (thumbnail)
        /// </summary>
        [MaxLength(500)]
        public string ThumbnailPath { get; set; }

        // ========== اطلاعات سیستمی ==========

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers LastUpdater { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// مخاطبین این قالب
        /// </summary>
        [InverseProperty(nameof(EmailTemplateRecipient.Template))]
        public virtual ICollection<EmailTemplateRecipient> Recipients { get; set; } = new HashSet<EmailTemplateRecipient>();

        // ========== Computed Properties ==========

        [NotMapped]
        public string CategoryText => Category switch
        {
            0 => "عمومی",
            1 => "خوش‌آمدگویی",
            2 => "یادآوری",
            3 => "تبریک",
            4 => "اطلاع‌رسانی",
            5 => "فاکتور",
            _ => "نامشخص"
        };

        [NotMapped]
        public int TotalRecipients => Recipients?.Count ?? 0;
    }
}