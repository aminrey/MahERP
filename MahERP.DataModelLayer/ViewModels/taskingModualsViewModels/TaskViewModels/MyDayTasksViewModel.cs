using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای صفحه "تسک‌های امروز من"
    /// </summary>
    public class MyDayTasksViewModel
    {
        /// <summary>
        /// تاریخ انتخاب شده (شمسی)
        /// </summary>
        public string SelectedDatePersian { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ انتخاب شده (میلادی)
        /// </summary>
        public DateTime SelectedDate { get; set; }

        /// <summary>
        /// تسک‌های برنامه‌ریزی شده
        /// </summary>
        public List<MyDayTaskItemViewModel> PlannedTasks { get; set; } = new();

        /// <summary>
        /// تسک‌هایی که روی آن‌ها کار شده
        /// </summary>
        public List<MyDayTaskItemViewModel> WorkedTasks { get; set; } = new();

        /// <summary>
        /// آمار کلی
        /// </summary>
        public MyDayStatsViewModel Stats { get; set; } = new();
    }

    /// <summary>
    /// آیتم تسک در "روز من"
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