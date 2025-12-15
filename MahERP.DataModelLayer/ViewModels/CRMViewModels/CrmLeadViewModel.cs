using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ViewModel برای نمایش سرنخ
    /// </summary>
    public class CrmLeadViewModel
    {
        public int Id { get; set; }

        // اتصال به Core
        [Display(Name = "فرد")]
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        // شعبه و کاربر
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        [Required(ErrorMessage = "کاربر مسئول الزامی است")]
        [Display(Name = "کاربر مسئول")]
        public string AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        // وضعیت
        [Required(ErrorMessage = "وضعیت الزامی است")]
        [Display(Name = "وضعیت")]
        public int StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? StatusColor { get; set; }
        public string? StatusIcon { get; set; }

        // اطلاعات تکمیلی
        [MaxLength(100)]
        [Display(Name = "منبع سرنخ")]
        public string? Source { get; set; }

        [Range(0, 100)]
        [Display(Name = "امتیاز")]
        public int Score { get; set; }

        [MaxLength(2000)]
        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        [Display(Name = "برچسب‌ها")]
        public string? Tags { get; set; }
        public List<string> TagsList { get; set; } = new();

        [Display(Name = "تاریخ آخرین تماس")]
        public DateTime? LastContactDate { get; set; }
        public string? LastContactDatePersian { get; set; }

        [Display(Name = "تاریخ پیگیری بعدی")]
        public DateTime? NextFollowUpDate { get; set; }
        public string? NextFollowUpDatePersian { get; set; }

        [Display(Name = "ارزش تخمینی")]
        public decimal? EstimatedValue { get; set; }
        public string? EstimatedValueFormatted { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // Computed
        public string? LeadType { get; set; }
        public bool IsContact { get; set; }
        public bool IsOrganization { get; set; }
        public string? DisplayName { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? PrimaryEmail { get; set; }
        public int InteractionsCount { get; set; }
        public int PendingFollowUpsCount { get; set; }
        public bool NeedsFollowUp { get; set; }
        public int? DaysSinceLastContact { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdateDatePersian { get; set; }
        public string? LastUpdaterName { get; set; }

        // Navigation
        public List<CrmLeadInteractionViewModel> RecentInteractions { get; set; } = new();
        public List<CrmFollowUpViewModel> PendingFollowUps { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای ایجاد/ویرایش سرنخ
    /// </summary>
    public class CrmLeadCreateViewModel
    {
        // انتخاب نوع سرنخ
        [Required(ErrorMessage = "نوع سرنخ را انتخاب کنید")]
        [Display(Name = "نوع سرنخ")]
        public string LeadType { get; set; } = "Contact"; // Contact یا Organization

        [Display(Name = "فرد")]
        public int? ContactId { get; set; }

        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        // ایجاد فرد/سازمان جدید
        public bool CreateNewEntity { get; set; }

        // اطلاعات فرد جدید
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [Display(Name = "شماره موبایل")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        // اطلاعات سازمان جدید
        [Display(Name = "نام سازمان")]
        public string? OrganizationName { get; set; }

        [Display(Name = "شماره تلفن سازمان")]
        public string? OrganizationPhone { get; set; }

        // شعبه
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        // کاربر مسئول
        [Display(Name = "کاربر مسئول")]
        public string? AssignedUserId { get; set; }

        // وضعیت
        [Display(Name = "وضعیت")]
        public int? StatusId { get; set; }

        // اطلاعات تکمیلی
        [MaxLength(100)]
        [Display(Name = "منبع سرنخ")]
        public string? Source { get; set; }

        [MaxLength(2000)]
        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        [Display(Name = "برچسب‌ها")]
        public string? Tags { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست سرنخ‌ها
    /// </summary>
    public class CrmLeadListViewModel
    {
        public List<CrmLeadViewModel> Leads { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // فیلترها
        public CrmLeadFilterViewModel Filters { get; set; } = new();

        // آمار
        public CrmLeadStatisticsViewModel Statistics { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای فیلتر سرنخ‌ها
    /// </summary>
    public class CrmLeadFilterViewModel
    {
        public string? SearchText { get; set; }
        public int? BranchId { get; set; }
        public string? AssignedUserId { get; set; }
        public int? StatusId { get; set; }
        public string? Source { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? NeedsFollowUp { get; set; }
        public bool IncludeInactive { get; set; }
        public string? LeadType { get; set; } // Contact / Organization
    }

    /// <summary>
    /// ViewModel برای آمار سرنخ‌ها
    /// </summary>
    public class CrmLeadStatisticsViewModel
    {
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int InProgressLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public int LostLeads { get; set; }
        public int NeedsFollowUpCount { get; set; }
        public int TodayInteractionsCount { get; set; }
        public decimal ConversionRate { get; set; }

        public Dictionary<string, int> LeadsByStatus { get; set; } = new();
        public Dictionary<string, int> LeadsBySource { get; set; } = new();
    }
}
