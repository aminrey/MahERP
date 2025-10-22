using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Organizations
{
    /// <summary>
    /// اعضای گروه سازمان‌ها در سطح شعبه
    /// فقط سازمان‌های BranchOrganization می‌توانند عضو این گروه‌ها شوند
    /// </summary>
    [Table("BranchOrganizationGroupMember_Tbl")]
    [Index(nameof(BranchGroupId), nameof(BranchOrganizationId), IsUnique = true, Name = "IX_BranchOrganizationGroupMember_Group_Organization")]
    public class BranchOrganizationGroupMember
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه گروه شعبه
        /// </summary>
        [Required]
        public int BranchGroupId { get; set; }

        [ForeignKey(nameof(BranchGroupId))]
        public virtual BranchOrganizationGroup BranchGroup { get; set; }

        /// <summary>
        /// شناسه BranchOrganization (نه Organization مستقیم)
        /// فقط سازمان‌هایی که به شعبه اضافه شده‌اند
        /// </summary>
        [Required]
        public int BranchOrganizationId { get; set; }

        [ForeignKey(nameof(BranchOrganizationId))]
        public virtual BranchOrganization BranchOrganization { get; set; }

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
        /// یادداشت
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