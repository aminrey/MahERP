using MahERP.DataModelLayer.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class RolePattern
    {
        public RolePattern()
        {
            RolePatternDetails = new HashSet<RolePatternDetails>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الگوی نقش الزامی است")]
        [MaxLength(100)]
        public string PatternName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح دسترسی کلی الگو
        /// 1- مدیر سیستم
        /// 2- مدیر
        /// 3- سرپرست
        /// 4- کارشناس
        /// 5- کاربر عادی
        /// </summary>
        public byte AccessLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemPattern { get; set; } = false;

        public DateTime CreateDate { get; set; }

        public string? CreatorUserId { get; set; } // اضافه کردن ? برای nullable
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public string? LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        // Navigation properties
        public virtual ICollection<RolePatternDetails> RolePatternDetails { get; set; }
        public virtual ICollection<UserRolePattern> UserRolePatterns { get; set; } = new HashSet<UserRolePattern>();
    }
}
