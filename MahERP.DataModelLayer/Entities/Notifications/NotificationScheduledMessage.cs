using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// پیام‌های زمان‌بندی شده و گروهی (اعلانیه/خبر)
    /// </summary>
    public class NotificationScheduledMessage
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public string? BodyHtml { get; set; }

        /// <summary>
        /// نوع پیام: 0=اعلانیه، 1=خبر، 2=پیام فوری
        /// </summary>
        public byte MessageType { get; set; } = 0;

        /// <summary>
        /// اولویت: 0=عادی، 1=مهم، 2=فوری
        /// </summary>
        public byte Priority { get; set; } = 0;

        // کانال‌های ارسال
        public bool SendBySystem { get; set; } = true;
        public bool SendByEmail { get; set; } = false;
        public bool SendBySms { get; set; } = false;
        public bool SendByTelegram { get; set; } = false;

        /// <summary>
        /// دریافت‌کنندگان (JSON)
        /// </summary>
        public string RecipientUserIds { get; set; } // JSON: ["user1", "user2"]

        /// <summary>
        /// شعبه‌های هدف (JSON)
        /// </summary>
        public string? TargetBranchIds { get; set; }

        /// <summary>
        /// نقش‌های هدف (JSON)
        /// </summary>
        public string? TargetRoles { get; set; }

        /// <summary>
        /// زمان‌بندی ارسال
        /// </summary>
        public DateTime? ScheduledDateTime { get; set; }

        /// <summary>
        /// وضعیت: 0=پیش‌نویس، 1=زمان‌بندی شده، 2=ارسال شده، 3=لغو شده
        /// </summary>
        public byte Status { get; set; } = 0;

        public int RecipientCount { get; set; } = 0;
        public int SentCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? SentDate { get; set; }
    }
}