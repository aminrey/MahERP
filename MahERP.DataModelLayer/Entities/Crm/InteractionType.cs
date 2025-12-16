using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// نوع تعامل - تعریف توسط ادمین سیستم (CRUD)
    /// هر نوع تعامل به یک LeadStageStatus وصل است
    /// </summary>
    [Table("InteractionType_Tbl")]
    public class InteractionType
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان نوع تعامل
        /// </summary>
        [Required(ErrorMessage = "عنوان نوع تعامل الزامی است")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// وضعیت لید که این نوع تعامل منجر به آن می‌شود
        /// مثال: "تماس اولیه" → "آگاهی"
        /// مثال: "جلسه دمو" → "ارزیابی"
        /// </summary>
        [Required(ErrorMessage = "وضعیت لید الزامی است")]
        public int LeadStageStatusId { get; set; }

        [ForeignKey(nameof(LeadStageStatusId))]
        public virtual LeadStageStatus LeadStageStatus { get; set; } = null!;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// رنگ (برای UI)
        /// </summary>
        [MaxLength(20)]
        public string? ColorCode { get; set; }

        /// <summary>
        /// آیکون
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== Audit ==========
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        // ========== Navigation ==========
        public virtual ICollection<Interaction> Interactions { get; set; } = new HashSet<Interaction>();
    }
}
