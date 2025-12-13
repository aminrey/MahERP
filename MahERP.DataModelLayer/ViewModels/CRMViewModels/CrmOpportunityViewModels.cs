using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    // ========== Pipeline Stage ViewModels ==========

    /// <summary>
    /// ViewModel مرحله Pipeline
    /// </summary>
    public class CrmPipelineStageViewModel
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        [Required(ErrorMessage = "نام مرحله الزامی است")]
        [MaxLength(100)]
        [Display(Name = "نام مرحله")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ترتیب")]
        public int DisplayOrder { get; set; }

        [MaxLength(20)]
        [Display(Name = "رنگ")]
        public string ColorCode { get; set; } = "#4285f4";

        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; }

        [Range(0, 100)]
        [Display(Name = "احتمال موفقیت (%)")]
        public int WinProbability { get; set; }

        [Display(Name = "مرحله برنده")]
        public bool IsWonStage { get; set; }

        [Display(Name = "مرحله باخت")]
        public bool IsLostStage { get; set; }

        [Display(Name = "مرحله پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // آمار
        public int OpportunitiesCount { get; set; }
        public decimal TotalValue { get; set; }
        public string TotalValueFormatted => TotalValue.ToString("N0");
    }

    // ========== Opportunity ViewModels ==========

    /// <summary>
    /// ViewModel فرصت فروش
    /// </summary>
    public class CrmOpportunityViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(300)]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        // شعبه
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        // مرحله
        [Required(ErrorMessage = "مرحله الزامی است")]
        [Display(Name = "مرحله")]
        public int StageId { get; set; }
        public string? StageName { get; set; }
        public string? StageColor { get; set; }
        public int StageProbability { get; set; }

        // سرنخ مبدا
        [Display(Name = "سرنخ مبدا")]
        public int? SourceLeadId { get; set; }
        public string? SourceLeadName { get; set; }

        // مشتری
        [Display(Name = "فرد")]
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;

        // مسئول
        [Required(ErrorMessage = "مسئول الزامی است")]
        [Display(Name = "مسئول")]
        public string AssignedUserId { get; set; } = string.Empty;
        public string? AssignedUserName { get; set; }

        // مالی
        [Display(Name = "ارزش")]
        public decimal? Value { get; set; }
        public string? ValueFormatted { get; set; }

        [Display(Name = "واحد پول")]
        public string Currency { get; set; } = "IRR";

        [Range(0, 100)]
        [Display(Name = "احتمال موفقیت (%)")]
        public int Probability { get; set; }

        [Display(Name = "ارزش وزنی")]
        public decimal? WeightedValue { get; set; }
        public string? WeightedValueFormatted { get; set; }

        // تاریخ‌ها
        [Display(Name = "تاریخ پیش‌بینی بسته شدن")]
        public DateTime? ExpectedCloseDate { get; set; }
        public string? ExpectedCloseDatePersian { get; set; }

        [Display(Name = "تاریخ واقعی بسته شدن")]
        public DateTime? ActualCloseDate { get; set; }
        public string? ActualCloseDatePersian { get; set; }

        public int? DaysToClose { get; set; }

        // دلایل
        [MaxLength(500)]
        [Display(Name = "دلیل از دست دادن")]
        public string? LostReason { get; set; }

        [MaxLength(200)]
        [Display(Name = "رقیب برنده")]
        public string? WinningCompetitor { get; set; }

        // سایر
        [MaxLength(100)]
        [Display(Name = "منبع")]
        public string? Source { get; set; }

        [Display(Name = "برچسب‌ها")]
        public string? Tags { get; set; }
        public List<string> TagsList { get; set; } = new();

        [MaxLength(2000)]
        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        // اقدام بعدی
        [Display(Name = "نوع اقدام بعدی")]
        public CrmNextActionType? NextActionType { get; set; }
        public string? NextActionTypeText { get; set; }

        [Display(Name = "تاریخ اقدام بعدی")]
        public DateTime? NextActionDate { get; set; }
        public string? NextActionDatePersian { get; set; }

        [Display(Name = "یادداشت اقدام")]
        public string? NextActionNote { get; set; }

        public int? NextActionTaskId { get; set; }

        // وضعیت
        public bool IsActive { get; set; } = true;
        public bool IsWon { get; set; }
        public bool IsLost { get; set; }
        public bool IsOpen { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdateDatePersian { get; set; }

        // محصولات
        public List<CrmOpportunityProductViewModel> Products { get; set; } = new();
        public int ProductsCount { get; set; }
        public decimal ProductsTotal { get; set; }

        // فعالیت‌ها
        public List<CrmOpportunityActivityViewModel> RecentActivities { get; set; } = new();
    }

    /// <summary>
    /// ViewModel ایجاد فرصت
    /// </summary>
    public class CrmOpportunityCreateViewModel
    {
        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(300)]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        [Display(Name = "مرحله")]
        public int? StageId { get; set; }

        [Display(Name = "فرد")]
        public int? ContactId { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        [Display(Name = "مسئول")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "ارزش")]
        public decimal? Value { get; set; }

        [Display(Name = "تاریخ پیش‌بینی بسته شدن")]
        public string? ExpectedCloseDatePersian { get; set; }

        [MaxLength(100)]
        [Display(Name = "منبع")]
        public string? Source { get; set; }

        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        // اقدام بعدی
        [Display(Name = "نوع اقدام بعدی")]
        public CrmNextActionType? NextActionType { get; set; }

        [Display(Name = "تاریخ اقدام بعدی")]
        public string? NextActionDatePersian { get; set; }

        [Display(Name = "ساعت")]
        public string? NextActionTime { get; set; }

        [Display(Name = "یادداشت اقدام")]
        public string? NextActionNote { get; set; }

        [Display(Name = "ایجاد تسک")]
        public bool CreateTaskForNextAction { get; set; } = true;
    }

    /// <summary>
    /// ViewModel تبدیل Lead به Opportunity
    /// </summary>
    public class ConvertLeadToOpportunityViewModel
    {
        [Required]
        public int LeadId { get; set; }

        public string? LeadName { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(300)]
        [Display(Name = "عنوان فرصت")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "ارزش تخمینی")]
        public decimal? Value { get; set; }

        [Display(Name = "تاریخ پیش‌بینی بسته شدن")]
        public string? ExpectedCloseDatePersian { get; set; }

        [Display(Name = "مرحله اولیه")]
        public int? StageId { get; set; }

        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        // اقدام بعدی (اجباری!)
        [Required(ErrorMessage = "نوع اقدام بعدی الزامی است")]
        [Display(Name = "نوع اقدام بعدی")]
        public CrmNextActionType NextActionType { get; set; } = CrmNextActionType.Call;

        [Required(ErrorMessage = "تاریخ اقدام بعدی الزامی است")]
        [Display(Name = "تاریخ اقدام بعدی")]
        public string NextActionDatePersian { get; set; } = string.Empty;

        [Display(Name = "ساعت")]
        public string? NextActionTime { get; set; } = "09:00";

        [Display(Name = "یادداشت اقدام")]
        public string? NextActionNote { get; set; }

        [Display(Name = "ایجاد تسک")]
        public bool CreateTaskForNextAction { get; set; } = true;

        // تنظیمات تبدیل
        [Display(Name = "بستن سرنخ")]
        public bool CloseLead { get; set; } = true;

        [Display(Name = "انتقال تعاملات")]
        public bool TransferInteractions { get; set; } = true;

        [Display(Name = "انتقال پیگیری‌ها")]
        public bool TransferFollowUps { get; set; } = true;
    }

    /// <summary>
    /// ViewModel محصول فرصت
    /// </summary>
    public class CrmOpportunityProductViewModel
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "نام محصول الزامی است")]
        [MaxLength(300)]
        [Display(Name = "نام محصول/خدمت")]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "تعداد")]
        public decimal Quantity { get; set; } = 1;

        [Display(Name = "قیمت واحد")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        [Display(Name = "تخفیف (%)")]
        public decimal DiscountPercent { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal TotalAmount { get; set; }

        public string UnitPriceFormatted => UnitPrice.ToString("N0");
        public string TotalAmountFormatted => TotalAmount.ToString("N0");
    }

    /// <summary>
    /// ViewModel فعالیت فرصت
    /// </summary>
    public class CrmOpportunityActivityViewModel
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime ActivityDate { get; set; }
        public string? ActivityDatePersian { get; set; }

        public int? PreviousStageId { get; set; }
        public string? PreviousStageName { get; set; }

        public int? NewStageId { get; set; }
        public string? NewStageName { get; set; }

        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    // ========== Pipeline Board (Kanban) ==========

    /// <summary>
    /// ViewModel نمای Kanban Pipeline
    /// </summary>
    public class CrmPipelineBoardViewModel
    {
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        /// <summary>
        /// مراحل Pipeline
        /// </summary>
        public List<CrmPipelineColumnViewModel> Columns { get; set; } = new();

        /// <summary>
        /// آمار کلی
        /// </summary>
        public CrmPipelineStatisticsViewModel Statistics { get; set; } = new();

        /// <summary>
        /// فیلترهای فعال
        /// </summary>
        public CrmOpportunityFilterViewModel Filters { get; set; } = new();
    }

    /// <summary>
    /// ستون Kanban (هر مرحله)
    /// </summary>
    public class CrmPipelineColumnViewModel
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageColor { get; set; } = "#4285f4";
        public string? StageIcon { get; set; }
        public int WinProbability { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsWonStage { get; set; }
        public bool IsLostStage { get; set; }

        /// <summary>
        /// فرصت‌های این مرحله
        /// </summary>
        public List<CrmOpportunityCardViewModel> Opportunities { get; set; } = new();

        /// <summary>
        /// تعداد فرصت‌ها
        /// </summary>
        public int Count => Opportunities.Count;

        /// <summary>
        /// مجموع ارزش
        /// </summary>
        public decimal TotalValue { get; set; }
        public string TotalValueFormatted => TotalValue.ToString("N0");
    }

    /// <summary>
    /// کارت فرصت در Kanban
    /// </summary>
    public class CrmOpportunityCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }

        public decimal? Value { get; set; }
        public string? ValueFormatted { get; set; }
        public string Currency { get; set; } = "IRR";

        public int Probability { get; set; }
        public decimal? WeightedValue { get; set; }

        public string? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public string? AssignedUserAvatar { get; set; }

        public DateTime? ExpectedCloseDate { get; set; }
        public string? ExpectedCloseDatePersian { get; set; }
        public int? DaysToClose { get; set; }
        public bool IsOverdue => DaysToClose.HasValue && DaysToClose.Value < 0;

        public CrmNextActionType? NextActionType { get; set; }
        public DateTime? NextActionDate { get; set; }
        public string? NextActionDatePersian { get; set; }
        public bool HasPendingAction => NextActionDate.HasValue && NextActionDate.Value.Date <= DateTime.Now.Date;

        public List<string> Tags { get; set; } = new();

        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
    }

    /// <summary>
    /// فیلتر فرصت‌ها
    /// </summary>
    public class CrmOpportunityFilterViewModel
    {
        public string? SearchText { get; set; }
        public int? BranchId { get; set; }
        public int? StageId { get; set; }
        public string? AssignedUserId { get; set; }
        public string? Source { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ExpectedCloseFrom { get; set; }
        public DateTime? ExpectedCloseTo { get; set; }
        public bool? IsOverdue { get; set; }
        public bool IncludeClosed { get; set; }
    }

    /// <summary>
    /// آمار Pipeline
    /// </summary>
    public class CrmPipelineStatisticsViewModel
    {
        public int TotalOpportunities { get; set; }
        public int OpenOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }

        public decimal TotalValue { get; set; }
        public string TotalValueFormatted => TotalValue.ToString("N0");

        public decimal TotalWeightedValue { get; set; }
        public string TotalWeightedValueFormatted => TotalWeightedValue.ToString("N0");

        public decimal WonValue { get; set; }
        public string WonValueFormatted => WonValue.ToString("N0");

        public decimal LostValue { get; set; }
        public string LostValueFormatted => LostValue.ToString("N0");

        public decimal WinRate { get; set; }
        public string WinRateFormatted => $"{WinRate:F1}%";

        public decimal AverageDealSize { get; set; }
        public string AverageDealSizeFormatted => AverageDealSize.ToString("N0");

        public int AverageDaysToClose { get; set; }

        public int OverdueCount { get; set; }
        public int NeedsActionCount { get; set; }

        /// <summary>
        /// ارزش به تفکیک مرحله
        /// </summary>
        public Dictionary<string, decimal> ValueByStage { get; set; } = new();

        /// <summary>
        /// تعداد به تفکیک مرحله
        /// </summary>
        public Dictionary<string, int> CountByStage { get; set; } = new();
    }
}
