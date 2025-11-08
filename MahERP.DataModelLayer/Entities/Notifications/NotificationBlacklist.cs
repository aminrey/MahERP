using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// لیست سیاه - کاربرانی که نباید اعلان خاصی دریافت کنند
    /// </summary>
    public class NotificationBlacklist
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// نوع اعلان که باید block شود (null = همه اعلان‌ها)
        /// </summary>
        public int? NotificationTypeConfigId { get; set; }
        [ForeignKey("NotificationTypeConfigId")]
        public virtual NotificationTypeConfig? NotificationTypeConfig { get; set; }

        /// <summary>
        /// دلیل افزودن به لیست سیاه
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual AppUsers? CreatedBy { get; set; }
    }
}