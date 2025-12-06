using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// ⭐⭐⭐ تنظیمات پیش‌فرض نمایش تسک برای مدیران بالاسری در شعبه
    /// </summary>
    public class BranchTaskVisibilitySettings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// شناسه کاربر (مدیر بالاسری)
        /// null = تنظیمات پیش‌فرض برای همه مدیران
        /// مقدار = تنظیمات شخصی برای یک مدیر خاص
        /// </summary>
        public string? ManagerUserId { get; set; }
        [ForeignKey("ManagerUserId")]
        public virtual AppUsers? Manager { get; set; }

        /// <summary>
        /// ⭐ لیست تیم‌هایی که باید به صورت پیش‌فرض نمایش داده شوند
        /// فرمت: "1,2,3,5" (شناسه‌های تیم جدا شده با کاما)
        /// null یا خالی = فقط تیم مستقیم
        /// "*" = همه تیم‌های زیرمجموعه
        /// </summary>
        [MaxLength(500)]
        public string? DefaultVisibleTeamIds { get; set; }

        /// <summary>
        /// آیا به صورت پیش‌فرض همه تیم‌های زیرمجموعه نمایش داده شوند؟
        /// </summary>
        public bool ShowAllSubTeamsByDefault { get; set; } = false;

        /// <summary>
        /// حداکثر تعداد تسک‌های قابل نمایش (محدودیت عملکردی)
        /// 0 = نامحدود
        /// </summary>
        public int MaxTasksToShow { get; set; } = 0;

        /// <summary>
        /// آیا این تنظیمات فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر ایجادکننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

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
        /// دریافت لیست تیم‌های قابل نمایش
        /// </summary>
        public List<int> GetVisibleTeamIds()
        {
            if (string.IsNullOrEmpty(DefaultVisibleTeamIds))
                return new List<int>();

            if (DefaultVisibleTeamIds == "*")
                return new List<int>(); // همه (باید در منطق جداگانه handle شود)

            return DefaultVisibleTeamIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var teamId) ? teamId : 0)
                .Where(id => id > 0)
                .ToList();
        }

        /// <summary>
        /// تنظیم لیست تیم‌ها
        /// </summary>
        public void SetVisibleTeamIds(List<int> teamIds)
        {
            if (teamIds == null || !teamIds.Any())
            {
                DefaultVisibleTeamIds = null;
                return;
            }

            DefaultVisibleTeamIds = string.Join(",", teamIds.Distinct().OrderBy(id => id));
        }
    }
}
