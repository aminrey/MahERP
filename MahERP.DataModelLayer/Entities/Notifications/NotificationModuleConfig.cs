using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// پیکربندی ماژول‌های اعلان (Tasking, CRM, HR, ...)
    /// </summary>
    public class NotificationModuleConfig
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد یکتای ماژول
        /// </summary>
        [Required, MaxLength(50)]
        public string ModuleCode { get; set; } // "TASKING", "CRM", "HR"

        /// <summary>
        /// نام فارسی ماژول
        /// </summary>
        [Required, MaxLength(100)]
        public string ModuleNameFa { get; set; }

        /// <summary>
        /// نام انگلیسی ماژول
        /// </summary>
        [MaxLength(100)]
        public string? ModuleNameEn { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// رنگ ماژول (Hex)
        /// </summary>
        [MaxLength(20)]
        public string? ColorCode { get; set; } // "#2196F3"

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        // Navigation
        public virtual ICollection<NotificationTypeConfig> NotificationTypes { get; set; }
            = new HashSet<NotificationTypeConfig>();
    }
}