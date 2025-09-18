using System;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تسک‌ها در تقویم
    /// حاوی اطلاعات ضروری برای نمایش در FullCalendar
    /// </summary>
    public class TaskCalendarViewModel
    {
        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تسک
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// کد تسک
        /// </summary>
        public string? TaskCode { get; set; }

        /// <summary>
        /// تاریخ مهلت انجام
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// آیا تسک تکمیل شده است؟
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// آیا تسک عقب افتاده است؟
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// شناسه طرف حساب
        /// </summary>
        public int? StakeholderId { get; set; }

        /// <summary>
        /// نام طرف حساب
        /// </summary>
        public string? StakeholderName { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی تسک
        /// </summary>
        public string? CategoryTitle { get; set; }

        /// <summary>
        /// نام شعبه مرتبط با تسک
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// رنگ نمایش در تقویم
        /// </summary>
        public string CalendarColor { get; set; }

        /// <summary>
        /// متن وضعیت تسک برای نمایش
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// تاریخ ایجاد تسک
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// شناسه کاربر سازنده تسک
        /// </summary>
        public string CreatorUserId { get; set; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public TaskCalendarViewModel()
        {
            Title = string.Empty;
            CalendarColor = "#007bff"; // رنگ پیش‌فرض آبی
            StatusText = "نامشخص";
            CreatorUserId = string.Empty;
        }
    }
}