using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel برای شماره تماس
    /// </summary>
    public class ContactPhoneViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "نوع شماره الزامی است")]
        [Display(Name = "نوع شماره")]
        public byte PhoneType { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [Display(Name = "شماره تماس")]
        [Phone(ErrorMessage = "فرمت شماره نامعتبر است")]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Display(Name = "داخلی")]
        [MaxLength(10)]
        public string? Extension { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "تایید شده")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        // ========== Computed Properties ==========
        
        [Display(Name = "نوع شماره")]
        public string PhoneTypeText => PhoneType switch
        {
            0 => "موبایل",
            1 => "تلفن ثابت",
            2 => "اضطراری",
            3 => "کاری",
            4 => "منزل",
            _ => "نامشخص"
        };

        [Display(Name = "شماره فرمت شده")]
        public string? FormattedNumber { get; set; }

        [Display(Name = "متن نمایشی")]
        public string? DisplayText { get; set; }
    }
}