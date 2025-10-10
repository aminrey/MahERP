using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای مدیریت گزارش کارهای انجام شده روی عملیات
    /// </summary>
    public class OperationWorkLogViewModel
    {
        /// <summary>
        /// شناسه گزارش کار (برای ویرایش)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// شناسه تسک - برای _LogWorkModal (اختیاری)
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// کد تسک - برای نمایش در مودال (اختیاری)
        /// </summary>
        [Display(Name = "کد تسک")]
        public string TaskCode { get; set; }

        /// <summary>
        /// شناسه عملیات (اختیاری - در صورت ثبت روی عملیات خاص)
        /// </summary>
        public int TaskOperationId { get; set; }

        /// <summary>
        /// عنوان عملیات (فقط برای نمایش)
        /// </summary>
        [Display(Name = "عنوان عملیات")]
        public string OperationTitle { get; set; }

        /// <summary>
        /// عنوان تسک (فقط برای نمایش)
        /// </summary>
        [Display(Name = "عنوان تسک")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// توضیحات کار انجام شده - تنها فیلد اجباری ⭐
        /// </summary>
        [Required(ErrorMessage = "لطفاً توضیحات کار انجام شده را وارد کنید")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "توضیحات باید بین 5 تا 2000 کاراکتر باشد")]
        [Display(Name = "توضیحات کار انجام شده")]
        public string WorkDescription { get; set; }

        /// <summary>
        /// Alias برای WorkDescription (برای سازگاری با _LogWorkModal)
        /// </summary>
        [Display(Name = "یادداشت کار")]
        public string WorkNote
        {
            get => WorkDescription;
            set => WorkDescription = value;
        }

        /// <summary>
        /// مدت زمان صرف شده (به دقیقه) - اختیاری
        /// </summary>
        [Range(0, 1440, ErrorMessage = "مدت زمان باید بین 0 تا 1440 دقیقه باشد")]
        [Display(Name = "مدت زمان (دقیقه)")]
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Alias برای DurationMinutes (برای سازگاری با _LogWorkModal)
        /// </summary>
        public int? WorkDurationMinutes
        {
            get => DurationMinutes;
            set => DurationMinutes = value;
        }

        /// <summary>
        /// درصد پیشرفت بعد از این کار - اختیاری
        /// </summary>
        [Range(0, 100, ErrorMessage = "درصد پیشرفت باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد پیشرفت")]
        public int? ProgressPercentage { get; set; }

        /// <summary>
        /// تاریخ انجام کار (میلادی) - اختیاری
        /// </summary>
        public DateTime? WorkDate { get; set; }

        /// <summary>
        /// تاریخ انجام کار (شمسی - برای نمایش) - اختیاری
        /// </summary>
        [Display(Name = "تاریخ انجام کار")]
        public string WorkDatePersian { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کامل کاربر (فقط برای نمایش)
        /// </summary>
        [Display(Name = "انجام دهنده")]
        public string UserName { get; set; }

        /// <summary>
        /// آیا کاربر قبلاً روی این تسک/عملیات کار کرده است؟
        /// </summary>
        public bool IsAlreadyWorkedOn { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد (شمسی)
        /// </summary>
        public string CreatedDatePersian { get; set; }

        /// <summary>
        /// لیست WorkLog های اخیر (برای نمایش در مودال)
        /// </summary>
        public List<OperationWorkLogViewModel> RecentWorkLogs { get; set; } = new List<OperationWorkLogViewModel>();

        /// <summary>
        /// تعداد کل WorkLog ها
        /// </summary>
        public int TotalWorkLogsCount { get; set; }

        /// <summary>
        /// آیا این گزارش قابل ویرایش است؟
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// آیا این گزارش قابل حذف است؟
        /// </summary>
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش خلاصه WorkLog ها در تاریخچه
    /// </summary>
    public class OperationWorkLogSummaryViewModel
    {
        public int OperationId { get; set; }
        public string OperationTitle { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskCode { get; set; }
        public int TotalWorkLogs { get; set; }
        public DateTime? LastWorkDate { get; set; }
        public string LastWorkDatePersian { get; set; }
        public string LastWorkDescription { get; set; }
        public string LastWorkerName { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int? CurrentProgressPercentage { get; set; }
    }
}