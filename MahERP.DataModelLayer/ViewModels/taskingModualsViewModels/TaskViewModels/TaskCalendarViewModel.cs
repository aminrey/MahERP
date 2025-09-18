using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تسک‌ها در تقویم بر اساس تاریخ مهلت انجام
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
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تسک
        /// </summary>
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        /// <summary>
        /// کد تسک
        /// </summary>
        [Display(Name = "کد تسک")]
        public string? TaskCode { get; set; }

        /// <summary>
        /// تاریخ مهلت انجام
        /// </summary>
        [Display(Name = "تاریخ مهلت")]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// تاریخ تکمیل
        /// </summary>
        [Display(Name = "تاریخ تکمیل")]
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// آیا تسک تکمیل شده است
        /// </summary>
        [Display(Name = "تکمیل شده")]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی
        /// </summary>
        [Display(Name = "دسته‌بندی")]
        public string CategoryTitle { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// نام طرف حساب
        /// </summary>
        [Display(Name = "طرف حساب")]
        public string StakeholderName { get; set; }

        /// <summary>
        /// شناسه طرف حساب
        /// </summary>
        public int? StakeholderId { get; set; }

        /// <summary>
        /// نام شعبه
        /// </summary>
        [Display(Name = "شعبه")]
        public string BranchName { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public string? CreatorUserId { get; set; }

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا تسک از مهلت گذشته است
        /// </summary>
        [Display(Name = "گذشته از مهلت")]
        public bool IsOverdue { get; set; }

        /// <summary>
        /// رنگ نمایش در تقویم بر اساس وضعیت تسک
        /// </summary>
        public string CalendarColor
        {
            get
            {
                if (IsCompleted)
                    return "#28a745"; // سبز برای تکمیل شده
                else if (IsOverdue)
                    return "#dc3545"; // قرمز برای عقب افتاده
                else
                    return "#007bff"; // آبی برای در حال انجام
            }
        }

        /// <summary>
        /// متن وضعیت تسک برای نمایش
        /// </summary>
        public string StatusText
        {
            get
            {
                if (IsCompleted)
                    return "تکمیل شده";
                else if (IsOverdue)
                    return "عقب افتاده";
                else
                    return "در حال انجام";
            }
        }
    }
}