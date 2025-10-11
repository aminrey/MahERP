using MahERP.DataModelLayer.Entities.AcControl;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// اعضای بخش‌های سازمانی
    /// </summary>

    [Index(nameof(DepartmentId), nameof(ContactId), IsUnique = true, Name = "IX_DepartmentMember_Department_Contact")]
    public class DepartmentMember
    {
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
        /// شناسه فرد
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// شناسه سمت
        /// </summary>
        [Required]
        public int PositionId { get; set; }

        [ForeignKey(nameof(PositionId))]
        public virtual DepartmentPosition? Position { get; set; }

        /// <summary>
        /// تاریخ پیوستن
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ ترک (در صورت خروج)
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? LeaveDate { get; set; }

        /// <summary>
        /// نوع استخدام
        /// 0 = تمام‌وقت
        /// 1 = پاره‌وقت
        /// 2 = قراردادی
        /// 3 = پروژه‌ای
        /// </summary>
        public byte EmploymentType { get; set; } = 0;

        /// <summary>
        /// آیا ناظر است؟
        /// </summary>
        public bool IsSupervisor { get; set; } = false;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ========== اطلاعات سیستمی ==========
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string EmploymentTypeText => EmploymentType switch
        {
            0 => "تمام‌وقت",
            1 => "پاره‌وقت",
            2 => "قراردادی",
            3 => "پروژه‌ای",
            _ => "نامشخص"
        };

        [NotMapped]
        public int ServiceDurationDays
        {
            get
            {
                var endDate = LeaveDate ?? DateTime.Now;
                return (endDate - JoinDate).Days;
            }
        }

        [NotMapped]
        public string ServiceDurationText
        {
            get
            {
                var days = ServiceDurationDays;
                if (days < 30) return $"{days} روز";
                if (days < 365) return $"{days / 30} ماه";
                return $"{days / 365} سال";
            }
        }
    }
}