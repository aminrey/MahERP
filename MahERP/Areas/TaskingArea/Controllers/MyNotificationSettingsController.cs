using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Notifications;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.TaskingArea.Controllers
{
    [Area("TaskingArea")]
    [Authorize]
    public class MyNotificationSettingsController : BaseController
    {
        private readonly INotificationSettingsRepository _notificationRepo;

        public MyNotificationSettingsController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            ModuleTrackingBackgroundService moduleTracking,
            INotificationSettingsRepository notificationRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking)
        {
            _notificationRepo = notificationRepo;
        }

        /// <summary>
        /// صفحه تنظیمات شخصی اعلان‌های کاربر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                var viewModel = await _notificationRepo.GetUserSettingsViewModelAsync(userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "MyNotificationSettings",
                    "Index",
                    "مشاهده تنظیمات شخصی اعلان‌ها");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "MyNotificationSettings",
                    "Index",
                    "خطا در بارگذاری تنظیمات شخصی",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری تنظیمات";
                return RedirectToAction("TaskDashboard", "Tasks");
            }
        }

        /// <summary>
        /// ذخیره تنظیمات شخصی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePreferences(UserNotificationSettingsViewModel model)
        {
            try
            {
                var userId = GetUserId();

                if (model.UserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "دسترسی غیرمجاز" } }
                    });
                }

                foreach (var pref in model.Preferences)
                {
                    var preference = await _notificationRepo.GetUserPreferenceAsync(userId, pref.TypeId);

                    if (preference != null)
                    {
                        preference.IsEnabled = pref.IsEnabled;
                        preference.ReceiveBySystem = pref.ReceiveBySystem;
                        preference.ReceiveByEmail = pref.ReceiveByEmail;
                        preference.ReceiveBySms = pref.ReceiveBySms;
                        preference.ReceiveByTelegram = pref.ReceiveByTelegram;
                        preference.DeliveryMode = pref.DeliveryMode;
                        
                        // Parse time strings
                        if (!string.IsNullOrEmpty(pref.PreferredDeliveryTime))
                        {
                            if (TimeSpan.TryParse(pref.PreferredDeliveryTime, out var deliveryTime))
                            {
                                preference.PreferredDeliveryTime = deliveryTime;
                            }
                        }

                        if (!string.IsNullOrEmpty(pref.QuietHoursStart))
                        {
                            if (TimeSpan.TryParse(pref.QuietHoursStart, out var startTime))
                            {
                                preference.QuietHoursStart = startTime;
                            }
                        }

                        if (!string.IsNullOrEmpty(pref.QuietHoursEnd))
                        {
                            if (TimeSpan.TryParse(pref.QuietHoursEnd, out var endTime))
                            {
                                preference.QuietHoursEnd = endTime;
                            }
                        }

                        await _notificationRepo.UpdateUserPreferenceAsync(preference);
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "MyNotificationSettings",
                    "SavePreferences",
                    "بروزرسانی تنظیمات شخصی اعلان‌ها");

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تنظیمات با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "MyNotificationSettings",
                    "SavePreferences",
                    "خطا در ذخیره تنظیمات",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره تنظیمات" } }
                });
            }
        }

        /// <summary>
        /// بازنشانی به تنظیمات پیش‌فرض
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDefault()
        {
            try
            {
                var userId = GetUserId();

                // حذف تنظیمات موجود
                var preferences = await _notificationRepo.GetUserPreferencesAsync(userId);
                foreach (var pref in preferences)
                {
                    // Set to default values
                    pref.IsEnabled = true;
                    pref.ReceiveBySystem = true;
                    pref.ReceiveByEmail = pref.NotificationTypeConfig.SupportsEmail;
                    pref.ReceiveBySms = false;
                    pref.ReceiveByTelegram = pref.NotificationTypeConfig.SupportsTelegram;
                    pref.DeliveryMode = 0;
                    pref.PreferredDeliveryTime = null;
                    pref.QuietHoursStart = null;
                    pref.QuietHoursEnd = null;

                    await _notificationRepo.UpdateUserPreferenceAsync(pref);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "MyNotificationSettings",
                    "ResetToDefault",
                    "بازنشانی تنظیمات به حالت پیش‌فرض");

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تنظیمات به حالت پیش‌فرض برگشت" } },
                    reload = true
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "MyNotificationSettings",
                    "ResetToDefault",
                    "خطا در بازنشانی تنظیمات",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بازنشانی تنظیمات" } }
                });
            }
        }
    }
}