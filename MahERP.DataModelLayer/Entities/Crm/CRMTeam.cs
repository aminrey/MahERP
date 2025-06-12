using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organization;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// ارتباط بین تعاملات CRM و تیم‌ها - تعیین تیم‌های مسئول یا مرتبط با هر تعامل CRM
    /// </summary>
    public class CRMTeam
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
        /// تیم مرتبط
        /// </summary>
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        /// <summary>
        /// نوع دسترسی تیم
        /// 0- مسئول اصلی - تیم اصلی رسیدگی کننده
        /// 1- همکار - تیم همکار در رسیدگی
        /// 2- مطلع - تیم فقط مطلع از موضوع
        /// </summary>
        public byte AccessType { get; set; }

        /// <summary>
        /// توضیحات نقش تیم در این تعامل CRM
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// تاریخ ایجاد ارتباط
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده ارتباط
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
    }
}
