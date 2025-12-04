using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ⭐ اضافه شد

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// عملیات پایه - دریافت و نمایش اعلان‌ها
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 🔔 عملیات پایه - Basic Operations

        /// <summary>
        /// دریافت نوتیفیکیشن‌های کاربر با فیلتر و صفحه‌بندی
        /// </summary>
        public async Task<CoreNotificationListViewModel> GetUserNotificationsAsync(
            string userId,
            byte? systemId = null,
            bool unreadOnly = false,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive)
                    .AsQueryable();

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                if (unreadOnly)
                    query = query.Where(n => !n.IsRead);

                var totalCount = await query.CountAsync();
                var unreadCount = await _context.CoreNotification_Tbl
                    .CountAsync(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                var notifications = await query
                    .OrderByDescending(n => n.CreateDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(n => n.Sender)
                    .Include(n => n.Details)
                    .Include(n => n.Deliveries)
                    .ToListAsync();

                return new CoreNotificationListViewModel
                {
                    Notifications = notifications.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت نوتیفیکیشن‌های کاربر");
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد خوانده نشده");
                return 0;
            }
        }

        /// <summary>
        /// دریافت جزئیات یک نوتیفیکیشن
        /// </summary>
        public async Task<CoreNotificationViewModel> GetNotificationByIdAsync(int notificationId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .Include(n => n.Sender)
                    .Include(n => n.Recipient)
                    .Include(n => n.Details)
                    .Include(n => n.Deliveries)
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.IsActive);

                return notification != null ? MapToViewModel(notification) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در دریافت نوتیفیکیشن {notificationId}");
                return null;
            }
        }

        #endregion
    }
}
