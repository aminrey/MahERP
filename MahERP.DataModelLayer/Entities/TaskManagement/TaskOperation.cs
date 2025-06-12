using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// ریز عملیات‌های تسک
    /// </summary>
    public class TaskOperation
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        [Required(ErrorMessage = "عنوان عملیات را وارد کنید")]
        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// ترتیب انجام عملیات
        /// </summary>
        public int OperationOrder { get; set; }

        /// <summary>
        /// وضعیت تکمیل
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// تاریخ تکمیل
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// کاربر تکمیل کننده
        /// </summary>
        public string CompletedByUserId { get; set; }
        [ForeignKey("CompletedByUserId")]
        public virtual AppUsers CompletedByUser { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
