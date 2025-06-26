using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان قرارداد الزامی است")]
        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ContractValue { get; set; }

        /// <summary>
        /// وضعیت قرارداد
        /// 0- پیش نویس
        /// 1- فعال
        /// 2- تکمیل شده
        /// 3- لغو شده
        /// 4- منقضی شده
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// نوع قرارداد
        /// 0- فروش
        /// 1- خرید
        /// 2- خدمات
        /// 3- اجاره
        /// 4- سایر
        /// </summary>
        public byte ContractType { get; set; }

        public string? ContractNumber { get; set; }

        public string? Terms { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        // Foreign Keys
        public int StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        public string? LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        // Navigation properties
        public virtual ICollection<Tasks> TaskList { get; set; } = new HashSet<Tasks>();
    }
}
