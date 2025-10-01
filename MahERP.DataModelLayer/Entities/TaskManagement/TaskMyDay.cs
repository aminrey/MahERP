using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// تسک‌های برنامه‌ریزی شده برای "روز من" هر کاربر
    /// </summary>
    public class TaskMyDay
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// تاریخ برنامه‌ریزی (تاریخ روزی که کاربر قصد کار روی تسک را دارد)
        /// </summary>
        [Required]
        public DateTime PlannedDate { get; set; }

        /// <summary>
        /// یادداشت کاربر هنگام اضافه کردن به "روز من"
        /// </summary>
        [MaxLength(500)]
        public string? PlanNote { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// آیا روی این تسک امروز کار شده است
        /// </summary>
        public bool IsWorkedOn { get; set; }

        /// <summary>
        /// تاریخ شروع کار روی تسک
        /// </summary>
        public DateTime? WorkStartDate { get; set; }

        /// <summary>
        /// یادداشت کاری (در صورت ثبت کار انجام شده)
        /// </summary>
        [MaxLength(1000)]
        public string? WorkNote { get; set; }

        /// <summary>
        /// مدت زمان کار روی تسک (بر حسب دقیقه)
        /// </summary>
        public int? WorkDurationMinutes { get; set; }

        /// <summary>
        /// آیا تسک فعال است (حذف نشده)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}