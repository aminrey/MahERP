using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// اتصال سازمان‌ها (Organization) به شعبه
    /// برای افزودن کل سازمان به همراه اعضای آن به شعبه
    /// </summary>
    [Table("BranchOrganization_Tbl")]
    [Index(nameof(BranchId), nameof(OrganizationId), IsUnique = true, Name = "IX_BranchOrganization_Branch_Organization")]
    public class BranchOrganization
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
        /// شناسه سازمان
        /// </summary>
        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// نوع رابطه
        /// 0 = مشتری
        /// 1 = تامین‌کننده
        /// 2 = همکار
        /// 3 = شریک
        /// </summary>
        [Required]
        public byte RelationType { get; set; }

        /// <summary>
        /// آیا تمام اعضای سازمان در تسک‌ها نمایان باشند؟
        /// true = تمام اعضای DepartmentMember این سازمان در شعبه قابل انتخاب هستند
        /// false = فقط سازمان ثبت می‌شود (بدون نمایش اعضا)
        /// </summary>
        public bool IncludeAllMembers { get; set; } = true;

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
        /// شناسه کاربری که این سازمان را به شعبه اختصاص داده
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
            3 => "شریک",
            _ => "نامشخص"
        };
    }
}