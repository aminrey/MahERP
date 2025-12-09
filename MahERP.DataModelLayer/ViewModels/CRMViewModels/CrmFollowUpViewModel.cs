using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ViewModel برای نمایش پیگیری
    /// </summary>
    public class CrmFollowUpViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "سرنخ")]
        public int LeadId { get; set; }
        public string? LeadDisplayName { get; set; }

        [Display(Name = "تعامل مرتبط")]
        public int? InteractionId { get; set; }

        // جزئیات پیگیری
        [Required(ErrorMessage = "نوع پیگیری الزامی است")]
        [Display(Name = "نوع پیگیری")]
        public byte FollowUpType { get; set; }
        public string? FollowUpTypeText { get; set; }
        public string? FollowUpTypeIcon { get; set; }

        [MaxLength(200)]
        [Display(Name = "عنوان")]
        public string? Title { get; set; }

        [MaxLength(1000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "تاریخ موعد الزامی است")]
        [Display(Name = "تاریخ موعد")]
        public DateTime DueDate { get; set; }
        public string? DueDatePersian { get; set; }
        public string? DueTime { get; set; }

        [Required]
        [Display(Name = "اولویت")]
        public byte Priority { get; set; }
        public string? PriorityText { get; set; }
        public string? PriorityTitle { get; set; }
        public string? PriorityColor { get; set; }

        // وضعیت
        [Required]
        [Display(Name = "وضعیت")]
        public byte Status { get; set; }
        public string? StatusText { get; set; }
        public string? StatusColor { get; set; }
        public string? StatusIcon { get; set; }

        [Display(Name = "تاریخ انجام")]
        public DateTime? CompletedDate { get; set; }
        public string? CompletedDatePersian { get; set; }

        [MaxLength(500)]
        [Display(Name = "نتیجه")]
        public string? CompletionResult { get; set; }

        // یادآوری
        [Display(Name = "یادآوری")]
        public bool HasReminder { get; set; }

        [Display(Name = "تاریخ یادآوری")]
        public DateTime? ReminderDate { get; set; }
        public string? ReminderDatePersian { get; set; }

        public bool ReminderSent { get; set; }

        [Display(Name = "دقایق قبل از موعد برای یادآوری")]
        public int ReminderMinutesBefore { get; set; } = 30;

        [Display(Name = "یادآور ایمیلی")]
        public bool SendEmailReminder { get; set; }

        [Display(Name = "یادآور پیامکی")]
        public bool SendSmsReminder { get; set; }

        // کاربر مسئول
        [Required(ErrorMessage = "کاربر مسئول الزامی است")]
        [Display(Name = "کاربر مسئول")]
        public string AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        // تسک
        [Display(Name = "تسک")]
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public bool IsConvertedToTask { get; set; }

        // Computed
        public bool IsOverdue { get; set; }
        public bool IsDueToday { get; set; }
        public int DaysUntilDue { get; set; }
        public string? DisplayTitle { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedDatePersian { get; set; }
        public string? CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد پیگیری
    /// </summary>
    public class CrmFollowUpCreateViewModel
    {
        [Required]
        public int LeadId { get; set; }

        public int? InteractionId { get; set; }

        [Required(ErrorMessage = "نوع پیگیری الزامی است")]
        [Display(Name = "نوع پیگیری")]
        public byte FollowUpType { get; set; }

        [MaxLength(200)]
        [Display(Name = "عنوان")]
        public string? Title { get; set; }

        [MaxLength(1000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "تاریخ موعد الزامی است")]
        [Display(Name = "تاریخ موعد")]
        public string DueDatePersian { get; set; }

        [Display(Name = "ساعت موعد")]
        public string? DueTime { get; set; }

        [Required]
        [Display(Name = "اولویت")]
        public byte Priority { get; set; } = 1;

        // یادآوری
        [Display(Name = "یادآوری")]
        public bool HasReminder { get; set; } = true;

        [Display(Name = "تاریخ یادآوری")]
        public string? ReminderDatePersian { get; set; }

        [Display(Name = "ساعت یادآوری")]
        public string? ReminderTime { get; set; }

        [Display(Name = "دقایق قبل از موعد")]
        public int ReminderMinutesBefore { get; set; } = 30;

        [Display(Name = "یادآور ایمیلی")]
        public bool SendEmailReminder { get; set; }

        [Display(Name = "یادآور پیامکی")]
        public bool SendSmsReminder { get; set; }

        // کاربر مسئول
        [Display(Name = "کاربر مسئول")]
        public string? AssignedUserId { get; set; }

        // تبدیل به تسک
        [Display(Name = "ایجاد تسک")]
        public bool CreateTask { get; set; }

        [Display(Name = "دسته‌بندی تسک")]
        public int? TaskCategoryId { get; set; }
    }

    /// <summary>
    /// ViewModel برای تکمیل پیگیری
    /// </summary>
    public class CrmFollowUpCompleteViewModel
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(500)]
        [Display(Name = "نتیجه")]
        public string? CompletionResult { get; set; }

        // ایجاد پیگیری بعدی
        [Display(Name = "ایجاد پیگیری بعدی")]
        public bool CreateNextFollowUp { get; set; }

        [Display(Name = "نوع پیگیری")]
        public byte? NextFollowUpType { get; set; }

        [Display(Name = "تاریخ پیگیری بعدی")]
        public string? NextFollowUpDatePersian { get; set; }

        [Display(Name = "توضیحات")]
        public string? NextFollowUpDescription { get; set; }

        // ثبت تعامل
        [Display(Name = "ثبت تعامل")]
        public bool CreateInteraction { get; set; }

        [Display(Name = "شرح تعامل")]
        public string? InteractionDescription { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست پیگیری‌ها
    /// </summary>
    public class CrmFollowUpListViewModel
    {
        public List<CrmFollowUpViewModel> FollowUps { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // فیلترها
        public int? LeadId { get; set; }
        public string? AssignedUserId { get; set; }
        public byte? StatusFilter { get; set; }
        public byte? PriorityFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool OnlyOverdue { get; set; }
        public bool OnlyToday { get; set; }

        // آمار
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public int TodayCount { get; set; }
        public int CompletedCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای داشبورد پیگیری‌ها
    /// </summary>
    public class CrmFollowUpDashboardViewModel
    {
        public List<CrmFollowUpViewModel> OverdueFollowUps { get; set; } = new();
        public List<CrmFollowUpViewModel> TodayFollowUps { get; set; } = new();
        public List<CrmFollowUpViewModel> UpcomingFollowUps { get; set; } = new();

        public int OverdueCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int TotalPendingCount { get; set; }
    }
}
