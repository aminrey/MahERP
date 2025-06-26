using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class RolePatternDetails
    {
        [Key]
        public int Id { get; set; }

        public int RolePatternId { get; set; }
        [ForeignKey("RolePatternId")]
        public virtual RolePattern RolePattern { get; set; }

        [Required]
        [MaxLength(50)]
        public string ControllerName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionName { get; set; }

        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        /// <summary>
        /// دسترسی به عملیات ویژه (مثل تایید، رد کردن)
        /// </summary>
        public bool CanApprove { get; set; }

        /// <summary>
        /// محدودیت سطح داده - دسترسی به داده‌های خود/شعبه/همه
        /// 0- فقط داده‌های خود
        /// 1- داده‌های شعبه
        /// 2- همه داده‌ها
        /// </summary>
        public byte DataAccessLevel { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
