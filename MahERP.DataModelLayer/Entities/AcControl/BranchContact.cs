using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// اتصال افراد (Contact) به شعبه
    /// برای افزودن افراد به صورت تکی به شعبه
    /// </summary>
    [Table("BranchContact_Tbl")]
    [Index(nameof(BranchId), nameof(ContactId), IsUnique = true, Name = "IX_BranchContact_Branch_Contact")]
    public class BranchContact
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// شناسه فرد
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact Contact { get; set; }

        /// <summary>
        /// نوع رابطه
        /// 0 = مشتری
        /// 1 = تامین‌کننده
        /// 2 = همکار
        /// 3 = سایر
        /// </summary>
        [Required]
        public byte RelationType { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ اختصاص به شعبه
        /// </summary>
        [Required]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که این فرد را به شعبه اختصاص داده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AssignedByUserId { get; set; }

        [ForeignKey(nameof(AssignedByUserId))]
        public virtual AppUsers AssignedBy { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string RelationTypeText => RelationType switch
        {
            0 => "مشتری",
            1 => "تامین‌کننده",
            2 => "همکار",
            3 => "سایر",
            _ => "نامشخص"
        };
    }
}