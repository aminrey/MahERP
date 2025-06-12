using MahERP.DataModelLayer.Entities.AcControl;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// تخصیص کاربران به زمان‌بندی تسک
    /// </summary>
    public class TaskScheduleAssignment
    {
        [Key]
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual TaskSchedule Schedule { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// 0- اصلی (اجراکننده)
        /// 1- رونوشت
        /// 2- ناظر
        /// </summary>
        public byte AssignmentType { get; set; }

        /// <summary>
        /// توضیحات رونوشت (برای حالت رونوشت)
        /// </summary>
        public string CopyDescription { get; set; }

        /// <summary>
        /// شناسه توضیح رونوشت پیش‌فرض (در صورت استفاده از توضیحات آماده)
        /// </summary>
        public int? PredefinedCopyDescriptionId { get; set; }
        [ForeignKey("PredefinedCopyDescriptionId")]
        public virtual PredefinedCopyDescription PredefinedCopyDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
    }
}
