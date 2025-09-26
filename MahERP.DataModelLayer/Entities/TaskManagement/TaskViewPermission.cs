using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organization;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// مجوزهای کلی مشاهده تسک - برای تعریف تبصره‌های دسترسی
    /// این entity برای تعریف مجوزهای کلی استفاده می‌شود، نه برای تسک‌های خاص
    /// </summary>
    public class TaskViewPermission
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کاربری که مجوز دریافت می‌کند (مبدا)
        /// </summary>
        [Required]
        public string GranteeUserId { get; set; }
        [ForeignKey("GranteeUserId")]
        public virtual AppUsers GranteeUser { get; set; }

        /// <summary>
        /// نوع مجوز
        /// 0 = مشاهده تسک‌های یک کاربر خاص
        /// 1 = مشاهده تسک‌های یک تیم خاص  
        /// 2 = مشاهده تسک‌های تیم و زیرتیم‌های آن
        /// </summary>
        [Required]
        public byte PermissionType { get; set; }

        /// <summary>
        /// کاربر مقصد (برای PermissionType = 0)
        /// </summary>
        public string? TargetUserId { get; set; }
        [ForeignKey("TargetUserId")]
        public virtual AppUsers? TargetUser { get; set; }

        /// <summary>
        /// تیم مقصد (برای PermissionType = 1, 2)
        /// </summary>
        public int? TargetTeamId { get; set; }
        [ForeignKey("TargetTeamId")]
        public virtual Team? TargetTeam { get; set; }

        /// <summary>
        /// تیمی که در آن مجوز تعریف شده
        /// </summary>
        [Required]
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        /// <summary>
        /// کاربری که مجوز را اعطا کرده
        /// </summary>
        [Required]
        public string AddedByUserId { get; set; }
        [ForeignKey("AddedByUserId")]
        public virtual AppUsers AddedByUser { get; set; }

        /// <summary>
        /// تاریخ اعطای مجوز
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار مجوز
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار مجوز
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن مجوز
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// توضیحات درباره دلیل اعطای مجوز
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// آخرین بروزرسانی‌کننده
        /// </summary>
        public string? LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        /// <summary>
        /// بررسی اینکه آیا مجوز در بازه زمانی معتبر است
        /// </summary>
        public bool IsValidAtTime(DateTime checkTime)
        {
            return IsActive &&
                   (!StartDate.HasValue || StartDate <= checkTime) &&
                   (!EndDate.HasValue || EndDate >= checkTime);
        }

        /// <summary>
        /// متن نوع مجوز برای نمایش
        /// </summary>
        [NotMapped]
        public string PermissionTypeText => PermissionType switch
        {
            0 => "مشاهده تسک‌های کاربر خاص",
            1 => "مشاهده تسک‌های تیم خاص",
            2 => "مشاهده تسک‌های تیم و زیرتیم‌ها",
            _ => "نامشخص"
        };
    }
}