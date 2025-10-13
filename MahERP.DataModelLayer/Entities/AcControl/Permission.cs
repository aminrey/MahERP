using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// دسترسی‌ها - ساختار درختی
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NameEn { get; set; } // نام انگلیسی (یکتا)

        [Required]
        [StringLength(100)]
        public string NameFa { get; set; } // نام فارسی

        [StringLength(500)]
        public string? Description { get; set; } // توضیحات

        [StringLength(50)]
        public string Code { get; set; } // کد یکتا (مثل: TASK.CREATE)

        [StringLength(50)]
        public string? Icon { get; set; } // آیکون

        [StringLength(50)]
        public string? Color { get; set; } // رنگ

        public int? ParentId { get; set; } // والد - برای ساختار درختی
        public Permission? Parent { get; set; }

        public int DisplayOrder { get; set; } // ترتیب نمایش

        public bool IsActive { get; set; } = true;
        public bool IsSystemPermission { get; set; } // دسترسی سیستمی که قابل حذف نیست

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? CreatorUserId { get; set; }
        public AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterUserId { get; set; }
        public AppUsers? LastUpdater { get; set; }

        // روابط
        public ICollection<Permission> Children { get; set; } = new List<Permission>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}