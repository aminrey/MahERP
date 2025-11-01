using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AppCoreArea.Controllers.CoreControllers
{
    /// <summary>
    /// کنترلر مدیریت نوتیفیکیشن‌های کاربران
    /// برای نمایش و مدیریت نوتیفیکیشن‌های دریافتی کاربر
    /// </summary>
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.NOTIFICATION")]

    public class NotificationController : BaseController
    {
        private readonly ICoreNotificationRepository _coreNotificationRepository;
        private readonly TaskNotificationService _taskNotificationService;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;


        public NotificationController(
            IUnitOfWork uow,
            ICoreNotificationRepository coreNotificationRepository,
            TaskNotificationService taskNotificationService,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository , BaseRepository)
        {
            _coreNotificationRepository = coreNotificationRepository;
            _taskNotificationService = taskNotificationService;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        /// <summary>
        /// صفحه اصلی نوتیفیکیشن‌های کاربر
        /// </summary>
        /// <param name="systemId">شناسه سیستم برای فیلتر (اختیاری)</param>
        /// <param name="unreadOnly">فقط خوانده نشده‌ها</param>
        /// <param name="page">شماره صفحه</param>
        /// <returns>صفحه لیست نوتیفیکیشن‌ها</returns>
        public async Task<IActionResult> Index(byte? systemId = null, bool unreadOnly = false, int page = 1)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // دریافت نوتیفیکیشن‌های کاربر
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(
                    userId, systemId, unreadOnly, page, 20);

                // تنظیم اطلاعات فیلتر برای نمایش در View
                ViewBag.SystemId = systemId;
                ViewBag.UnreadOnly = unreadOnly;
                ViewBag.CurrentPage = page;

                // لیست سیستم‌ها برای فیلتر
                ViewBag.Systems = new[]
                {
                    new { Id = (byte)1, Name = "سیستم مدیریت مالی" },
                    new { Id = (byte)2, Name = "سیستم منابع انسانی" },
                    new { Id = (byte)3, Name = "سیستم فروش و CRM" },
                    new { Id = (byte)4, Name = "سیستم خرید و تدارکات" },
                    new { Id = (byte)5, Name = "سیستم انبار و لجستیک" },
                    new { Id = (byte)6, Name = "سیستم تولید و کنترل کیفیت" },
                    new { Id = (byte)7, Name = "سیستم مدیریت پروژه و تسک‌ها" }
                };

                // ثبت لاگ مشاهده
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Core",
                    "NotificationIndex",
                    $"مشاهده لیست نوتیفیکیشن‌ها - سیستم: {systemId?.ToString() ?? "همه"}, خوانده نشده: {unreadOnly}"
                );

                return View(notifications);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "NotificationIndex",
                    "خطا در دریافت لیست نوتیفیکیشن‌ها",
                    ex
                );
                
                return View(new CoreNotificationListViewModel());
            }
        }

        /// <summary>
        /// نمایش جزئیات یک نوتیفیکیشن
        /// </summary>
        /// <param name="id">شناسه نوتیفیکیشن</param>
        /// <returns>صفحه جزئیات نوتیفیکیشن</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // دریافت جزئیات نوتیفیکیشن
                var notification = await _coreNotificationRepository.GetNotificationByIdAsync(id);
                
                if (notification == null || notification.RecipientUserId != userId)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Core",
                        "NotificationDetails",
                        "تلاش برای مشاهده نوتیفیکیشن غیرموجود یا غیرمجاز",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("Index");
                }

                // علامت‌گذاری به عنوان کلیک شده
                await _coreNotificationRepository.MarkAsClickedAsync(id, userId);

                // ثبت لاگ مشاهده
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Core",
                    "NotificationDetails",
                    $"مشاهده جزئیات نوتیفیکیشن: {notification.Title}",
                    recordId: id.ToString(),
                    entityType: "CoreNotification",
                    recordTitle: notification.Title
                );

                return View(notification);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "NotificationDetails",
                    "خطا در دریافت جزئیات نوتیفیکیشن",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن به عنوان خوانده شده - AJAX
        /// </summary>
        /// <param name="id">شناسه نوتیفیکیشن</param>
        /// <returns>نتیجه JSON</returns>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _coreNotificationRepository.MarkAsReadAsync(id, userId);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Core",
                        "MarkNotificationAsRead",
                        "علامت‌گذاری نوتیفیکیشن به عنوان خوانده شده",
                        recordId: id.ToString()
                    );

                    return Json(new { success = true, message = "نوتیفیکیشن به عنوان خوانده شده علامت‌گذاری شد" });
                }
                else
                {
                    return Json(new { success = false, message = "نوتیفیکیشن یافت نشد" });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "MarkNotificationAsRead",
                    "خطا در علامت‌گذاری نوتیفیکیشن",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new { success = false, message = "خطا در علامت‌گذاری نوتیفیکیشن" });
            }
        }

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌ها به عنوان خوانده شده - AJAX
        /// </summary>
        /// <param name="systemId">شناسه سیستم (اختیاری)</param>
        /// <returns>نتیجه JSON</returns>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead(byte? systemId = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var count = await _coreNotificationRepository.MarkAllAsReadAsync(userId, systemId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Core",
                    "MarkAllNotificationsAsRead",
                    $"علامت‌گذاری همه نوتیفیکیشن‌ها به عنوان خوانده شده - تعداد: {count}, سیستم: {systemId?.ToString() ?? "همه"}"
                );

                return Json(new { 
                    success = true, 
                    message = $"{count} نوتیفیکیشن به عنوان خوانده شده علامت‌گذاری شد",
                    count = count
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "MarkAllNotificationsAsRead",
                    "خطا در علامت‌گذاری همه نوتیفیکیشن‌ها",
                    ex
                );

                return Json(new { success = false, message = "خطا در علامت‌گذاری نوتیفیکیشن‌ها" });
            }
        }

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده - AJAX
        /// </summary>
        /// <param name="systemId">شناسه سیستم (اختیاری)</param>
        /// <returns>تعداد نوتیفیکیشن‌های خوانده نشده</returns>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount(byte? systemId = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var count = await _coreNotificationRepository.GetUnreadNotificationCountAsync(userId, systemId);

                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "GetUnreadNotificationCount",
                    "خطا در دریافت تعداد نوتیفیکیشن‌های خوانده نشده",
                    ex
                );

                return Json(new { success = false, count = 0 });
            }
        }

        /// <summary>
        /// دریافت آخرین نوتیفیکیشن‌ها برای نمایش در Bell Icon - AJAX
        /// </summary>
        /// <param name="count">تعداد نوتیفیکیشن‌های مورد نیاز</param>
        /// <returns>لیست آخرین نوتیفیکیشن‌ها</returns>
        [HttpGet]
        public async Task<IActionResult> GetLatestNotifications(int count = 5)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // دریافت آخرین نوتیفیکیشن‌های کاربر
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(
                    userId, null, false, 1, count);

                var result = notifications.Notifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.ShortText,
                    systemName = n.SystemName,
                    createTime = n.RelativeTime,
                    isRead = n.IsRead,
                    actionUrl = n.ActionUrl,
                    icon = n.Icon,
                    cssClass = n.CssClass,
                    priorityClass = n.PriorityClass
                }).ToList();

                return Json(new { 
                    success = true, 
                    notifications = result,
                    unreadCount = notifications.UnreadCount
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "GetLatestNotifications",
                    "خطا در دریافت آخرین نوتیفیکیشن‌ها",
                    ex
                );

                return Json(new { success = false, notifications = new List<object>(), unreadCount = 0 });
            }
        }

        /// <summary>
        /// هدایت به رکورد مرتبط با نوتیفیکشن
        /// </summary>
        /// <param name="id">شناسه نوتیفیکشن</param>
        /// <returns>هدایت به رکورد مرتبط</returns>
        public async Task<IActionResult> GoToRelated(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // دریافت اطلاعات نوتیفیکیشن
                var notification = await _coreNotificationRepository.GetNotificationByIdAsync(id);
                
                if (notification == null || notification.RecipientUserId != userId)
                {
                    return RedirectToAction("Index");
                }

                // علامت‌گذاری به عنوان کلیک شده
                await _coreNotificationRepository.MarkAsClickedAsync(id, userId);

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Core",
                    "GoToRelatedRecord",
                    $"هدایت به رکورد مرتبط با نوتیفیکیشن: {notification.Title}",
                    recordId: id.ToString(),
                    entityType: "CoreNotification",
                    recordTitle: notification.Title
                );

                // هدایت به URL مشخص شده یا صفحه پیش‌فرض
                if (!string.IsNullOrEmpty(notification.ActionUrl))
                {
                    return Redirect(notification.ActionUrl);
                }
                else
                {
                    // هدایت بر اساس نوع سیستم و رکورد
                    return notification.SystemId switch
                    {
                        7 when notification.RelatedRecordType == "Task" => 
                            RedirectToAction("Details", "Tasks", new { area = "AdminArea", id = notification.RelatedRecordId }),
                        3 when notification.RelatedRecordType == "CRM" => 
                            RedirectToAction("Details", "CRM", new { area = "AdminArea", id = notification.RelatedRecordId }),
                        _ => RedirectToAction("Details", new { id = id })
                    };
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "GoToRelatedRecord",
                    "خطا در هدایت به رکورد مرتبط",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// صفحه آمار نوتیفیکیشن‌های کاربر
        /// </summary>
        /// <param name="fromDate">تاریخ شروع (شمسی)</param>
        /// <param name="toDate">تاریخ پایان (شمسی)</param>
        /// <returns>صفحه آمار نوتیفیکیشن‌ها</returns>
        public async Task<IActionResult> Statistics(string fromDate = null, string toDate = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // تبدیل تاریخ‌های شمسی به میلادی
                DateTime? fromDateTime = null;
                DateTime? toDateTime = null;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    fromDateTime = ConvertDateTime.ConvertShamsiToMiladi(fromDate);
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    toDateTime = ConvertDateTime.ConvertShamsiToMiladi(toDate);
                }

                // دریافت آمار اصلی
                var stats = await _coreNotificationRepository.GetUserNotificationStatsAsync(userId, fromDateTime, toDateTime);

                // دریافت آمار تفصیلی
                var systemStats = await GetNotificationsBySystemAsync(userId, fromDateTime, toDateTime);
                var priorityStats = await GetNotificationsByPriorityAsync(userId, fromDateTime, toDateTime);
                var typeStats = await GetNotificationsByTypeAsync(userId, fromDateTime, toDateTime);
                var dailyActivity = await GetDailyActivityAsync(userId, fromDateTime, toDateTime);

                // تکمیل آمار
                var viewModel = new CoreNotificationStatsViewModel
                {
                    TotalNotifications = stats.TotalNotifications,
                    ReadNotifications = stats.ReadNotifications,
                    UnreadNotifications = stats.UnreadNotifications,
                    ClickedNotifications = stats.ClickedNotifications,
                    NotificationsBySystem = systemStats,
                    NotificationsByPriority = priorityStats,
                    NotificationsByType = typeStats,
                    DailyActivity = dailyActivity
                };

                // تنظیم اطلاعات فیلتر برای نمایش در View
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                // ثبت لاگ مشاهده آمار
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Core",
                    "NotificationStatistics",
                    $"مشاهده آمار نوتیفیکیشن‌ها - از تاریخ: {fromDate ?? "ابتدا"} تا {toDate ?? "انتها"}"
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "NotificationStatistics",
                    "خطا در دریافت آمار نوتیفیکیشن‌ها",
                    ex
                );
                
                return View(new CoreNotificationStatsViewModel());
            }
        }

        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌ها بر اساس سیستم
        /// </summary>
        private async Task<List<SystemNotificationStat>> GetNotificationsBySystemAsync(string userId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // استفاده مستقیم از repository
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(userId, null, false, 1, int.MaxValue);
                
                var filteredNotifications = notifications.Notifications.AsQueryable();

                if (fromDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) >= fromDate.Value);

                if (toDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) <= toDate.Value);

                var systemStats = filteredNotifications
                    .GroupBy(n => new { n.SystemId, n.SystemName })
                    .Select(g => new SystemNotificationStat
                    {
                        SystemId = g.Key.SystemId,
                        SystemName = g.Key.SystemName,
                        Count = g.Count(),
                        UnreadCount = g.Count(n => !n.IsRead)
                    })
                    .ToList();

                return systemStats;
            }
            catch
            {
                return new List<SystemNotificationStat>();
            }
        }

        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌ها بر اساس اولویت
        /// </summary>
        private async Task<List<PriorityNotificationStat>> GetNotificationsByPriorityAsync(string userId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // استفاده مستقیم از repository
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(userId, null, false, 1, int.MaxValue);
                
                var filteredNotifications = notifications.Notifications.AsQueryable();

                if (fromDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) >= fromDate.Value);

                if (toDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) <= toDate.Value);

                var priorityStats = filteredNotifications
                    .GroupBy(n => n.Priority)
                    .Select(g => new PriorityNotificationStat
                    {
                        Priority = g.Key,
                        PriorityName = GetPriorityName(g.Key),
                        Count = g.Count(),
                        UnreadCount = g.Count(n => !n.IsRead)
                    })
                    .ToList();

                return priorityStats;
            }
            catch
            {
                return new List<PriorityNotificationStat>();
            }
        }

        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌ها بر اساس نوع
        /// </summary>
        private async Task<List<TypeNotificationStat>> GetNotificationsByTypeAsync(string userId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // استفاده مستقیم از repository
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(userId, null, false, 1, int.MaxValue);
                
                var filteredNotifications = notifications.Notifications.AsQueryable();

                if (fromDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) >= fromDate.Value);

                if (toDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => DateTime.Parse(n.CreateDatePersian) <= toDate.Value);

                var typeStats = filteredNotifications
                    .GroupBy(n => n.NotificationTypeGeneral)
                    .Select(g => new TypeNotificationStat
                    {
                        NotificationType = g.Key,
                        TypeName = GetNotificationTypeName(g.Key),
                        Count = g.Count(),
                        ReadCount = g.Count(n => n.IsRead),
                        UnreadCount = g.Count(n => !n.IsRead),
                        LastNotificationDate = g.Max(n => (DateTime?)n.CreateDate)
                    })
                    .ToList();

                return typeStats;
            }
            catch
            {
                return new List<TypeNotificationStat>();
            }
        }

        /// <summary>
        /// دریافت فعالیت روزانه نوتیفیکیشن‌ها
        /// </summary>
        private async Task<List<DailyNotificationActivity>> GetDailyActivityAsync(string userId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // استفاده مستقیم از repository
                var notifications = await _coreNotificationRepository.GetUserNotificationsAsync(userId, null, false, 1, int.MaxValue);
                
                var filteredNotifications = notifications.Notifications.AsQueryable();

                // اگر بازه زمانی مشخص نشده، 30 روز گذشته را نمایش دهیم
                if (!fromDate.HasValue && !toDate.HasValue)
                {
                    fromDate = DateTime.Now.AddDays(-30);
                    toDate = DateTime.Now;
                }

                if (fromDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => n.CreateDate >= fromDate.Value);

                if (toDate.HasValue)
                    filteredNotifications = filteredNotifications.Where(n => n.CreateDate <= toDate.Value);

                var dailyActivity = filteredNotifications
                    .GroupBy(n => n.CreateDate.Date)
                    .Select(g => new DailyNotificationActivity
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return dailyActivity;
            }
            catch
            {
                return new List<DailyNotificationActivity>();
            }
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
        /// صفحه تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        /// <returns>صفحه تنظیمات</returns>
        public async Task<IActionResult> Settings()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // دریافت تنظیمات نوتیفیکیشن برای تمام سیستم‌ها
                var settings = new List<CoreNotificationSettingViewModel>();
                
                for (byte systemId = 1; systemId <= 7; systemId++)
                {
                    var setting = await _coreNotificationRepository.GetUserNotificationSettingsAsync(userId, systemId);
                    settings.Add(setting);
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Core",
                    "NotificationSettings",
                    "مشاهده تنظیمات نوتیفیکیشن"
                );

                return View(settings);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "NotificationSettings",
                    "خطا در دریافت تنظیمات نوتیفیکیشن",
                    ex
                );
                
                return View(new List<CoreNotificationSettingViewModel>());
            }
        }

        /// <summary>
        /// ذخیره تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        /// <param name="settings">لیست تنظیمات</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(List<CoreNotificationSettingViewModel> settings)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                foreach (var setting in settings)
                {
                    setting.UserId = userId;
                    
                    var entity = _mapper.Map<CoreNotificationSetting>(setting);
                    await _coreNotificationRepository.UpdateUserNotificationSettingsAsync(entity);
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Core",
                    "SaveNotificationSettings",
                    "ذخیره تنظیمات نوتیفیکیشن"
                );

                TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Core",
                    "SaveNotificationSettings",
                    "خطا در ذخیره تنظیمات نوتیفیکیشن",
                    ex
                );
                
                TempData["ErrorMessage"] = "خطا در ذخیره تنظیمات";
                return RedirectToAction("Settings");
            }
        }
    }
}