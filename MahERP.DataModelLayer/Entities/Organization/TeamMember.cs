using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Organization
{
    /// <summary>
    /// اعضای تیم
    /// </summary>
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تیم
        /// </summary>
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// سمت عضو در تیم (رابطه با TeamPosition)
        /// </summary>
        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public virtual TeamPosition? Position { get; set; }

 

        /// <summary>
        /// توضیحات نقش کاربر در تیم
        /// </summary>
        [MaxLength(500)]
        public string? RoleDescription { get; set; }

        /// <summary>
        /// نوع عضویت
        /// 0- عضو عادی
        /// 1- عضو ویژه
        /// 2- مدیر تیم
        /// </summary>
        public byte MembershipType { get; set; }

        /// <summary>
        /// تاریخ شروع عضویت
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان عضویت (در صورت وجود)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// کاربری که این عضو را به تیم اضافه کرده است
        /// </summary>
        public string AddedByUserId { get; set; }
        [ForeignKey("AddedByUserId")]
        public virtual AppUsers AddedByUser { get; set; }

        /// <summary>
        /// وضعیت فعال بودن عضویت
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// سطح قدرت عضو (از سمت گرفته می‌شود)
        /// </summary>
        [NotMapped]
        public int PowerLevel => Position?.PowerLevel ?? 999; // 999 برای اعضای بدون سمت

        /// <summary>
        /// نام سمت برای نمایش
        /// </summary>
        [NotMapped]
        public string PositionTitle => Position?.Title  ?? "عضو";
    }
}