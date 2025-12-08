using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// شماره‌های تماس سازمان‌ها
    /// </summary>
    [Index(nameof(OrganizationId), nameof(PhoneNumber), IsUnique = true, Name = "IX_OrganizationPhone_Org_Number")]
    public class OrganizationPhone
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه سازمان
        /// </summary>
        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// نوع شماره
        /// 0 = موبایل
        /// 1 = تلفن ثابت
        /// 2 = کاری
        /// </summary>
        [Required]
        public byte PhoneType { get; set; }

        /// <summary>
        /// شماره تماس (بدون فاصله و خط تیره)
        /// </summary>
        [Required]
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
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        // ========== اطلاعات سیستمی ==========
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string PhoneTypeText => PhoneType switch
        {
            0 => "موبایل",
            1 => "تلفن ثابت",
            2 => "کاری",
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
                return text;
            }
        }
    }
}
