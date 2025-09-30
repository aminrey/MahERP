using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای تنظیم تاریخ‌های شخصی انجام‌دهنده تسک
    /// </summary>
    public class TaskPersonalDatesViewModel
    {
        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// شناسه انتصاب تسک
        /// </summary>
        public int TaskAssignmentId { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// کد تسک
        /// </summary>
        public string TaskCode { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ شروع پیشنهادی سازنده تسک (شمسی)
        /// </summary>
        [Display(Name = "تاریخ شروع پیشنهادی سازنده")]
        public string? OriginalStartDatePersian { get; set; }

        /// <summary>
        /// تاریخ پایان پیشنهادی سازنده تسک (شمسی)
        /// </summary>
        [Display(Name = "مهلت اصلی")]
        public string? OriginalDueDatePersian { get; set; }

        /// <summary>
        /// تاریخ شروع شخصی کاربر (شمسی)
        /// </summary>
        [Display(Name = "تاریخ شروع من")]
        public string? PersonalStartDatePersian { get; set; }

        /// <summary>
        /// تاریخ پایان شخصی کاربر (شمسی)
        /// </summary>
        [Display(Name = "تاریخ پایان من")]
        public string? PersonalEndDatePersian { get; set; }

        /// <summary>
        /// تاریخ شروع شخصی کاربر (میلادی)
        /// </summary>
        public DateTime? PersonalStartDate { get; set; }

        /// <summary>
        /// تاریخ پایان شخصی کاربر (میلادی)
        /// </summary>
        public DateTime? PersonalEndDate { get; set; }

        /// <summary>
        /// یادداشت شخصی کاربر در مورد زمان‌بندی
        /// </summary>
        [Display(Name = "یادداشت زمان‌بندی")]
        [MaxLength(500, ErrorMessage = "یادداشت نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string? PersonalTimeNote { get; set; }

        /// <summary>
        /// آیا کاربر مجاز به تغییر تاریخ‌ها است
        /// </summary>
        public bool CanModifyDates { get; set; } = true;

        /// <summary>
        /// نام کامل کاربر انجام‌دهنده
        /// </summary>
        public string AssignedUserName { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdated { get; set; }
    }
}