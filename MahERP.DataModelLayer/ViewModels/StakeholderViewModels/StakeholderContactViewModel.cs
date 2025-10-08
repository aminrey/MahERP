using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    /// <summary>
    /// ویو مدل برای نمایش و ویرایش اطلاعات افراد مرتبط با طرف حساب
    /// </summary>
    public class StakeholderContactViewModel
    {
        public int Id { get; set; }

        public int StakeholderId { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; }

        [Display(Name = "عنوان شغلی")]
        public string? JobTitle { get; set; }

        [Display(Name = "دپارتمان")]
        public string? Department { get; set; }

        [Display(Name = "تلفن ثابت")]
        [Phone(ErrorMessage = "فرمت تلفن صحیح نیست")]
        public string? Phone { get; set; }

        [Display(Name = "تلفن همراه")]
        [Phone(ErrorMessage = "فرمت تلفن همراه صحیح نیست")]
        public string? Mobile { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string? Email { get; set; }

        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        [Display(Name = "نوع تماس")]
        public byte ContactType { get; set; }

        [Display(Name = "سطح اهمیت")]
        public byte ImportanceLevel { get; set; }

        [Display(Name = "تماس اصلی")]
        public bool IsPrimary { get; set; }

        [Display(Name = "تصمیم‌گیرنده")]
        public bool IsDecisionMaker { get; set; }

        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string ContactTypeText => ContactType switch
        {
            0 => "کارمند",
            1 => "مدیر",
            2 => "نماینده",
            3 => "تصمیم‌گیرنده",
            4 => "تماس‌گیرنده",
            5 => "سایر",
            _ => "نامشخص"
        };

        public string ImportanceLevelText => ImportanceLevel switch
        {
            0 => "پایین",
            1 => "متوسط",
            2 => "بالا",
            3 => "خیلی بالا",
            _ => "نامشخص"
        };
    }
}