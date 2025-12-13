using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Crm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// ⭐⭐⭐ مرحله Pipeline فروش
    /// هر شعبه می‌تواند Pipeline اختصاصی خود را داشته باشد
    /// </summary>
    [Table("CrmPipelineStages")]
    public class CrmPipelineStage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شعبه (هر شعبه Pipeline مجزا دارد)
        /// </summary>
        [Required]
        public int BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        /// <summary>
        /// نام مرحله
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات مرحله
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// رنگ مرحله (برای Kanban)
        /// </summary>
        [MaxLength(20)]
        public string ColorCode { get; set; } = "#4285f4";

        /// <summary>
        /// آیکون مرحله
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// درصد احتمال موفقیت در این مرحله
        /// </summary>
        [Range(0, 100)]
        public int WinProbability { get; set; }

        /// <summary>
        /// آیا مرحله پایانی موفق است؟ (Won)
        /// </summary>
        public bool IsWonStage { get; set; }

        /// <summary>
        /// آیا مرحله پایانی ناموفق است؟ (Lost)
        /// </summary>
        public bool IsLostStage { get; set; }

        /// <summary>
        /// آیا مرحله اولیه (پیش‌فرض) است؟
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر ایجادکننده
        /// </summary>
        [MaxLength(450)]
        public string? CreatorUserId { get; set; }

        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        // Navigation
        public virtual ICollection<CrmOpportunity> Opportunities { get; set; } = new List<CrmOpportunity>();

        // ========== Computed Properties ==========

        /// <summary>
        /// آیا مرحله پایانی است؟
        /// </summary>
        [NotMapped]
        public bool IsFinalStage => IsWonStage || IsLostStage;

        /// <summary>
        /// نام نمایشی با درصد
        /// </summary>
        [NotMapped]
        public string DisplayName => $"{Name} ({WinProbability}%)";
    }

    /// <summary>
    /// ⭐⭐⭐ فرصت فروش (Opportunity)
    /// هر فرصت در یک مرحله از Pipeline قرار دارد
    /// </summary>
    [Table("CrmOpportunities")]
    public class CrmOpportunity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان فرصت
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// شعبه
        /// </summary>
        [Required]
        public int BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        /// <summary>
        /// مرحله فعلی در Pipeline
        /// </summary>
        [Required]
        public int StageId { get; set; }

        [ForeignKey("StageId")]
        public virtual CrmPipelineStage? Stage { get; set; }

        /// <summary>
        /// سرنخ مبدا (اگر از Lead تبدیل شده باشد)
        /// </summary>
        public int? SourceLeadId { get; set; }

        [ForeignKey("SourceLeadId")]
        public virtual CrmLead? SourceLead { get; set; }

        /// <summary>
        /// فرد مرتبط
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey("ContactId")]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// سازمان مرتبط
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// کاربر مسئول
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AssignedUserId { get; set; } = string.Empty;

        [ForeignKey("AssignedUserId")]
        public virtual AppUsers? AssignedUser { get; set; }

        /// <summary>
        /// ارزش فرصت (مبلغ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Value { get; set; }

        /// <summary>
        /// واحد پول
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "IRR";

        /// <summary>
        /// درصد احتمال موفقیت (می‌تواند از Stage یا دستی باشد)
        /// </summary>
        [Range(0, 100)]
        public int Probability { get; set; }

        /// <summary>
        /// ارزش وزنی = Value * Probability / 100
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? WeightedValue { get; set; }

        /// <summary>
        /// تاریخ پیش‌بینی بسته شدن
        /// </summary>
        public DateTime? ExpectedCloseDate { get; set; }

        /// <summary>
        /// تاریخ واقعی بسته شدن
        /// </summary>
        public DateTime? ActualCloseDate { get; set; }

        /// <summary>
        /// دلیل از دست دادن (متن قدیمی - برای سازگاری)
        /// </summary>
        [MaxLength(500)]
        public string? LostReason { get; set; }

        /// <summary>
        /// ⭐ شناسه دلیل از دست رفتن (جدول جدید)
        /// </summary>
        [Display(Name = "دلیل از دست رفتن")]
        public int? LostReasonId { get; set; }

        [ForeignKey(nameof(LostReasonId))]
        public virtual CrmLostReason? LostReasonNavigation { get; set; }

        /// <summary>
        /// ⭐ توضیحات از دست رفتن
        /// </summary>
        [Display(Name = "توضیحات از دست رفتن")]
        [MaxLength(1000)]
        public string? LostReasonNote { get; set; }

        /// <summary>
        /// رقیب برنده (اگر Lost شود)
        /// </summary>
        [MaxLength(200)]
        public string? WinningCompetitor { get; set; }

        /// <summary>
        /// منبع فرصت
        /// </summary>
        [MaxLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// برچسب‌ها
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // ========== Next Action ==========

        /// <summary>
        /// نوع اقدام بعدی
        /// </summary>
        public Enums.CrmNextActionType? NextActionType { get; set; }

        /// <summary>
        /// تاریخ اقدام بعدی
        /// </summary>
        public DateTime? NextActionDate { get; set; }

        /// <summary>
        /// یادداشت اقدام بعدی
        /// </summary>
        [MaxLength(500)]
        public string? NextActionNote { get; set; }

        /// <summary>
        /// شناسه تسک اقدام بعدی
        /// </summary>
        public int? NextActionTaskId { get; set; }

        // ========== Audit ==========

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر ایجادکننده
        /// </summary>
        [MaxLength(450)]
        public string? CreatorUserId { get; set; }

        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// کاربر آخرین بروزرسانی
        /// </summary>
        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        // Navigation
        public virtual ICollection<CrmOpportunityActivity> Activities { get; set; } = new List<CrmOpportunityActivity>();
        public virtual ICollection<CrmOpportunityProduct> Products { get; set; } = new List<CrmOpportunityProduct>();

        // ========== Computed Properties ==========

        /// <summary>
        /// نام نمایشی مشتری
        /// </summary>
        [NotMapped]
        public string CustomerName => Contact?.FullName ?? Organization?.DisplayName ?? "نامشخص";

        /// <summary>
        /// نوع مشتری
        /// </summary>
        [NotMapped]
        public string CustomerType => ContactId.HasValue ? "Contact" : OrganizationId.HasValue ? "Organization" : "Unknown";

        /// <summary>
        /// آیا برنده شده؟
        /// </summary>
        [NotMapped]
        public bool IsWon => Stage?.IsWonStage == true;

        /// <summary>
        /// آیا باخته؟
        /// </summary>
        [NotMapped]
        public bool IsLost => Stage?.IsLostStage == true;

        /// <summary>
        /// آیا هنوز باز است؟
        /// </summary>
        [NotMapped]
        public bool IsOpen => !IsWon && !IsLost && IsActive;

        /// <summary>
        /// ارزش فرمت شده
        /// </summary>
        [NotMapped]
        public string ValueFormatted => Value?.ToString("N0") ?? "-";

        /// <summary>
        /// روزهای باقی‌مانده تا ExpectedCloseDate
        /// </summary>
        [NotMapped]
        public int? DaysToClose => ExpectedCloseDate.HasValue 
            ? (ExpectedCloseDate.Value.Date - DateTime.Now.Date).Days 
            : null;

        /// <summary>
        /// لیست برچسب‌ها
        /// </summary>
        [NotMapped]
        public List<string> TagsList => string.IsNullOrEmpty(Tags)
            ? new List<string>()
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .ToList();
    }

    /// <summary>
    /// فعالیت‌های فرصت فروش
    /// </summary>
    [Table("CrmOpportunityActivities")]
    public class CrmOpportunityActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public virtual CrmOpportunity? Opportunity { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ActivityType { get; set; } = string.Empty;

        /// <summary>
        /// عنوان
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// تاریخ فعالیت
        /// </summary>
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        /// <summary>
        /// مرحله قبلی (برای تغییر مرحله)
        /// </summary>
        public int? PreviousStageId { get; set; }

        /// <summary>
        /// مرحله جدید (برای تغییر مرحله)
        /// </summary>
        public int? NewStageId { get; set; }

        /// <summary>
        /// کاربر انجام‌دهنده
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUsers? User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// محصولات/خدمات فرصت فروش
    /// </summary>
    [Table("CrmOpportunityProducts")]
    public class CrmOpportunityProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public virtual CrmOpportunity? Opportunity { get; set; }

        /// <summary>
        /// شناسه محصول (از ماژول انبار)
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// نام محصول/خدمت (دستی)
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// تعداد
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// قیمت واحد
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// تخفیف (درصد)
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// مبلغ کل
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
