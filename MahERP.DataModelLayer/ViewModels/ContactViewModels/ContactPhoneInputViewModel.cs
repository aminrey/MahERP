using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel ساده برای دریافت شماره‌های تماس در فرم Create
    /// </summary>
    public class ContactPhoneInputViewModel
    {
        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [Display(Name = "شماره تماس")]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Display(Name = "نوع شماره")]
        public byte PhoneType { get; set; } = 0; // پیش‌فرض: موبایل

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "پیش‌فرض پیامک")]
        public bool IsSmsDefault { get; set; } = false;

        // ========== Computed Properties ==========

        [Display(Name = "نوع شماره")]
        public string PhoneTypeText => PhoneType switch
        {
            0 => "موبایل",
            1 => "ثابت",
            2 => "کاری",
            3 => "منزل",
            _ => "نامشخص"
        };
    }
}
