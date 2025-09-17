using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl
{
    /// <summary>
    /// ViewModel برای انتصاب دسته‌بندی تسک به شعبه
    /// </summary>
    public class BranchTaskCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "شناسه شعبه الزامی است")]
        public int BranchId { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی تکی - فقط برای ویرایش استفاده می‌شود
        /// </summary>
        [Display(Name = "دسته‌بندی")]
        public int TaskCategoryId { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ انتساب")]
        public DateTime AssignDate { get; set; }

        public string AssignedByUserId { get; set; }

        // اطلاعات نمایشی
        public string BranchName { get; set; }
        public string TaskCategoryTitle { get; set; }
        public string AssignedByUserName { get; set; }

        /// <summary>
        /// لیست تمام دسته‌بندی‌ها برای انتخاب در فرم
        /// </summary>
        public List<TaskCategoryViewModel>? TaskCategoriesInitial { get; set; }

        /// <summary>
        /// لیست شناسه‌های دسته‌بندی‌های انتخاب شده برای اختصاص به شعبه
        /// فقط برای افزودن چندین دسته‌بندی استفاده می‌شود
        /// </summary>
        [Display(Name = "دسته‌بندی‌های انتخابی")]
        public List<int> TaskCategoriesSelected { get; set; }

        public string StatusText
        {
            get
            {
                return IsActive ? "فعال" : "غیرفعال";
            }
        }
    }
}