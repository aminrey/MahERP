using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class Branch
    {
        public Branch()
        {
            // Initialize collections
            BranchUsers = new HashSet<BranchUser>();
            TaskList = new HashSet<Tasks>();
            
            // ❌ DEPRECATED - فقط برای سازگاری با تسک‌های قدیمی
            Stakeholders = new HashSet<Stakeholder>();
            
            // ✅ NEW - سیستم جدید
            BranchContacts = new HashSet<BranchContact>();
            BranchOrganizations = new HashSet<BranchOrganization>();
            
            ChildBranches = new HashSet<Branch>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام شعبه الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? ManagerName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsMainBranch { get; set; }

        public int? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public virtual Branch? ParentBranch { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastUpdateDate { get; set; }
      
        /// <summary>
        /// آدرس مرکز پیام لاگ به همه اعضای یک شعبه
        /// </summary>
        public string? TelegramBotToken { get; set; }
        public string? TelegramBotTokenName { get; set; }

        // ========== Navigation Properties =========
        
        public virtual ICollection<BranchUser> BranchUsers { get; set; }
        public virtual ICollection<Tasks> TaskList { get; set; }
        public virtual ICollection<Branch> ChildBranches { get; set; }
        
        // ❌ DEPRECATED - نگه می‌داریم برای سازگاری با تسک‌های قدیمی
        [Obsolete("از BranchContacts و BranchOrganizations استفاده کنید")]
        public virtual ICollection<Stakeholder> Stakeholders { get; set; }
        
        // ✅ NEW - سیستم جدید
        /// <summary>
        /// افراد مرتبط با شعبه (تکی)
        /// </summary>
        [InverseProperty(nameof(BranchContact.Branch))]
        public virtual ICollection<BranchContact> BranchContacts { get; set; }
        
        /// <summary>
        /// سازمان‌های مرتبط با شعبه (گروهی)
        /// </summary>
        [InverseProperty(nameof(BranchOrganization.Branch))]
        public virtual ICollection<BranchOrganization> BranchOrganizations { get; set; }
    }
}
