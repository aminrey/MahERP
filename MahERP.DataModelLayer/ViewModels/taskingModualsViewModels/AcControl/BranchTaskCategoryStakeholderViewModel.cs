using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using MahERP.CommonLayer.PublicClasses; // اضافه شده
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ تخصیص
        /// </summary>
        [Display(Name = "تاریخ تخصیص")]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر تخصیص دهنده
        /// </summary>
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
        [Display(Name = "تخصیص دهنده")]
        public string? AssignedByUserName { get; set; }

        // لیست‌های مورد نیاز برای فرم‌ها
        /// <summary>
        /// لیست دسته‌بندی‌های در دسترس برای انتخاب
        /// </summary>
        public List<TaskCategoryItemViewModel>? TaskCategoryInitial { get; set; }

        /// <summary>
        /// لیست طرف حساب‌های در دسترس برای انتخاب
        /// </summary>
        public List<StakeholderItemViewModel>? StakeholdersInitial { get; set; }

        /// <summary>
        /// متن وضعیت فعال/غیرفعال برای نمایش
        /// </summary>
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// تاریخ شمسی تخصیص برای نمایش
        /// </summary>
        public string AssignDatePersian => ConvertDateTime.ConvertMiladiToShamsi(AssignDate, "yyyy/MM/dd");
    }

    /// <summary>
    /// ViewModel برای آیتم‌های دسته‌بندی تسک در لیست‌های کشویی
    /// </summary>
    public class TaskCategoryItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel برای آیتم‌های طرف حساب در لیست‌های کشویی
    /// </summary>
    public class StakeholderItemViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? CompanyName { get; set; }
        public byte StakeholderType { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// نام کامل برای نمایش
        /// </summary>
        public string DisplayName
        {
            get
            {
                var fullName = $"{FirstName} {LastName}";
                if (!string.IsNullOrEmpty(CompanyName))
                    fullName += $" ({CompanyName})";
                return fullName;
            }
        }

        /// <summary>
        /// نوع طرف حساب به صورت متنی
        /// </summary>
        public string StakeholderTypeText
        {
            get
            {
                return StakeholderType switch
                {
                    0 => "مشتری",
                    1 => "تامین کننده",
                    2 => "همکار",
                    3 => "سایر",
                    _ => "نامشخص"
                };
            }
        }
    }
}