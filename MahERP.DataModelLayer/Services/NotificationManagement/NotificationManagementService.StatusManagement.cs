using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// مدیریت وضعیت اعلان‌ها (خوانده شده، کلیک شده)
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 🔧 مدیریت وضعیت - Status Management

        /// <summary>
        /// علامت‌گذاری به عنوان خوانده شده
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null) return false;

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAsReadAsync");
                return false;
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان کلیک شده
        /// </summary>
        public async Task<bool> MarkAsClickedAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null) return false;

                notification.IsClicked = true;
                notification.ClickDate = DateTime.Now;

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAsClickedAsync");
                return false;
            }
        }

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های مرتبط با یک رکورد
        /// </summary>
        public async Task<int> MarkRelatedNotificationsAsReadAsync(string userId, byte systemId, string relatedRecordId)
        {
            try
            {
                var notifications = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId &&
                               n.SystemId == systemId &&
                               n.RelatedRecordId == relatedRecordId &&
                               n.IsActive &&
                               !n.IsRead)
                    .ToListAsync();

                var readDate = DateTime.Now;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadDate = readDate;
                }

                await _context.SaveChangesAsync();
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkRelatedNotificationsAsReadAsync");
                return 0;
            }
        }

        /// <summary>
        /// علامت‌گذاری همه به عنوان خوانده شده
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                var notifications = await query.ToListAsync();

                var readDate = DateTime.Now;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadDate = readDate;
                }

                await _context.SaveChangesAsync();
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در MarkAllAsReadAsync");
                return 0;
            }
        }

        #endregion
    }
}
