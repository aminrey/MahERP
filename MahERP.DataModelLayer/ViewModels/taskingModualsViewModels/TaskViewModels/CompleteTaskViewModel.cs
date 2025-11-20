using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای تکمیل تسک
    /// </summary>
    public class CompleteTaskViewModel
    {
        [Required(ErrorMessage = "شناسه تسک الزامی است")]
        public int TaskId { get; set; }

        public string? TaskTitle { get; set; }

        public string? TaskCode { get; set; }

        /// <summary>
        /// گزارش تکمیل
        /// </summary>
        [Required(ErrorMessage = "گزارش تکمیل الزامی است")]
        [Display(Name = "گزارش تکمیل")]
        [MaxLength(2000, ErrorMessage = "گزارش حداکثر 2000 کاراکتر")]
        public string? CompletionReport { get; set; }

        /// <summary>
        /// یادداشت اضافی
        /// </summary>
        [Display(Name = "یادداشت اضافی")]
        public string? AdditionalNote { get; set; }

        /// <summary>
        /// آیا همه عملیات تکمیل شده
        /// </summary>
        public bool AllOperationsCompleted { get; set; }

        /// <summary>
        /// تعداد عملیات باقیمانده
        /// </summary>
        public int PendingOperationsCount { get; set; }

        /// <summary>
        /// آیا قبلاً تکمیل شده
        /// </summary>
        public bool IsAlreadyCompleted { get; set; }

        /// <summary>
        /// ⭐⭐⭐ نوع تکمیل تسک
        /// </summary>
        public bool IsIndependentCompletion { get; set; }

        /// <summary>
        /// ⭐⭐⭐ تعداد کل اعضا
        /// </summary>
        public int TotalMembers { get; set; }

        /// <summary>
        /// ⭐⭐⭐ تعداد اعضای تکمیل کرده
        /// </summary>
        public int CompletedMembers { get; set; }
        public bool FromList { get; set; }
        public int rowNum{ get; set; }

    }
}