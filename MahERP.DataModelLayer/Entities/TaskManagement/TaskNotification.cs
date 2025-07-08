using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskNotification
    {
        [Key]
        public int Id { get; set; }

        public int? TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks? Task { get; set; }

        public int? CommentId { get; set; }
        [ForeignKey("CommentId")]
        public virtual TaskComment? Comment { get; set; }

        public int? OperationId { get; set; }
        [ForeignKey("OperationId")]
        public virtual TaskOperation? Operation { get; set; }

        public string? RecipientUserId { get; set; }
        [ForeignKey("RecipientUserId")]
        public virtual AppUsers? Recipient { get; set; }

        /// <summary>
        /// 0- تسک جدید
        /// 1- تغییر در تسک
        /// 2- کامنت جدید
        /// 3- تکمیل تسک
        /// 4- تاخیر در تسک
        /// 5- عملیات جدید
        /// 6- تکمیل عملیات
        /// 7- منشن در کامنت
        /// 8- تایید یا رد تسک
        /// </summary>
        public byte NotificationType { get; set; }

        public string? Title { get; set; }
        
        public string? Message { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
        
        /// <summary>
        /// 0- سیستمی
        /// 1- ایمیل
        /// 2- پیامک
        /// 3- تلگرام
        /// </summary>
        public byte DeliveryType { get; set; }
        
        public bool IsDelivered { get; set; }
        
        public DateTime? DeliveryDate { get; set; }
    }
}
