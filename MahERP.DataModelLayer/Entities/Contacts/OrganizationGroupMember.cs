using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// اعضای گروه سازمان‌ها
    /// رابطه N:M بین Organization و OrganizationGroup
    /// </summary>
    [Table("OrganizationGroupMember_Tbl")]
    [Index(nameof(GroupId), nameof(OrganizationId), IsUnique = true, Name = "IX_OrganizationGroupMember_Group_Organization")]
    public class OrganizationGroupMember
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه گروه
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual OrganizationGroup Group { get; set; }

        /// <summary>
        /// شناسه سازمان
        /// </summary>
        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization Organization { get; set; }

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