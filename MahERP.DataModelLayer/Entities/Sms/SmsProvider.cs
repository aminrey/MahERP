using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Sms
{
    /// <summary>
    /// خدمات‌دهندگان پیامک (SMS Providers)
    /// </summary>
    [Table("SmsProvider_Tbl")]
    [Index(nameof(ProviderCode), IsUnique = true, Name = "IX_SmsProvider_Code")]
    public class SmsProvider
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد یکتای خدمات‌دهنده (مثل: SUNWAY, PAYAMITO)
        /// </summary>
        [Required(ErrorMessage = "کد خدمات‌دهنده الزامی است")]
        [MaxLength(50)]
        public string ProviderCode { get; set; }

        /// <summary>
        /// نام نمایشی خدمات‌دهنده
        /// </summary>
        [Required(ErrorMessage = "نام خدمات‌دهنده الزامی است")]
        [MaxLength(200)]
        public string ProviderName { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        // ========== اطلاعات لاگین ==========

        /// <summary>
        /// نام کاربری
        /// </summary>
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(200)]
        public string Username { get; set; }

        /// <summary>
        /// رمز عبور (رمزنگاری شده)
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MaxLength(500)]
        public string Password { get; set; }

        /// <summary>
        /// شماره اختصاصی (برای ارسال)
        /// </summary>
        [MaxLength(20)]
        public string? SenderNumber { get; set; }

        /// <summary>
        /// API URL (اختیاری - برای Providerهایی که URL متغیر دارند)
        /// </summary>
        [MaxLength(500)]
        public string? ApiUrl { get; set; }

        /// <summary>
        /// API Key (اختیاری - برای Providerهایی که نیاز به کلید API دارند)
        /// </summary>
        [MaxLength(500)]
        public string? ApiKey { get; set; }

        /// <summary>
        /// تنظیمات اضافی (JSON Format)
        /// برای ذخیره پارامترهای اختصاصی هر Provider
        /// </summary>
        public string? AdditionalSettings { get; set; }

        // ========== وضعیت ==========

        /// <summary>
        /// آیا این Provider پیش‌فرض است؟
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// اعتبار باقیمانده (کش می‌شود)
        /// </summary>
        public long? RemainingCredit { get; set; }

        /// <summary>
        /// آخرین زمان بررسی اعتبار
        /// </summary>
        public DateTime? LastCreditCheckDate { get; set; }

        // ========== اطلاعات سیستمی ==========

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        [NotMapped]
        public string CreditDisplay => RemainingCredit.HasValue 
            ? RemainingCredit.Value.ToString("N0") 
            : "نامشخص";
    }
}