using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کوچک طرف حساب را وارد کنید")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی طرف حساب را وارد کنید")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام شرکت")]
        public string CompanyName { get; set; }

        [Display(Name = "تلفن ثابت")]
        public string Phone { get; set; }
        
        [Display(Name = "تلفن همراه")]
        public string Mobile { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string Email { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "کد پستی")]
        public string PostalCode { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }
        
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "نوع طرف حساب")]
        public byte StakeholderType { get; set; }

        [Display(Name = "وضعیت")]
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
                switch (StakeholderType)
                {
                    case 0:
                        return "مشتری";
                    case 1:
                        return "تامین کننده";
                    case 2:
                        return "همکار";
                    case 3:
                        return "سایر";
                    default:
                        return "نامشخص";
                }
            }
        }
    }
}