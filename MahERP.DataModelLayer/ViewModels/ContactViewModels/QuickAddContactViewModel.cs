using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel برای افزودن سریع فرد (Quick Add)
    /// فقط فیلدهای ضروری
    /// </summary>
    public class QuickAddContactViewModel
    {
        /// <summary>
        /// نام (اختیاری)
        /// </summary>
        [Display(Name = "نام")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی (الزامی)
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// کد ملی (اختیاری)
        /// </summary>
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        /// <summary>
        /// ایمیل (اختیاری)
        /// </summary>
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(200)]
        public string? PrimaryEmail { get; set; }

        /// <summary>
        /// شعبه (برای اتصال به شعبه)
        /// </summary>
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        /// <summary>
        /// شناسه سازمان (اختیاری - اگر می‌خواهیم به سازمان هم متصل کنیم)
        /// </summary>
        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        /// <summary>
        /// نوع رابطه با سازمان (اگر OrganizationId پر شد)
        /// </summary>
        [Display(Name = "نوع رابطه با سازمان")]
        public byte? OrganizationRelationType { get; set; }

        /// <summary>
        /// لیست شماره‌های تماس
        /// </summary>
        public List<QuickAddPhoneViewModel> Phones { get; set; } = new List<QuickAddPhoneViewModel>();
    }

    /// <summary>
    /// ViewModel برای شماره تماس در Quick Add
    /// </summary>
    public class QuickAddPhoneViewModel
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
        /// 3 = منزل
        /// </summary>
        [Display(Name = "نوع شماره")]
        public byte PhoneType { get; set; } = 0;

        /// <summary>
        /// شماره پیش‌فرض (برای احراز هویت)
        /// </summary>
        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// شماره پیش‌فرض پیامک
        /// </summary>
        [Display(Name = "پیش‌فرض پیامک")]
        public bool IsSmsDefault { get; set; } = false;
    }
}
