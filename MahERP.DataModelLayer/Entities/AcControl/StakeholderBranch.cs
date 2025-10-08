using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// ارتباط طرف حساب با شعبه
    /// برای تعیین اینکه کدام طرف حساب‌ها به کدام شعبات اختصاص یافته‌اند
    /// </summary>
    public class StakeholderBranch
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه طرف حساب
        /// </summary>
        [Required]
        public int StakeholderId { get; set; }
        
        [ForeignKey(nameof(StakeholderId))]
        public virtual Stakeholder? Stakeholder { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        [Required]
        public int BranchId { get; set; }
        
        [ForeignKey(nameof(BranchId))]
        public virtual Branch? Branch { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ اختصاص طرف حساب به شعبه
        /// </summary>
        [Required]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که این اختصاص را انجام داده
        /// </summary>
        [MaxLength(450)]
        public string? AssignedByUserId { get; set; }
        
        [ForeignKey(nameof(AssignedByUserId))]
        public virtual AppUsers? AssignedBy { get; set; }

        /// <summary>
        /// شناسه کاربر ایجادکننده (برای سازگاری با سیستم)
        /// </summary>
        [MaxLength(450)]
        public string? CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
