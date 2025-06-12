using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class BranchUser
    {
        [Key]
        public int Id { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// 0- کارشناس
        /// 1- ناظر
        /// 2- مدیر
        /// </summary>
        public byte Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AssignDate { get; set; }

        public string AssignedByUserId { get; set; }
        [ForeignKey("AssignedByUserId")]
        public virtual AppUsers AssignedByUser { get; set; }
    }
}
