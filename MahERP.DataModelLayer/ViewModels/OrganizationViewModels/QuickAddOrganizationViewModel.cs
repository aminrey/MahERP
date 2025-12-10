using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای افزودن سریع سازمان (Quick Add)
    /// فقط فیلدهای ضروری
    /// </summary>
    public class QuickAddOrganizationViewModel
    {
        /// <summary>
        /// نام سازمان (الزامی)
        /// </summary>
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [Display(Name = "نام سازمان")]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// نام برند (اختیاری)
        /// </summary>
        [Display(Name = "نام برند")]
        [MaxLength(100)]
        public string? Brand { get; set; }

        /// <summary>
        /// شماره ثبت (اختیاری)
        /// </summary>
        [Display(Name = "شماره ثبت")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// ایمیل (اختیاری)
        /// </summary>
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>
        /// شعبه (برای اتصال به شعبه)
        /// </summary>
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        /// <summary>
        /// نوع سازمان
        /// 0 = شرکت
        /// 1 = سازمان
        /// 2 = موسسه
        /// 3 = نهاد
        /// </summary>
        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; } = 0;

        /// <summary>
        /// لیست شماره‌های تماس
        /// </summary>
        public List<QuickAddOrganizationPhoneViewModel> Phones { get; set; } = new List<QuickAddOrganizationPhoneViewModel>();
    }

    /// <summary>
    /// ViewModel برای شماره تماس سازمان در Quick Add
    /// </summary>
    public class QuickAddOrganizationPhoneViewModel
    {
        /// <summary>
        /// شماره تماس
        /// </summary>
        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// نوع شماره
        /// 0 = موبایل
        /// 1 = ثابت
        /// 2 = کاری
        /// </summary>
        [Display(Name = "نوع شماره")]
        public byte PhoneType { get; set; } = 1; // پیش‌فرض: ثابت

        /// <summary>
        /// داخلی (اختیاری)
        /// </summary>
        [Display(Name = "داخلی")]
        [MaxLength(10)]
        public string? Extension { get; set; }

        /// <summary>
        /// شماره پیش‌فرض
        /// </summary>
        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; } = false;
    }
}
