using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// تعامل - ثبت هر نوع ارتباط با Contact
    /// </summary>
    [Table("Interaction_Tbl")]
    public class Interaction
    {
        [Key]
        public int Id { get; set; }

        // ========== فرد مرتبط ==========

        /// <summary>
        /// شناسه فرد
        /// </summary>
        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact Contact { get; set; } = null!;

        // ========== سازمان مرتبط (اختیاری) ==========

        /// <summary>
        /// شناسه سازمان (اگر تعامل از طریق سازمان باشد)
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        // ========== نوع تعامل ==========

        /// <summary>
        /// نوع تعامل
        /// </summary>
        [Required(ErrorMessage = "نوع تعامل الزامی است")]
        public int InteractionTypeId { get; set; }

        [ForeignKey(nameof(InteractionTypeId))]
        public virtual InteractionType InteractionType { get; set; } = null!;

        // ========== وضعیت بعد از خرید (اختیاری) ==========

        /// <summary>
        /// وضعیت بعد از خرید (فقط برای مشتریان)
        /// null = تعامل قبل از خرید (لید)
        /// مقدار = تعامل بعد از خرید (مشتری)
        /// </summary>
        public int? PostPurchaseStageId { get; set; }

        [ForeignKey(nameof(PostPurchaseStageId))]
        public virtual PostPurchaseStage? PostPurchaseStage { get; set; }

        // ========== جزئیات تعامل ==========

        /// <summary>
        /// موضوع تعامل
        /// </summary>
        [MaxLength(300)]
        public string? Subject { get; set; }

        /// <summary>
        /// شرح تعامل
        /// </summary>
        [Required(ErrorMessage = "شرح تعامل الزامی است")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ و ساعت تعامل
        /// </summary>
        [Required]
        public DateTime InteractionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// مدت زمان تعامل (دقیقه)
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// نتیجه/خروجی تعامل
        /// </summary>
        [MaxLength(1000)]
        public string? Result { get; set; }

        /// <summary>
        /// اقدام بعدی پیشنهادی
        /// </summary>
        [MaxLength(500)]
        public string? NextAction { get; set; }

        /// <summary>
        /// تاریخ پیشنهادی برای اقدام بعدی
        /// </summary>
        public DateTime? NextActionDate { get; set; }

        // ========== ارجاع/توصیه ==========

        /// <summary>
        /// آیا این تعامل شامل معرفی/توصیه فرد جدید است؟
        /// (وقتی PostPurchaseStageId = Referral باشد)
        /// </summary>
        public bool HasReferral { get; set; } = false;

        /// <summary>
        /// آیا این Contact توسط کسی معرفی شده؟
        /// (برای اولین تعامل با لید)
        /// </summary>
        public bool IsReferred { get; set; } = false;

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
        /// اهداف مرتبط با این تعامل (M:N)
        /// </summary>
        public virtual ICollection<InteractionGoal> InteractionGoals { get; set; } = new HashSet<InteractionGoal>();

        /// <summary>
        /// ارجاع‌هایی که این تعامل به عنوان تعامل توصیه‌کننده است
        /// </summary>
        [InverseProperty(nameof(Referral.ReferrerInteraction))]
        public virtual ICollection<Referral> ReferralsAsReferrer { get; set; } = new HashSet<Referral>();

        /// <summary>
        /// ارجاع‌هایی که این تعامل به عنوان اولین تعامل معرفی‌شده است
        /// </summary>
        [InverseProperty(nameof(Referral.ReferredInteraction))]
        public virtual ICollection<Referral> ReferralsAsReferred { get; set; } = new HashSet<Referral>();
    }
}
