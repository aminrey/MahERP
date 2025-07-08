using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// زمان‌بندی خودکار تسک‌ها
    /// </summary>
    public class TaskSchedule
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// شناسه قالب تسک مرتبط
        /// </summary>
        public int TaskTemplateId { get; set; }
        [ForeignKey("TaskTemplateId")]
        public virtual TaskTemplate TaskTemplate { get; set; }

        /// <summary>
        /// نوع تکرار
        /// 0- یکبار
        /// 1- روزانه
        /// 2- هفتگی
        /// 3- ماهانه
        /// 4- سالانه
        /// </summary>
        public byte RecurrenceType { get; set; }

        /// <summary>
        /// زمان شروع اولین اجرا
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// زمان پایان (برای زمان‌بندی‌های تکرارشونده)
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// تکرار هر X روز/هفته/ماه (بسته به نوع تکرار)
        /// </summary>
        public int RecurrenceInterval { get; set; }

        /// <summary>
        /// روزهای هفته (برای تکرار هفتگی)
        /// بیت‌های 0 تا 6 به ترتیب نشان‌دهنده شنبه تا جمعه است
        /// مثال: 1 = شنبه، 2 = یکشنبه، 4 = دوشنبه، ...، 64 = جمعه
        /// ترکیب روزها با جمع مقادیر (شنبه و دوشنبه = 1+4=5)
        /// </summary>
        public byte WeekDays { get; set; }

        /// <summary>
        /// روز ماه (برای تکرار ماهانه)
        /// </summary>
        public byte MonthDay { get; set; }

        /// <summary>
        /// ماه سال (برای تکرار سالانه)
        /// 1-12 به ترتیب ماه‌های سال
        /// </summary>
        public byte YearMonth { get; set; }

        /// <summary>
        /// آیا زمان‌بندی فعال است
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// آخرین زمان اجرای موفق
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// زمان اجرای بعدی (محاسبه شده)
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// وضعیت آخرین اجرا
        /// 0- در انتظار اولین اجرا
        /// 1- موفق
        /// 2- ناموفق
        /// </summary>
        public byte LastRunStatus { get; set; }

        /// <summary>
        /// پیام خطای آخرین اجرا (در صورت وجود)
        /// </summary>
        public string? LastRunErrorMessage { get; set; }

        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? ModifierUserId { get; set; }
        [ForeignKey("ModifierUserId")]
        public virtual AppUsers? Modifier { get; set; }

        // Navigation properties
        public virtual ICollection<TaskScheduleAssignment> Assignments { get; set; } = new HashSet<TaskScheduleAssignment>();
        public virtual ICollection<TaskScheduleExecution> Executions { get; set; } = new HashSet<TaskScheduleExecution>();
    }
}
