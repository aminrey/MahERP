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
            MentionedUsers = new HashSet<TaskCommentMention>();
            Notifications = new HashSet<TaskNotification>();
            Attachments = new HashSet<TaskCommentAttachment>();
        }

        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        [Required(ErrorMessage = "متن کامنت الزامی است")]
        public string CommentText { get; set; }

        /// <summary>
        /// آیا این کامنت خصوصی است؟ (فقط برای سازنده قابل مشاهده)
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// آیا این کامنت مهم است؟
        /// </summary>
        public bool IsImportant { get; set; }

        /// <summary>
        /// نوع کامنت
        /// 0- کامنت عادی
        /// 1- بازخورد
        /// 2- سوال
        /// 3- درخواست تغییر
        /// 4- تأیید
        /// 5- رد
        /// </summary>
        public byte CommentType { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// کامنت والد (برای پاسخ به کامنت‌ها)
        /// </summary>
        public int? ParentCommentId { get; set; }
        [ForeignKey("ParentCommentId")]
        public virtual TaskComment? ParentComment { get; set; }

        /// <summary>
        /// تاریخ ویرایش کامنت
        /// </summary>
        public DateTime? EditDate { get; set; }

        /// <summary>
        /// آیا کامنت ویرایش شده است؟
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// کاربران منشن شده در کامنت
        /// </summary>
        public virtual ICollection<TaskCommentMention> MentionedUsers { get; set; }

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
        public string? MentionedUserId { get; set; }
        [ForeignKey("MentionedUserId")]
        public virtual AppUsers? MentionedUser { get; set; }

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
