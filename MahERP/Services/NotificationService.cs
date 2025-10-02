using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MahERP.DataModelLayer;
using Microsoft.EntityFrameworkCore;

namespace MahERP.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            AppDbContext context,
            IMemoryCache cache,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _cache = cache;
            _hubContext = hubContext;
        }

        public async Task CreateTaskNotificationAsync(string userId, List<TaskViewModel> newTasks)
        {
            foreach (var task in newTasks)
            {
                var notification = new NotificationViewModel
                {
                    UserId = userId,
                    Title = "تسک جدید دریافت شده",
                    Message = $"تسک '{task.Title}' به شما واگذار شده است",
                    Type = NotificationType.NewTask,
                    // ⭐ اصلاح: استفاده از enum جدید
                    Priority = task.Important ? NotificationPriority.High : NotificationPriority.Normal,
                    RelatedId = task.Id,
                    ActionUrl = $"/AdminArea/Tasks/Details/{task.Id}",
                    CreateTime = DateTime.Now,
                    IsRead = false
                };

                await SaveNotificationAsync(notification);
                await SendRealTimeNotificationAsync(userId, notification);
            }
        }

        public async Task CreateReminderNotificationAsync(string userId, List<TaskReminderItemViewModel> newReminders)
        {
            foreach (var reminder in newReminders)
            {
                var notification = new NotificationViewModel
                {
                    UserId = userId,
                    Title = "یادآوری جدید",
                    Message = reminder.Title,
                    Type = NotificationType.Reminder,
                    // ⭐ اصلاح: تطبیق با enum جدید
                    Priority = reminder.Priority >= 4 ? NotificationPriority.Urgent :
                              reminder.Priority >= 2 ? NotificationPriority.High : NotificationPriority.Normal,
                    RelatedId = reminder.Id,
                    ActionUrl = reminder.TaskId.HasValue ? $"/AdminArea/Tasks/Details/{reminder.TaskId}" : "/AdminArea/Tasks/TaskReminders",
                    CreateTime = DateTime.Now,
                    IsRead = false
                };

                await SaveNotificationAsync(notification);
                await SendRealTimeNotificationAsync(userId, notification);
            }
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 10)
        {
            try
            {
                // چک کردن کش
                var cacheKey = $"user_notifications_{userId}";
                if (_cache.TryGetValue(cacheKey, out List<NotificationViewModel> cachedNotifications))
                {
                    return cachedNotifications.Take(count).ToList();
                }

                // ⭐ دریافت مستقیم از دیتابیس با Entity Framework
                var coreNotifications = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && n.IsActive)
                    .OrderByDescending(n => n.CreateDate)
                    .Take(count * 2) // بیشتر بگیریم برای کش
                    .Include(n => n.Details) // بارگذاری جزئیات
                    .ToListAsync();

                var notifications = new List<NotificationViewModel>();

                foreach (var coreNotification in coreNotifications)
                {
                    var notificationViewModel = new NotificationViewModel
                    {
                        Id = coreNotification.Id,
                        UserId = coreNotification.RecipientUserId,
                        Title = coreNotification.Title,
                        Message = coreNotification.Message,
                        Type = (NotificationType)coreNotification.NotificationTypeGeneral,
                        Priority = (NotificationPriority)coreNotification.Priority,
                        // ⭐ اصلاح: RelatedRecordId string است، پس باید تبدیل کنیم
                        RelatedId = !string.IsNullOrEmpty(coreNotification.RelatedRecordId) &&
                                   int.TryParse(coreNotification.RelatedRecordId, out int relatedId) ? relatedId : null,
                        ActionUrl = coreNotification.ActionUrl,
                        CreateTime = coreNotification.CreateDate,
                        IsRead = coreNotification.IsRead
                    };

                    notifications.Add(notificationViewModel);
                }

                // ذخیره در کش برای 5 دقیقه
                _cache.Set(cacheKey, notifications, TimeSpan.FromMinutes(5));

                return notifications.Take(count).ToList();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in GetUserNotificationsAsync: {ex.Message}");
                return new List<NotificationViewModel>();
            }
        }

        public async Task<int> GetUnreadNotificationsCountAsync(string userId)
        {
            try
            {
                // چک کردن کش
                var cacheKey = $"unread_count_{userId}";
                if (_cache.TryGetValue(cacheKey, out int cachedCount))
                {
                    return cachedCount;
                }

                // ⭐ شمارش مستقیم از دیتابیس
                var unreadCount = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && !n.IsRead && n.IsActive)
                    .CountAsync();

                // ذخیره در کش برای 2 دقیقه
                _cache.Set(cacheKey, unreadCount, TimeSpan.FromMinutes(2));

                return unreadCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadNotificationsCountAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            try
            {
                // ⭐ پیدا کردن نوتیفیکیشن مستقیم از دیتابیس
                var notification = await _context.CoreNotification_Tbl
                    .Where(n => n.Id == notificationId && n.RecipientUserId == userId)
                    .FirstOrDefaultAsync();

                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;

                    // ⭐ بروزرسانی مستقیم
                    _context.CoreNotification_Tbl.Update(notification);
                    await _context.SaveChangesAsync();

                    // پاک کردن کش
                    InvalidateUserNotificationCache(userId);

                    // اطلاع‌رسانی real-time
                    await _hubContext.Clients.User(userId)
                        .SendAsync("NotificationMarkedAsRead", notificationId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkNotificationAsReadAsync: {ex.Message}");
                throw;
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                // ⭐ پیدا کردن همه نوتیفیکیشن‌های خوانده نشده
                var unreadNotifications = await _context.CoreNotification_Tbl
                    .Where(n => n.RecipientUserId == userId && !n.IsRead && n.IsActive)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                }

                // ⭐ ذخیره تغییرات
                if (unreadNotifications.Any())
                {
                    _context.CoreNotification_Tbl.UpdateRange(unreadNotifications);
                    await _context.SaveChangesAsync();
                }

                // پاک کردن کش
                InvalidateUserNotificationCache(userId);

                // اطلاع‌رسانی real-time
                await _hubContext.Clients.User(userId)
                    .SendAsync("AllNotificationsMarkedAsRead");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkAllNotificationsAsReadAsync: {ex.Message}");
                throw;
            }
        }

        private async Task SaveNotificationAsync(NotificationViewModel notification)
        {
            try
            {
                // ⭐ ایجاد CoreNotification مستقیم با تمام فیلدهای مطلوب
                var coreNotification = new CoreNotification
                {
                    SystemId = 7, // 7 = سیستم مدیریت پروژه و تسک‌ها
                    SystemName = "TaskManagement",
                    RecipientUserId = notification.UserId,
                    SenderUserId = null, // فعلاً null، بعداً می‌توان اضافه کرد
                    NotificationTypeGeneral = (byte)notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreateDate = notification.CreateTime,
                    IsRead = notification.IsRead,
                    IsClicked = false,
                    Priority = (byte)notification.Priority,
                    ActionUrl = notification.ActionUrl,
                    // ⭐ اصلاح: RelatedRecordId string است
                    RelatedRecordId = notification.RelatedId?.ToString(),
                    RelatedRecordType = notification.Type == NotificationType.NewTask ? "Task" : "TaskReminder",
                    RelatedRecordTitle = notification.Title,
                    IsActive = true,
                    BranchId = null // می‌توان بعداً اضافه کرد
                };

                // ⭐ اضافه کردن به دیتابیس
                _context.CoreNotification_Tbl.Add(coreNotification);
                await _context.SaveChangesAsync();

                // ⭐ ایجاد CoreNotificationDetail
                var detail = new CoreNotificationDetail
                {
                    CoreNotificationId = coreNotification.Id,
                    NotificationTypeSpecific = (byte)notification.Type,
                    FieldName = null,
                    OldValue = null,
                    NewValue = null,
                    AdditionalData = notification.Type == NotificationType.NewTask
                        ? $"{{\"TaskId\":{notification.RelatedId},\"TaskTitle\":\"{notification.Title.Replace("\"", "\\\"")}\"}}"
                        : $"{{\"ReminderId\":{notification.RelatedId}}}",
                    Description = $"نوتیفیکیشن از سیستم مدیریت تسک‌ها",
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.CoreNotificationDetail_Tbl.Add(detail);
                await _context.SaveChangesAsync();

                // تنظیم Id برای notification
                notification.Id = coreNotification.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveNotificationAsync: {ex.Message}");
                throw;
            }
        }

        private async Task SendRealTimeNotificationAsync(string userId, NotificationViewModel notification)
        {
            try
            {
                // ارسال real-time به کاربر از طریق SignalR
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);

                // بروزرسانی کش
                InvalidateUserNotificationCache(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendRealTimeNotificationAsync: {ex.Message}");
            }
        }

        private void InvalidateUserNotificationCache(string userId)
        {
            _cache.Remove($"user_notifications_{userId}");
            _cache.Remove($"unread_count_{userId}");
        }
    }

    public enum NotificationType
    {
        NewTask = 1,
        Reminder = 6, // ⭐ اصلاح: 6 = یادآوری در CoreNotification
        TaskUpdate = 2,
        System = 0
    }

    public enum NotificationPriority
    {
        Normal = 0,   // عادی - تطابق با CoreNotification
        High = 1,     // مهم - تطابق با CoreNotification  
        Urgent = 2,   // فوری - تطابق با CoreNotification
        Critical = 3  // بحرانی - اضافه شده برای تطابق کامل
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public int? RelatedId { get; set; }
        public string ActionUrl { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsRead { get; set; }
        public string Icon => Type switch
        {
            NotificationType.NewTask => "fa-tasks",
            NotificationType.Reminder => "fa-bell",
            NotificationType.TaskUpdate => "fa-edit",
            _ => "fa-info-circle"
        };
        public string PriorityClass => Priority switch
        {
            NotificationPriority.Normal => "",
            NotificationPriority.High => "priority-high",
            NotificationPriority.Urgent => "priority-urgent",
            _ => ""
        };
    }
}