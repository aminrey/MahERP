using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای تنظیم تسک در "روز من"
    /// </summary>
    public class TaskMyDayViewModel
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
        /// تاریخ برنامه‌ریزی (شمسی)
        /// </summary>
        [Display(Name = "تاریخ برنامه‌ریزی")]
        public string PlannedDatePersian { get; set; } = string.Empty;

        /// <summary>
        /// یادداشت برنامه‌ریزی
        /// </summary>
        [Display(Name = "یادداشت برنامه‌ریزی")]
        [MaxLength(500, ErrorMessage = "یادداشت نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? PlanNote { get; set; }

        /// <summary>
        /// آیا قبلاً در روز من قرار دارد
        /// </summary>
        public bool IsAlreadyInMyDay { get; set; }
    }
}