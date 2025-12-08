using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش شماره تماس سازمان
    /// </summary>
    public class OrganizationPhoneViewModel
    {
        public int Id { get; set; }

        [Display(Name = "سازمان")]
        public int OrganizationId { get; set; }

        [Display(Name = "شماره تماس")]
        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Display(Name = "نوع شماره")]
        public byte PhoneType { get; set; }

        [Display(Name = "داخلی")]
        [MaxLength(10)]
        public string? Extension { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        // Computed Properties
        public string? PhoneTypeText { get; set; }
        public string? FormattedNumber { get; set; }
    }

    /// <summary>
    /// ViewModel برای ورودی شماره تماس سازمان در فرم Create/Edit
    /// </summary>
    public class OrganizationPhoneInputViewModel
    {
        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        public byte PhoneType { get; set; } = 1; // پیش‌فرض: ثابت

        public string? Extension { get; set; }

        public bool IsDefault { get; set; }
    }
}
