using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class Contract
    {
        public Contract()
        {
            // Initialize collections
            TaskList = new HashSet<Tasks>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان قرارداد الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "شماره قرارداد الزامی است")]
        [MaxLength(50)]
        public string ContractNumber { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        [Required(ErrorMessage = "تاریخ شروع قرارداد الزامی است")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ContractValue { get; set; }

        /// <summary>
        /// 0- پیش‌نویس
        /// 1- فعال
        /// 2- تمام شده
        /// 3- لغو شده
        /// </summary>
        public byte Status { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public string LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers LastUpdater { get; set; }

        // Navigation properties
        public virtual ICollection<Tasks> TaskList { get; set; }
    }
}
