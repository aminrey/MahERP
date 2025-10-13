using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Entities.Email
{
    /// <summary>
    /// مخاطبین قالب ایمیل
    /// </summary>
    [Table("EmailTemplateRecipient_Tbl")]
    public class EmailTemplateRecipient
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه قالب
        /// </summary>
        [Required]
        public int TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual EmailTemplate Template { get; set; }

        /// <summary>
        /// نوع مخاطب: 0=Contact, 1=Organization
        /// </summary>
        [Required]
        public byte RecipientType { get; set; }

        /// <summary>
        /// شناسه Contact
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact Contact { get; set; }

        /// <summary>
        /// شناسه Organization
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// تاریخ افزودن
        /// </summary>
        [Required]
        public DateTime AddedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string AddedByUserId { get; set; }

        [ForeignKey(nameof(AddedByUserId))]
        public virtual AppUsers AddedBy { get; set; }

        // ========== Computed Properties ==========

        [NotMapped]
        public string RecipientName => RecipientType switch
        {
            0 => Contact?.FullName ?? "نامشخص",
            1 => Organization?.DisplayName ?? "نامشخص",
            _ => "نامشخص"
        };

        [NotMapped]
        public string RecipientEmail => RecipientType switch
        {
            0 => Contact?.PrimaryEmail ?? "",
            1 => Organization?.Email ?? "",
            _ => ""
        };
    }
}