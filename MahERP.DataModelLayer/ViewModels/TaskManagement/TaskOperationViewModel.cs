using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای عملیات تسک
    /// </summary>
    public class TaskOperationViewModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required(ErrorMessage = "عنوان عملیات الزامی است")]
        [MaxLength(500, ErrorMessage = "حداکثر طول عنوان 500 کاراکتر است")]
        [Display(Name = "عنوان عملیات")]
        public string Title { get; set; }

        [MaxLength(2000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ترتیب")]
        public int OperationOrder { get; set; }

        [Display(Name = "تکمیل شده")]
        public bool IsCompleted { get; set; }

        [Display(Name = "ستاره‌دار")]
        public bool IsStarred { get; set; }

        [Display(Name = "الزامی")]
        public bool IsRequired { get; set; } = true;

        [Display(Name = "تاریخ تکمیل")]
        public DateTime? CompletionDate { get; set; }

        public string? CompletionDatePersian { get; set; }

        public string? CompletedByUserId { get; set; }

        public string? CompletedByUserName { get; set; }

        [MaxLength(1000)]
        [Display(Name = "یادداشت تکمیل")]
        public string? CompletionNote { get; set; }

        [Range(0, 1000)]
        [Display(Name = "ساعت تخمینی")]
        public decimal? EstimatedHours { get; set; }

        [Range(0, 1000)]
        [Display(Name = "ساعت واقعی")]
        public decimal? ActualHours { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedDatePersian { get; set; }

        public string? CreatorUserId { get; set; }

        public string? CreatorUserName { get; set; }

        // ⭐ افزوده شده: لیست WorkLog ها
        /// <summary>
        /// لیست کارهای انجام شده روی این عملیات
        /// </summary>
        public List<OperationWorkLogViewModel> WorkLogs { get; set; } = new List<OperationWorkLogViewModel>();

        /// <summary>
        /// تعداد کل کارهای ثبت شده
        /// </summary>
        public int TotalWorkLogsCount => WorkLogs?.Count ?? 0;

        /// <summary>
        /// آخرین کار ثبت شده
        /// </summary>
        public OperationWorkLogViewModel? LastWorkLog => WorkLogs?.OrderByDescending(w => w.WorkDate).FirstOrDefault();

        /// <summary>
        /// مجموع زمان صرف شده (به دقیقه)
        /// </summary>
        public int TotalDurationMinutes => WorkLogs?.Sum(w => w.DurationMinutes ?? 0) ?? 0;

        /// <summary>
        /// آیا این عملیات WorkLog دارد؟
        /// </summary>
        public bool HasWorkLogs => WorkLogs?.Any() == true;
    }
}