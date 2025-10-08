using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// چارت سازمانی طرف حساب (برای اشخاص حقوقی)
    /// مشابه Team برای شعبات ما
    /// </summary>
    public class StakeholderOrganization
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// طرف حساب مرتبط (باید شخص حقوقی باشد)
        /// </summary>
        public int StakeholderId { get; set; }
        
        [ForeignKey(nameof(StakeholderId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Stakeholder? Stakeholder { get; set; }

        /// <summary>
        /// عنوان واحد سازمانی
        /// </summary>

        public string? Title { get; set; }

        /// <summary>
        /// توضیحات واحد
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// واحد سازمانی والد
        /// </summary>
        public int? ParentOrganizationId { get; set; }
        
        [ForeignKey(nameof(ParentOrganizationId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual StakeholderOrganization? ParentOrganization { get; set; }

        /// <summary>
        /// مدیر این واحد (از بین StakeholderContact‌ها)
        /// </summary>
        public int? ManagerContactId { get; set; }
        
        [ForeignKey(nameof(ManagerContactId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual StakeholderContact? ManagerContact { get; set; }

        /// <summary>
        /// سطح در سلسله مراتب
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        public DateTime CreateDate { get; set; }

        [MaxLength(450)]
        public string? CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }
        
        [ForeignKey(nameof(LastUpdaterUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers? LastUpdater { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [InverseProperty(nameof(StakeholderOrganization.ParentOrganization))]
        public virtual ICollection<StakeholderOrganization>? ChildOrganizations { get; set; }

        [InverseProperty(nameof(StakeholderOrganizationMember.Organization))]
        public virtual ICollection<StakeholderOrganizationMember>? Members { get; set; }

        [InverseProperty(nameof(StakeholderOrganizationPosition.Organization))]
        public virtual ICollection<StakeholderOrganizationPosition>? Positions { get; set; }

        /// <summary>
        /// دریافت سمت پیش‌فرض واحد
        /// </summary>
        [NotMapped]
        public StakeholderOrganizationPosition? DefaultPosition => 
            Positions?.FirstOrDefault(p => p.IsDefault && p.IsActive);

        /// <summary>
        /// دریافت سمت‌ها به ترتیب سطح قدرت
        /// </summary>
        [NotMapped]
        public IEnumerable<StakeholderOrganizationPosition> PositionsByPowerLevel => 
            Positions?.Where(p => p.IsActive).OrderBy(p => p.PowerLevel) ?? Enumerable.Empty<StakeholderOrganizationPosition>();
    }
}