using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// کاربران مجاز به مشاهده تسک (بدون دسترسی ویرایش)
    /// لیست اشتراک و نمایش
    /// </summary>
    public class TaskViewer
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// نوع دسترسی
        /// 0- اضافه شده دستی (مجوز خاص)
        /// 1- دسترسی بر اساس سلسله مراتب (مدیر تیم)
        /// 2- دسترسی بر اساس عضویت در تیم
        /// 3- دسترسی عمومی (VisibilityLevel تسک)
        /// 4- سازنده تسک
        /// 5- منتصب به تسک
        /// </summary>
        public byte AccessType { get; set; }

        /// <summary>
        /// شناسه تیم مرتبط (اگر AccessType برابر با 2 باشد)
        /// </summary>
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        /// <summary>
        /// نوع مجوز خاص (برای AccessType = 0)
        /// 0 = مشاهده تسک‌های یک کاربر خاص
        /// 1 = مشاهده تسک‌های یک تیم خاص  
        /// 2 = مشاهده تسک‌های تیم و زیرتیم‌های آن
        /// </summary>
        public byte? SpecialPermissionType { get; set; }

        /// <summary>
        /// کاربری که مجوز را اعطا کرده
        /// </summary>
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
        /// آیا کاربر این تسک را مشاهده کرده است
        /// </summary>
        public bool IsViewed { get; set; }

        /// <summary>
        /// تاریخ آخرین مشاهده تسک
        /// </summary>
        public DateTime? ViewDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن مجوز
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// توضیحات اضافی درباره دلیل اضافه شدن کاربر به لیست مشاهده کنندگان
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
        /// متن نوع دسترسی برای نمایش
        /// </summary>
        [NotMapped]
        public string AccessTypeText => AccessType switch
        {
            0 => "مجوز خاص",
            1 => "مدیر تیم",
            2 => "عضو تیم",
            3 => "دسترسی عمومی",
            4 => "سازنده",
            5 => "منتصب",
            _ => "نامشخص"
        };
    }
}

