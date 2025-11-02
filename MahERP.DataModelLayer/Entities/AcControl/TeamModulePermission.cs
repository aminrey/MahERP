using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.Core;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// دسترسی تیم به ماژول‌های سیستم (اولویت متوسط)
    /// </summary>
    public class TeamModulePermission
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تیم
        /// </summary>
        [Required]
        public int TeamId { get; set; }
        
        [ForeignKey(nameof(TeamId))]
        public virtual Team? Team { get; set; }

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