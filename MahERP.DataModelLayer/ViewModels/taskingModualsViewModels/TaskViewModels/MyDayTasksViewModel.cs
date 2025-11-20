using MahERP.CommonLayer.PublicClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای صفحه "تسک‌های امروز من" - اصلاح شده
    /// </summary>
    public class MyDayTasksViewModel
    {
        /// <summary>
        /// تاریخ انتخاب شده
        /// </summary>
        public DateTime SelectedDate { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// تاریخ انتخاب شده (شمسی)
        /// </summary>
        public string SelectedDatePersian { get; set; } = string.Empty;

        /// <summary>
        /// تسک‌های برنامه‌ریزی شده (برای سازگاری)
        /// </summary>
        public List<MyDayTaskItemViewModel> PlannedTasks { get; set; } = new();

        /// <summary>
        /// تسک‌هایی که روی آن‌ها کار شده (برای سازگاری)
        /// </summary>
        public List<MyDayTaskItemViewModel> WorkedTasks { get; set; } = new();

        /// <summary>
        /// ⭐ تسک‌ها گروه‌بندی شده بر اساس تاریخ
        /// </summary>
        public Dictionary<string, List<MyDayTaskItemViewModel>> TasksByDate { get; set; } = new();

        /// <summary>
        /// آمار کلی
        /// </summary>
        public MyDayStatsViewModel Stats { get; set; } = new();

        /// <summary>
        /// تاریخ‌های موجود (مرتب شده)
        /// </summary>
        public List<string> AvailableDates => TasksByDate.Keys.OrderByDescending(x => x).ToList();
    }

    /// <summary>
    /// ViewModel برای هر آیتم تسک در "روز من" - اصلاح شده
    /// </summary>
    public class MyDayTaskItemViewModel
    {
        public int TaskId { get; set; }
        public int TaskAssignmentId { get; set; }
        public int MyDayId { get; set; } // ⭐ شناسه TaskMyDay

        [Display(Name = "کد تسک")]
        public string TaskCode { get; set; }

        [Display(Name = "عنوان تسک")]
        public string TaskTitle { get; set; }

        [Display(Name = "توضیحات")]
        public string? TaskDescription { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string? CategoryTitle { get; set; }

        /// <summary>
        /// ⚠️ Deprecated - برای backward compatibility
        /// </summary>
        [Display(Name = "طرف حساب (قدیمی)")]
        public string? StakeholderName { get; set; }

        /// <summary>
        /// ⭐⭐⭐ NEW - نام کامل Contact
        /// </summary>
        [Display(Name = "نام فرد")]
        public string? ContactFullName { get; set; }

        /// <summary>
        /// ⭐⭐⭐ NEW - نام سازمان
        /// </summary>
        [Display(Name = "نام سازمان")]
        public string? OrganizationName { get; set; }

        [Display(Name = "اولویت")]
        public byte TaskPriority { get; set; }

        [Display(Name = "مهم")]
        public bool IsImportant { get; set; }

        [Display(Name = "متمرکز")]
        public bool IsFocused { get; set; }

        [Display(Name = "یادداشت برنامه")]
        public string? PlanNote { get; set; }

        [Display(Name = "یادداشت کار")]
        public string? WorkNote { get; set; }

        [Display(Name = "مدت زمان کار (دقیقه)")]
        public int? WorkDurationMinutes { get; set; }

        [Display(Name = "کار انجام شده")]
        public bool IsWorkedOn { get; set; }

        [Display(Name = "تاریخ شروع کار")]
        public DateTime? WorkStartDate { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "وضعیت تسک")]
        public byte TaskStatus { get; set; }

        [Display(Name = "درصد پیشرفت")]
        public int ProgressPercentage { get; set; }

        [Display(Name = "تاریخ برنامه‌ریزی")]
        public DateTime PlannedDate { get; set; }

        [Display(Name = "تاریخ برنامه‌ریزی (شمسی)")]
        public string PlannedDatePersian { get; set; }

        /// <summary>
        /// ⭐ بررسی تکمیل شدن تسک
        /// </summary>
        public bool IsCompleted => TaskStatus >= 2;
        /// <summary>
        /// آیا این تسک برای امروز است؟
        /// </summary>
        [NotMapped]
        public bool IsToday => PlannedDate.Date == DateTime.Now.Date;

        /// <summary>
        /// آیا این تسک عقب افتاده است؟
        /// </summary>
        [NotMapped]
        public bool IsOverdue => PlannedDate.Date < DateTime.Now.Date && !IsCompleted;

  
    }

    /// <summary>
    /// ViewModel برای نمایش آمار "روز من"
    /// </summary>
    public class MyDayStatsViewModel
    {
        [Display(Name = "تعداد کل تسک‌های برنامه‌ریزی شده")]
        public int TotalPlannedTasks { get; set; }

        [Display(Name = "تعداد تسک‌هایی که روی آن‌ها کار شده")]
        public int WorkedTasks { get; set; }

        [Display(Name = "تعداد تسک‌های تکمیل شده")]
        public int CompletedTasks { get; set; }

        [Display(Name = "مجموع زمان کار (دقیقه)")]
        public int TotalWorkTimeMinutes { get; set; }

        /// <summary>
        /// ⭐ فرمت زمان کار (به صورت ساعت:دقیقه)
        /// </summary>
        [Display(Name = "مجموع زمان کار")]
        public string TotalWorkTimeFormatted
        {
            get
            {
                if (TotalWorkTimeMinutes == 0)
                    return "0 دقیقه";

                var hours = TotalWorkTimeMinutes / 60;
                var minutes = TotalWorkTimeMinutes % 60;

                if (hours > 0 && minutes > 0)
                    return $"{hours} ساعت و {minutes} دقیقه";
                else if (hours > 0)
                    return $"{hours} ساعت";
                else
                    return $"{minutes} دقیقه";
            }
        }
    }

    /// <summary>
    /// ⭐⭐⭐ ViewModel برای افزودن تسک به روز من - به‌روز شده
    /// </summary>
    public class AddToMyDayViewModel
    {
        [Required(ErrorMessage = "شناسه تسک الزامی است")]
        public int TaskAssignmentId { get; set; }

        /// <summary>
        /// ⭐⭐⭐ شناسه تسک (برای بازگشت به لیست)
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// ⭐⭐⭐ آیا از لیست تسک‌ها آمده؟
        /// </summary>
        public bool FromList { get; set; }

        [Display(Name = "تاریخ برنامه‌ریزی")]
        public DateTime PlannedDate { get; set; } = DateTime.Now.Date;

        [Required(ErrorMessage = "تاریخ برنامه‌ریزی الزامی است")]
        [Display(Name = "تاریخ برنامه‌ریزی")]

        public string PlannedDateString { get; set; } = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.Date,"yyyy/MM/dd");

        [MaxLength(500)]
        [Display(Name = "یادداشت برنامه‌ریزی")]
        public string? PlanNote { get; set; }   
    }

    /// <summary>
    /// ViewModel برای ثبت گزارش کار در روز من
    /// </summary>
    public class MyDayLogWorkViewModel
    {
        [Required]
        public int MyDayId { get; set; }

        [Required(ErrorMessage = "توضیحات کار الزامی است")]
        [MaxLength(1000)]
        [Display(Name = "توضیحات کار انجام شده")]
        public string WorkNote { get; set; } = string.Empty;

        [Display(Name = "مدت زمان (دقیقه)")]
        [Range(0, 1440, ErrorMessage = "مدت زمان باید بین 0 تا 1440 دقیقه باشد")]
        public int? DurationMinutes { get; set; }
    }
}
