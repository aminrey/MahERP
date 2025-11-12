using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش لیست تسک‌های زمان‌بندی شده
    /// </summary>
    public class ScheduledTaskListViewModel
    {
        public List<ScheduledTaskCardViewModel> ScheduledTasks { get; set; } = new();
        
        public ScheduledTaskStatsViewModel Stats { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای کارت هر تسک زمان‌بندی شده
    /// </summary>
    public class ScheduledTaskCardViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "عنوان زمان‌بندی")]
        public string ScheduleTitle { get; set; }
        
        [Display(Name = "توضیحات")]
        public string? ScheduleDescription { get; set; }
        
        [Display(Name = "عنوان تسک")]
        public string TaskTitle { get; set; }
        
        [Display(Name = "نوع زمان‌بندی")]
        public byte ScheduleType { get; set; }
        
        [Display(Name = "نوع زمان‌بندی")]
        public string ScheduleTypeText => ScheduleType switch
        {
            0 => "یکبار",
            1 => "روزانه",
            2 => "هفتگی",
            3 => "ماهانه",
            _ => "نامشخص"
        };
        
        [Display(Name = "ساعت اجرا")]
        public string? ScheduledTime { get; set; }
        
        [Display(Name = "روزهای هفته")]
        public string? ScheduledDaysOfWeek { get; set; }
        
        [Display(Name = "روز ماه")]
        public int? ScheduledDayOfMonth { get; set; }
        
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "تاریخ شروع (شمسی)")]
        public string? StartDatePersian { get; set; }
        
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "تاریخ پایان (شمسی)")]
        public string? EndDatePersian { get; set; }
        
        [Display(Name = "اجرای بعدی")]
        public DateTime? NextExecutionDate { get; set; }
        
        [Display(Name = "اجرای بعدی (شمسی)")]
        public string? NextExecutionDatePersian { get; set; }
        
        [Display(Name = "آخرین اجرا")]
        public DateTime? LastExecutionDate { get; set; }
        
        [Display(Name = "آخرین اجرا (شمسی)")]
        public string? LastExecutionDatePersian { get; set; }
        
        [Display(Name = "تعداد اجرا")]
        public int ExecutionCount { get; set; }
        
        [Display(Name = "حداکثر دفعات")]
        public int? MaxOccurrences { get; set; }
        
        [Display(Name = "تکراری")]
        public bool IsRecurring { get; set; }
        
        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
        
        [Display(Name = "زمان‌بندی فعال")]
        public bool IsScheduleEnabled { get; set; }
        
        [Display(Name = "اجرا شده")]
        public bool IsExecuted { get; set; }
        
        [Display(Name = "شعبه")]
        public int? BranchId { get; set; }
        
        [Display(Name = "نام شعبه")]
        public string? BranchName { get; set; }
        
        [Display(Name = "ایجاد کننده")]
        public string CreatedByUserName { get; set; }
        
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedDatePersian { get; set; }
        
        // ⭐ اطلاعات تسک
        public string? TaskCode { get; set; }
        public byte Priority { get; set; }
        public bool Important { get; set; }
        public byte TaskType { get; set; }
        
        // ⭐ Badge Classes
        public string StatusBadgeClass => IsScheduleEnabled && IsActive 
            ? "bg-success" 
            : IsExecuted 
                ? "bg-info" 
                : "bg-secondary";
        
        public string StatusText => IsExecuted 
            ? "اجرا شده" 
            : IsScheduleEnabled && IsActive 
                ? "فعال" 
                : "غیرفعال";
        
        public string PriorityBadgeClass => Priority switch
        {
            0 => "bg-secondary",
            1 => "bg-warning",
            2 => "bg-danger",
            _ => "bg-secondary"
        };
        
        public string PriorityText => Priority switch
        {
            0 => "عادی",
            1 => "مهم",
            2 => "فوری",
            _ => "نامشخص"
        };
        
        // ⭐ محاسبه روزها
        public int? DaysUntilNextExecution
        {
            get
            {
                if (!NextExecutionDate.HasValue) return null;
                var days = (NextExecutionDate.Value.Date - DateTime.UtcNow.Date).Days;
                return days;
            }
        }
        
        public string DaysUntilNextExecutionText
        {
            get
            {
                if (!DaysUntilNextExecution.HasValue) return "";
                
                var days = DaysUntilNextExecution.Value;
                if (days < 0) return "گذشته";
                if (days == 0) return "امروز";
                if (days == 1) return "فردا";
                return $"{days} روز دیگر";
            }
        }
        
        // ⭐ پیشرفت
        public int ProgressPercentage
        {
            get
            {
                if (!MaxOccurrences.HasValue || MaxOccurrences.Value == 0) return 0;
                return (int)((ExecutionCount / (double)MaxOccurrences.Value) * 100);
            }
        }
    }

    /// <summary>
    /// آمار تسک‌های زمان‌بندی شده
    /// </summary>
    public class ScheduledTaskStatsViewModel
    {
        [Display(Name = "کل تسک‌های زمان‌بندی شده")]
        public int TotalScheduled { get; set; }
        
        [Display(Name = "فعال")]
        public int ActiveCount { get; set; }
        
        [Display(Name = "غیرفعال")]
        public int InactiveCount { get; set; }
        
        [Display(Name = "تکمیل شده")]
        public int CompletedCount { get; set; }
        
        [Display(Name = "در انتظار")]
        public int PendingCount { get; set; }
        
        [Display(Name = "امروز")]
        public int TodayCount { get; set; }
        
        [Display(Name = "این هفته")]
        public int ThisWeekCount { get; set; }
    }
}
