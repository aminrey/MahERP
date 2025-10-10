using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// گزارش کارهای انجام شده روی هر عملیات
    /// برای ثبت جزئیات کارهایی که روی یک عملیات خاص انجام شده است
    /// </summary>
    public class TaskOperationWorkLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه عملیات مرتبط
        /// </summary>
        public int TaskOperationId { get; set; }
        [ForeignKey("TaskOperationId")]
        public virtual TaskOperation TaskOperation { get; set; }

        /// <summary>
        /// شناسه کاربری که کار را انجام داده
        /// </summary>
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// توضیحات کار انجام شده
        /// مثال: "پیاده‌سازی صفحه Index تسک‌ها با قابلیت فیلتر"
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
        /// اختیاری - برای محاسبه زمان واقعی صرف شده
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// درصد پیشرفت عملیات بعد از این کار
        /// برای ردیابی پیشرفت دقیق‌تر
        /// </summary>
        [Range(0, 100)]
        public int? ProgressPercentage { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد در سیستم
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// آیا این گزارش حذف شده؟
        /// برای soft delete
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف (در صورت حذف)
        /// </summary>
        public DateTime? DeletedDate { get; set; }
    }
}