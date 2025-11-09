using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای ثبت و نمایش گزارش کار انجام شده روی تسک (سطح کلی تسک)
    /// </summary>
    public class TaskWorkLogViewModel
    {
        /// <summary>
        /// شناسه گزارش کار (برای ویرایش/نمایش)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// شناسه تسک
        /// </summary>
        [Required(ErrorMessage = "شناسه تسک الزامی است")]
        public int TaskId { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string? TaskTitle { get; set; }

        /// <summary>
        /// کد تسک
        /// </summary>
        public string? TaskCode { get; set; }
        public string? UserProfileImage { get; set; }
        /// <summary>
        /// توضیحات کار انجام شده - اجباری
        /// </summary>
        [Required(ErrorMessage = "لطفاً توضیحات کار انجام شده را وارد کنید")]
        [Display(Name = "توضیحات کار انجام شده")]
        public string WorkDescription { get; set; } = string.Empty;

        /// <summary>
        /// مدت زمان صرف شده (به دقیقه) - اختیاری
        /// </summary>
        [Range(0, 1440, ErrorMessage = "مدت زمان باید بین 0 تا 1440 دقیقه باشد")]
        [Display(Name = "مدت زمان (دقیقه)")]
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// درصد پیشرفت کلی تسک بعد از این کار - اختیاری
        /// </summary>
        [Range(0, 100, ErrorMessage = "درصد پیشرفت باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد پیشرفت")]
        public int? ProgressPercentage { get; set; }

        /// <summary>
        /// تاریخ انجام کار (میلادی)
        /// </summary>
        public DateTime WorkDate { get; set; }

        /// <summary>
        /// تاریخ انجام کار (شمسی)
        /// </summary>
        public string? WorkDatePersian { get; set; }

        /// <summary>
        /// شناسه کاربر انجام‌دهنده
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// نام کاربر انجام‌دهنده
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد (میلادی)
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد (شمسی)
        /// </summary>
        public string? CreatedDatePersian { get; set; }

        /// <summary>
        /// آیا قبلاً روی این تسک کار شده است (برای UI)
        /// </summary>
        public bool IsAlreadyWorkedOn { get; set; }
    }
}