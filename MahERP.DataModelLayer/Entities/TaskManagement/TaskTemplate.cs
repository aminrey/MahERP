using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// قالب‌های آماده تسک
    /// </summary>
    public class TaskTemplate
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual TaskCategory? Category { get; set; }

        public byte TaskType { get; set; }

        public byte Priority { get; set; }

        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TaskTemplateOperation> Operations { get; set; } = new HashSet<TaskTemplateOperation>();

        public virtual ICollection<TaskSchedule> TaskSchedule { get; set; } = new HashSet<TaskSchedule>();

        /// <summary>
        /// استفاده در زمان‌بندی
        /// آیا در زمان ایجاد تسک زمان‌بندی شده، مدیر شعبه به عنوان ناظر اضافه شود
        /// </summary>
        public bool AddBranchManagerAsSupervisor { get; set; }

        /// <summary>
        /// استفاده در زمان‌بندی
        /// مدت زمان پیش‌فرض برای انجام تسک (روز)
        /// </summary>
        public int DefaultDurationDays { get; set; }
    }
}
