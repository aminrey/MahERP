using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای مدیریت یادآوری‌های تسک
    /// ترکیب نسخه قدیمی و جدید
    /// </summary>
    public class TaskReminderViewModel
    {
        /// <summary>
        /// شناسه یادآوری (برای ویرایش)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// شناسه تسک
        /// </summary>
        [Required(ErrorMessage = "شناسه تسک الزامی است")]
        public int TaskId { get; set; }

        /// <summary>
        /// عنوان تسک (فقط برای نمایش)
        /// </summary>
        [Display(Name = "عنوان تسک")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// کد تسک (فقط برای نمایش)
        /// </summary>
        [Display(Name = "کد تسک")]
        public string TaskCode { get; set; }

        /// <summary>
        /// عنوان یادآوری
        /// </summary>
        [Required(ErrorMessage = "عنوان یادآوری الزامی است")]
        [MaxLength(200, ErrorMessage = "عنوان یادآوری حداکثر 200 کاراکتر می‌باشد")]
        [Display(Name = "عنوان یادآوری")]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات یادآوری
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر می‌باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// نوع یادآوری
        /// 0 - یکبار در زمان مشخص
        /// 1 - تکراری با فاصله زمانی مشخص (روزانه، هفتگی و...)
        /// 2 - قبل از پایان مهلت (X روز قبل از deadline)
        /// 3 - در روز شروع تسک
        /// 4 - در روز پایان مهلت
        /// </summary>
        [Required(ErrorMessage = "نوع یادآوری الزامی است")]
        [Display(Name = "نوع یادآوری")]
        public byte ReminderType { get; set; }

        /// <summary>
        /// فاصله تکرار (برای نوع تکراری) - به روز
        /// مثال: 1 = روزانه، 7 = هفتگی، 30 = ماهانه
        /// </summary>
        [Display(Name = "فاصله تکرار (روز)")]
        public int? IntervalDays { get; set; }

        /// <summary>
        /// چند روز قبل از پایان مهلت (برای ReminderType = 2)
        /// </summary>
        [Display(Name = "چند روز قبل از مهلت")]
        public int? DaysBeforeDeadline { get; set; }

        /// <summary>
        /// تاریخ شروع یادآوری (میلادی - برای ذخیره)
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ شروع یادآوری (شمسی - برای نمایش)
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public string StartDatePersian { get; set; }

        /// <summary>
        /// تاریخ پایان یادآوری (میلادی - برای ذخیره)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تاریخ پایان یادآوری (شمسی - برای نمایش)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public string EndDatePersian { get; set; }

        /// <summary>
        /// ساعت ارسال یادآوری
        /// </summary>
        [Display(Name = "ساعت ارسال")]
        public TimeSpan? NotificationTime { get; set; } = new TimeSpan(9, 0, 0); // 9:00 صبح پیش‌فرض

        /// <summary>
        /// آیا این یادآوری پیش‌فرض سیستم است؟
        /// </summary>
        [Display(Name = "یادآوری پیش‌فرض")]
        public bool IsSystemDefault { get; set; }

        /// <summary>
        /// آیا این یادآوری فعال است؟
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد (شمسی)
        /// </summary>
        public string CreatedDatePersian { get; set; }

        /// <summary>
        /// نام کاربر ایجادکننده
        /// </summary>
        [Display(Name = "ایجادکننده")]
        public string CreatorName { get; set; }

        /// <summary>
        /// متن نوع یادآوری برای نمایش
        /// </summary>
        [Display(Name = "نوع یادآوری")]
        public string ReminderTypeText => ReminderType switch
        {
            0 => "یکبار در زمان مشخص",
            1 => IntervalDays.HasValue ? $"هر {IntervalDays} روز" : "تکراری",
            2 => DaysBeforeDeadline.HasValue ? $"{DaysBeforeDeadline} روز قبل از پایان مهلت" : "قبل از مهلت",
            3 => "در روز شروع تسک",
            4 => "در روز پایان مهلت",
            _ => "نامشخص"
        };
    }

    /// <summary>
    /// ViewModel برای خلاصه یادآوری‌ها (برای لیست‌ها)
    /// </summary>
    public class TaskReminderSummaryViewModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public byte ReminderType { get; set; }
        public string ReminderTypeText { get; set; }
        public string NotificationTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemDefault { get; set; }
        public string CreatedDatePersian { get; set; }
        public string CreatorName { get; set; }
    }
}