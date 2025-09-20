using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.Entities.Organization
{
    /// <summary>
    /// تیم‌ها و گروه‌های سازمانی
    /// </summary>
    public class Team
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان تیم یا گروه
        /// </summary>
        [Required(ErrorMessage = "عنوان تیم الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تیم
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// شناسه تیم والد (در صورت وجود)
        /// </summary>
        public int? ParentTeamId { get; set; }
        [ForeignKey("ParentTeamId")]
        public virtual Team? ParentTeam { get; set; }

        /// <summary>
        /// مدیر تیم
        /// </summary>
        public string? ManagerUserId { get; set; }
        [ForeignKey("ManagerUserId")]
        public virtual AppUsers? Manager { get; set; }

        /// <summary>
        /// شعبه مرتبط با تیم
        /// </summary>
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        /// <summary>
        /// سطح دسترسی تیم در سازمان
        /// 0- عمومی (بدون محدودیت)
        /// 1- محدود (فقط برای اعضا و مدیران بالادست)
        /// </summary>
        public byte AccessLevel { get; set; }

        /// <summary>
        /// اولویت تیم برای نمایش در لیست‌ها
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// تاریخ ایجاد تیم
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده تیم
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// کاربر آخرین بروزرسانی کننده
        /// </summary>
        public string LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        /// <summary>
        /// وضعیت فعال بودن تیم
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [InverseProperty("ParentTeam")]
        public virtual ICollection<Team>? ChildTeams { get; set; }
        
        [InverseProperty("Team")]
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }
        
        [InverseProperty("Team")]
        public virtual ICollection<TeamPosition>? TeamPositions { get; set; } = new HashSet<TeamPosition>();

        /// <summary>
        /// دریافت سمت پیش‌فرض تیم
        /// </summary>
        [NotMapped]
        public TeamPosition? DefaultPosition => TeamPositions?.FirstOrDefault(p => p.IsDefault && p.IsActive);

        /// <summary>
        /// دریافت سمت‌ها به ترتیب سطح قدرت
        /// </summary>
        [NotMapped]
        public IEnumerable<TeamPosition> PositionsByPowerLevel => 
            TeamPositions?.Where(p => p.IsActive).OrderBy(p => p.PowerLevel) ?? Enumerable.Empty<TeamPosition>();
    }
}

