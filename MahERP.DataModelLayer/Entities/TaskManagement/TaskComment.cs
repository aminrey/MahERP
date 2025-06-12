using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskComment
    {
        public TaskComment()
        {
            Notifications = new HashSet<TaskNotification>();
            Attachments = new HashSet<TaskCommentAttachment>();
            MentionedUsers = new HashSet<TaskCommentMention>();
        }

        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        [Required(ErrorMessage = "متن کامنت را وارد کنید")]
        public string Content { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// کاربران منشن شده در کامنت
        /// </summary>
        public virtual ICollection<TaskCommentMention> MentionedUsers { get; set; }

        /// <summary>
        /// آیا کامنت خصوصی است (فقط برای تیم داخلی قابل مشاهده)
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// کامنت‌های پاسخ داده شده
        /// </summary>
        public int? ParentCommentId { get; set; }
        [ForeignKey("ParentCommentId")]
        public virtual TaskComment ParentComment { get; set; }

        // Navigation properties
        public virtual ICollection<TaskNotification> Notifications { get; set; }
        public virtual ICollection<TaskCommentAttachment> Attachments { get; set; }
    }

    /// <summary>
    /// کلاس رابط برای ذخیره‌سازی کاربران منشن شده در کامنت‌ها
    /// </summary>
    public class TaskCommentMention
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه کامنت
        /// </summary>
        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        public virtual TaskComment Comment { get; set; }

        /// <summary>
        /// شناسه کاربر منشن شده
        /// </summary>
        public string MentionedUserId { get; set; }
        [ForeignKey("MentionedUserId")]
        public virtual AppUsers MentionedUser { get; set; }

        /// <summary>
        /// تاریخ منشن کردن
        /// </summary>
        public DateTime MentionDate { get; set; }

        /// <summary>
        /// وضعیت مشاهده توسط کاربر
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// تاریخ مشاهده توسط کاربر
        /// </summary>
        public DateTime? ReadDate { get; set; }
    }
}
