using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
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

        public BaseController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger)
        {
            _uow = uow;
            _userManager = userManager;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _activityLogger = activityLogger;
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

    }
}
