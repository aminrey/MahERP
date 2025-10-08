using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "نوع شخص")]
        [Required(ErrorMessage = "نوع شخص الزامی است")]
        public byte PersonType { get; set; }

        [Display(Name = "نوع طرف حساب")]
        public byte StakeholderType { get; set; }

        // ========== فیلدهای مشترک ==========
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string? Email { get; set; }

        [Display(Name = "تلفن ثابت")]
        [Phone(ErrorMessage = "فرمت تلفن صحیح نیست")]
        public string? Phone { get; set; }

        [Display(Name = "تلفن همراه")]
        [Phone(ErrorMessage = "فرمت تلفن همراه صحیح نیست")]
        public string? Mobile { get; set; }

        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [Display(Name = "کد پستی")]
        public string? PostalCode { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        // ========== فیلدهای شخص حقیقی ==========
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        public string? BirthDate { get; set; }

        [Display(Name = "جنسیت")]
        public byte? Gender { get; set; }

        // ========== فیلدهای شخص حقوقی ==========
        [Display(Name = "نام شرکت")]
        public string? CompanyName { get; set; }

        [Display(Name = "نام برند")]
        public string? CompanyBrand { get; set; }

        [Display(Name = "شماره ثبت")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "کد اقتصادی")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        public string? EconomicCode { get; set; }

        [Display(Name = "تاریخ ثبت شرکت")]
        public string? RegistrationDate { get; set; }

        [Display(Name = "آدرس ثبت شده")]
        public string? RegisteredAddress { get; set; }

        [Display(Name = "وب‌سایت")]
        [Url(ErrorMessage = "فرمت وب‌سایت صحیح نیست")]
        public string? Website { get; set; }

        [Display(Name = "نماینده قانونی")]
        public string? LegalRepresentative { get; set; }

        // ========== فیلدهای سیستمی ==========
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }

        // ========== Computed Properties ==========
        public string DisplayName => PersonType == 0 
            ? $"{FirstName} {LastName}" 
            : CompanyName ?? "نامشخص";

        public string PersonTypeText => PersonType switch
        {
            0 => "شخص حقیقی",
            1 => "شخص حقوقی",
            _ => "نامشخص"
        };

        public string StakeholderTypeText => StakeholderType switch
        {
            0 => "مشتری",
            1 => "تامین‌کننده",
            2 => "همکار",
            3 => "سایر",
            _ => "نامشخص"
        };

        public string GenderText => Gender switch
        {
            0 => "مرد",
            1 => "زن",
            _ => "-"
        };

        public StakeholderCRMViewModel? CRMInfo { get; set; }
    }
}