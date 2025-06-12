using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// کاربران مجاز به مشاهده تسک‌های ایجاد شده در زمان‌بندی
    /// </summary>
    public class TaskScheduleViewer
    {
        [Key]
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual TaskSchedule Schedule { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        public string AddedByUserId { get; set; }
        [ForeignKey("AddedByUserId")]
        public virtual AppUsers AddedByUser { get; set; }

        public DateTime AddedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
