using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// اعضای گروه افراد
    /// رابطه N:M بین Contact و ContactGroup
    /// </summary>
    [Table("ContactGroupMember_Tbl")]
    [Index(nameof(GroupId), nameof(ContactId), IsUnique = true, Name = "IX_ContactGroupMember_Group_Contact")]
    public class ContactGroupMember
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه گروه
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual ContactGroup Group { get; set; }

        /// <summary>
        /// شناسه فرد
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact Contact { get; set; }

        /// <summary>
        /// تاریخ افزودن به گروه
        /// </summary>
        [Required]
        public DateTime AddedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// یادداشت (دلیل افزودن، توضیحات و...)
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // ========== System Info ==========
        [Required]
        [MaxLength(450)]
        public string AddedByUserId { get; set; }

        [ForeignKey(nameof(AddedByUserId))]
        public virtual AppUsers? AddedByUser { get; set; }

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string AddedDatePersian => 
            CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(AddedDate, "yyyy/MM/dd");
    }
}