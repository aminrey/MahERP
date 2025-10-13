using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Entities
{
    public class Settings
    {
        [Key]
        public int id { get; set; }

        /// <summary>
        /// SMTP Server Host
        /// </summary>
        [MaxLength(200)]
        public string? SmtpHost { get; set; }

        /// <summary>
        /// SMTP Port
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// استفاده از SSL
        /// </summary>
        public bool SmtpEnableSsl { get; set; } = true;

        /// <summary>
        /// SMTP Username
        /// </summary>
        [MaxLength(200)]
        public string? SmtpUsername { get; set; }

        /// <summary>
        /// SMTP Password
        /// </summary>
        [MaxLength(500)]
        public string? SmtpPassword { get; set; }

        /// <summary>
        /// ایمیل فرستنده
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? SmtpFromEmail { get; set; }

        /// <summary>
        /// نام فرستنده
        /// </summary>
        [MaxLength(200)]
        public string? SmtpFromName { get; set; }

        /// <summary>
        /// حداکثر حجم پیوست (MB)
        /// </summary>
        public int MaxAttachmentSizeMB { get; set; } = 25;
    }
}