using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// کامنت‌های ثبت شده برای فعالیت‌ها
    /// </summary>
    public class ActivityComment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه فعالیت مرتبط
        /// </summary>
        public int ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual ActivityBase Activity { get; set; }

        /// <summary>
        /// متن کامنت
        /// </summary>
        [Required]
        public string CommentText { get; set; }

        /// <summary>
        /// تاریخ ایجاد کامنت
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده کامنت
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// شناسه کامنت والد (برای پاسخ به کامنت‌ها)
        /// </summary>
        public int? ParentCommentId { get; set; }
        [ForeignKey("ParentCommentId")]
        public virtual ActivityComment ParentComment { get; set; }

        /// <summary>
        /// پاسخ‌های این کامنت
        /// </summary>
        public virtual ICollection<ActivityComment> Replies { get; set; }
    }
}
