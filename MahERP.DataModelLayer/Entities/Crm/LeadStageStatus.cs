using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// وضعیت لید در قیف فروش (جدول استاتیک - Seeded)
    /// آگاهی → علاقه‌مندی → ارزیابی → تصمیم‌گیری → خرید
    /// </summary>
    [Table("LeadStageStatus_Tbl")]
    public class LeadStageStatus
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نوع مرحله (Enum)
        /// </summary>
        [Required]
        public LeadStageType StageType { get; set; }

        /// <summary>
        /// عنوان فارسی
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// عنوان انگلیسی
        /// </summary>
        [MaxLength(100)]
        public string? TitleEnglish { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// رنگ (برای UI)
        /// </summary>
        [MaxLength(20)]
        public string ColorCode { get; set; } = "#6c757d";

        /// <summary>
        /// آیکون
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== Navigation ==========
        public virtual ICollection<InteractionType> InteractionTypes { get; set; } = new HashSet<InteractionType>();
    }
}
