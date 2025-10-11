using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// بخش‌های سازمانی (چارت سازمانی)
    /// </summary>
    public class OrganizationDepartment
    {
        public OrganizationDepartment()
        {
            Members = new HashSet<DepartmentMember>();
            Positions = new HashSet<DepartmentPosition>();
            ChildDepartments = new HashSet<OrganizationDepartment>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه سازمان
        /// </summary>
        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// بخش والد (برای چارت سلسله مراتبی)
        /// </summary>
        public int? ParentDepartmentId { get; set; }

        [ForeignKey(nameof(ParentDepartmentId))]
        public virtual OrganizationDepartment? ParentDepartment { get; set; }

        /// <summary>
        /// عنوان بخش
        /// </summary>
        [Required(ErrorMessage = "عنوان بخش الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// کد بخش (برای شناسایی سریع)
        /// </summary>
        [MaxLength(50)]
        public string? Code { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح در سلسله مراتب (0 = ریشه)
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// مدیر بخش
        /// </summary>
        public int? ManagerContactId { get; set; }

        [ForeignKey(nameof(ManagerContactId))]
        public virtual Contact? ManagerContact { get; set; }

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

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Navigation Properties ==========
        
        /// <summary>
        /// زیربخش‌ها
        /// </summary>
        [InverseProperty(nameof(OrganizationDepartment.ParentDepartment))]
        public virtual ICollection<OrganizationDepartment> ChildDepartments { get; set; } = new HashSet<OrganizationDepartment>();

        /// <summary>
        /// سمت‌های تعریف شده در این بخش
        /// </summary>
        [InverseProperty(nameof(DepartmentPosition.Department))]
        public virtual ICollection<DepartmentPosition> Positions { get; set; } = new HashSet<DepartmentPosition>();

        /// <summary>
        /// اعضای این بخش
        /// </summary>
        [InverseProperty(nameof(DepartmentMember.Department))]
        public virtual ICollection<DepartmentMember> Members { get; set; } = new HashSet<DepartmentMember>();

        // ========== Computed Properties ==========

        [NotMapped]
        public string FullPath
        {
            get
            {
                var path = Title;
                var parent = ParentDepartment;
                while (parent != null)
                {
                    path = $"{parent.Title} / {path}";
                    parent = parent.ParentDepartment;
                }
                return path;
            }
        }

        [NotMapped]
        public int ActiveMembersCount => Members?.Count(m => m.IsActive) ?? 0;

        [NotMapped]
        public int ActivePositionsCount => Positions?.Count(p => p.IsActive) ?? 0;

        /// ✅ اضافه شده - نام مدیر بخش
        [NotMapped]
        public string? ManagerName => ManagerContact != null
            ? $"{ManagerContact.FirstName} {ManagerContact.LastName}"
            : null;

        [NotMapped]
        public IEnumerable<OrganizationDepartment> ActiveChildDepartments =>
            ChildDepartments?.Where(d => d.IsActive).OrderBy(d => d.DisplayOrder)
            ?? Enumerable.Empty<OrganizationDepartment>();
    }
}