using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای افزودن یادداشت به عملیات (هنگام تیک زدن)
    /// </summary>
    public class OperationNoteViewModel
    {
        public int OperationId { get; set; }

        public string OperationTitle { get; set; }

        public string TaskTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "حداکثر طول یادداشت 1000 کاراکتر است")]
        [Display(Name = "یادداشت تکمیل")]
        public string? CompletionNote { get; set; }

        public bool IsCompleted { get; set; }

        /// <summary>
        /// آیا کاربر می‌خواهد WorkLog هم ثبت کند؟
        /// </summary>
        [Display(Name = "ثبت جزئیات کار انجام شده")]
        public bool AddWorkLog { get; set; }

        /// <summary>
        /// توضیحات کار انجام شده (در صورت انتخاب AddWorkLog)
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "توضیحات کار انجام شده")]
        public string? WorkDescription { get; set; }

        /// <summary>
        /// مدت زمان صرف شده (دقیقه)
        /// </summary>
        [Range(0, 1440)]
        [Display(Name = "مدت زمان (دقیقه)")]
        public int? DurationMinutes { get; set; }
    }
}