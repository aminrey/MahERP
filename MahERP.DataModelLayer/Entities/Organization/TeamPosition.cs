using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.Entities.Organization
{
    /// <summary>
    /// سمت‌ها و مقام‌های مختلف در هر تیم
    /// </summary>
    public class TeamPosition
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تیم مرتبط
        /// </summary>
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        /// <summary>
        /// عنوان سمت
        /// </summary>
        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات سمت
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح قدرت و اختیار در تیم (عدد کمتر = قدرت بیشتر)
        /// 0 = بالاترین سطح (مدیر تیم)
        /// 1 = سطح دوم (معاون/سرپرست)
        /// 2 = سطح سوم (کارشناس ارشد)
        /// و الی آخر...
        /// </summary>
        public int PowerLevel { get; set; }

        /// <summary>
        /// آیا این سمت می‌تواند تسک‌های سمت‌های پایین‌تر را مشاهده کند
        /// </summary>
        public bool CanViewSubordinateTasks { get; set; } = true;

        /// <summary>
        /// آیا این سمت می‌تواند تسک‌های همسطح خود را مشاهده کند
        /// </summary>
        public bool CanViewPeerTasks { get; set; } = false;

        /// <summary>
        /// حداکثر تعداد اعضا در این سمت (null = بدون محدودیت)
        /// </summary>
        public int? MaxMembers { get; set; }

        /// <summary>
        /// ترتیب نمایش در لیست‌ها
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// آیا این سمت پیش‌فرض برای اعضای جدید است
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// وضعیت فعال بودن سمت
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// آخرین بروزرسانی کننده
        /// </summary>
        public string? LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        // Navigation properties
        [InverseProperty("Position")]
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new HashSet<TeamMember>();

        /// <summary>
        /// تعداد اعضای فعلی در این سمت
        /// </summary>
        [NotMapped]
        public int CurrentMembersCount => TeamMembers?.Count(tm => tm.IsActive) ?? 0;

        /// <summary>
        /// آیا این سمت می‌تواند عضو جدید بپذیرد
        /// </summary>
        [NotMapped]
        public bool CanAddMember => !MaxMembers.HasValue || CurrentMembersCount < MaxMembers.Value;
    }
}