using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کوچک طرف حساب را وارد کنید")]
        [Display(Name = "نام")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی طرف حساب را وارد کنید")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string LastName { get; set; }

        [Display(Name = "نام شرکت")]
        [MaxLength(200, ErrorMessage = "نام شرکت نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string CompanyName { get; set; }

        [Display(Name = "تلفن ثابت")]
        [MaxLength(20, ErrorMessage = "تلفن ثابت نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9\-\+\(\)]*$", ErrorMessage = "فرمت تلفن نامعتبر است")]
        public string Phone { get; set; }
        
        [Display(Name = "تلفن همراه")]
        [MaxLength(20, ErrorMessage = "تلفن همراه نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9\-\+\(\)]*$", ErrorMessage = "فرمت موبایل نامعتبر است")]
        public string Mobile { get; set; }

        [Display(Name = "ایمیل")]
        [MaxLength(100, ErrorMessage = "ایمیل نمی‌تواند بیش از 100 کاراکتر باشد")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Address { get; set; }

        [Display(Name = "کد پستی")]
        [MaxLength(20, ErrorMessage = "کد پستی نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "کد پستی فقط شامل اعداد می‌باشد")]
        public string PostalCode { get; set; }

        [Display(Name = "کد ملی")]
        [MaxLength(20, ErrorMessage = "کد ملی نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "کد ملی فقط شامل اعداد می‌باشد")]
        public string NationalCode { get; set; }
        
        [Display(Name = "توضیحات")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "نوع طرف حساب")]
        public byte StakeholderType { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        // اطلاعات CRM
        public StakeholderCRMViewModel CRMInfo { get; set; }

        // نمایش نام کامل
        public string FullName => $"{FirstName} {LastName}";

        // نمایش نوع طرف حساب به صورت متنی
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