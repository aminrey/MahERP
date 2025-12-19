using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    #region LeadStageStatus ViewModels

    /// <summary>
    /// ViewModel برای نمایش وضعیت لید
    /// </summary>
    public class LeadStageStatusViewModel
    {
        public int Id { get; set; }
        public LeadStageType StageType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleEnglish { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string ColorCode { get; set; } = "#6c757d";
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region PostPurchaseStage ViewModels

    /// <summary>
    /// ViewModel برای نمایش وضعیت بعد از خرید
    /// </summary>
    public class PostPurchaseStageViewModel
    {
        public int Id { get; set; }
        public PostPurchaseStageType StageType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleEnglish { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string ColorCode { get; set; } = "#6c757d";
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region InteractionType ViewModels

    /// <summary>
    /// ViewModel برای نمایش نوع تعامل
    /// </summary>
    public class InteractionTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(150)]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "وضعیت لید الزامی است")]
        [Display(Name = "وضعیت لید")]
        public int LeadStageStatusId { get; set; }
        
        public string? LeadStageStatusTitle { get; set; }
        public string? LeadStageStatusColor { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(20)]
        [Display(Name = "رنگ")]
        public string? ColorCode { get; set; }

        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // For dropdowns
        public List<LeadStageStatusViewModel> LeadStageStatuses { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای لیست انواع تعامل
    /// </summary>
    public class InteractionTypeListViewModel
    {
        public List<InteractionTypeViewModel> InteractionTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }

    #endregion

    #region Goal ViewModels

    /// <summary>
    /// ViewModel برای نمایش هدف
    /// </summary>
    public class GoalViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(200)]
        [Display(Name = "عنوان هدف")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [MaxLength(200)]
        [Display(Name = "نام محصول/خدمت")]
        public string? ProductName { get; set; }

        // Target
        [Display(Name = "فرد")]
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        // Status
        public int? CurrentLeadStageStatusId { get; set; }
        public string? CurrentLeadStageStatusTitle { get; set; }
        public string? CurrentLeadStageStatusColor { get; set; }

        [Display(Name = "تبدیل شده به مشتری")]
        public bool IsConverted { get; set; }
        
        public DateTime? ConversionDate { get; set; }
        public string? ConversionDatePersian { get; set; }

        [Display(Name = "ارزش تخمینی")]
        public decimal? EstimatedValue { get; set; }
        public string? EstimatedValueFormatted { get; set; }

        [Display(Name = "ارزش واقعی")]
        public decimal? ActualValue { get; set; }
        public string? ActualValueFormatted { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorName { get; set; }

        // Computed
        public string TargetName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public int InteractionsCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد/ویرایش هدف
    /// </summary>
    public class GoalCreateViewModel
    {
        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(200)]
        [Display(Name = "عنوان هدف")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [MaxLength(200)]
        [Display(Name = "نام محصول/خدمت")]
        public string? ProductName { get; set; }

        [Display(Name = "فرد")]
        public int? ContactId { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        [Display(Name = "ارزش تخمینی")]
        public decimal? EstimatedValue { get; set; }
    }

    /// <summary>
    /// ViewModel برای فیلتر اهداف
    /// </summary>
    public class GoalFilterViewModel
    {
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }
        public int? LeadStageStatusId { get; set; }
        public bool? IsConverted { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public string? FromDatePersian { get; set; }
        public string? ToDatePersian { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار اهداف
    /// </summary>
    public class GoalStatisticsViewModel
    {
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int ConvertedGoals { get; set; }
        public int InactiveGoals { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal TotalEstimatedValue { get; set; }
        public decimal TotalActualValue { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست اهداف
    /// </summary>
    public class GoalListViewModel
    {
        public List<GoalViewModel> Goals { get; set; } = new();
        public List<LeadStageStatus> LeadStageStatuses { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public GoalFilterViewModel? Filters { get; set; }
        public GoalStatisticsViewModel? Statistics { get; set; }
    }

    #endregion

    #region Interaction ViewModels

    /// <summary>
    /// ViewModel برای نمایش تعامل
    /// </summary>
    public class InteractionViewModel
    {
        public int Id { get; set; }

        // Contact
        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        [Display(Name = "فرد")]
        public int ContactId { get; set; }

        public string? ContactName { get; set; }
        public ContactType? ContactType { get; set; }
        public string? ContactTypeName { get; set; }

        // Interaction Type
        [Required(ErrorMessage = "نوع تعامل الزامی است")]
        [Display(Name = "نوع تعامل")]
        public int InteractionTypeId { get; set; }

        public string? InteractionTypeName { get; set; }
        public string? InteractionTypeColor { get; set; }
        
        // Lead Stage (از نوع تعامل می‌آید)
        public string? LeadStageName { get; set; }
        public string? LeadStageColor { get; set; }

        // PostPurchaseStage (فقط برای مشتری)
        [Display(Name = "وضعیت بعد از خرید")]
        public int? PostPurchaseStageId { get; set; }
        public string? PostPurchaseStageName { get; set; }
        public string? PostPurchaseStageColor { get; set; }

        // Goals (M:N)
        [Display(Name = "اهداف")]
        public List<int> GoalIds { get; set; } = new();
        public List<GoalViewModel> Goals { get; set; } = new();

        // Details
        [MaxLength(300)]
        [Display(Name = "موضوع")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "شرح تعامل الزامی است")]
        [Display(Name = "شرح تعامل")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "تاریخ و ساعت")]
        public DateTime InteractionDate { get; set; } = DateTime.Now;
        public string? InteractionDatePersian { get; set; }
        public string? InteractionTime { get; set; }

        [Display(Name = "مدت (دقیقه)")]
        public int? DurationMinutes { get; set; }

        [MaxLength(1000)]
        [Display(Name = "نتیجه")]
        public string? Result { get; set; }

        [MaxLength(500)]
        [Display(Name = "اقدام بعدی")]
        public string? NextAction { get; set; }

        [Display(Name = "تاریخ اقدام بعدی")]
        public DateTime? NextActionDate { get; set; }
        public string? NextActionDatePersian { get; set; }

        // Referral
        [Display(Name = "شامل معرفی")]
        public bool HasReferral { get; set; }

        [Display(Name = "معرفی شده")]
        public bool IsReferred { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorName { get; set; }

        // For Form
        public List<InteractionTypeViewModel> InteractionTypes { get; set; } = new();
        public List<PostPurchaseStageViewModel> PostPurchaseStages { get; set; } = new();
        public List<GoalViewModel> AvailableGoals { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای ایجاد تعامل
    /// </summary>
    public class InteractionCreateViewModel
    {
        // ========== Step 1: Branch Selection =========
        [Display(Name = "شعبه")]
        public int? BranchId { get; set; }

        // ========== Step 2: Contact Selection =========
        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        [Display(Name = "فرد")]
        public int ContactId { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        // ========== Step 3: Interaction Details =========
        [Required(ErrorMessage = "نوع تعامل الزامی است")]
        [Display(Name = "نوع تعامل")]
        public int InteractionTypeId { get; set; }

        [Display(Name = "وضعیت بعد از خرید")]
        public int? PostPurchaseStageId { get; set; }

        [Display(Name = "اهداف")]
        public List<int> GoalIds { get; set; } = new();

        [MaxLength(300)]
        [Display(Name = "موضوع")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "شرح تعامل الزامی است")]
        [Display(Name = "شرح تعامل")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "تاریخ تعامل")]
        public string? InteractionDatePersian { get; set; }

        [Display(Name = "ساعت تعامل")]
        public string? InteractionTime { get; set; }

        [Display(Name = "مدت (دقیقه)")]
        public int? DurationMinutes { get; set; }

        [MaxLength(1000)]
        [Display(Name = "نتیجه")]
        public string? Result { get; set; }

        [MaxLength(500)]
        [Display(Name = "اقدام بعدی")]
        public string? NextAction { get; set; }

        [Display(Name = "تاریخ اقدام بعدی")]
        public string? NextActionDatePersian { get; set; }

        // ========== Referral Section ==========

        /// <summary>
        /// آیا این تعامل شامل معرفی فرد جدید است؟ (برای مشتری)
        /// </summary>
        [Display(Name = "معرفی فرد جدید")]
        public bool HasReferral { get; set; }

        /// <summary>
        /// شناسه فرد معرفی‌شده (اگر HasReferral = true)
        /// </summary>
        [Display(Name = "فرد معرفی‌شده")]
        public int? ReferredContactId { get; set; }

        /// <summary>
        /// آیا این لید توسط کسی معرفی شده؟ (برای لید)
        /// </summary>
        [Display(Name = "معرفی شده توسط")]
        public bool IsReferred { get; set; }

        /// <summary>
        /// شناسه معرفی‌کننده (اگر IsReferred = true)
        /// </summary>
        [Display(Name = "معرفی‌کننده")]
        public int? ReferrerContactId { get; set; }

        // ========== Form Data =========
        public List<InteractionTypeViewModel> InteractionTypes { get; set; } = new();
        public List<PostPurchaseStageViewModel> PostPurchaseStages { get; set; } = new();
        public List<GoalViewModel> AvailableGoals { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای فیلتر تعاملات
    /// </summary>
    public class InteractionFilterViewModel
    {
        public int? ContactId { get; set; }
        public int? InteractionTypeId { get; set; }
        public int? GoalId { get; set; }
        public int? PostPurchaseStageId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromDatePersian { get; set; }
        public string? ToDatePersian { get; set; }
        public string? SearchText { get; set; }
        public string? SearchTerm { get; set; }
        public bool IncludeInactive { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست تعاملات
    /// </summary>
    public class InteractionListViewModel
    {
        public List<InteractionViewModel> Interactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public InteractionFilterViewModel Filters { get; set; } = new();
    }

    #endregion

    #region Referral ViewModels

    /// <summary>
    /// ViewModel برای نمایش ارجاع/توصیه
    /// </summary>
    public class ReferralViewModel
    {
        public int Id { get; set; }

        // Referrer (معرفی‌کننده - مشتری)
        public int ReferrerContactId { get; set; }
        public string? ReferrerContactName { get; set; }

        // Referred (معرفی‌شده)
        public int ReferredContactId { get; set; }
        public string? ReferredContactName { get; set; }
        public ContactType? ReferredContactType { get; set; }

        // Interactions
        public int? ReferrerInteractionId { get; set; }
        public int? ReferredInteractionId { get; set; }

        // Details
        public DateTime ReferralDate { get; set; }
        public string? ReferralDatePersian { get; set; }
        public string? Notes { get; set; }

        // Status
        public ReferralStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#6c757d";
        public DateTime? StatusChangeDate { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatorName { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد ارجاع
    /// </summary>
    public class ReferralCreateViewModel
    {
        [Required(ErrorMessage = "توصیه‌کننده الزامی است")]
        [Display(Name = "توصیه‌کننده (مشتری)")]
        public int ReferrerContactId { get; set; }

        [Required(ErrorMessage = "معرفی‌شده الزامی است")]
        [Display(Name = "فرد معرفی‌شده")]
        public int ReferredContactId { get; set; }

        [Display(Name = "تعامل توصیه‌کننده")]
        public int? ReferrerInteractionId { get; set; }

        [Display(Name = "تعامل معرفی‌شده")]
        public int? ReferredInteractionId { get; set; }

        [MaxLength(1000)]
        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }
    }

    #endregion

    #region Stakeholder Interaction View ViewModels

    /// <summary>
    /// ViewModel برای صفحه اصلی مشاهده تعاملات یک Stakeholder
    /// </summary>
    public class StakeholderInteractionPageViewModel
    {
        // انتخاب شعبه و Stakeholder
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        
        // نوع Stakeholder
        public StakeholderType StakeholderType { get; set; }
        
        // اطلاعات Contact (اگر انتخاب شده)
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public ContactType? ContactType { get; set; }
        public string? ContactTypeName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        
        // اطلاعات Organization (اگر انتخاب شده)
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationPhone { get; set; }
        public string? OrganizationAddress { get; set; }
        
        // آمار خلاصه
        public int TotalInteractionsCount { get; set; }
        public int ActiveGoalsCount { get; set; }
        public string? LastInteractionDatePersian { get; set; }
        
        // اهداف مرتبط (برای نمایش Tab ها)
        public List<GoalSummaryViewModel> Goals { get; set; } = new();
        
        // آیا هدفی وجود دارد؟
        public bool HasGoals => Goals.Any();
    }

    /// <summary>
    /// Enum برای نوع Stakeholder
    /// </summary>
    public enum StakeholderType
    {
        Contact = 1,
        Organization = 2
    }

    /// <summary>
    /// ViewModel خلاصه هدف (برای نمایش در Tab)
    /// </summary>
    public class GoalSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? LeadStageTitle { get; set; }
        public string? LeadStageColor { get; set; }
        public bool IsConverted { get; set; }
        public bool IsActive { get; set; }
        public int InteractionsCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست تعاملات یک هدف خاص (Partial)
    /// </summary>
    public class GoalInteractionsViewModel
    {
        public int GoalId { get; set; }
        public string GoalTitle { get; set; } = string.Empty;
        public List<InteractionViewModel> Interactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // فیلترها
        public InteractionFilterViewModel? Filters { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست تعاملات بدون هدف (Partial)
    /// </summary>
    public class StakeholderInteractionsViewModel
    {
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }
        public List<InteractionViewModel> Interactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // فیلترها
        public InteractionFilterViewModel? Filters { get; set; }
    }

    /// <summary>
    /// ViewModel برای 10 تعامل اخیر سیستم (Sidebar)
    /// </summary>
    public class RecentInteractionsViewModel
    {
        public List<RecentInteractionItemViewModel> Interactions { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای یک آیتم تعامل اخیر
    /// </summary>
    public class RecentInteractionItemViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string InteractionTypeName { get; set; } = string.Empty;
        public string? InteractionTypeColor { get; set; }
        public string? InteractionTypeIcon { get; set; }
        public string DatePersian { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty; // مثال: "2 ساعت پیش"
    }

    #endregion

    #region Stakeholder Goal View ViewModels

    /// <summary>
    /// ViewModel برای صفحه اصلی مشاهده اهداف یک Stakeholder
    /// </summary>
    public class StakeholderGoalsPageViewModel
    {
        // انتخاب شعبه و Stakeholder
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        
        // نوع Stakeholder
        public StakeholderType StakeholderType { get; set; }
        
        // اطلاعات Contact (اگر انتخاب شده)
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public ContactType? ContactType { get; set; }
        public string? ContactTypeName { get; set; }
        
        // اطلاعات Organization (اگر انتخاب شده)
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        
        // آمار خلاصه
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int ConvertedGoals { get; set; }
        
        // لیست اهداف
        public List<GoalViewModel> Goals { get; set; } = new();
        
        // آیا هدفی وجود دارد؟
        public bool HasGoals => Goals.Any();
    }

    /// <summary>
    /// ViewModel برای 10 هدف اخیر سیستم (Sidebar)
    /// </summary>
    public class RecentGoalsViewModel
    {
        public List<RecentGoalItemViewModel> Goals { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای یک آیتم هدف اخیر
    /// </summary>
    public class RecentGoalItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty; // "Contact" or "Organization"
        public string? CurrentLeadStageTitle { get; set; }
        public string? CurrentLeadStageColor { get; set; }
        public bool IsConverted { get; set; }
        public string LastChangeDatePersian { get; set; } = string.Empty;
    }

    #endregion

    #region QuickAdd ViewModels

    /// <summary>
    /// ViewModel برای افزودن سریع Contact
    /// </summary>
    public class QuickAddContactViewModel
    {
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "نام")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "شماره تماس")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        [MaxLength(200)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "انتخاب شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        // اگر قرار باشه به سازمان لینک بشه
        public int? OrganizationId { get; set; }
    }

    /// <summary>
    /// ViewModel برای افزودن سریع Organization
    /// </summary>
    public class QuickAddOrganizationViewModel
    {
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [Display(Name = "نام سازمان")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "نام برند")]
        [MaxLength(200)]
        public string? Brand { get; set; }

        [Display(Name = "شماره تماس")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        [MaxLength(200)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "انتخاب شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; } = 0; // پیش‌فرض: شرکت
    }

    /// <summary>
    /// Response بعد از ساخت موفق QuickAdd
    /// </summary>
    public class QuickAddResponseViewModel
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        
        // برای Contact
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        
        // برای Organization
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        
        // برای Select2
        public string? SelectValue { get; set; } // id که باید انتخاب بشه
        public string? SelectText { get; set; }  // متن نمایشی
    }

    #endregion

    // Note: CrmSelectListItem is already defined in QuickLeadEntryViewModel.cs
    // Using the existing one to avoid duplication
}
