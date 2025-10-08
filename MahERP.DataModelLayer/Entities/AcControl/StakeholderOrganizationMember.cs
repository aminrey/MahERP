using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// اعضای واحد سازمانی طرف حساب
    /// </summary>
    public class StakeholderOrganizationMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }
        
        [ForeignKey(nameof(OrganizationId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual StakeholderOrganization Organization { get; set; }

        [Required]
        public int ContactId { get; set; }
        
        [ForeignKey(nameof(ContactId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual StakeholderContact Contact { get; set; }

        /// <summary>
        /// سمت این عضو در واحد
        /// </summary>
        public int? PositionId { get; set; }
        
        [ForeignKey(nameof(PositionId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual StakeholderOrganizationPosition? Position { get; set; }

        /// <summary>
        /// آیا این عضو نقش ناظر دارد؟
        /// </summary>
        public bool IsSupervisor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LeaveDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers Creator { get; set; }

        public bool IsActive { get; set; } = true;
    }
}