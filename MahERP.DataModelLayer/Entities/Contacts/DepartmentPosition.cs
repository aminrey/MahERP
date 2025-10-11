using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// سمت‌های قابل تعریف در بخش‌های سازمانی
    /// </summary>
    public class DepartmentPosition
    {
        public DepartmentPosition()
        {
            Members = new HashSet<DepartmentMember>();
        }
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه بخش
        /// </summary>
        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual OrganizationDepartment Department { get; set; }

        /// <summary>
        /// عنوان سمت
        /// </summary>
        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// کد سمت
        /// </summary>
        [MaxLength(50)]
        public string? Code { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح قدرت (عدد کمتر = قدرت بیشتر)
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int PowerLevel { get; set; } = 50;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// حداقل حقوق (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinSalary { get; set; }

        /// <summary>
        /// حداکثر حقوق (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxSalary { get; set; }

        /// <summary>
        /// آیا این سمت پیش‌فرض است؟
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// آیا می‌تواند زیردست استخدام کند؟
        /// </summary>
        public bool CanHireSubordinates { get; set; } = false;

        /// <summary>
        /// آیا نیاز به تایید مدیر دارد؟
        /// </summary>
        public bool RequiresApproval { get; set; } = false;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== اطلاعات سیستمی ==========
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        // ========== Navigation Properties ==========
        
        /// <summary>
        /// اعضایی که این سمت را دارند
        /// </summary>
        [InverseProperty(nameof(DepartmentMember.Position))]
        public virtual ICollection<DepartmentMember> Members { get; set; } = new HashSet<DepartmentMember>();

        // ========== Computed Properties ==========
        
        [NotMapped]
        public int ActiveMembersCount => Members?.Count(m => m.IsActive) ?? 0;

        [NotMapped]
        public string SalaryRangeText
        {
            get
            {
                if (MinSalary.HasValue && MaxSalary.HasValue)
                    return $"{MinSalary:N0} - {MaxSalary:N0} ریال";
                if (MinSalary.HasValue)
                    return $"از {MinSalary:N0} ریال";
                if (MaxSalary.HasValue)
                    return $"تا {MaxSalary:N0} ریال";
                return "تعیین نشده";
            }
        }
    }
}