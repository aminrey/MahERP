using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای تکمیل تسک
    /// </summary>
    public class CompleteTaskViewModel
    {
        public int TaskId { get; set; }
        
        [Display(Name = "عنوان تسک")]
        public string? TaskTitle { get; set; }
        
        [Display(Name = "کد تسک")]
        public string? TaskCode { get; set; }
        
        [Required(ErrorMessage = "گزارش تکمیل الزامی است")]
        [Display(Name = "گزارش تکمیل")]
        public string CompletionReport { get; set; }
        
        [Display(Name = "یادداشت اضافی")]
        public string? AdditionalNote { get; set; }
        
        /// <summary>
        /// آیا همه عملیات تکمیل شده است؟
        /// </summary>
        public bool AllOperationsCompleted { get; set; }
        
        /// <summary>
        /// تعداد عملیات تکمیل نشده
        /// </summary>
        public int PendingOperationsCount { get; set; }
        
        /// <summary>
        /// آیا تسک قبلاً تکمیل شده است؟
        /// </summary>
        public bool IsAlreadyCompleted { get; set; }
    }
}