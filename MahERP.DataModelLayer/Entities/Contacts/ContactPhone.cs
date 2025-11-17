using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// شماره‌های تماس افراد
    /// </summary>
    [Index(nameof(ContactId), nameof(PhoneNumber), IsUnique = true, Name = "IX_ContactPhone_Contact_Number")]
    public class ContactPhone
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه فرد
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// نوع شماره
        /// 0 = موبایل
        /// 1 = تلفن ثابت
        /// 2 = اضطراری
        /// 3 = کاری
        /// 4 = منزل
        /// </summary>
        [Required]
        public byte PhoneType { get; set; }

        /// <summary>
        /// شماره تماس (بدون فاصله و خط تیره)
        /// </summary>
        [MaxLength(15)]
        [Phone(ErrorMessage = "فرمت شماره نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// داخلی (برای تلفن ثابت)
        /// </summary>
        [MaxLength(10)]
        public string? Extension { get; set; }

        /// <summary>
        /// آیا این شماره پیش‌فرض است؟
        /// (برای احراز هویت استفاده می‌شود)
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// آیا این شماره برای ارسال پیامک پیش‌فرض است؟
        /// هر Contact فقط یک شماره با IsSmsDefault = true دارد
        /// </summary>
        public bool IsSmsDefault { get; set; } = false;

        /// <summary>
        /// آیا شماره تایید شده است؟
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// تاریخ تایید شماره
        /// </summary>
        public DateTime? VerifiedDate { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }
        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string PhoneTypeText => PhoneType switch
        {
            0 => "موبایل",
            1 => "تلفن ثابت",
            2 => "اضطراری",
            3 => "کاری",
            4 => "منزل",
            _ => "نامشخص"
        };

        [NotMapped]
        public string FormattedNumber
        {
            get
            {
                if (string.IsNullOrEmpty(PhoneNumber)) return "-";
                
                // فرمت موبایل: 0912 345 6789
                if (PhoneType == 0 && PhoneNumber.Length == 11)
                {
                    return $"{PhoneNumber.Substring(0, 4)} {PhoneNumber.Substring(4, 3)} {PhoneNumber.Substring(7)}";
                }
                
                // فرمت تلفن ثابت: 021-12345678
                if (PhoneType == 1 && PhoneNumber.Length > 3)
                {
                    var areaCode = PhoneNumber.Substring(0, PhoneNumber.Length - 8);
                    var number = PhoneNumber.Substring(PhoneNumber.Length - 8);
                    return $"{areaCode}-{number}";
                }
                
                return PhoneNumber;
            }
        }

        [NotMapped]
        public string DisplayText
        {
            get
            {
                var text = $"{FormattedNumber} ({PhoneTypeText})";
                if (!string.IsNullOrEmpty(Extension))
                    text += $" داخلی: {Extension}";
                if (IsDefault)
                    text += " [پیش‌فرض]";
                if (IsSmsDefault)
                    text += " [پیامک]";
                return text;
            }
        }
    }
}