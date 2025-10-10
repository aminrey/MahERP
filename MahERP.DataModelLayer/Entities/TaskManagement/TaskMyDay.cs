using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>   
    /// تسک‌های برنامه‌ریزی شده برای "روز من" - مرتبط با TaskAssignment
    /// این جدول فقط برای کاربرانی که تسک به آن‌ها assign شده معنی دارد
    /// </summary>
    public class TaskMyDay
    {
        /// <summary>
        /// شناسه منحصر به فرد
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ⭐ شناسه TaskAssignment مرتبط (کلید خارجی)
        /// </summary>
        [Required(ErrorMessage = "شناسه انتساب تسک الزامی است")]
        public int TaskAssignmentId { get; set; }

        [ForeignKey("TaskAssignmentId")]
        public virtual TaskAssignment TaskAssignment { get; set; }

        /// <summary>
        /// تاریخ برنامه‌ریزی شده برای انجام این تسک (تاریخ روز)
        /// </summary>
        [Required(ErrorMessage = "تاریخ برنامه‌ریزی الزامی است")]
        [DataType(DataType.Date)]
        public DateTime PlannedDate { get; set; }

        /// <summary>
        /// یادداشت برنامه‌ریزی (اختیاری)
        /// مثال: "شروع از ساعت 10 صبح"
        /// </summary>
        [MaxLength(500)]
        public string? PlanNote { get; set; }

        /// <summary>
        /// تاریخ شروع کار روی تسک (زمانی که کاربر شروع کرد)
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
        /// تاریخ ایجاد رکورد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// آیا این تسک حذف شده از "روز من"؟
        /// </summary>
        public bool IsRemoved { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? RemovedDate { get; set; }
    }
}