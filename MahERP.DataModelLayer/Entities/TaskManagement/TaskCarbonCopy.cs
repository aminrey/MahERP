using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// رونوشت دستی تسک - ناظران اضافه شده توسط کاربر
    /// </summary>
    [Table("TaskCarbonCopy_Tbl")]
    public class TaskCarbonCopy
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تسک
        /// </summary>
        [Required]
        public int TaskId { get; set; }

        /// <summary>
        /// کاربر رونوشت شده (ناظر دستی)
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        /// <summary>
        /// کاربری که رونوشت را اضافه کرده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AddedByUserId { get; set; }

        /// <summary>
        /// تاریخ افزودن رونوشت
        /// </summary>
        [Required]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// یادداشت/دلیل رونوشت
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        // ⭐ Navigation Properties
        [ForeignKey(nameof(TaskId))]
        public virtual Tasks Task { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUsers User { get; set; }

        [ForeignKey(nameof(AddedByUserId))]
        public virtual AppUsers AddedByUser { get; set; }
    }
}
