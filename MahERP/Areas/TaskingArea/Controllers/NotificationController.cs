using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.TaskingArea.Controllers
{
    [Area("TaskingArea")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly NotificationManagementService _notificationService;
        private readonly ILogger<NotificationController> _logger; // ⭐⭐⭐ اضافه کردن فیلد

        public NotificationController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            ModuleTrackingBackgroundService moduleTracking,
            NotificationManagementService notificationService,
            ILogger<NotificationController> logger,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// صفحه لیست اعلان‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(byte? systemId = null, bool unreadOnly = false, int page = 1)
        {
            try
            {
                var userId = GetUserId();
                
                var model = await _notificationService.GetUserNotificationsAsync(
                    userId,
                    systemId,
                    unreadOnly,
                    page,
                    pageSize: 20
                );

                // ⭐ علامت‌گذاری همه به عنوان دیده شده (View شد)
                await _notificationService.MarkAllAsReadAsync(userId, systemId);

                ViewBag.SystemId = systemId;
                ViewBag.UnreadOnly = unreadOnly;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Notifications",
                    "Index",
                    "مشاهده لیست اعلان‌ها");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Notifications", "Index", "خطا در نمایش اعلان‌ها", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری اعلان‌ها";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// دریافت اعلان‌های Header (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHeaderNotifications()
        {
            try
            {
                var userId = GetUserId();
                
                var model = await _notificationService.GetUserNotificationsAsync(
                    userId,
                    systemId: 7, // فقط تسکینگ
                    unreadOnly: true,
                    pageNumber: 1,
                    pageSize: 10
                );

                return PartialView("_HeaderNotificationsList", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اعلان‌های هدر");
                return PartialView("_HeaderNotificationsList", null);
            }
        }

        /// <summary>
        /// دریافت تعداد اعلان‌های خوانده نشده (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId, systemId: 7);
                
                return Json(new { success = true, count });
            }
            catch
            {
                return Json(new { success = false, count = 0 });
            }
        }

        /// <summary>
        /// علامت‌گذاری یک اعلان به عنوان خوانده شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.MarkAsReadAsync(id, userId);
                
                return Json(new { success = result });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// علامت‌گذاری همه اعلان‌ها به عنوان خوانده شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                var count = await _notificationService.MarkAllAsReadAsync(userId, systemId: 7);
                
                return Json(new { success = true, count });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان کلیک شده و Redirect
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Click(int id)
        {
            try
            {
                var userId = GetUserId();
                var notification = await _notificationService.GetNotificationByIdAsync(id);

                if (notification == null || notification.RecipientUserId != userId)
                {
                    return RedirectToAction("Index");
                }

                // علامت‌گذاری به عنوان کلیک شده
                await _notificationService.MarkAsClickedAsync(id, userId);

                // Redirect به صفحه مربوطه
                if (!string.IsNullOrEmpty(notification.ActionUrl))
                {
                    return Redirect(notification.ActionUrl);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Notifications", "Click", "خطا در کلیک اعلان", ex);
                return RedirectToAction("Index");
            }
        }
    }
}