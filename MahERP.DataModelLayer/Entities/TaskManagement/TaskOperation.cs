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

        /// <summary>
        /// عنوان عملیات
        /// </summary>
        [Required(ErrorMessage = "عنوان عملیات الزامی است")]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات عملیات
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// ترتیب عملیات
        /// </summary>
        public int OperationOrder { get; set; }

        /// <summary>
        /// آیا عملیات تکمیل شده است؟
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// تاریخ تکمیل عملیات
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// کاربری که عملیات را تکمیل کرده
        /// </summary>
        public string? CompletedByUserId { get; set; }
        [ForeignKey("CompletedByUserId")]
        public virtual AppUsers? CompletedByUser { get; set; }

        /// <summary>
        /// یادداشت تکمیل
        /// </summary>
        public string? CompletionNote { get; set; }

        /// <summary>
        /// مدت زمان تخمینی انجام (به ساعت)
        /// </summary>
        public decimal? EstimatedHours { get; set; }

        /// <summary>
        /// مدت زمان واقعی انجام (به ساعت)
        /// </summary>
        public decimal? ActualHours { get; set; }

        /// <summary>
        /// آیا این عملیات الزامی است؟
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
    }
}
