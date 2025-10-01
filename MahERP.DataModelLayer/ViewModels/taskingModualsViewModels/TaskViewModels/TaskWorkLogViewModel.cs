using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای ثبت کار انجام شده روی تسک
    /// </summary>
    public class TaskWorkLogViewModel
    {
        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// کد تسک
        /// </summary>
        public string TaskCode { get; set; } = string.Empty;

        /// <summary>
        /// یادداشت کاری
        /// </summary>
        [Display(Name = "توضیحات کار انجام شده")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string? WorkNote { get; set; }

        /// <summary>
        /// مدت زمان کار (بر حسب دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان کار (دقیقه)")]
        [Range(1, 480, ErrorMessage = "مدت زمان باید بین 1 تا 480 دقیقه باشد")]
        public int? WorkDurationMinutes { get; set; }

        /// <summary>
        /// آیا قبلاً روی این تسک کار شده است
        /// </summary>
        public bool IsAlreadyWorkedOn { get; set; }
    }
}