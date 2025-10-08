using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// سمت‌های موجود در واحد سازمانی طرف حساب
    /// </summary>
    public class StakeholderOrganizationPosition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }
        
        [ForeignKey(nameof(OrganizationId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual StakeholderOrganization Organization { get; set; }

        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح قدرت (برای تعیین سلسله مراتب)
        /// عدد کمتر = قدرت بیشتر
        /// </summary>
        public int PowerLevel { get; set; }

        /// <summary>
        /// آیا این سمت پیش‌فرض است؟
        /// </summary>
        public bool IsDefault { get; set; }

        public int DisplayOrder { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers Creator { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [InverseProperty(nameof(StakeholderOrganizationMember.Position))]
        public virtual ICollection<StakeholderOrganizationMember> Members { get; set; } = new HashSet<StakeholderOrganizationMember>();
    }
}