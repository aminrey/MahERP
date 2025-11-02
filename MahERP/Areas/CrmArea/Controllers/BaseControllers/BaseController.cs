using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace MahERP.Areas.CrmArea.Controllers.BaseControllers
{
    [Area("CrmArea")]
    public class BaseController : Controller
    {
        protected readonly IUnitOfWork _uow;
        protected readonly UserManager<AppUsers> _userManager;
        protected readonly PersianDateHelper _persianDateHelper;
        protected readonly IMemoryCache _memoryCache;
        protected readonly ActivityLoggerService _activityLogger;
        protected readonly IUserManagerRepository _userRepository;
        protected readonly IBaseRepository _baseRepository;
        protected readonly ModuleTrackingBackgroundService _moduleTracking;

        public BaseController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,ModuleTrackingBackgroundService moduleTracking)

        {
            _uow = uow;
            _userManager = userManager;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _activityLogger = activityLogger;
            _userRepository = userRepository;
            _baseRepository = baseRepository;
            _moduleTracking = moduleTracking;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                SetUserInfoInViewBag();
                SetModuleSettingsInViewBag();
                TrackCurrentModule(context);

                // بررسی فعال بودن ماژول CRM
                if (!_baseRepository.IsCrmModuleEnabled())
                {
                    context.Result = RedirectToAction("Index", "Dashboard", new { area = "AppCoreArea" });
                    return;
                }
            }
            base.OnActionExecuting(context);
        }
        /// <summary>
        /// ⭐⭐⭐ ردیابی ماژول فعلی کاربر در Background
        /// </summary>
        private void TrackCurrentModule(ActionExecutingContext context)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return;

                // ⭐ تشخیص ماژول بر اساس Area
                var area = context.RouteData.Values["area"]?.ToString();

                ModuleType? currentModule = area switch
                {
                    "AppCoreArea" => ModuleType.Core,
                    "TaskingArea" => ModuleType.Tasking,
                    "CrmArea" => ModuleType.CRM,
                    _ => null // برای Account یا صفحات عمومی
                };

                // ⭐ اگر ماژول مشخص بود، در Background ذخیره کن
                if (currentModule.HasValue)
                {
                    _moduleTracking.EnqueueModuleTracking(userId, currentModule.Value);
                }
            }
            catch (Exception ex)
            {
                // Silent fail - نباید باعث خطا در صفحه شود
                // می‌توانید در اینجا Log کنید
                System.Diagnostics.Debug.WriteLine($"⚠️ TrackCurrentModule failed: {ex.Message}");
            }
        }
        public string GetUserId()
        {
            string userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userId = _userManager.GetUserId(HttpContext.User);
            }
            return userId;
        }

        /// <summary>
        /// تنظیم اطلاعات کاربر در ViewBag برای استفاده در Header
        /// </summary>
        private void SetUserInfoInViewBag()
        {
            try
            {
                var userId = GetUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    // چک کردن کش
                    var cacheKey = $"user_header_info_{userId}";
                    if (!_memoryCache.TryGetValue(cacheKey, out object cachedUserInfo))
                    {
                        // دریافت اطلاعات کاربر از دیتابیس
                        var userInfo = _userRepository.GetUserInfoData(userId);
                        if (userInfo != null)
                        {
                            var headerUserInfo = new
                            {
                                FullName = userInfo.FullNamesString,
                                Email = userInfo.Email,
                                UserName = userInfo.UserName,
                                FirstName = userInfo.FirstName,
                                LastName = userInfo.LastName,
                                PhoneNumber = userInfo.PhoneNumber,
                                CompanyName = userInfo.CompanyName,
                                PositionName = userInfo.PositionName ?? "کارمند",
                                City = userInfo.City,
                                ProfileImagePath = userInfo.ProfileImagePath,
                                Province = userInfo.Province
                            };

                            // ذخیره در کش برای 15 دقیقه
                            _memoryCache.Set(cacheKey, headerUserInfo, TimeSpan.FromMinutes(15));
                            cachedUserInfo = headerUserInfo;
                        }
                    }

                    // تنظیم ViewBag
                    if (cachedUserInfo != null)
                    {
                        ViewBag.CurrentUser = cachedUserInfo;
                        ViewBag.UserName = ((dynamic)cachedUserInfo).FullName ?? ((dynamic)cachedUserInfo).UserName;
                    }
                    else
                    {
                        ViewBag.UserName = User.Identity.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، از نام کاربری ساده استفاده کن
                ViewBag.UserName = User.Identity.Name;
                // Log error if needed
            }
        }

        /// <summary>
        /// تنظیم وضعیت فعال بودن ماژول‌ها در ViewBag
        /// </summary>
        private void SetModuleSettingsInViewBag()
        {
            try
            {
                var settings = _baseRepository.GetSystemSettings();
                if (settings != null)
                {
                    ViewBag.IsTaskingModuleEnabled = settings.IsTaskingModuleEnabled;
                    ViewBag.IsCrmModuleEnabled = settings.IsCrmModuleEnabled;
                }
                else
                {
                    // مقادیر پیش‌فرض
                    ViewBag.IsTaskingModuleEnabled = true;
                    ViewBag.IsCrmModuleEnabled = true;
                    ViewBag.IsCommunicationModuleEnabled = true;
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، همه ماژول‌ها فعال
                ViewBag.IsTaskingModuleEnabled = true;
                ViewBag.IsCrmModuleEnabled = true;
                ViewBag.IsCommunicationModuleEnabled = true;
                // Log error if needed
            }
        }
    }
}