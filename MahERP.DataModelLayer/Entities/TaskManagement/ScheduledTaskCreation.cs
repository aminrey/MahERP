using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// جدول زمان‌بندی ایجاد خودکار تسک‌ها
    /// برای ایجاد تسک‌های دوره‌ای یا زمان‌بندی شده
    /// </summary>
    [Table("ScheduledTaskCreation_Tbl")]
    public class ScheduledTaskCreation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان زمان‌بندی (برای شناسایی راحت‌تر)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ScheduleTitle { get; set; }

        /// <summary>
        /// توضیحات زمان‌بندی
        /// </summary>
        [MaxLength(500)]
        public string? ScheduleDescription { get; set; }

        /// <summary>
        /// تمام اطلاعات تسک به صورت JSON
        /// شامل: Assignments, Operations, Reminders, Viewers, Attachments و...
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string TaskDataJson { get; set; }

        /// <summary>
        /// نوع زمان‌بندی
        /// 0 = یکبار (OneTime)
        /// 1 = روزانه (Daily)
        /// 2 = هفتگی (Weekly)
        /// 3 = ماهانه (Monthly)
        /// </summary>
        [Required]
        public byte ScheduleType { get; set; }

        /// <summary>
        /// ساعت اجرا (فرمت: HH:mm مثل "08:00")
        /// </summary>
        [MaxLength(5)]
        public string? ScheduledTime { get; set; }

        /// <summary>
        /// روزهای هفته (برای Weekly)
        /// فرمت: "0,2,4" (یکشنبه، سه‌شنبه، پنج‌شنبه)
        /// 0=Sunday, 1=Monday, ..., 6=Saturday
        /// </summary>
        [MaxLength(50)]
        public string? ScheduledDaysOfWeek { get; set; }

        /// <summary>
        /// روز ماه (برای Monthly)
        /// مقدار بین 1 تا 31
        /// </summary>
        public int? ScheduledDayOfMonth { get; set; }

        /// <summary>
        /// تاریخ شروع زمان‌بندی (میلادی UTC)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان زمان‌بندی (میلادی UTC) - اختیاری
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تاریخ بعدی اجرا (میلادی UTC)
        /// </summary>
        public DateTime? NextExecutionDate { get; set; }

        /// <summary>
        /// تاریخ آخرین اجرا (میلادی UTC)
        /// </summary>
        public DateTime? LastExecutionDate { get; set; }

        /// <summary>
        /// آیا تکرار دارد؟
        /// true = تکراری (Daily, Weekly, Monthly)
        /// false = یکبار (OneTime)
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// حداکثر تعداد دفعات اجرا (null = بی‌نهایت)
        /// </summary>
        public int? MaxOccurrences { get; set; }

        /// <summary>
        /// تعداد دفعاتی که تا کنون اجرا شده
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// آیا زمان‌بندی فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا اجرا شده؟ (برای OneTime)
        /// </summary>
        public bool IsExecuted { get; set; }

        /// <summary>
        /// آیا فعال‌سازی زمان‌بندی فعال است؟
        /// برای توقف موقت بدون حذف
        /// </summary>
        public bool IsScheduleEnabled { get; set; }

        /// <summary>
        /// کاربر ایجادکننده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual AppUsers? CreatedByUser { get; set; }
        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// کاربر آخرین ویرایش
        /// </summary>
        [MaxLength(450)]
        public string? ModifiedByUserId { get; set; }
        [ForeignKey("ModifiedByUserId")]
        public virtual AppUsers? ModifiedByUser { get; set; }
        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
        /// <summary>
        /// یادداشت‌ها یا پیام‌های خطا
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }
    }
}
