using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MahERP.Extensions
{
    public static class NotificationExtensions
    {
        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌های کاربر
        /// </summary>
        public static async Task<NotificationStatsViewModel> GetUserNotificationStatsAsync(
            this AppDbContext context, 
            string userId)
        {
            var stats = await context.CoreNotification_Tbl
                .Where(n => n.RecipientUserId == userId)
                .GroupBy(n => 1)
                .Select(g => new NotificationStatsViewModel
                {
                    TotalCount = g.Count(),
                    UnreadCount = g.Count(n => !n.IsRead),
                    TodayCount = g.Count(n => n.CreateDate.Date == DateTime.Today),
                    ThisWeekCount = g.Count(n => n.CreateDate >= DateTime.Today.AddDays(-7))
                })
                .FirstOrDefaultAsync();

            return stats ?? new NotificationStatsViewModel();
        }

        /// <summary>
        /// حذف نوتیفیکیشن‌های قدیمی
        /// </summary>
        public static async Task<int> CleanupOldNotificationsAsync(
            this AppDbContext context, 
            int daysOld = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);
            
            var oldNotifications = await context.CoreNotification_Tbl
                .Where(n => n.CreateDate < cutoffDate && n.IsRead)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                context.CoreNotification_Tbl.RemoveRange(oldNotifications);
                return await context.SaveChangesAsync();
            }

            return 0;
        }
    }

    public class NotificationStatsViewModel
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
    }
}