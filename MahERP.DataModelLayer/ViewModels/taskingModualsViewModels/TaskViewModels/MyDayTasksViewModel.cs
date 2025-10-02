using System;
using System.Collections.Generic;
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
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public string? CategoryTitle { get; set; }
        public string? StakeholderName { get; set; }
        public byte TaskPriority { get; set; }
        public bool IsImportant { get; set; }
        public string? PlanNote { get; set; }
        public string? WorkNote { get; set; }
        public int? WorkDurationMinutes { get; set; }
        public bool IsWorkedOn { get; set; }
        public DateTime? WorkStartDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public byte TaskStatus { get; set; }
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// ⭐ تاریخ برنامه‌ریزی
        /// </summary>
        public DateTime PlannedDate { get; set; }

        /// <summary>
        /// ⭐ تاریخ برنامه‌ریزی (شمسی)
        /// </summary>
        public string PlannedDatePersian { get; set; } = string.Empty;

        /// <summary>
        /// آیا تسک امروز است؟
        /// </summary>
        public bool IsToday => PlannedDate.Date == DateTime.Now.Date;

        /// <summary>
        /// آیا تسک گذشته است؟
        /// </summary>
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
}