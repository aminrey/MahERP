using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    /// <summary>
    /// ویو مدل برای نمایش و ویرایش اطلاعات افراد مرتبط با طرف حساب
    /// </summary>
    public class StakeholderContactViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "شناسه طرف حساب الزامی است")]
        public int StakeholderId { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [Display(Name = "نام")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string LastName { get; set; }

        [Display(Name = "سمت")]
        [MaxLength(150, ErrorMessage = "سمت نمی‌تواند بیش از 150 کاراکتر باشد")]
        public string? Position { get; set; }

        [Display(Name = "تلفن ثابت")]
        [MaxLength(20, ErrorMessage = "تلفن ثابت نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9\-\+\(\)]*$", ErrorMessage = "فرمت تلفن نامعتبر است")]
        public string? Phone { get; set; }

        [Display(Name = "تلفن همراه")]
        [MaxLength(20, ErrorMessage = "تلفن همراه نمی‌تواند بیش از 20 کاراکتر باشد")]
        [RegularExpression(@"^[0-9\-\+\(\)]*$", ErrorMessage = "فرمت موبایل نامعتبر است")]
        public string? Mobile { get; set; }

        [Display(Name = "ایمیل")]
        [MaxLength(100, ErrorMessage = "ایمیل نمی‌تواند بیش از 100 کاراکتر باشد")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string? Email { get; set; }

        [Display(Name = "نوع ارتباط")]
        public byte ContactType { get; set; }

        [Display(Name = "یادداشت")]
        [MaxLength(500, ErrorMessage = "یادداشت نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? Notes { get; set; }

        [Display(Name = "مخاطب اصلی")]
        public bool IsPrimary { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ ثبت")]
        public DateTime CreateDate { get; set; }

        public string? CreatorUserId { get; set; }

        /// <summary>
        /// نمایش نام کامل فرد مرتبط
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// نمایش نوع ارتباط به صورت متنی
        /// </summary>
        public string ContactTypeText => ContactType switch
        {
            0 => "اصلی",
            1 => "مدیر",
            2 => "کارمند",
            3 => "نماینده",
            4 => "سایر",
            _ => "نامشخص"
        };
    }
}