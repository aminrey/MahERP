using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// یادآوری برای تسک‌ها
    /// </summary>
    public class TaskReminder
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        public DateTime ReminderDate { get; set; }

        /// <summary>
        /// 0- یکبار
        /// 1- روزانه
        /// 2- هفتگی
        /// 3- ماهانه
        /// </summary>
        public byte RepeatType { get; set; }

        public string Message { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSent { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
