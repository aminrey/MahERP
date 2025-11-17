using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Entities.Sms
{
    /// <summary>
    /// مخاطبین قالب پیامک
    /// </summary>
    [Table("SmsTemplateRecipient_Tbl")]
    public class SmsTemplateRecipient
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه قالب
        /// </summary>
        [Required]
        public int TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual SmsTemplate? Template { get; set; }

        /// <summary>
        /// نوع مخاطب: 0=Contact, 1=Organization
        /// </summary>
        [Required]
        public byte RecipientType { get; set; }

        /// <summary>
        /// شناسه Contact (اگر نوع = 0)
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// شناسه شماره تماس خاص Contact (اختیاری - اگر نباشد از DefaultPhone استفاده می‌شود)
        /// </summary>
        public int? ContactPhoneId { get; set; }

        [ForeignKey(nameof(ContactPhoneId))]
        public virtual ContactPhone? ContactPhone { get; set; }

        /// <summary>
        /// شناسه Organization (اگر نوع = 1)
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// تاریخ افزودن
        /// </summary>
        [Required]
        public DateTime AddedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربری که اضافه کرده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AddedByUserId { get; set; }

        [ForeignKey(nameof(AddedByUserId))]
        public virtual AppUsers? AddedBy { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string RecipientTypeText => RecipientType switch
        {
            0 => "شخص",
            1 => "سازمان",
            _ => "نامشخص"
        };

        [NotMapped]
        public string RecipientName => RecipientType switch
        {
            0 => Contact?.FullName ?? "نامشخص",
            1 => Organization?.DisplayName ?? "نامشخص",
            _ => "نامشخص"
        };

        [NotMapped]
        public string RecipientContact
        {
            get
            {
                if (RecipientType == 0) // Contact
                {
                    // اگر شماره خاصی انتخاب شده، از اون استفاده کن
                    if (ContactPhone != null)
                        return ContactPhone.FormattedNumber;
                    
                    // وگرنه از شماره پیش‌فرض استفاده کن
                    return Contact?.DefaultPhone?.FormattedNumber ?? Contact?.PrimaryEmail ?? "";
                }
                else if (RecipientType == 1) // Organization
                {
                    return Organization?.PrimaryPhone ?? Organization?.Email ?? "";
                }
                
                return "";
            }
        }

        /// <summary>
        /// شماره واقعی برای ارسال پیامک
        /// </summary>
        [NotMapped]
        public string ActualPhoneNumber
        {
            get
            {
                if (RecipientType == 0) // Contact
                {
                    // اگر شماره خاصی انتخاب شده
                    if (ContactPhone != null)
                        return ContactPhone.PhoneNumber;
                    
                    // وگرنه از شماره پیش‌فرض
                    return Contact?.DefaultPhone?.PhoneNumber ?? "";
                }
                else if (RecipientType == 1) // Organization
                {
                    return Organization?.PrimaryPhone ?? "";
                }
                
                return "";
            }
        }
    }
}