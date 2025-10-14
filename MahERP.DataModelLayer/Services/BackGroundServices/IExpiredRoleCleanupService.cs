namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// رابط سرویس پاکسازی خودکار نقش‌های منقضی شده
    /// 
    /// 📖 توضیحات:
    /// این سرویس به صورت خودکار در Background اجرا می‌شود و نقش‌هایی که تاریخ پایان آن‌ها گذشته است را غیرفعال می‌کند.
    /// 
    /// ⚙️ نحوه کار:
    /// - هر 1 ساعت یک بار اجرا می‌شود (قابل تنظیم)
    /// - UserRole هایی که EndDate < DateTime.Now را پیدا می‌کند
    /// - IsActive آن‌ها را false می‌کند
    /// - دسترسی‌های کاربر را همگام‌سازی می‌کند
    /// 
    /// 🔧 توسعه آینده:
    /// - می‌توان Notification برای کاربران ارسال کرد
    /// - می‌توان Email به مدیران ارسال کرد
    /// - می‌توان گزارش حذف‌ها را لاگ کرد
    /// </summary>
    public interface IExpiredRoleCleanupService
    {
        /// <summary>
        /// پاکسازی نقش‌های منقضی شده
        /// </summary>
        /// <returns>تعداد نقش‌های پاکسازی شده</returns>
        Task<int> CleanupExpiredRolesAsync();
    }
}