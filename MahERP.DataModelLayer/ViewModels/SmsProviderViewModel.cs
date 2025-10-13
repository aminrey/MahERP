using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels
{
    public class SmsProviderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "کد خدمات‌دهنده الزامی است")]
        [Display(Name = "کد خدمات‌دهنده")]
        public string ProviderCode { get; set; }

        [Required(ErrorMessage = "نام خدمات‌دهنده الزامی است")]
        [Display(Name = "نام خدمات‌دهنده")]
        public string ProviderName { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Display(Name = "شماره اختصاصی")]
        public string SenderNumber { get; set; }

        [Display(Name = "API URL")]
        public string ApiUrl { get; set; }

        [Display(Name = "API Key")]
        public string ApiKey { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "اعتبار باقیمانده")]
        public long? RemainingCredit { get; set; }

        public DateTime? LastCreditCheckDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatorUserId { get; set; }

        // برای نمایش
        public string StatusText => IsActive ? "فعال" : "غیرفعال";
        public string CreditDisplay => RemainingCredit.HasValue 
            ? RemainingCredit.Value.ToString("N0") 
            : "نامشخص";
    }

      public class SmsProviderListViewModel
    {
        public List<SmsProviderViewModel> Providers { get; set; } = new();
        public string CurrentDefaultProvider { get; set; } = "تنظیم نشده";
    }

    
}