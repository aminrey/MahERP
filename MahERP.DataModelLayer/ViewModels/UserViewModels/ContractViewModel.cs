using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class ContractViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان قرارداد الزامی است")]
        [Display(Name = "عنوان قرارداد")]
        [MaxLength(200, ErrorMessage = "عنوان قرارداد نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string Title { get; set; }

        [Required(ErrorMessage = "شماره قرارداد الزامی است")]
        [Display(Name = "شماره قرارداد")]
        [MaxLength(50, ErrorMessage = "شماره قرارداد نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string ContractNumber { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string Description { get; set; }

        [Required(ErrorMessage = "انتخاب طرف حساب الزامی است")]
        [Display(Name = "طرف حساب")]
        public int StakeholderId { get; set; }

        [Required(ErrorMessage = "تاریخ شروع قرارداد الزامی است")]
        [Display(Name = "تاریخ شروع")]
        public string StartDatePersian { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string EndDatePersian { get; set; }

        [Display(Name = "مبلغ قرارداد (تومان)")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ قرارداد باید عددی مثبت باشد")]
        public decimal ContractValue { get; set; }

        [Display(Name = "وضعیت")]
        public byte Status { get; set; } = 1;

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        public string LastUpdaterUserId { get; set; }

        // اطلاعات نمایشی
        public string StakeholderFullName { get; set; }
        public string CreatorFullName { get; set; }
        public string LastUpdaterFullName { get; set; }

        public string StatusText
        {
            get
            {
                return Status switch
                {
                    0 => "پیش‌نویس",
                    1 => "فعال",
                    2 => "تمام شده",
                    3 => "لغو شده",
                    _ => "نامشخص"
                };
            }
        }
    }
}