using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// لاگ تغییرات دسترسی‌های کاربر
    /// </summary>
    public class PermissionChangeLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUsers? User { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

        /// <summary>
        /// نوع تغییر:
        /// 1 = اضافه شدن از نقش
        /// 2 = حذف شدن
        /// 3 = تغییر دستی
        /// 4 = بازگشت به حالت نقش
        /// </summary>
        public byte ChangeType { get; set; }

        [StringLength(100)]
        public string? ChangeDescription { get; set; }

        public int? OldSourceRoleId { get; set; }
        public int? NewSourceRoleId { get; set; }

        public bool OldIsActive { get; set; }
        public bool NewIsActive { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public string ChangedByUserId { get; set; }
        public AppUsers? ChangedByUser { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}