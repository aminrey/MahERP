using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش ناظران تسک
    /// </summary>
    public class TaskViewersViewModel
    {
        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// ناظران خودکار (سیستمی)
        /// </summary>
        public List<TaskViewerItem> SystemViewers { get; set; } = new();

        /// <summary>
        /// ناظران رونوشت شده (دستی)
        /// </summary>
        public List<TaskCarbonCopyItem> CarbonCopyViewers { get; set; } = new();

        /// <summary>
        /// آیا کاربر جاری می‌تواند ناظر اضافه کند؟
        /// </summary>
        public bool CanAddViewer { get; set; }

        /// <summary>
        /// کل تعداد ناظران
        /// </summary>
        public int TotalViewers => SystemViewers.Count + CarbonCopyViewers.Count;
    }

    /// <summary>
    /// آیتم ناظر سیستمی
    /// </summary>
    public class TaskViewerItem
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// دلیل نظارت (مدیر تیم، سرپرست، ...)
        /// </summary>
        public string ViewerReason { get; set; }

        /// <summary>
        /// نام تیم (در صورت مرتبط بودن)
        /// </summary>
        public string TeamName { get; set; }
    }

    /// <summary>
    /// آیتم رونوشت دستی
    /// </summary>
    public class TaskCarbonCopyItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// کاربر افزوده‌کننده
        /// </summary>
        public string AddedByUserName { get; set; }
        
        /// <summary>
        /// تاریخ افزودن (شمسی)
        /// </summary>
        public string AddedDatePersian { get; set; }
        
        /// <summary>
        /// یادداشت رونوشت
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// آیا کاربر جاری می‌تواند این رونوشت را حذف کند؟
        /// </summary>
        public bool CanRemove { get; set; }
    }
}
