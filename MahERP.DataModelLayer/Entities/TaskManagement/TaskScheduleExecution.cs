using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// سوابق اجرای زمان‌بندی تسک
    /// </summary>
    public class TaskScheduleExecution
    {
        [Key]
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual TaskSchedule Schedule { get; set; }

        /// <summary>
        /// شناسه تسک ایجاد شده
        /// </summary>
        public int? CreatedTaskId { get; set; }
        [ForeignKey("CreatedTaskId")]
        public virtual Tasks CreatedTask { get; set; }

        /// <summary>
        /// زمان اجرا
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// وضعیت اجرا
        /// 0- در حال اجرا
        /// 1- موفق
        /// 2- ناموفق
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// پیام خطا (در صورت وجود)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// مدت زمان اجرا (ثانیه)
        /// </summary>
        public double ExecutionDuration { get; set; }
    }
}
