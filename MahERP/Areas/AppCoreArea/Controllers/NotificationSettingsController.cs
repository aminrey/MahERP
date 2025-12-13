using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Notifications;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("NOTIFICATION.SETTINGS")]
    public class NotificationSettingsController : BaseController
    {
        private readonly INotificationSettingsRepository _notificationRepo;

        public NotificationSettingsController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            INotificationSettingsRepository notificationRepo,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _notificationRepo = notificationRepo;
        }

        #region تنظیمات کلی

        /// <summary>
        /// صفحه اصلی تنظیمات اعلان‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _notificationRepo.GetSettingsViewModelAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationSettings",
                    "Index",
                    "مشاهده تنظیمات اعلان‌ها");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "Index",
                    "خطا در بارگذاری تنظیمات",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری تنظیمات";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// ذخیره تنظیمات کلی سیستم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGlobalSettings(NotificationSettingsViewModel model)
        {
            try
            {
                // TODO: ذخیره در جدول تنظیمات کلی
                // فعلاً فقط پیام موفقیت

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationSettings",
                    "SaveGlobalSettings",
                    "بروزرسانی تنظیمات کلی سیستم اعلان");

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تنظیمات با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "SaveGlobalSettings",
                    "خطا در ذخیره تنظیمات کلی",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره تنظیمات" } }
                });
            }
        }

        #endregion

        #region مدیریت انواع اعلان

        /// <summary>
        /// فعال/غیرفعال کردن نوع اعلان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNotificationType(int typeId, bool isActive)
        {
            try
            {
                var result = await _notificationRepo.ToggleNotificationTypeAsync(typeId, isActive);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "نوع اعلان یافت نشد" } }
                    });
                }

                var type = await _notificationRepo.GetNotificationTypeByIdAsync(typeId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationSettings",
                    "ToggleNotificationType",
                    $"{(isActive ? "فعال" : "غیرفعال")} کردن اعلان: {type.TypeNameFa}",
                    recordId: typeId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = $"اعلان با موفقیت {(isActive ? "فعال" : "غیرفعال")} شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "ToggleNotificationType",
                    "خطا در تغییر وضعیت اعلان",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تغییر وضعیت" } }
                });
            }
        }

        /// <summary>
        /// تغییر وضعیت کانال برای نوع اعلان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleChannel(int typeId, string channel, bool isActive)
        {
            try
            {
                var type = await _notificationRepo.GetNotificationTypeByIdAsync(typeId);

                if (type == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "نوع اعلان یافت نشد" } }
                    });
                }

                // بروزرسانی کانال
                switch (channel.ToLower())
                {
                    case "email":
                        type.SupportsEmail = isActive;
                        break;
                    case "sms":
                        type.SupportsSms = isActive;
                        break;
                    case "telegram":
                        type.SupportsTelegram = isActive;
                        break;
                    default:
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "error", text = "کانال نامعتبر" } }
                        });
                }

                await _notificationRepo.UpdateNotificationTypeConfigAsync(type);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationSettings",
                    "ToggleChannel",
                    $"تغییر وضعیت کانال {channel} برای {type.TypeNameFa}",
                    recordId: typeId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تنظیمات کانال با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "ToggleChannel",
                    "خطا در تغییر وضعیت کانال",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تغییر وضعیت کانال" } }
                });
            }
        }

        #endregion

        #region لیست سیاه

        /// <summary>
        /// مدیریت لیست سیاه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Blacklist()
        {
            try
            {
                var blacklist = await _notificationRepo.GetBlacklistAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationSettings",
                    "Blacklist",
                    "مشاهده لیست سیاه اعلان‌ها");

                return View(blacklist);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "Blacklist",
                    "خطا در بارگذاری لیست سیاه",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری لیست سیاه";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// افزودن به لیست سیاه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToBlacklist(string userId, int? typeId, string reason)
        {
            try
            {
                var currentUserId = GetUserId();

                var blacklist = new NotificationBlacklist
                {
                    UserId = userId,
                    NotificationTypeConfigId = typeId,
                    Reason = reason,
                    IsActive = true,
                    CreatedByUserId = currentUserId
                };

                var result = await _notificationRepo.AddToBlacklistAsync(blacklist);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در افزودن به لیست سیاه" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "NotificationSettings",
                    "AddToBlacklist",
                    $"افزودن کاربر به لیست سیاه: {userId}");

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "کاربر با موفقیت به لیست سیاه اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "AddToBlacklist",
                    "خطا در افزودن به لیست سیاه",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن به لیست سیاه" } }
                });
            }
        }

        /// <summary>
        /// حذف از لیست سیاه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBlacklist(int blacklistId)
        {
            try
            {
                var result = await _notificationRepo.RemoveFromBlacklistAsync(blacklistId);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "رکورد یافت نشد" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "NotificationSettings",
                    "RemoveFromBlacklist",
                    "حذف از لیست سیاه",
                    recordId: blacklistId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "با موفقیت از لیست سیاه حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "RemoveFromBlacklist",
                    "خطا در حذف از لیست سیاه",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف از لیست سیاه" } }
                });
            }
        }

        #endregion

        #region API برای Modal لیست سیاه

        /// <summary>
        /// دریافت لیست کاربران برای Modal لیست سیاه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsersForBlacklist()
        {
            try
            {
                var users = _uow.UserManagerUW
                    .Get(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.FirstName + " " + u.LastName,
                        email = u.Email,
                        userName = u.UserName
                    })
                    .ToList();

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "GetUsersForBlacklist",
                    "خطا در دریافت لیست کاربران",
                    ex);

                return Json(new { success = false, message = "خطا در بارگذاری لیست کاربران" });
            }
        }

        /// <summary>
        /// دریافت انواع اعلان برای Modal لیست سیاه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotificationTypesForBlacklist()
        {
            try
            {
                var types = await _notificationRepo.GetAllNotificationTypesAsync();

                var result = types
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.ModuleConfig.ModuleNameFa)
                    .ThenBy(t => t.TypeNameFa)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.TypeNameFa,
                        module = t.ModuleConfig.ModuleNameFa,
                        displayName = $"{t.ModuleConfig.ModuleNameFa} - {t.TypeNameFa}"
                    })
                    .ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "GetNotificationTypesForBlacklist",
                    "خطا در دریافت انواع اعلان",
                    ex);

                return Json(new { success = false, message = "خطا در بارگذاری انواع اعلان" });
            }
        }

        #endregion

        #region تست ارسال

        /// <summary>
        /// صفحه تست ارسال اعلان
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestNotification()
        {
            try
            {
                var types = await _notificationRepo.GetModuleNotificationTypesAsync(1); // ماژول تسکینگ

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationSettings",
                    "TestNotification",
                    "مشاهده صفحه تست ارسال اعلان");

                return View(types);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "TestNotification",
                    "خطا در بارگذاری صفحه تست",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری صفحه تست";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ارسال تست اعلان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestNotification(int typeId, byte channel, string testMessage)
        {
            try
            {
                var currentUserId = GetUserId();
                var type = await _notificationRepo.GetNotificationTypeByIdAsync(typeId);

                if (type == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "نوع اعلان یافت نشد" } }
                    });
                }

                // TODO: ارسال اعلان تستی
                // از TaskNotificationService استفاده کنید

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "NotificationSettings",
                    "SendTestNotification",
                    $"ارسال تست اعلان: {type.TypeNameFa} - کانال: {channel}");

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "اعلان تستی با موفقیت ارسال شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "SendTestNotification",
                    "خطا در ارسال اعلان تستی",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ارسال اعلان تستی" } }
                });
            }
        }

        #endregion

        #region ویرایش نوع اعلان

        /// <summary>
        /// نمایش صفحه ویرایش نوع اعلان
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditType(int id)
        {
            try
            {
                var viewModel = await _notificationRepo.GetEditTypeViewModelAsync(id);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "نوع اعلان یافت نشد";
                    return RedirectToAction("Index");
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationSettings",
                    "EditType",
                    $"نمایش صفحه ویرایش نوع اعلان: {viewModel.TypeNameFa}",
                    recordId: id.ToString());

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "EditType",
                    "خطا در نمایش صفحه ویرایش",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری صفحه ویرایش";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ذخیره ویرایش نوع اعلان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditType(NotificationTypeEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new
                    {
                        status = "validation-error",
                        message = errors
                    });
                }

                var type = await _notificationRepo.GetNotificationTypeByIdAsync(model.Id);

                if (type == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "نوع اعلان یافت نشد" } }
                    });
                }

                // بروزرسانی فیلدها
                type.TypeNameFa = model.TypeNameFa;
                type.Description = model.Description;
                type.IsActive = model.IsActive;
                type.DefaultPriority = model.DefaultPriority;
                type.AllowUserCustomization = model.AllowUserCustomization;
                type.SupportsEmail = model.SupportsEmail;
                type.SupportsSms = model.SupportsSms;
                type.SupportsTelegram = model.SupportsTelegram;

                var result = await _notificationRepo.UpdateNotificationTypeConfigAsync(type);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در بروزرسانی" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationSettings",
                    "EditType",
                    $"ویرایش نوع اعلان: {model.TypeNameFa}",
                    recordId: model.Id.ToString());

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "NotificationSettings", new { area = "AppCoreArea" }),
                    message = new[] { new { status = "success", text = "تنظیمات با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "EditType",
                    "خطا در ذخیره تنظیمات",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره تنظیمات" } }
                });
            }
        }

    

#endregion

        #region مدیریت دریافت‌کنندگان

        /// <summary>
        /// صفحه مدیریت دریافت‌کنندگان یک نوع اعلان
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManageRecipients(int id)
        {
            try
            {
                var viewModel = await _notificationRepo.GetManageRecipientsViewModelAsync(id);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "نوع اعلان یافت نشد";
                    return RedirectToAction("Index");
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationSettings",
                    "ManageRecipients",
                    $"مدیریت دریافت‌کنندگان: {viewModel.TypeName}",
                    recordId: id.ToString());

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "ManageRecipients",
                    "خطا در بارگذاری صفحه",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری صفحه";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تغییر حالت ارسال (SendMode)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSendMode(int typeId, byte sendMode)
        {
            try
            {
                var result = await _notificationRepo.UpdateSendModeAsync(typeId, sendMode);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در بروزرسانی" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationSettings",
                    "UpdateSendMode",
                    $"تغییر حالت ارسال به {sendMode}",
                    recordId: typeId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "حالت ارسال با موفقیت تغییر کرد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "UpdateSendMode",
                    "خطا در تغییر حالت ارسال",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تغییر حالت ارسال" } }
                });
            }
        }
        /// <summary>
        /// Partial View برای Modal افزودن دریافت‌کننده
        /// </summary>
        [HttpGet]
        public IActionResult AddRecipientModal(int id)
        {
            ViewBag.TypeId = id;
            return PartialView("_AddRecipientModal");
        }


        /// <summary>
        /// افزودن کاربر به لیست دریافت‌کنندگان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecipient(int typeId, string userId, string reason)
        {
            try
            {
                var currentUserId = GetUserId();

                var result = await _notificationRepo.AddRecipientAsync(typeId, userId, reason, currentUserId);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "کاربر قبلاً در لیست وجود دارد" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "NotificationSettings",
                    "AddRecipient",
                    $"افزودن کاربر {userId} به لیست دریافت‌کنندگان",
                    recordId: typeId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "کاربر با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "AddRecipient",
                    "خطا در افزودن کاربر",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن کاربر" } }
                });
            }
        }

        /// <summary>
        /// حذف کاربر از لیست دریافت‌کنندگان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRecipient(int recipientId)
        {
            try
            {
                var result = await _notificationRepo.RemoveRecipientAsync(recipientId);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "رکورد یافت نشد" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "NotificationSettings",
                    "RemoveRecipient",
                    "حذف کاربر از لیست دریافت‌کنندگان",
                    recordId: recipientId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "کاربر با موفقیت حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationSettings",
                    "RemoveRecipient",
                    "خطا در حذف کاربر",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف کاربر" } }
                });
            }
        }

      
        #endregion

    }
}