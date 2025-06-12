using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// جدول ارتباطی بین فعالیت پایه و CRM - این جدول ارتباط بین ماژول CRM و فعالیت مرکزی را برقرار می‌کند
    /// </summary>
    public class ActivityCRM
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
        /// شناسه تعامل CRM مرتبط
        /// </summary>
        public int CRMId { get; set; }
        [ForeignKey("CRMId")]
        public virtual CRMInteraction CRMInteraction { get; set; }

        /// <summary>
        /// نوع ارتباط بین فعالیت و CRM
        /// 0- فعالیت از CRM ایجاد شده (CRM اصل است)
        /// 1- CRM از فعالیت ایجاد شده (فعالیت اصل است)
        /// </summary>
        public byte RelationType { get; set; }

        /// <summary>
        /// توضیحات تکمیلی درباره ارتباط
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

        /// <summary>
        /// وضعیت فعال بودن ارتباط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
