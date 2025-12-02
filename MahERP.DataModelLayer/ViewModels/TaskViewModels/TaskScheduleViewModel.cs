using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای زمان‌بندی ایجاد تسک
    /// </summary>
    public class TaskScheduleViewModel
    {
        /// <summary>
        /// آیا زمان‌بندی فعال است؟
        /// </summary>
        public bool IsScheduled { get; set; }

        /// <summary>
        /// عنوان زمان‌بندی
        /// </summary>
        [MaxLength(200)]
        public string? ScheduleTitle { get; set; }

        /// <summary>
        /// توضیحات زمان‌بندی
        /// </summary>
        [MaxLength(500)]
        public string? ScheduleDescription { get; set; }

        /// <summary>
        /// نوع زمان‌بندی
        /// 0 = یکبار
        /// 1 = روزانه
        /// 2 = هفتگی
        /// 3 = ماهانه
        /// </summary>
        public byte ScheduleType { get; set; }

        /// <summary>
        /// ساعت اجرا (HH:mm)
        /// </summary>
        public string? ScheduledTime { get; set; }

        /// <summary>
        /// روزهای هفته (برای Weekly)
        /// </summary>
        public string? ScheduledDaysOfWeek { get; set; }

        /// <summary>
        /// روز ماه (برای Monthly) - یک روز
        /// ⚠️ DEPRECATED: از ScheduledDaysOfMonth استفاده کنید
        /// </summary>
        [Obsolete("از ScheduledDaysOfMonth استفاده کنید")]
        public int? ScheduledDayOfMonth { get; set; }

        /// <summary>
        /// ⭐⭐⭐ روزهای ماه (برای Monthly) - چند روز
        /// مثال: "10,15,25" = روزهای 10، 15، 25 هر ماه
        /// </summary>
        [MaxLength(100)]
        public string? ScheduledDaysOfMonth { get; set; }

        /// <summary>
        /// تاریخ شروع (شمسی)
        /// </summary>
        public string? StartDatePersian { get; set; }

        /// <summary>
        /// تاریخ پایان (شمسی)
        /// </summary>
        public string? EndDatePersian { get; set; }

        /// <summary>
        /// تاریخ یکباره (شمسی) - برای OneTime
        /// </summary>
        public string? OneTimeExecutionDatePersian { get; set; }

        /// <summary>
        /// حداکثر تعداد دفعات (null = بی‌نهایت)
        /// </summary>
        public int? MaxOccurrences { get; set; }

        /// <summary>
        /// آیا تکرار دارد؟
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// آیا باید الان هم تسک را بسازد؟ (علاوه بر زمان‌بندی)
        /// </summary>
        public bool CreateImmediately { get; set; }
    }
}
