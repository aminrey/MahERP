using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// دسترسی‌های مستقیم کاربر - برای ویرایش دستی
    /// </summary>
    public class UserPermission
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUsers? User { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

        /// <summary>
        /// منبع دسترسی:
        /// 1 = از طریق نقش (Role)
        /// 2 = دستی (Manual)
        /// 3 = ترکیبی (Combined)
        /// </summary>
        public byte SourceType { get; set; } = 1;

        public int? SourceRoleId { get; set; } // اگر از نقش باشد
        public Role? SourceRole { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsManuallyModified { get; set; } // آیا دستی تغییر کرده؟

        public DateTime AssignDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        public string? AssignedByUserId { get; set; }
        public AppUsers? AssignedByUser { get; set; }

        public string? ModifiedByUserId { get; set; }
        public AppUsers? ModifiedByUser { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}