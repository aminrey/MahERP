using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// اعضای گروه افراد در سطح شعبه
    /// فقط افراد BranchContact می‌توانند عضو این گروه‌ها شوند
    /// </summary>
    [Table("BranchContactGroupMember_Tbl")]
    [Index(nameof(BranchGroupId), nameof(BranchContactId), IsUnique = true, Name = "IX_BranchContactGroupMember_Group_Contact")]
    public class BranchContactGroupMember
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه گروه شعبه
        /// </summary>
        [Required]
        public int BranchGroupId { get; set; }

        [ForeignKey(nameof(BranchGroupId))]
        public virtual BranchContactGroup BranchGroup { get; set; }

        /// <summary>
        /// شناسه BranchContact (نه Contact مستقیم)
        /// فقط افرادی که به شعبه اضافه شده‌اند
        /// </summary>
        [Required]
        public int BranchContactId { get; set; }

        [ForeignKey(nameof(BranchContactId))]
        public virtual BranchContact BranchContact { get; set; }

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