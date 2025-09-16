using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;


namespace MahERP.Areas.AdminArea.Controllers.BaseControllers
{
    [Authorize]
    [Area("AdminArea")]
    public class DashboardController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly IMapper _Mapper;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUsers> _UserManager;
        private new readonly PersianDateHelper _persianDateHelper; // Add this field
        private readonly IMemoryCache _memoryCache; // Add this field

        public DashboardController(
            IWebHostEnvironment env,
            IUnitOfWork Context,
            IMapper Mapper,
            UserManager<AppUsers> UserManager,
            PersianDateHelper persianDateHelper, // Add this parameter
            IMemoryCache memoryCache, // Add this parameter
            ActivityLoggerService activityLogger // Add this parameter
        ) : base(Context, UserManager, persianDateHelper, memoryCache, activityLogger) // Pass the new parameters to the base constructor
        {
            _Context = Context;
            _UserManager = UserManager;
            _Mapper = Mapper;
            _env = env;
            _persianDateHelper = persianDateHelper; // Initialize the new field
            _memoryCache = memoryCache; // Initialize the new field
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
