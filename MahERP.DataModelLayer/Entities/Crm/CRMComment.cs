using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// کامنت‌های ثبت شده برای تعاملات CRM
    /// </summary>
    public class CRMComment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// تعامل CRM مرتبط
        /// </summary>
        public int CRMInteractionId { get; set; }
        [ForeignKey("CRMInteractionId")]
        public virtual CRMInteraction CRMInteraction { get; set; }

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
        public virtual CRMComment ParentComment { get; set; }

        /// <summary>
        /// پاسخ‌های این کامنت
        /// </summary>
        public virtual ICollection<CRMComment> Replies { get; set; }
    }
}
