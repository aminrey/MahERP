using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class Stakeholder
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کوچک طرف حساب را وارد کنید")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی طرف حساب را وارد کنید")]
        public string LastName { get; set; }

        public string? CompanyName { get; set; }

        public string? Phone { get; set; }
        
        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? PostalCode { get; set; }

        public string? NationalCode { get; set; }
        
        public string? Description { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// نوع طرف حساب
        /// 0- مشتری
        /// 1- تامین کننده
        /// 2- همکار
        /// 3- سایر
        /// </summary>
        public byte StakeholderType { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// حذف منطقی
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<StakeholderBranch> StakeholderBranches { get; set; } = new HashSet<StakeholderBranch>();
        public virtual ICollection<Contract> Contracts { get; set; } = new HashSet<Contract>();
        public virtual ICollection<Tasks> TaskList { get; set; } = new HashSet<Tasks>();
        public virtual ICollection<StakeholderContact> StakeholderContacts { get; set; } = new HashSet<StakeholderContact>();
        public DateTime LastUpdateDate { get; set; }
    }
}
