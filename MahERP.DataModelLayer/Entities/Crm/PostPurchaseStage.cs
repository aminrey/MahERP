using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// وضعیت مشتری بعد از خرید (جدول استاتیک - Seeded)
    /// حفظ مشتری، ارجاع/توصیه، وفادارسازی، VIP
    /// </summary>
    [Table("PostPurchaseStage_Tbl")]
    public class PostPurchaseStage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نوع مرحله (Enum)
        /// </summary>
        [Required]
        public PostPurchaseStageType StageType { get; set; }

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
        public virtual ICollection<Interaction> Interactions { get; set; } = new HashSet<Interaction>();
    }
}
