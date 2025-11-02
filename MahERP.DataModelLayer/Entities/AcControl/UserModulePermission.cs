
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// دسترسی کاربر به ماژول‌های سیستم (اولویت بالا - Override)
    /// </summary>
    public class UserModulePermission
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUsers? User { get; set; }

        /// <summary>
        /// نوع ماژول (0=Core, 1=Tasking, 2=CRM)
        /// </summary>
        [Required]
        public byte ModuleType { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// تاریخ اعطای دسترسی
        /// </summary>
        public DateTime GrantedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربری که دسترسی را اعطا کرده
        /// </summary>
        public string GrantedByUserId { get; set; }

        [ForeignKey(nameof(GrantedByUserId))]
        public virtual AppUsers? GrantedByUser { get; set; }

        /// <summary>
        /// توضیحات (اختیاری)
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}