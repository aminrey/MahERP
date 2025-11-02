using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// تنظیمات کاربر برای ماژول‌ها (ذخیره آخرین ماژول استفاده شده)
    /// </summary>
    public class UserModulePreference
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه کاربر (یکتا)
        /// </summary>
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual AppUsers? User { get; set; }

        /// <summary>
        /// آخرین ماژول استفاده شده (0=Core, 1=Tasking, 2=CRM)
        /// </summary>
        [Required]
        public byte LastUsedModule { get; set; }

        /// <summary>
        /// تاریخ آخرین دسترسی
        /// </summary>
        public DateTime LastAccessDate { get; set; } = DateTime.Now;

        /// <summary>
        /// ماژول پیش‌فرض (اختیاری - برای override کردن لاجیک خودکار)
        /// </summary>
        public byte? DefaultModule { get; set; }
    }
}