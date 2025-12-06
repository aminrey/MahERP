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
using MahERP.Helpers; // ⭐⭐⭐ اضافه شد

namespace MahERP.Areas.AppCoreArea.Controllers.BaseControllers
{
    [Area("AppCoreArea")]
    public class BaseController : Controller
    {
        protected readonly IUnitOfWork _uow;
        protected readonly UserManager<AppUsers> _userManager;
        protected readonly PersianDateHelper _persianDateHelper;
        protected readonly IMemoryCache _memoryCache;
        protected readonly ActivityLoggerService _activityLogger;
        protected readonly IUserManagerRepository _userRepository;
        protected readonly IBaseRepository _baseRepository;
        protected readonly IModuleTrackingService _moduleTracking;
        protected readonly IModuleAccessService _moduleAccessService;

        public BaseController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService)
        {
            _uow = uow;
            _userManager = userManager;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _activityLogger = activityLogger;
            _userRepository = userRepository;
            _baseRepository = baseRepository;
            _moduleTracking = moduleTracking;
            _moduleAccessService = moduleAccessService;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                SetUserInfoInViewBag();
                SetModuleSettingsInViewBag();
                TrackCurrentModule(context);
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
        /// تنظیم وضعیت فعال بودن ماژول‌ها و دسترسی کاربر در ViewBag
        /// </summary>
        private void SetModuleSettingsInViewBag()
        {
            try
            {
                var userId = GetUserId();
                var settings = _baseRepository.GetSystemSettings();

                if (settings != null)
                {
                    // ⭐ تنظیمات سیستمی - آیا ماژول‌ها به طور کلی فعال هستند
                    ViewBag.IsTaskingModuleEnabled = settings.IsTaskingModuleEnabled;
                    ViewBag.IsCrmModuleEnabled = settings.IsCrmModuleEnabled;
                    ViewBag.IsCommunicationModuleEnabled = true; // فعلاً پیش‌فرض
                }
                else
                {
                    // مقادیر پیش‌فرض
                    ViewBag.IsTaskingModuleEnabled = true;
                    ViewBag.IsCrmModuleEnabled = true;
                    ViewBag.IsCommunicationModuleEnabled = true;
                }

                // ⭐⭐⭐ بررسی دسترسی کاربر به ماژول‌ها
                if (!string.IsNullOrEmpty(userId))
                {
                    SetUserModuleAccessInViewBag(userId).Wait();
                }
                else
                {
                    // اگر کاربر لاگین نیست، دسترسی به هیچ ماژولی ندارد
                    ViewBag.HasCoreAccess = false;
                    ViewBag.HasTaskingAccess = false;
                    ViewBag.HasCrmAccess = false;
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، همه ماژول‌ها فعال و دسترسی کاربر true (برای جلوگیری از قطع سرویس)
                ViewBag.IsTaskingModuleEnabled = true;
                ViewBag.IsCrmModuleEnabled = true;
                ViewBag.IsCommunicationModuleEnabled = true;
                ViewBag.HasCoreAccess = true;
                ViewBag.HasTaskingAccess = true;
                ViewBag.HasCrmAccess = true;
                // Log error if needed
            }
        }

        /// <summary>
        /// ⭐⭐⭐ بررسی دسترسی کاربر به هر ماژول و تنظیم در ViewBag
        /// </summary>
        private async Task SetUserModuleAccessInViewBag(string userId)
        {
            try
            {
                // بررسی کش
                var cacheKey = $"user_module_access_{userId}";
                if (!_memoryCache.TryGetValue(cacheKey, out Dictionary<string, bool> cachedAccess))
                {
                    cachedAccess = new Dictionary<string, bool>();

                    // ⭐ بررسی دسترسی به ماژول Core
                    var coreAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.Core);
                    cachedAccess["Core"] = coreAccess.HasAccess;

                    // ⭐ بررسی دسترسی به ماژول Tasking
                    var taskingAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.Tasking);
                    cachedAccess["Tasking"] = taskingAccess.HasAccess;

                    // ⭐ بررسی دسترسی به ماژول CRM
                    var crmAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                    cachedAccess["CRM"] = crmAccess.HasAccess;

                    // ذخیره در کش برای 30 دقیقه
                    _memoryCache.Set(cacheKey, cachedAccess, TimeSpan.FromMinutes(30));
                }

                // تنظیم ViewBag
                ViewBag.HasCoreAccess = cachedAccess["Core"];
                ViewBag.HasTaskingAccess = cachedAccess["Tasking"];
                ViewBag.HasCrmAccess = cachedAccess["CRM"];
            }
            catch (Exception ex)
            {
                // در صورت خطا، دسترسی true (برای جلوگیری از قطع سرویس)
                ViewBag.HasCoreAccess = true;
                ViewBag.HasTaskingAccess = true;
                ViewBag.HasCrmAccess = true;
                System.Diagnostics.Debug.WriteLine($"⚠️ SetUserModuleAccessInViewBag failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ⭐⭐⭐ محاسبه URL بازگشت برای صفحات
        /// </summary>
        /// <param name="defaultAction">Action پیش‌فرض در صورت نبود Return URL</param>
        /// <param name="defaultController">Controller پیش‌فرض</param>
        /// <param name="defaultArea">Area پیش‌فرض</param>
        /// <returns>URL بازگشت امن</returns>
        protected string GetBackUrl(
            string defaultAction = "Index",
            string? defaultController = null,
            string? defaultArea = null)
        {
            try
            {
                // ⭐ ابتدا از Query String بخوان
                var returnUrl = Request.Query["returnUrl"].ToString();

                // ⭐ تشخیص Area فعلی اگر مشخص نشده
                if (string.IsNullOrEmpty(defaultArea))
                {
                    defaultArea = RouteData.Values["area"]?.ToString() ?? "AppCoreArea";
                }

                // ⭐ استفاده از Helper برای ساخت URL امن
                return this.GetSafeReturnUrl(
                    returnUrl,
                    defaultAction,
                    defaultController ?? ControllerContext.ActionDescriptor.ControllerName,
                    defaultArea);
            }
            catch
            {
                // در صورت خطا، به صفحه پیش‌فرض برو
                var area = defaultArea ?? RouteData.Values["area"]?.ToString() ?? "AppCoreArea";
                return Url.Action(
                    defaultAction,
                    defaultController ?? ControllerContext.ActionDescriptor.ControllerName,
                    new { area = area }) ?? "/";
            }
        }

        /// <summary>
        /// ⭐⭐⭐ ذخیره اطلاعات Return URL در ViewBag
        /// </summary>
        protected void SetBackUrlInViewBag(
            string defaultAction = "Index",
            string? defaultController = null,
            string? defaultArea = null)
        {
            ViewBag.BackUrl = GetBackUrl(defaultAction, defaultController, defaultArea);
            ViewBag.CurrentUrl = Request.Path + Request.QueryString;
        }
    }
}