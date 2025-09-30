using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private new readonly PersianDateHelper _persianDateHelper;
        private readonly IMemoryCache _memoryCache;
        private readonly IMainDashboardRepository _mainDashboardRepository;

        public DashboardController(
            IWebHostEnvironment env,
            IUnitOfWork Context,
            IMapper Mapper,
            UserManager<AppUsers> UserManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IMainDashboardRepository mainDashboardRepository
        ) : base(Context, UserManager, persianDateHelper, memoryCache, activityLogger)
        {
            _Context = Context;
            _UserManager = UserManager;
            _Mapper = Mapper;
            _env = env;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _mainDashboardRepository = mainDashboardRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var model = await _mainDashboardRepository.PrepareDashboardModelAsync(userId, User.Identity.Name);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Dashboard", "Index", "مشاهده داشبورد اصلی");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "Index", "خطا در نمایش داشبورد", ex);
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// دریافت آمار کلی برای داشبورد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var stats = await _mainDashboardRepository.GetUserDashboardStatsAsync(userId);

                return Json(new
                {
                    success = true,
                    tasksStats = stats.TasksStats,
                    contractsStats = stats.ContractsStats,
                    stakeholdersStats = stats.StakeholdersStats,
                    recentActivities = stats.RecentActivities
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetDashboardSummary", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// دریافت تسک‌های فوری برای داشبورد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUrgentTasks()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var urgentTasks = await _mainDashboardRepository.GetUrgentTasksAsync(userId);

                return PartialView("_UrgentTasksList", urgentTasks);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetUrgentTasks", "خطا در دریافت تسک‌های فوری", ex);
                return PartialView("_UrgentTasksList", new List<TaskSummaryViewModel>());
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentTaskActivities()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var activities = await _mainDashboardRepository.GetRecentTaskActivitiesAsync(userId, take: 10);

                return PartialView("_RecentTaskActivities", activities);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetRecentTaskActivities", "خطا در دریافت فعالیت‌های اخیر", ex);
                return PartialView("_RecentTaskActivities", new List<RecentActivityViewModel>());
            }
        }
    }
}