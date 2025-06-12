using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// جدول ارتباطی بین فعالیت پایه و تسک - این جدول ارتباط بین ماژول تسکینگ و فعالیت مرکزی را برقرار می‌کند
    /// </summary>
    public class ActivityTask
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
        /// شناسه تسک مرتبط
        /// </summary>
        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        /// <summary>
        /// نوع ارتباط بین فعالیت و تسک
        /// 0- فعالیت از تسک ایجاد شده (تسک اصل است)
        /// 1- تسک از فعالیت ایجاد شده (فعالیت اصل است)
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
