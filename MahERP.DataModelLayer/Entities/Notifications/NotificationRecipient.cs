using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// لیست دریافت‌کنندگان یا محروم‌شدگان هر نوع اعلان
    /// </summary>
    [Table("NotificationRecipient_Tbl")]
    public class NotificationRecipient
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه نوع اعلان
        /// </summary>
        [Required]
        public int NotificationTypeConfigId { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// دلیل افزودن/حذف (اختیاری)
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// ایجاد شده توسط
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        // Navigation Properties
        public virtual NotificationTypeConfig? NotificationTypeConfig { get; set; }
        public virtual AppUsers? User { get; set; }
        public virtual AppUsers? CreatedBy { get; set; }
    }
}