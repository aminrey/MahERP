using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// تاریخچه تغییرات تسک
    /// </summary>
    public class TaskHistory
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// 0- ایجاد تسک
        /// 1- ویرایش تسک
        /// 2- تغییر وضعیت
        /// 3- اضافه کردن کاربر
        /// 4- حذف کاربر
        /// 5- اضافه کردن عملیات
        /// 6- تکمیل عملیات
        /// 7- افزودن پیوست
        /// 8- حذف پیوست
        /// </summary>
        public byte ActionType { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// اطلاعات قبلی (در صورت ویرایش)
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// اطلاعات جدید
        /// </summary>
        public string NewValue { get; set; }

        public DateTime ActionDate { get; set; }
    }
}
