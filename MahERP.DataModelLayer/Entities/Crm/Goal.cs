using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// هدف فروش - هر Contact/Organization می‌تواند چندین هدف داشته باشد
    /// مثال: فروش سایت، فروش دستگاه بازی، ...
    /// </summary>
    [Table("Goal_Tbl")]
    public class Goal
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان هدف
        /// </summary>
        [Required(ErrorMessage = "عنوان هدف الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// نام محصول/خدمت مرتبط
        /// </summary>
        [MaxLength(200)]
        public string? ProductName { get; set; }

        // ========== اتصال به Contact یا Organization ==========

        /// <summary>
        /// شناسه فرد (اختیاری - یکی از ContactId یا OrganizationId باید مقدار داشته باشد)
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// شناسه سازمان (اختیاری)
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        // ========== وضعیت هدف ==========

        /// <summary>
        /// آخرین وضعیت لید در این هدف
        /// بر اساس آخرین تعامل محاسبه می‌شود
        /// </summary>
        public int? CurrentLeadStageStatusId { get; set; }

        [ForeignKey(nameof(CurrentLeadStageStatusId))]
        public virtual LeadStageStatus? CurrentLeadStageStatus { get; set; }

        /// <summary>
        /// آیا هدف به خرید منجر شده؟
        /// </summary>
        public bool IsConverted { get; set; } = false;

        /// <summary>
        /// تاریخ تبدیل به مشتری (خرید)
        /// </summary>
        public DateTime? ConversionDate { get; set; }

        /// <summary>
        /// ارزش تخمینی هدف (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// ارزش واقعی پس از خرید (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? ActualValue { get; set; }

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
        
        /// <summary>
        /// تعاملات مرتبط با این هدف (M:N)
        /// </summary>
        public virtual ICollection<InteractionGoal> InteractionGoals { get; set; } = new HashSet<InteractionGoal>();

        // ========== Computed ==========

        [NotMapped]
        public string TargetName => Contact != null ? Contact.FullName : Organization?.Name ?? "نامشخص";

        [NotMapped]
        public string TargetType => ContactId.HasValue ? "Contact" : "Organization";
    }
}
