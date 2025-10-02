using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.BaseControllers
{
    [Area("AdminArea")]
    public class BaseController : Controller
    {
        protected readonly IUnitOfWork _uow;
        protected readonly UserManager<AppUsers> _userManager;
        protected readonly PersianDateHelper _persianDateHelper;
        protected readonly IMemoryCache _memoryCache;
        protected readonly ActivityLoggerService _activityLogger;
        protected readonly IUserManagerRepository _userRepository;

        public BaseController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository)
        {
            _uow = uow;
            _userManager = userManager;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _activityLogger = activityLogger;
            _userRepository = userRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                SetUserInfoInViewBag();
            }
            base.OnActionExecuting(context);
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
    }
}