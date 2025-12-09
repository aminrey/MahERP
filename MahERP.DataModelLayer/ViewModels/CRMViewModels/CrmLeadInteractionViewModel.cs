using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تعامل
    /// </summary>
    public class CrmLeadInteractionViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "سرنخ")]
        public int LeadId { get; set; }
        public string? LeadDisplayName { get; set; }

        // نوع تعامل
        [Required(ErrorMessage = "نوع تعامل الزامی است")]
        [Display(Name = "نوع تعامل")]
        public byte InteractionType { get; set; }
        public string? InteractionTypeText { get; set; }
        public string? InteractionTypeIcon { get; set; }
        public string? InteractionTypeColor { get; set; }

        [Display(Name = "جهت")]
        public byte? Direction { get; set; }
        public string? DirectionText { get; set; }
        public string? DirectionIcon { get; set; }

        // جزئیات
        [MaxLength(300)]
        [Display(Name = "موضوع")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "شرح تعامل الزامی است")]
        [Display(Name = "شرح")]
        public string Description { get; set; }

        [Display(Name = "نتیجه")]
        public byte? Result { get; set; }
        public string? ResultTitle { get; set; }

        [Display(Name = "مدت زمان (دقیقه)")]
        public int? DurationMinutes { get; set; }
        public string? DurationText { get; set; }

        [MaxLength(20)]
        [Display(Name = "شماره تلفن")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [Required]
        [Display(Name = "تاریخ تعامل")]
        public DateTime InteractionDate { get; set; }
        public string? InteractionDatePersian { get; set; }
        public string? InteractionTime { get; set; }

        // تسک مرتبط
        [Display(Name = "تسک مرتبط")]
        public int? RelatedTaskId { get; set; }
        public string? RelatedTaskTitle { get; set; }
        public bool HasRelatedTask { get; set; }

        // پیگیری
        [Display(Name = "نیاز به پیگیری")]
        public bool NeedsFollowUp { get; set; }
        [Display(Name = "تاریخ پیگیری")]
        public string? FollowUpDatePersian { get; set; }
        [Display(Name = "یادداشت پیگیری")]
        public string? FollowUpNote { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        // پیگیری‌های مرتبط
        public int FollowUpsCount { get; set; }
        public List<CrmFollowUpViewModel> FollowUps { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای ایجاد تعامل
    /// </summary>
    public class CrmLeadInteractionCreateViewModel
    {
        [Required]
        public int LeadId { get; set; }

        [Required(ErrorMessage = "نوع تعامل الزامی است")]
        [Display(Name = "نوع تعامل")]
        public byte InteractionType { get; set; }

        [Display(Name = "جهت")]
        public byte? Direction { get; set; }

        [MaxLength(300)]
        [Display(Name = "موضوع")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "شرح تعامل الزامی است")]
        [Display(Name = "شرح")]
        public string Description { get; set; }

        [Display(Name = "نتیجه")]
        public byte? Result { get; set; }

        [Display(Name = "مدت زمان (دقیقه)")]
        public int? DurationMinutes { get; set; }

        [MaxLength(20)]
        [Display(Name = "شماره تلفن")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [Display(Name = "تاریخ تعامل")]
        public string? InteractionDatePersian { get; set; }

        [Display(Name = "ساعت تعامل")]
        public string? InteractionTime { get; set; }

        // ایجاد پیگیری همزمان
        [Display(Name = "نیاز به پیگیری")]
        public bool NeedsFollowUp { get; set; }

        [Display(Name = "تاریخ پیگیری")]
        public string? FollowUpDatePersian { get; set; }

        [Display(Name = "نوع پیگیری")]
        public byte? FollowUpType { get; set; }

        [Display(Name = "یادداشت پیگیری")]
        public string? FollowUpNote { get; set; }

        // ایجاد تسک مرتبط
        [Display(Name = "ایجاد تسک")]
        public bool CreateTask { get; set; }

        [Display(Name = "عنوان تسک")]
        public string? TaskTitle { get; set; }

        [Display(Name = "توضیحات تسک")]
        public string? TaskDescription { get; set; }

        [Display(Name = "تاریخ سررسید تسک")]
        public string? TaskDueDatePersian { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست تعاملات
    /// </summary>
    public class CrmLeadInteractionListViewModel
    {
        public int LeadId { get; set; }
        public string? LeadDisplayName { get; set; }
        public List<CrmLeadInteractionViewModel> Interactions { get; set; } = new();
        public int TotalCount { get; set; }

        // فیلترها
        public byte? InteractionTypeFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
