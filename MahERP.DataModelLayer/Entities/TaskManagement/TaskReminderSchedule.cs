using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// برنامه‌ریزی یادآوری‌های تسک - قوانین ایجاد یادآوری
    /// </summary>
    public class TaskReminderSchedule
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
        /// عنوان یادآوری
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات یادآوری
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// نوع یادآوری
        /// 0 - یکبار در زمان مشخص
        /// 1 - تکراری با فاصله زمانی مشخص (روزانه، هفتگی و...)
        /// 2 - قبل از پایان مهلت (X روز قبل از deadline)
        /// 3 - در روز شروع تسک
        /// 4 - در روز پایان مهلت
        /// </summary>
        public byte ReminderType { get; set; }

        /// <summary>
        /// فاصله تکرار (برای نوع تکراری) - به روز
        /// مثال: 1 = روزانه، 7 = هفتگی، 30 = ماهانه
        /// </summary>
        public int? IntervalDays { get; set; }

        /// <summary>
        /// چند روز قبل از پایان مهلت (برای ReminderType = 2)
        /// </summary>
        public int? DaysBeforeDeadline { get; set; }

        /// <summary>
        /// تاریخ شروع یادآوری
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان یادآوری (در صورت وجود)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ساعت ارسال یادآوری
        /// </summary>
        public TimeSpan NotificationTime { get; set; } = new TimeSpan(9, 0, 0); // 9:00 صبح پیش‌فرض

        /// <summary>
        /// آیا این یادآوری فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// آیا این یادآوری پیش‌فرض سیستم است؟
        /// </summary>
        public bool IsSystemDefault { get; set; }

        /// <summary>
        /// آخرین بار که یادآوری اجرا شده
        /// </summary>
        public DateTime? LastExecuted { get; set; }

        /// <summary>
        /// ⭐ تعداد دفعات ارسال شده (برای کنترل حداکثر ارسال)
        /// </summary>
        public int SentCount { get; set; } = 0;

        /// <summary>
        /// ⭐ حداکثر تعداد ارسال (null = نامحدود)
        /// برای یادآوری‌های یکباره = 1
        /// برای تکراری = تعداد دفعات تکرار
        /// </summary>
        public int? MaxSendCount { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر ایجادکننده
        /// </summary>
        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// یادآوری‌های تولید شده از این برنامه‌ریزی
        /// </summary>
        public virtual ICollection<TaskReminderEvent> GeneratedEvents { get; set; } = new List<TaskReminderEvent>();

        /// <summary>
        /// متن نوع یادآوری برای نمایش
        /// </summary>
        [NotMapped]
        public string ReminderTypeText => ReminderType switch
        {
            0 => "یکبار در زمان مشخص",
            1 => $"هر {IntervalDays} روز",
            2 => $"{DaysBeforeDeadline} روز قبل از پایان مهلت",
            3 => "در روز شروع تسک",
            4 => "در روز پایان مهلت",
            _ => "نامشخص"
        };
    }
}