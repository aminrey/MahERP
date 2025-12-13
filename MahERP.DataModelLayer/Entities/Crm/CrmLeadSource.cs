using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// منابع سرنخ CRM - نحوه آشنایی با سرنخ
    /// </summary>
    [Table("CrmLeadSource_Tbl")]
    public class CrmLeadSource
    {
        public CrmLeadSource()
        {
            Leads = new HashSet<CrmLead>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نام منبع (فارسی)
        /// </summary>
        [Required(ErrorMessage = "نام منبع الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// نام انگلیسی
        /// </summary>
        [MaxLength(100)]
        public string? NameEnglish { get; set; }

        /// <summary>
        /// کد منبع (برای یکپارچگی)
        /// </summary>
        [MaxLength(50)]
        public string? Code { get; set; }

        /// <summary>
        /// آیکون FontAwesome
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; } = "fa-globe";

        /// <summary>
        /// کد رنگ
        /// </summary>
        [MaxLength(20)]
        public string? ColorCode { get; set; } = "#6c757d";

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// آیا پیش‌فرض است؟
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// آیا سیستمی است؟ (قابل حذف نیست)
        /// </summary>
        public bool IsSystem { get; set; } = false;

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
        /// سرنخ‌هایی که از این منبع هستند
        /// </summary>
        [InverseProperty(nameof(CrmLead.LeadSource))]
        public virtual ICollection<CrmLead> Leads { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string DisplayName => string.IsNullOrEmpty(NameEnglish) 
            ? Name 
            : $"{Name} ({NameEnglish})";

        [NotMapped]
        public int LeadsCount => Leads?.Count ?? 0;

        [NotMapped]
        public bool CanDelete => !IsSystem && LeadsCount == 0;
    }
}
