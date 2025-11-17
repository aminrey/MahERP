using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.BackgroundJobs
{
    /// <summary>
    /// کارهای پس‌زمینه (Background Jobs)
    /// </summary>
    [Table("BackgroundJob_Tbl")]
    public class BackgroundJob
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نوع Job
        /// 0=SMS Bulk Send, 1=Email Bulk Send, 2=Report Generation, 3=Data Export
        /// </summary>
        [Required]
        public byte JobType { get; set; }

        /// <summary>
        /// عنوان Job
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// وضعیت Job
        /// 0=Pending, 1=Running, 2=Completed, 3=Failed, 4=Cancelled
        /// </summary>
        [Required]
        public byte Status { get; set; } = 0;

        /// <summary>
        /// درصد پیشرفت (0-100)
        /// </summary>
        [Range(0, 100)]
        public int Progress { get; set; } = 0;

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// تعداد پردازش شده
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// تعداد موفق
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// تعداد ناموفق
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ اتمام
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// شناسه کاربر ایجادکننده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers? CreatedBy { get; set; }

        /// <summary>
        /// اطلاعات اضافی (JSON)
        /// </summary>
        public string? Metadata { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "در انتظار",
            1 => "در حال اجرا",
            2 => "تکمیل شده",
            3 => "ناموفق",
            4 => "لغو شده",
            _ => "نامشخص"
        };

        [NotMapped]
        public string JobTypeText => JobType switch
        {
            0 => "ارسال انبوه پیامک",
            1 => "ارسال انبوه ایمیل",
            2 => "تولید گزارش",
            3 => "خروجی اطلاعات",
            _ => "نامشخص"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            0 => "bg-secondary",
            1 => "bg-primary",
            2 => "bg-success",
            3 => "bg-danger",
            4 => "bg-warning",
            _ => "bg-secondary"
        };

        [NotMapped]
        public TimeSpan? Duration => CompletedDate.HasValue 
            ? CompletedDate.Value - StartDate 
            : null;
    }
}
