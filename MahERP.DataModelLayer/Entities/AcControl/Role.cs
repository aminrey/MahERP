using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// نقش‌ها - ساده‌تر از RolePattern
    /// </summary>
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? NameEn { get; set; }

        [Required]
        [StringLength(100)]
        public string NameFa { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int Priority { get; set; } // اولویت نقش (1 = بالاترین)

        public bool IsActive { get; set; } = true;
        public bool IsSystemRole { get; set; } // نقش سیستمی

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? CreatorUserId { get; set; }
        public AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterUserId { get; set; }
        public AppUsers? LastUpdater { get; set; }

        // روابط
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}