using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class BranchViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام شعبه الزامی است")]
        [Display(Name = "نام شعبه")]
        [MaxLength(100, ErrorMessage = "نام شعبه نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(200, ErrorMessage = "آدرس نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string Address { get; set; }

        [Display(Name = "تلفن")]
        [MaxLength(20, ErrorMessage = "تلفن نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9\-\+\(\)]*$", ErrorMessage = "فرمت تلفن نامعتبر است")]
        public string Phone { get; set; }

        [Display(Name = "ایمیل")]
        [MaxLength(50, ErrorMessage = "ایمیل نمی‌تواند بیش از 50 کاراکتر باشد")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        [Display(Name = "نام مدیر")]
        [MaxLength(100, ErrorMessage = "نام مدیر نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string ManagerName { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "شعبه اصلی")]
        public bool IsMainBranch { get; set; }

        [Display(Name = "شعبه مادر")]
        public int? ParentId { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }
    }


    public class BranchUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "شناسه شعبه الزامی است")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "کاربر")]
        public string UserId { get; set; }

        [Display(Name = "نقش")]
        public byte Role { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ انتساب")]
        public DateTime AssignDate { get; set; }

        public string AssignedByUserId { get; set; }

        // اطلاعات نمایشی
        public string BranchName { get; set; }
        public string UserFullName { get; set; }
        public string RoleText 
        { 
            get
            {
                return Role switch
                {
                    0 => "کارشناس",
                    1 => "ناظر",
                    2 => "مدیر",
                    _ => "نامشخص"
                };
            }
        }
    }
}