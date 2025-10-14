using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// نقش‌های کاربر - تخصیص نقش به کاربر
    /// </summary>
    public class UserRole
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUsers? User { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AssignDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; } // تاریخ شروع اعتبار
        public DateTime? EndDate { get; set; } // تاریخ پایان اعتبار
        public DateTime? LastUpdateDate { get; set; } 
        public string? LastUpdaterUserId { get; set; }

        public string? AssignedByUserId { get; set; }
        public AppUsers? AssignedByUser { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}