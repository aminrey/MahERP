using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای ایجاد سازمان جدید در CRM
    /// </summary>
    public class OrganizationCreateViewModel
    {
        /// <summary>
        /// شناسه شعبه (الزامی برای CRM)
        /// </summary>
        [Required(ErrorMessage = "انتخاب شعبه الزامی است")]
        public int BranchId { get; set; }

        // ========== اطلاعات Organization ==========

        /// <summary>
        /// نام سازمان
        /// </summary>
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [MaxLength(200)]
        [Display(Name = "نام سازمان")]
        public string Name { get; set; }

        /// <summary>
        /// نام برند
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "نام برند")]
        public string? Brand { get; set; }

        /// <summary>
        /// شماره ثبت شرکت
        /// </summary>
        [MaxLength(11)]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        [Display(Name = "شماره ثبت")]
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// کد اقتصادی
        /// </summary>
        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        [Display(Name = "کد اقتصادی")]
        public string? EconomicCode { get; set; }

        /// <summary>
        /// وب‌سایت
        /// </summary>
        [MaxLength(200)]
        [Url(ErrorMessage = "فرمت وب‌سایت نامعتبر است")]
        [Display(Name = "وب‌سایت")]
        public string? Website { get; set; }

        /// <summary>
        /// نماینده قانونی
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "نماینده قانونی")]
        public string? LegalRepresentative { get; set; }

        /// <summary>
        /// تلفن اصلی
        /// </summary>
        [MaxLength(15)]
        [Phone]
        [Display(Name = "تلفن")]
        public string? PrimaryPhone { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        /// <summary>
        /// کد پستی
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "کد پستی")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// نوع سازمان
        /// 0 = شرکت, 1 = سازمان, 2 = موسسه, 3 = نهاد
        /// </summary>
        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; } = 0;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// ViewModel برای ویرایش سازمان در CRM
    /// </summary>
    public class OrganizationEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [MaxLength(200)]
        [Display(Name = "نام سازمان")]
        public string Name { get; set; }

        [MaxLength(100)]
        [Display(Name = "نام برند")]
        public string? Brand { get; set; }

        [MaxLength(11)]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        [Display(Name = "شماره ثبت")]
        public string? RegistrationNumber { get; set; }

        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        [Display(Name = "کد اقتصادی")]
        public string? EconomicCode { get; set; }

        [MaxLength(200)]
        [Url]
        [Display(Name = "وب‌سایت")]
        public string? Website { get; set; }

        [MaxLength(200)]
        [Display(Name = "نماینده قانونی")]
        public string? LegalRepresentative { get; set; }

        [MaxLength(15)]
        [Phone]
        [Display(Name = "تلفن اصلی")]
        public string? PrimaryPhone { get; set; }

        [MaxLength(15)]
        [Phone]
        [Display(Name = "تلفن دوم")]
        public string? SecondaryPhone { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [MaxLength(500)]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [MaxLength(20)]
        [Display(Name = "کد پستی")]
        public string? PostalCode { get; set; }

        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; }

        [MaxLength(2000)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
    }
}
