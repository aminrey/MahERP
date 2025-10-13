using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// دسترسی‌های نقش - جدول رابطه نقش و دسترسی
    /// </summary>
    public class RolePermission
    {
        public int Id { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AssignDate { get; set; } = DateTime.Now;
        public string? AssignedByUserId { get; set; }
        public AppUsers? AssignedByUser { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}