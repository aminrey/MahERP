using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using MahERP.CommonLayer.PublicClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// پیاده‌سازی مخزن سیستم نوتیفیکیشن کلی ERP
    /// مطابق معماری سه لایه و قوانین پروژه
    /// </summary>
    public class CoreNotificationRepository : ICoreNotificationRepository
    {
        private readonly AppDbContext _context;

        public CoreNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        #region عملیات پایه - Basic Operations

        /// <summary>
        /// دریافت نوتیفیکیشن‌های کاربر با قابلیت صفحه‌بندی و فیلتر
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
                // کوئری پایه برای دریافت نوتیفیکیشن‌های کاربر
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive)
                    .AsQueryable();

                // اعمال فیلتر سیستم
                if (systemId.HasValue)
                {
                    query = query.Where(n => n.SystemId == systemId.Value);
                }

                // اعمال فیلتر خوانده نشده
                if (unreadOnly)
                {
                    query = query.Where(n => !n.IsRead);
                }

                // محاسبه تعداد کل
                var totalCount = await query.CountAsync();

                // محاسبه تعداد خوانده نشده
                var unreadCount = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead)
                    .CountAsync();

                // اعمال صفحه‌بندی و مرتب‌سازی بر اساس تاریخ ایجاد (جدیدترین اول)
                var notifications = await query
                    .OrderByDescending(n => n.CreateDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(n => n.Sender)
                    .Include(n => n.Details)
                    .Include(n => n.Deliveries)
                    .ToListAsync();

                // تبدیل به ویو مدل
                var notificationViewModels = notifications.Select(n => MapToViewModel(n)).ToList();

                return new CoreNotificationListViewModel
                {
                    Notifications = notificationViewModels,
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // لاگ خطا
                throw new Exception($"خطا در دریافت نوتیفیکیشن‌های کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده کاربر
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                {
                    query = query.Where(n => n.SystemId == systemId.Value);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تعداد نوتیفیکیشن‌های خوانده نشده: {ex.Message}", ex);
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
                throw new Exception($"خطا در دریافت جزئیات نوتیفیکیشن: {ex.Message}", ex);
            }
        }

        #endregion

        #region ایجاد نوتیفیکیشن - Create Notifications

        /// <summary>
        /// ایجاد نوتیفیکیشن جدید
        /// </summary>
        public async Task<int> CreateNotificationAsync(CoreNotification notification)
        {
            try
            {
                notification.CreateDate = DateTime.Now;
                notification.IsActive = true;

                _context.CoreNotification_Tbl.Add(notification);
                await _context.SaveChangesAsync();

                return notification.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد نوتیفیکیشن: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ایجاد نوتیفیکیشن برای چندین کاربر
        /// </summary>
        public async Task<List<int>> CreateBulkNotificationAsync(
            List<string> userIds,
            byte systemId,
            byte notificationType,
            string title,
            string message,
            string senderUserId,
            string actionUrl = null,
            string relatedRecordId = null,
            string relatedRecordType = null,
            string relatedRecordTitle = null,
            byte priority = 0)
        {
            try
            {
                var notifications = new List<CoreNotification>();
                var createDate = DateTime.Now;

                // ایجاد نوتیفیکیشن برای هر کاربر
                foreach (var userId in userIds.Distinct())
                {
                    // بررسی اینکه کاربر با خودش یکی نباشد (فرستنده نوتیفیکیشن دریافت نکند)
                    if (userId == senderUserId)
                        continue;

                    var notification = new CoreNotification
                    {
                        SystemId = systemId,
                        SystemName = GetSystemName(systemId),
                        RecipientUserId = userId,
                        SenderUserId = senderUserId,
                        NotificationTypeGeneral = notificationType,
                        Title = title,
                        Message = message,
                        CreateDate = createDate,
                        ActionUrl = actionUrl,
                        RelatedRecordId = relatedRecordId,
                        RelatedRecordType = relatedRecordType,
                        RelatedRecordTitle = relatedRecordTitle,
                        Priority = priority,
                        IsRead = false,
                        IsClicked = false,
                        IsActive = true
                    };

                    notifications.Add(notification);
                }

                if (notifications.Any())
                {
                    _context.CoreNotification_Tbl.AddRange(notifications);
                    await _context.SaveChangesAsync();
                }

                return notifications.Select(n => n.Id).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد نوتیفیکیشن‌های گروهی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ایجاد نوتیفیکیشن تغییر رکورد
        /// </summary>
        public async Task<List<int>> CreateRecordChangeNotificationAsync(
            List<string> userIds,
            byte systemId,
            string recordId,
            string recordType,
            string recordTitle,
            string changedByUserId,
            List<CoreNotificationDetailViewModel> changeDetails)
        {
            try
            {
                // ایجاد نوتیفیکیشن اصلی
                var title = $"تغییر در {recordType}";
                var message = $"رکورد {recordTitle} توسط کاربر تغییر یافته است";
                var actionUrl = GenerateActionUrl(systemId, recordType, recordId);

                var notificationIds = await CreateBulkNotificationAsync(
                    userIds, systemId, 1, title, message, changedByUserId,
                    actionUrl, recordId, recordType, recordTitle, 1);

                // ایجاد جزئیات تغییرات برای هر نوتیفیکیشن
                var details = new List<CoreNotificationDetail>();
                foreach (var notificationId in notificationIds)
                {
                    foreach (var detail in changeDetails)
                    {
                        var notificationDetail = new CoreNotificationDetail
                        {
                            CoreNotificationId = notificationId,
                            NotificationTypeSpecific = detail.NotificationTypeSpecific,
                            FieldName = detail.FieldName,
                            OldValue = detail.OldValue,
                            NewValue = detail.NewValue,
                            Description = detail.Description,
                            CreateDate = DateTime.Now,
                            IsActive = true
                        };

                        details.Add(notificationDetail);
                    }
                }

                if (details.Any())
                {
                    _context.CoreNotificationDetail_Tbl.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                return notificationIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد نوتیفیکیشن تغییر رکورد: {ex.Message}", ex);
            }
        }

        #endregion

        #region مدیریت وضعیت - Status Management

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن به عنوان خوانده شده
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null)
                    return false;

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
                throw new Exception($"خطا در علامت‌گذاری نوتیفیکیشن به عنوان خوانده شده: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن به عنوان کلیک شده
        /// </summary>
        public async Task<bool> MarkAsClickedAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.CoreNotification_Tbl
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && n.IsActive);

                if (notification == null)
                    return false;

                // علامت‌گذاری به عنوان کلیک شده و خوانده شده
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
                throw new Exception($"خطا در علامت‌گذاری نوتیفیکیشن به عنوان کلیک شده: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های مرتبط با یک رکورد به عنوان خوانده شده
        /// </summary>
        public async Task<int> MarkRelatedNotificationsAsReadAsync(string userId, byte systemId, string relatedRecordId)
        {
            try
            {
                var notifications = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId 
                             && n.SystemId == systemId 
                             && n.RelatedRecordId == relatedRecordId 
                             && n.IsActive 
                             && !n.IsRead)
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
                throw new Exception($"خطا در علامت‌گذاری نوتیفیکیشن‌های مرتبط: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های کاربر به عنوان خوانده شده
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string userId, byte? systemId = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive && !n.IsRead);

                if (systemId.HasValue)
                {
                    query = query.Where(n => n.SystemId == systemId.Value);
                }

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
                throw new Exception($"خطا در علامت‌گذاری همه نوتیفیکیشن‌ها: {ex.Message}", ex);
            }
        }

        #endregion

        #region مدیریت ارسال - Delivery Management

        /// <summary>
        /// ایجاد رکورد ارسال برای نوتیفیکیشن
        /// </summary>
        public async Task<int> CreateNotificationDeliveryAsync(int notificationId, byte deliveryMethod, string deliveryAddress)
        {
            try
            {
                var delivery = new CoreNotificationDelivery
                {
                    CoreNotificationId = notificationId,
                    DeliveryMethod = deliveryMethod,
                    DeliveryAddress = deliveryAddress,
                    DeliveryStatus = 0, // در انتظار ارسال
                    AttemptCount = 0,
                    MaxAttempts = 3,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotificationDelivery_Tbl.Add(delivery);
                await _context.SaveChangesAsync();

                return delivery.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد رکورد ارسال: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی وضعیت ارسال
        /// </summary>
        public async Task<bool> UpdateDeliveryStatusAsync(int deliveryId, byte status, string errorMessage = null, string externalId = null)
        {
            try
            {
                var delivery = await _context.CoreNotificationDelivery_Tbl
                    .FirstOrDefaultAsync(d => d.Id == deliveryId && d.IsActive);

                if (delivery == null)
                    return false;

                delivery.DeliveryStatus = status;
                delivery.AttemptCount++;
                
                if (status == 1 || status == 2) // ارسال شده یا تحویل داده شده
                {
                    delivery.DeliveryDate = DateTime.Now;
                }
                
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    delivery.ErrorMessage = errorMessage;
                }
                
                if (!string.IsNullOrEmpty(externalId))
                {
                    delivery.ExternalId = externalId;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در به‌روزرسانی وضعیت ارسال: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت نوتیفیکیشن‌هایی که باید ارسال شوند
        /// </summary>
        public async Task<List<CoreNotificationDeliveryViewModel>> GetPendingDeliveriesAsync(
            byte deliveryMethod, 
            int maxAttempts = 3, 
            int limit = 100)
        {
            try
            {
                var deliveries = await _context.CoreNotificationDelivery_Tbl
                    .Include(d => d.CoreNotification)
                    .Where(d => d.DeliveryMethod == deliveryMethod 
                             && d.DeliveryStatus == 0 
                             && d.AttemptCount < maxAttempts 
                             && d.IsActive)
                    .OrderBy(d => d.CreateDate)
                    .Take(limit)
                    .ToListAsync();

                return deliveries.Select(d => MapDeliveryToViewModel(d)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت ارسال‌های در انتظار: {ex.Message}", ex);
            }
        }

        #endregion

        #region تنظیمات کاربر - User Settings

        /// <summary>
        /// دریافت تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        public async Task<CoreNotificationSettingViewModel> GetUserNotificationSettingsAsync(string userId, byte systemId)
        {
            try
            {
                var setting = await _context.CoreNotificationSetting_Tbl
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SystemId == systemId && s.IsActive);

                return setting != null ? MapSettingToViewModel(setting) : CreateDefaultSetting(userId, systemId);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تنظیمات کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        public async Task<bool> UpdateUserNotificationSettingsAsync(CoreNotificationSetting settings)
        {
            try
            {
                var existingSetting = await _context.CoreNotificationSetting_Tbl
                    .FirstOrDefaultAsync(s => s.UserId == settings.UserId 
                                           && s.SystemId == settings.SystemId 
                                           && s.NotificationTypeGeneral == settings.NotificationTypeGeneral);

                if (existingSetting != null)
                {
                    // به‌روزرسانی تنظیمات موجود
                    existingSetting.IsSystemEnabled = settings.IsSystemEnabled;
                    existingSetting.IsEmailEnabled = settings.IsEmailEnabled;
                    existingSetting.IsSmsEnabled = settings.IsSmsEnabled;
                    existingSetting.IsTelegramEnabled = settings.IsTelegramEnabled;
                    existingSetting.StartTime = settings.StartTime;
                    existingSetting.EndTime = settings.EndTime;
                    existingSetting.SendOnHolidays = settings.SendOnHolidays;
                    existingSetting.LastUpdateDate = DateTime.Now;
                }
                else
                {
                    // ایجاد تنظیمات جدید
                    settings.CreateDate = DateTime.Now;
                    settings.IsActive = true;
                    _context.CoreNotificationSetting_Tbl.Add(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در به‌روزرسانی تنظیمات کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر برای دریافت نوتیفیکیشن تنظیم کرده است
        /// </summary>
        public async Task<bool> IsUserNotificationEnabledAsync(string userId, byte systemId, byte notificationType, byte deliveryMethod = 0)
        {
            try
            {
                var setting = await _context.CoreNotificationSetting_Tbl
                    .FirstOrDefaultAsync(s => s.UserId == userId 
                                           && s.SystemId == systemId 
                                           && s.NotificationTypeGeneral == notificationType 
                                           && s.IsActive);

                if (setting == null)
                    return true; // پیش‌فرض فعال

                return deliveryMethod switch
                {
                    0 => setting.IsSystemEnabled,
                    1 => setting.IsEmailEnabled,
                    2 => setting.IsSmsEnabled,
                    3 => setting.IsTelegramEnabled,
                    _ => setting.IsSystemEnabled
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در بررسی تنظیمات کاربر: {ex.Message}", ex);
            }
        }

        #endregion

        #region آمار و گزارش - Statistics & Reports

        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌های کاربر
        /// </summary>
        public async Task<CoreNotificationStatsViewModel> GetUserNotificationStatsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive);

                if (fromDate.HasValue)
                    query = query.Where(n => n.CreateDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(n => n.CreateDate <= toDate.Value);

                var stats = new CoreNotificationStatsViewModel
                {
                    TotalNotifications = await query.CountAsync(),
                    ReadNotifications = await query.CountAsync(n => n.IsRead),
                    UnreadNotifications = await query.CountAsync(n => !n.IsRead),
                    ClickedNotifications = await query.CountAsync(n => n.IsClicked)
                };

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت آمار کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت آمار کلی سیستم نوتیفیکیشن
        /// </summary>
        public async Task<CoreNotificationSystemStatsViewModel> GetSystemNotificationStatsAsync(byte? systemId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.CoreNotification_Tbl.Where(n => n.IsActive);

                if (systemId.HasValue)
                    query = query.Where(n => n.SystemId == systemId.Value);

                if (fromDate.HasValue)
                    query = query.Where(n => n.CreateDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(n => n.CreateDate <= toDate.Value);

                var today = DateTime.Today;
                var todayNotifications = await _context.CoreNotification_Tbl
                    .CountAsync(n => n.CreateDate >= today && n.IsActive);

                var stats = new CoreNotificationSystemStatsViewModel
                {
                    TotalSystemNotifications = await query.CountAsync(),
                    TotalRecipients = await query.Select(n => n.RecipientUserId).Distinct().CountAsync(),
                    TodayNotifications = todayNotifications
                };

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت آمار سیستم: {ex.Message}", ex);
            }
        }

        #endregion

        #region پاکسازی - Cleanup

        /// <summary>
        /// حذف نوتیفیکیشن‌های قدیمی
        /// </summary>
        public async Task<int> DeleteOldNotificationsAsync(DateTime beforeDate, bool keepImportant = true)
        {
            try
            {
                var query = _context.CoreNotification_Tbl
                    .Where(n => n.CreateDate < beforeDate && n.IsActive);

                if (keepImportant)
                {
                    query = query.Where(n => n.Priority < 2); // حفظ نوتیفیکیشن‌های مهم و فوری
                }

                var notifications = await query.ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsActive = false; // حذف منطقی
                }

                await _context.SaveChangesAsync();
                return notifications.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در حذف نوتیفیکیشن‌های قدیمی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// آرشیو کردن نوتیفیکیشن‌های قدیمی
        /// </summary>
        public async Task<int> ArchiveOldNotificationsAsync(DateTime beforeDate)
        {
            try
            {
                // پیاده‌سازی آرشیو در صورت نیاز
                // می‌توان نوتیفیکیشن‌ها را به جدول آرشیو منتقل کرد
                
                return 0; // فعلاً پیاده‌سازی نشده
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در آرشیو نوتیفیکیشن‌ها: {ex.Message}", ex);
            }
        }

        #endregion

        #region متدهای کمکی - Helper Methods

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        private CoreNotificationViewModel MapToViewModel(CoreNotification notification)
        {
            return new CoreNotificationViewModel
            {
                Id = notification.Id,
                SystemId = notification.SystemId,
                SystemName = notification.SystemName,
                RecipientUserId = notification.RecipientUserId,
                RecipientUserName = notification.Recipient?.UserName,
                SenderUserId = notification.SenderUserId,
                SenderUserName = notification.Sender?.UserName,
                NotificationTypeGeneral = notification.NotificationTypeGeneral,
                NotificationTypeName = GetNotificationTypeName(notification.NotificationTypeGeneral),
                Title = notification.Title,
                Message = notification.Message,
                CreateDate = notification.CreateDate,
                CreateDatePersian = ConvertDateTime.ConvertMiladiToShamsi(notification.CreateDate, "yyyy/MM/dd"),
                CreateTime = notification.CreateDate.ToString("HH:mm"),
                IsRead = notification.IsRead,
                ReadDate = notification.ReadDate,
                ReadDatePersian = notification.ReadDate.HasValue ? 
                    ConvertDateTime.ConvertMiladiToShamsi(notification.ReadDate, "yyyy/MM/dd HH:mm") : null,
                IsClicked = notification.IsClicked,
                ClickDate = notification.ClickDate,
                Priority = notification.Priority,
                PriorityName = GetPriorityName(notification.Priority),
                PriorityClass = GetPriorityClass(notification.Priority),
                ActionUrl = notification.ActionUrl,
                RelatedRecordId = notification.RelatedRecordId,
                RelatedRecordType = notification.RelatedRecordType,
                RelatedRecordTitle = notification.RelatedRecordTitle,
                IsActive = notification.IsActive,
                RelativeTime = GetRelativeTime(notification.CreateDate),
                Icon = GetNotificationIcon(notification.NotificationTypeGeneral),
                CssClass = GetNotificationCssClass(notification.IsRead, notification.Priority),
                Details = notification.Details?.Select(d => MapDetailToViewModel(d)).ToList() ?? new List<CoreNotificationDetailViewModel>(),
                Deliveries = notification.Deliveries?.Select(d => MapDeliveryToViewModel(d)).ToList() ?? new List<CoreNotificationDeliveryViewModel>()
            };
        }

        /// <summary>
        /// تبدیل جزئیات به ViewModel
        /// </summary>
        private CoreNotificationDetailViewModel MapDetailToViewModel(CoreNotificationDetail detail)
        {
            return new CoreNotificationDetailViewModel
            {
                Id = detail.Id,
                CoreNotificationId = detail.CoreNotificationId,
                NotificationTypeSpecific = detail.NotificationTypeSpecific,
                FieldName = detail.FieldName,
                FieldDisplayName = GetFieldDisplayName(detail.FieldName),
                OldValue = detail.OldValue,
                NewValue = detail.NewValue,
                Description = detail.Description,
                HighlightClass = "highlight-change"
            };
        }

        /// <summary>
        /// تبدیل ارسال به ViewModel
        /// </summary>
        private CoreNotificationDeliveryViewModel MapDeliveryToViewModel(CoreNotificationDelivery delivery)
        {
            return new CoreNotificationDeliveryViewModel
            {
                Id = delivery.Id,
                CoreNotificationId = delivery.CoreNotificationId,
                DeliveryMethod = delivery.DeliveryMethod,
                DeliveryMethodName = GetDeliveryMethodName(delivery.DeliveryMethod),
                DeliveryAddress = delivery.DeliveryAddress,
                DeliveryStatus = delivery.DeliveryStatus,
                DeliveryStatusName = GetDeliveryStatusName(delivery.DeliveryStatus),
                AttemptDate = delivery.AttemptDate,
                DeliveryDate = delivery.DeliveryDate,
                AttemptCount = delivery.AttemptCount,
                ErrorMessage = delivery.ErrorMessage,
                ExternalId = delivery.ExternalId,
                StatusClass = GetDeliveryStatusClass(delivery.DeliveryStatus),
                StatusIcon = GetDeliveryStatusIcon(delivery.DeliveryStatus)
            };
        }

        /// <summary>
        /// تبدیل تنظیمات به ViewModel
        /// </summary>
        private CoreNotificationSettingViewModel MapSettingToViewModel(CoreNotificationSetting setting)
        {
            return new CoreNotificationSettingViewModel
            {
                Id = setting.Id,
                UserId = setting.UserId,
                SystemId = setting.SystemId,
                SystemName = GetSystemName(setting.SystemId),
                NotificationTypeGeneral = setting.NotificationTypeGeneral,
                NotificationTypeName = GetNotificationTypeName(setting.NotificationTypeGeneral),
                IsSystemEnabled = setting.IsSystemEnabled,
                IsEmailEnabled = setting.IsEmailEnabled,
                IsSmsEnabled = setting.IsSmsEnabled,
                IsTelegramEnabled = setting.IsTelegramEnabled,
                StartTime = setting.StartTime,
                EndTime = setting.EndTime,
                SendOnHolidays = setting.SendOnHolidays,
                IsActive = setting.IsActive
            };
        }

        /// <summary>
        /// ایجاد تنظیمات پیش‌فرض
        /// </summary>
        private CoreNotificationSettingViewModel CreateDefaultSetting(string userId, byte systemId)
        {
            return new CoreNotificationSettingViewModel
            {
                UserId = userId,
                SystemId = systemId,
                SystemName = GetSystemName(systemId),
                IsSystemEnabled = true,
                IsEmailEnabled = false,
                IsSmsEnabled = false,
                IsTelegramEnabled = false,
                SendOnHolidays = true,
                IsActive = true
            };
        }

        /// <summary>
        /// دریافت نام سیستم بر اساس شناسه
        /// </summary>
        private string GetSystemName(byte systemId)
        {
            return systemId switch
            {
                1 => "سیستم مدیریت مالی",
                2 => "سیستم منابع انسانی",
                3 => "سیستم فروش و CRM",
                4 => "سیستم خرید و تدارکات",
                5 => "سیستم انبار و لجستیک",
                6 => "سیستم تولید و کنترل کیفیت",
                7 => "سیستم مدیریت پروژه و تسک‌ها",
                _ => "سیستم نامشخص"
            };
        }

        /// <summary>
        /// دریافت نام نوع نوتیفیکیشن
        /// </summary>
        private string GetNotificationTypeName(byte type)
        {
            return type switch
            {
                0 => "اطلاع‌رسانی عمومی",
                1 => "ایجاد رکورد جدید",
                2 => "ویرایش رکورد",
                3 => "حذف رکورد",
                4 => "تایید/رد",
                5 => "هشدار",
                6 => "یادآوری",
                7 => "خطا/مشکل",
                8 => "تکمیل فرآیند",
                9 => "اختصاص/انتساب",
                10 => "تغییر وضعیت",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت نام اولویت
        /// </summary>
        private string GetPriorityName(byte priority)
        {
            return priority switch
            {
                0 => "عادی",
                1 => "مهم",
                2 => "فوری",
                3 => "بحرانی",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت کلاس CSS اولویت
        /// </summary>
        private string GetPriorityClass(byte priority)
        {
            return priority switch
            {
                0 => "priority-normal",
                1 => "priority-important",
                2 => "priority-urgent",
                3 => "priority-critical",
                _ => "priority-normal"
            };
        }

        /// <summary>
        /// دریافت آیکون نوتیفیکیشن
        /// </summary>
        private string GetNotificationIcon(byte type)
        {
            return type switch
            {
                0 => "fa-info-circle",
                1 => "fa-plus-circle",
                2 => "fa-edit",
                3 => "fa-trash",
                4 => "fa-check-circle",
                5 => "fa-exclamation-triangle",
                6 => "fa-clock",
                7 => "fa-times-circle",
                8 => "fa-flag-checkered",
                9 => "fa-user-plus",
                10 => "fa-exchange-alt",
                _ => "fa-bell"
            };
        }

        /// <summary>
        /// دریافت کلاس CSS نوتیفیکیشن
        /// </summary>
        private string GetNotificationCssClass(bool isRead, byte priority)
        {
            var baseClass = isRead ? "notification-read" : "notification-unread";
            var priorityClass = GetPriorityClass(priority);
            return $"{baseClass} {priorityClass}";
        }

        /// <summary>
        /// دریافت زمان نسبی
        /// </summary>
        private string GetRelativeTime(DateTime createDate)
        {
            var timeSpan = DateTime.Now - createDate;
            
            if (timeSpan.TotalMinutes < 1)
                return "الان";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} روز پیش";
            else
                return ConvertDateTime.ConvertMiladiToShamsi(createDate, "yyyy/MM/dd");
        }

        /// <summary>
        /// دریافت نام نمایشی فیلد
        /// </summary>
        private string GetFieldDisplayName(string fieldName)
        {
            return fieldName switch
            {
                "Title" => "عنوان",
                "Description" => "توضیحات",
                "DueDate" => "تاریخ سررسید",
                "Priority" => "اولویت",
                "Status" => "وضعیت",
                "AssignedUsers" => "کاربران منصوب",
                _ => fieldName
            };
        }

        /// <summary>
        /// تولید URL عمل
        /// </summary>
        private string GenerateActionUrl(byte systemId, string recordType, string recordId)
        {
            return systemId switch
            {
                7 when recordType == "Task" => $"/AdminArea/Tasks/Details/{recordId}",
                3 when recordType == "CRM" => $"/AdminArea/CRM/Details/{recordId}",
                _ => $"/AdminArea/{recordType}/Details/{recordId}"
            };
        }

        /// <summary>
        /// دریافت نام روش تحویل
        /// </summary>
        private string GetDeliveryMethodName(byte method)
        {
            return method switch
            {
                0 => "سیستم داخلی",
                1 => "ایمیل",
                2 => "پیامک",
                3 => "تلگرام",
                4 => "واتساپ",
                5 => "پوش نوتیفیکیشن",
                6 => "دسکتاپ نوتیفیکیشن",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت نام وضعیت تحویل
        /// </summary>
        private string GetDeliveryStatusName(byte status)
        {
            return status switch
            {
                0 => "در انتظار ارسال",
                1 => "ارسال شده",
                2 => "تحویل داده شده",
                3 => "خطا در ارسال",
                4 => "لغو شده",
                5 => "مسدود شده",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت کلاس CSS وضعیت تحویل
        /// </summary>
        private string GetDeliveryStatusClass(byte status)
        {
            return status switch
            {
                0 => "status-pending",
                1 => "status-sent",
                2 => "status-delivered",
                3 => "status-failed",
                4 => "status-cancelled",
                5 => "status-blocked",
                _ => "status-unknown"
            };
        }

        /// <summary>
        /// دریافت آیکون وضعیت تحویل
        /// </summary>
        private string GetDeliveryStatusIcon(byte status)
        {
            return status switch
            {
                0 => "fa-clock",
                1 => "fa-paper-plane",
                2 => "fa-check-circle",
                3 => "fa-times-circle",
                4 => "fa-ban",
                5 => "fa-lock",
                _ => "fa-question-circle"
            };
        }

        #endregion
    }
}