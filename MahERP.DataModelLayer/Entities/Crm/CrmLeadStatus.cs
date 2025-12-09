using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// وضعیت‌های سرنخ CRM (پویا با CRUD)
    /// </summary>
    [Table("CrmLeadStatus_Tbl")]
    public class CrmLeadStatus
    {
        public CrmLeadStatus()
        {
            Leads = new HashSet<CrmLead>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان وضعیت (فارسی)
        /// </summary>
        [Required(ErrorMessage = "عنوان وضعیت الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// عنوان انگلیسی
        /// </summary>
        [MaxLength(100)]
        public string? TitleEnglish { get; set; }

        /// <summary>
        /// کد رنگ (برای نمایش در UI)
        /// </summary>
        [MaxLength(20)]
        public string? ColorCode { get; set; } = "#6c757d";

        /// <summary>
        /// آیکون FontAwesome
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; } = "fa-circle";

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// آیا این وضعیت پیش‌فرض است؟ (برای سرنخ‌های جدید)
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// آیا وضعیت نهایی است؟ (مشتری شد / از دست رفت)
        /// </summary>
        public bool IsFinal { get; set; } = false;

        /// <summary>
        /// آیا نتیجه مثبت است؟ (مشتری شد = true، از دست رفت = false)
        /// فقط برای وضعیت‌های نهایی معنادار است
        /// </summary>
        public bool IsPositive { get; set; } = false;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== Audit Fields ==========

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// سرنخ‌هایی که این وضعیت را دارند
        /// </summary>
        [InverseProperty(nameof(CrmLead.Status))]
        public virtual ICollection<CrmLead> Leads { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string DisplayTitle => string.IsNullOrEmpty(TitleEnglish) 
            ? Title 
            : $"{Title} ({TitleEnglish})";

        [NotMapped]
        public int LeadsCount => Leads?.Count ?? 0;

        /// <summary>
        /// کلاس CSS بر اساس رنگ
        /// </summary>
        [NotMapped]
        public string BadgeClass
        {
            get
            {
                if (IsFinal && IsPositive) return "bg-success";
                if (IsFinal && !IsPositive) return "bg-danger";
                if (IsDefault) return "bg-primary";
                return "bg-secondary";
            }
        }
    }
}
