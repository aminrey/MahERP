using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// اعضای تیمی که به تسک دسترسی دارند (برای تسک‌های تیمی)
    /// </summary>
    public class TaskTeamMember
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        /// <summary>
        /// آیا تمام اعضای تیم به تسک دسترسی دارند
        /// </summary>
        public bool IncludeAllMembers { get; set; } = true;

        /// <summary>
        /// شناسه کاربر خاص (اگر IncludeAllMembers برابر با false باشد)
        /// </summary>
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
    }
}

