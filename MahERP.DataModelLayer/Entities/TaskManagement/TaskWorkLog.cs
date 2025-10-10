using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// گزارش کارهای انجام شده روی تسک (سطح کلی تسک)
    /// برای ثبت لحظه‌ای فعالیت‌های کاربران روی تسک
    /// </summary>
    public class TaskWorkLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تسک مرتبط
        /// </summary>
        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        /// <summary>
        /// شناسه کاربری که کار را انجام داده
        /// </summary>
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// توضیحات کار انجام شده
        /// مثال: "بررسی مستندات و برآورد زمان"
        /// </summary>
        [Required(ErrorMessage = "توضیحات کار انجام شده الزامی است")]
        [MaxLength(2000)]
        public string WorkDescription { get; set; }

        /// <summary>
        /// تاریخ انجام کار
        /// </summary>
        public DateTime WorkDate { get; set; }

        /// <summary>
        /// مدت زمان صرف شده برای این کار (به دقیقه)
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// درصد پیشرفت کلی تسک بعد از این کار
        /// </summary>
        [Range(0, 100)]
        public int? ProgressPercentage { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد در سیستم
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// آیا این گزارش حذف شده؟
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف (در صورت حذف)
        /// </summary>
        public DateTime? DeletedDate { get; set; }
    }
}