using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.CommonLayer.PublicClasses; // اضافه شده

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl
{
    /// <summary>
    /// ViewModel برای مدیریت انتصاب دسته‌بندی تسک به شعبه با طرف حساب
    /// </summary>
    public class BranchTaskCategoryStakeholderViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        [Required(ErrorMessage = "شناسه شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی تسک انتخاب شده
        /// </summary>
        [Required(ErrorMessage = "حداقل یک دسته‌بندی باید انتخاب شود")]
        [Display(Name = "دسته‌بندی تسک")]
        public int TaskCategoryIdSelected { get; set; }

        /// <summary>
        /// شناسه طرف حساب (اجباری)
        /// </summary>
        [Required(ErrorMessage = "انتخاب طرف حساب الزامی است")]
        [Display(Name = "طرف حساب")]
        public int StakeholderId { get; set; }

        /// <summary>
        /// لیست دسته‌بندی‌های انتخاب شده (برای افزودن چندتایی)
        /// </summary>
        [Display(Name = "دسته‌بندی‌های انتخاب شده")]
        public List<int>? TaskCategoriesSelected { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        [Required]
        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ تخصیص
        /// </summary>
        [Required]
        [Display(Name = "تاریخ تخصیص")]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر تخصیص دهنده
        /// </summary>
        [Display(Name = "تخصیص دهنده")]
        public string? AssignedByUserId { get; set; }

        // اطلاعات نمایشی

        /// <summary>
        /// نام شعبه (برای نمایش)
        /// </summary>
        [Display(Name = "نام شعبه")]
        public string? BranchName { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی تسک (برای نمایش)
        /// </summary>
        [Display(Name = "عنوان دسته‌بندی")]
        public string? TaskCategoryTitle { get; set; }

        /// <summary>
        /// نام طرف حساب (برای نمایش)
        /// </summary>
        [Display(Name = "نام طرف حساب")]
        public string? StakeholderName { get; set; }

        /// <summary>
        /// نام کاربر تخصیص دهنده (برای نمایش)
        /// </summary>
        [Display(Name = "نام تخصیص دهنده")]
        public string? AssignedByUserName { get; set; }

        /// <summary>
        /// تاریخ تخصیص به صورت شمسی (برای نمایش)
        /// </summary>
        [Display(Name = "تاریخ تخصیص شمسی")]
        public string? AssignDatePersian { get; set; }

        /// <summary>
        /// وضعیت به صورت متنی (برای نمایش)
        /// </summary>
        [Display(Name = "وضعیت")]
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        // لیست‌های مورد نیاز برای فرم‌ها

        /// <summary>
        /// لیست دسته‌بندی‌های در دسترس برای انتخاب
        /// </summary>
        public List<TaskCategoryItemViewModel>? TaskCategoryInitial { get; set; }

        /// <summary>
        /// لیست طرف حساب‌های در دسترس برای انتخاب
        /// </summary>
        public List<StakeholderItemViewModel>? StakeholdersInitial { get; set; }
    }

    /// <summary>
    /// ViewModel کوچک برای آیتم‌های دسته‌بندی تسک
    /// </summary>
    public class TaskCategoryItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel کوچک برای آیتم‌های طرف حساب
    /// </summary>
    public class StakeholderItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام شرکت")]
        public string? CompanyName { get; set; }

        [Display(Name = "نوع طرف حساب")]
        public byte StakeholderType { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش نام کامل با در نظر گیری نام شرکت
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(CompanyName) 
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {LastName} ({CompanyName})";

        /// <summary>
        /// نمایش نوع طرف حساب به صورت متنی
        /// </summary>
        public string StakeholderTypeText
        {
            get
            {
                return StakeholderType switch
                {
                    0 => "مشتری",
                    1 => "تامین کننده", 
                    2 => "نماینده فروش",
                    3 => "شریک",
                    4 => "بازاریاب",
                    _ => "نامشخص"
                };
            }
        }
    }
}