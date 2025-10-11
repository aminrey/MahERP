using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// ارتباط افراد با سازمان (خارج از چارت سازمانی)
    /// برای مشتریان، تامین‌کنندگان، شرکا و...
    /// </summary>
    [Index(nameof(OrganizationId), nameof(ContactId), nameof(RelationType), IsUnique = true, Name = "IX_OrganizationContact_Org_Contact_Type")]
    public class OrganizationContact
    {
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
        /// شناسه فرد
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact Contact { get; set; }

        /// <summary>
        /// نوع رابطه
        /// 0 = کارمند
        /// 1 = مشتری
        /// 2 = تامین‌کننده
        /// 3 = شریک
        /// 4 = مشاور
        /// </summary>
        [Required]
        public byte RelationType { get; set; }

        /// <summary>
        /// عنوان شغلی
        /// </summary>
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        /// <summary>
        /// نام بخش (در صورت عدم استفاده از چارت)
        /// </summary>
        [MaxLength(100)]
        public string? Department { get; set; }

        /// <summary>
        /// آیا تماس اصلی است؟
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// آیا تصمیم‌گیرنده است؟
        /// </summary>
        public bool IsDecisionMaker { get; set; } = false;

        /// <summary>
        /// سطح اهمیت
        /// 0 = پایین
        /// 1 = متوسط
        /// 2 = بالا
        /// 3 = خیلی بالا
        /// </summary>
        public byte ImportanceLevel { get; set; } = 1;

        /// <summary>
        /// تاریخ شروع همکاری
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان همکاری
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

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
        public string RelationTypeText => RelationType switch
        {
            0 => "کارمند",
            1 => "مشتری",
            2 => "تامین‌کننده",
            3 => "شریک",
            4 => "مشاور",
            _ => "نامشخص"
        };

        [NotMapped]
        public string ImportanceLevelText => ImportanceLevel switch
        {
            0 => "پایین",
            1 => "متوسط",
            2 => "بالا",
            3 => "خیلی بالا",
            _ => "نامشخص"
        };
    }
}