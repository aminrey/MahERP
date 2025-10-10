using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public int MyDayId { get; set; }
        public int TaskId { get; set; }
        public int TaskAssignmentId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public string? CategoryTitle { get; set; }
        public string? StakeholderName { get; set; }
        public byte TaskPriority { get; set; }
        public bool IsImportant { get; set; }
        public bool IsFocused { get; set; }
        public string? PlanNote { get; set; }
        public string? WorkNote { get; set; }
        public int? WorkDurationMinutes { get; set; }
        public bool IsWorkedOn { get; set; }
        public DateTime? WorkStartDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public byte TaskStatus { get; set; }
        public int ProgressPercentage { get; set; } // ⭐ اضافه شده
        public bool IsEditable { get; set; } // ⭐ اضافه شده
        public bool IsCompleted { get; set; } // ⭐ اضافه شده
        public DateTime PlannedDate { get; set; }
        public string PlannedDatePersian { get; set; } = string.Empty;

        public bool IsToday => PlannedDate.Date == DateTime.Now.Date;
        public bool IsOverdue => PlannedDate.Date < DateTime.Now.Date && !IsWorkedOn;
    }
    /// <summary>
    /// آمار "روز من"
    /// </summary>
    public class MyDayStatsViewModel
    {
        public int TotalPlannedTasks { get; set; }
        public int WorkedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalWorkTimeMinutes { get; set; }
        public string TotalWorkTimeFormatted => $"{TotalWorkTimeMinutes / 60}:{TotalWorkTimeMinutes % 60:D2}";
    }
    public class AddToMyDayViewModel
    {
        [Required(ErrorMessage = "شناسه تسک الزامی است")]
        public int TaskAssignmentId { get; set; }

        [Required(ErrorMessage = "تاریخ برنامه‌ریزی الزامی است")]
        [Display(Name = "تاریخ برنامه‌ریزی")]
        public DateTime PlannedDate { get; set; } = DateTime.Now.Date;

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