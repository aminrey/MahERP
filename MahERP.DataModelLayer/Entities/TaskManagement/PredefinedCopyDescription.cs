using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// متن‌های آماده برای توضیحات رونوشت
    /// </summary>
    public class PredefinedCopyDescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; }
    }
}
