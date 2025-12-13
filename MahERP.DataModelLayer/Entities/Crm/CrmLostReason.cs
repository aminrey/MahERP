using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// دلایل از دست رفتن سرنخ/فرصت - Lost Reasons
    /// </summary>
    [Table("CrmLostReason_Tbl")]
    public class CrmLostReason
    {
        public CrmLostReason()
        {
            Leads = new HashSet<CrmLead>();
            Opportunities = new HashSet<CrmOpportunity>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان دلیل (فارسی)
        /// </summary>
        [Required(ErrorMessage = "عنوان دلیل الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// عنوان انگلیسی
        /// </summary>
        [MaxLength(200)]
        public string? TitleEnglish { get; set; }

        /// <summary>
        /// کد دلیل
        /// </summary>
        [MaxLength(50)]
        public string? Code { get; set; }

        /// <summary>
        /// نوع: Lead یا Opportunity یا Both
        /// 0 = هر دو
        /// 1 = فقط Lead
        /// 2 = فقط Opportunity
        /// </summary>
        public byte AppliesTo { get; set; } = 0;

        /// <summary>
        /// دسته‌بندی دلیل
        /// 0 = قیمت
        /// 1 = رقابت
        /// 2 = زمان‌بندی
        /// 3 = کیفیت
        /// 4 = نیاز نداشتن
        /// 5 = سایر
        /// </summary>
        public byte Category { get; set; } = 5;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// آیکون FontAwesome
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; } = "fa-times-circle";

        /// <summary>
        /// کد رنگ
        /// </summary>
        [MaxLength(20)]
        public string? ColorCode { get; set; } = "#dc3545";

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// آیا سیستمی است؟ (قابل حذف نیست)
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// آیا نیاز به توضیح اضافی دارد؟
        /// </summary>
        public bool RequiresNote { get; set; } = false;

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
        /// سرنخ‌هایی که با این دلیل از دست رفته‌اند
        /// </summary>
        [InverseProperty(nameof(CrmLead.LostReason))]
        public virtual ICollection<CrmLead> Leads { get; set; }

        /// <summary>
        /// فرصت‌هایی که با این دلیل از دست رفته‌اند
        /// </summary>
        [InverseProperty(nameof(CrmOpportunity.LostReasonNavigation))]
        public virtual ICollection<CrmOpportunity> Opportunities { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string DisplayTitle => string.IsNullOrEmpty(TitleEnglish) 
            ? Title 
            : $"{Title} ({TitleEnglish})";

        [NotMapped]
        public int UsageCount => (Leads?.Count ?? 0) + (Opportunities?.Count ?? 0);

        [NotMapped]
        public bool CanDelete => !IsSystem && UsageCount == 0;

        [NotMapped]
        public string AppliesToText => AppliesTo switch
        {
            0 => "هر دو",
            1 => "فقط Lead",
            2 => "فقط Opportunity",
            _ => "نامشخص"
        };

        [NotMapped]
        public string CategoryText => Category switch
        {
            0 => "قیمت",
            1 => "رقابت",
            2 => "زمان‌بندی",
            3 => "کیفیت",
            4 => "نیاز نداشتن",
            5 => "سایر",
            _ => "نامشخص"
        };
    }
}
