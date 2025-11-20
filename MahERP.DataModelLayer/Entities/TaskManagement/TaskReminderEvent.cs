using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// رویداد یادآوری تسک - Event های زمان‌بندی شده
    /// </summary>
    public class TaskReminderEvent
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
        /// ⭐⭐⭐ شناسه یادآوری Schedule (اختیاری - برای یادآوری‌های سیستمی)
        /// </summary>
        public int? ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual TaskReminderSchedule? Schedule { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده
        /// </summary>
        [Required]
        public string RecipientUserId { get; set; }
        [ForeignKey("RecipientUserId")]
        public virtual AppUsers RecipientUser { get; set; }

        /// <summary>
        /// عنوان یادآوری
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// متن یادآوری
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// نوع یادآوری
        /// 0 - شروع تسک جدید
        /// 1 - یادآوری تکراری
        /// 2 - یادآوری قبل از مهلت تسک
        /// 3 - یادآوری عملیات تسک
        /// 4 - یادآوری تکمیل تسک
        /// 5 - سایر موارد
        /// </summary>
        public byte EventType { get; set; }

        /// <summary>
        /// تاریخ زمان‌بندی شده
        /// </summary>
        public DateTime ScheduledDateTime { get; set; }

        /// <summary>
        /// آیا یادآوری ارسال شده
        /// </summary>
        public bool IsSent { get; set; }

        /// <summary>
        /// تاریخ ارسال واقعی
        /// </summary>
        public DateTime? SentDateTime { get; set; }

        /// <summary>
        /// آیا یادآوری خوانده شده
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// تاریخ خواندن
        /// </summary>
        public DateTime? ReadDateTime { get; set; }

        /// <summary>
        /// اولویت یادآوری (1 تا 5)
        /// </summary>
        public byte Priority { get; set; } = 3;

        /// <summary>
        /// کانال ارسال یادآوری (سیستم، SMS، ایمیل و...)
        /// </summary>
        [MaxLength(50)]
        public string NotificationChannel { get; set; } = "System"; // System, Email, SMS, Telegram

        /// <summary>
        /// تاریخ ایجاد رویداد
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربری که رویداد را ایجاد کرده (اختیاری، برای رویدادهای دستی)
        /// </summary>
        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// متن فارسی برای نوع رویداد
        /// </summary>
        [NotMapped]
        public string EventTypeText => EventType switch
        {
            0 => "شروع تسک جدید",
            1 => "یادآوری تکراری",
            2 => "یادآوری قبل از مهلت تسک",
            3 => "یادآوری عملیات تسک",
            4 => "یادآوری تکمیل تسک",
            5 => "سایر موارد",
            _ => "نامشخص"
        };

        /// <summary>
        /// کلاس CSS برای نمایش اولویت
        /// </summary>
        [NotMapped]
        public string PriorityClass => Priority switch
        {
            1 => "text-success",          // کم
            2 => "text-info",             // متوسط کم
            3 => "text-warning",          // متوسط
            4 => "text-danger",           // بالا
            5 => "text-danger fw-bold",   // فوری
            _ => "text-muted"
        };

        /// <summary>
        /// آیکون اولویت
        /// </summary>
        [NotMapped]
        public string PriorityIcon => Priority switch
        {
            1 => "fa-info-circle",
            2 => "fa-bell",
            3 => "fa-exclamation-circle",
            4 => "fa-exclamation-triangle",
            5 => "fa-fire",
            _ => "fa-bell"
        };
    }
}