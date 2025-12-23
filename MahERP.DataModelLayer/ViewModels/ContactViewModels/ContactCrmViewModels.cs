using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel برای ایجاد فرد جدید در CRM
    /// </summary>
    public class ContactCreateViewModel
    {
        /// <summary>
        /// شناسه شعبه (الزامی برای CRM)
        /// </summary>
        [Required(ErrorMessage = "انتخاب شعبه الزامی است")]
        public int BranchId { get; set; }

        /// <summary>
        /// نوع رابطه با شعبه
        /// 0 = مشتری, 1 = تامین‌کننده, 2 = همکار, 3 = سایر
        /// </summary>
        [Required]
        public byte BranchRelationType { get; set; } = 0; // پیش‌فرض: مشتری

        // ========== اطلاعات Contact ==========

        /// <summary>
        /// نام
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(100)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        [MaxLength(10)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string? NationalCode { get; set; }

        /// <summary>
        /// ایمیل اصلی
        /// </summary>
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [Display(Name = "ایمیل")]
        public string? PrimaryEmail { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "آدرس")]
        public string? PrimaryAddress { get; set; }

        /// <summary>
        /// کد پستی
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "کد پستی")]
        public string? PrimaryPostalCode { get; set; }

        /// <summary>
        /// جنسیت
        /// 0 = مرد, 1 = زن, 2 = سایر
        /// </summary>
        [Display(Name = "جنسیت")]
        public byte? Gender { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "یادداشت‌ها")]
        public string? Notes { get; set; }

        /// <summary>
        /// شماره تلفن اصلی (برای QuickAdd)
        /// </summary>
        [MaxLength(15)]
        [Phone]
        [Display(Name = "شماره تلفن")]
        public string? PrimaryPhone { get; set; }
    }

    /// <summary>
    /// ViewModel برای ویرایش فرد در CRM
    /// </summary>
    public class ContactEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [MaxLength(100)]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(100)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [MaxLength(10)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string? NationalCode { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "ایمیل اصلی")]
        public string? PrimaryEmail { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "ایمیل دوم")]
        public string? SecondaryEmail { get; set; }

        [MaxLength(500)]
        [Display(Name = "آدرس اصلی")]
        public string? PrimaryAddress { get; set; }

        [MaxLength(500)]
        [Display(Name = "آدرس دوم")]
        public string? SecondaryAddress { get; set; }

        [MaxLength(20)]
        [Display(Name = "کد پستی")]
        public string? PrimaryPostalCode { get; set; }

        [Display(Name = "جنسیت")]
        public byte? Gender { get; set; }

        [MaxLength(2000)]
        [Display(Name = "یادداشت‌ها")]
        public string? Notes { get; set; }
    }
}
