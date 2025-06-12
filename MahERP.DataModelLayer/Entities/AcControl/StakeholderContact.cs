using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// افراد مرتبط با طرف حساب (مانند: مدیر، کارشناس، کارمند)
    /// </summary>
    public class StakeholderContact
    {
        [Key]
        public int Id { get; set; }

        public int StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Position { get; set; }

        public string Phone { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// اولویت تماس
        /// 0- اصلی
        /// 1- ثانویه
        /// </summary>
        public byte ContactPriority { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreateDate { get; set; }
    }
}
