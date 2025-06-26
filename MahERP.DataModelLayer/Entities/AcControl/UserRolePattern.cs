using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class UserRolePattern
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        public int RolePatternId { get; set; }
        [ForeignKey("RolePatternId")]
        public virtual RolePattern RolePattern { get; set; }

        public DateTime AssignDate { get; set; }

        public string AssignedByUserId { get; set; }
        [ForeignKey("AssignedByUserId")]
        public virtual AppUsers AssignedByUser { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }
    }
}