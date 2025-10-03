using MahERP.Attributes;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.AdminArea.Controllers.CoreControllers
{
    /// <summary>
    /// کنترلر مدیریت لاگ فعالیت‌های کاربران
    /// این کنترلر شامل تمام عملیات مربوط به نمایش، جستجو و مدیریت لاگ‌های سیستم است
    /// </summary>
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("UserActivityLog")]

    public class UserActivityLogController : Controller
    {
        private readonly IUserActivityLogRepository _logRepository;

        /// <summary>
        /// سازنده کنترلر
        /// </summary>
        /// <param name="logRepository">مخزن لاگ فعالیت‌ها</param>
        public UserActivityLogController(IUserActivityLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        #region صفحات اصلی - Main Pages

        /// <summary>
        /// صفحه اصلی نمایش لیست لاگ‌ها
        /// </summary>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>صفحه لیست لاگ‌ها</returns>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            try
            {
                ViewBag.Title = "لاگ فعالیت‌های سیستم";
                ViewBag.CurrentPage = "SystemLogs";

                var result = await _logRepository.GetLogsAsync(page, pageSize);
                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت لاگ‌ها: {ex.Message}";
                return View(new LogListViewModel());
            }
        }

        /// <summary>
        /// صفحه جستجوی پیشرفته لاگ‌ها
        /// </summary>
        /// <returns>صفحه جستجو</returns>
        public IActionResult Search()
        {
            ViewBag.Title = "جستجوی پیشرفته لاگ‌ها";
            ViewBag.CurrentPage = "SystemLogs";

            var searchModel = new LogSearchViewModel();
            return View(searchModel);
        }

        /// <summary>
        /// انجام جستجوی پیشرفته
        /// </summary>
        /// <param name="searchModel">مدل جستجو</param>
        /// <returns>نتایج جستجو</returns>
        [HttpPost]
        public async Task<IActionResult> Search(LogSearchViewModel searchModel)
        {
            try
            {
                ViewBag.Title = "نتایج جستجوی لاگ‌ها";
                ViewBag.CurrentPage = "SystemLogs";

                if (ModelState.IsValid)
                {
                    var result = await _logRepository.SearchLogsAsync(searchModel);
                    ViewBag.SearchModel = searchModel;
                    return View("SearchResults", result);
                }

                return View(searchModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در جستجو: {ex.Message}";
                return View(searchModel);
            }
        }

        /// <summary>
        /// نمایش جزئیات یک لاگ خاص
        /// </summary>
        /// <param name="id">شناسه لاگ</param>
        /// <returns>صفحه جزئیات</returns>
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                ViewBag.Title = "جزئیات لاگ فعالیت";
                ViewBag.CurrentPage = "SystemLogs";

                var log = await _logRepository.GetLogByIdAsync(id);
                if (log == null)
                {
                    TempData["ErrorMessage"] = "لاگ مورد نظر یافت نشد.";
                    return RedirectToAction(nameof(Index));
                }

                return View(log);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت جزئیات: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region لاگ‌های تخصصی - Specialized Logs

        /// <summary>
        /// نمایش لاگ‌های حساس سیستم
        /// </summary>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لاگ‌های حساس</returns>
        public async Task<IActionResult> SensitiveLogs(int page = 1, int pageSize = 50)
        {
            try
            {
                ViewBag.Title = "لاگ‌های حساس سیستم";
                ViewBag.CurrentPage = "SensitiveLogs";

                var result = await _logRepository.GetSensitiveLogsAsync(page, pageSize);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت لاگ‌های حساس: {ex.Message}";
                return View("Index", new LogListViewModel());
            }
        }

        /// <summary>
        /// نمایش لاگ‌های خطا
        /// </summary>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لاگ‌های خطا</returns>
        public async Task<IActionResult> ErrorLogs(int page = 1, int pageSize = 50)
        {
            try
            {
                ViewBag.Title = "لاگ‌های خطا";
                ViewBag.CurrentPage = "ErrorLogs";

                var result = await _logRepository.GetErrorLogsAsync(page, pageSize);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت لاگ‌های خطا: {ex.Message}";
                return View("Index", new LogListViewModel());
            }
        }

        /// <summary>
        /// نمایش فعالیت‌های مشکوک
        /// </summary>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>فعالیت‌های مشکوک</returns>
        public async Task<IActionResult> SuspiciousActivities(int page = 1, int pageSize = 50)
        {
            try
            {
                ViewBag.Title = "فعالیت‌های مشکوک";
                ViewBag.CurrentPage = "SuspiciousLogs";

                var result = await _logRepository.GetSuspiciousActivitiesAsync(page, pageSize);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت فعالیت‌های مشکوک: {ex.Message}";
                return View("Index", new LogListViewModel());
            }
        }

        #endregion

        #region لاگ‌های کاربری - User Specific Logs

        /// <summary>
        /// نمایش لاگ‌های یک کاربر خاص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لاگ‌های کاربر</returns>
        public async Task<IActionResult> UserLogs(string userId, int page = 1, int pageSize = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["ErrorMessage"] = "شناسه کاربر مشخص نشده است.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Title = $"لاگ‌های کاربر: {userId}";
                ViewBag.CurrentPage = "UserLogs";
                ViewBag.UserId = userId;

                var result = await _logRepository.GetUserLogsAsync(userId, page, pageSize);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت لاگ‌های کاربر: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// نمایش لاگ‌های یک ماژول خاص
        /// </summary>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لاگ‌های ماژول</returns>
        public async Task<IActionResult> ModuleLogs(string moduleName, int page = 1, int pageSize = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(moduleName))
                {
                    TempData["ErrorMessage"] = "نام ماژول مشخص نشده است.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Title = $"لاگ‌های ماژول: {moduleName}";
                ViewBag.CurrentPage = "ModuleLogs";
                ViewBag.ModuleName = moduleName;

                var result = await _logRepository.GetModuleLogsAsync(moduleName, page, pageSize);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت لاگ‌های ماژول: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region آمار و گزارش‌ها - Statistics & Reports

        /// <summary>
        /// صفحه آمار و گزارش‌های لاگ
        /// </summary>
        /// <returns>صفحه آمار</returns>
        public async Task<IActionResult> Statistics()
        {
            try
            {
                ViewBag.Title = "آمار و گزارش‌های سیستم";
                ViewBag.CurrentPage = "LogStatistics";

                var statistics = await _logRepository.GetLogStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت آمار: {ex.Message}";
                return View(new LogStatisticsViewModel());
            }
        }

        /// <summary>
        /// دریافت آمار فعالیت کاربران
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار کاربران</returns>
        public async Task<IActionResult> UserActivityStats(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // تنظیم تاریخ‌های پیش‌فرض
                fromDate ??= DateTime.Now.AddDays(-30);
                toDate ??= DateTime.Now;

                ViewBag.Title = "آمار فعالیت کاربران";
                ViewBag.CurrentPage = "UserStats";
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var stats = await _logRepository.GetUserActivityStatsAsync(fromDate.Value, toDate.Value);
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت آمار کاربران: {ex.Message}";
                return View(new List<UserActivityStatViewModel>());
            }
        }

        /// <summary>
        /// دریافت آمار فعالیت ماژول‌ها
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار ماژول‌ها</returns>
        public async Task<IActionResult> ModuleActivityStats(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // تنظیم تاریخ‌های پیش‌فرض
                fromDate ??= DateTime.Now.AddDays(-30);
                toDate ??= DateTime.Now;

                ViewBag.Title = "آمار فعالیت ماژول‌ها";
                ViewBag.CurrentPage = "ModuleStats";
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var stats = await _logRepository.GetModuleActivityStatsAsync(fromDate.Value, toDate.Value);
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت آمار ماژول‌ها: {ex.Message}";
                return View(new List<ModuleActivityStatViewModel>());
            }
        }

        /// <summary>
        /// دریافت کاربران پرفعالیت
        /// </summary>
        /// <param name="topCount">تعداد کاربران برتر</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>لیست کاربران پرفعالیت</returns>
        public async Task<IActionResult> TopActiveUsers(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                ViewBag.Title = "کاربران پرفعالیت";
                ViewBag.CurrentPage = "TopUsers";
                ViewBag.TopCount = topCount;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var topUsers = await _logRepository.GetTopActiveUsersAsync(topCount, fromDate, toDate);
                return View(topUsers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت کاربران پرفعالیت: {ex.Message}";
                return View(new List<TopUserActivityViewModel>());
            }
        }

        #endregion

        #region امنیت و نظارت - Security & Monitoring

        /// <summary>
        /// نمایش تلاش‌های ناموفق ورود
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تلاش‌های ناموفق</returns>
        public async Task<IActionResult> FailedLoginAttempts(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // تنظیم تاریخ‌های پیش‌فرض
                fromDate ??= DateTime.Now.AddDays(-7);
                toDate ??= DateTime.Now;

                ViewBag.Title = "تلاش‌های ناموفق ورود";
                ViewBag.CurrentPage = "FailedLogins";
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var attempts = await _logRepository.GetFailedLoginAttemptsAsync(fromDate.Value, toDate.Value);
                return View(attempts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت تلاش‌های ناموفق: {ex.Message}";
                return View(new List<FailedLoginAttemptViewModel>());
            }
        }

        /// <summary>
        /// نمایش فعالیت‌های خارج از ساعت کاری
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>فعالیت‌های خارج از ساعت</returns>
        public async Task<IActionResult> AfterHoursActivities(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // تنظیم تاریخ‌های پیش‌فرض
                fromDate ??= DateTime.Now.AddDays(-7);
                toDate ??= DateTime.Now;

                ViewBag.Title = "فعالیت‌های خارج از ساعت کاری";
                ViewBag.CurrentPage = "AfterHours";
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var activities = await _logRepository.GetAfterHoursActivitiesAsync(fromDate.Value, toDate.Value);
                return View("Index", activities);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت فعالیت‌های خارج از ساعت: {ex.Message}";
                return View("Index", new LogListViewModel());
            }
        }

        #endregion

        #region مدیریت و نگهداری - Management & Maintenance

        /// <summary>
        /// صفحه مدیریت آرشیو لاگ‌ها
        /// </summary>
        /// <returns>صفحه مدیریت آرشیو</returns>
        public async Task<IActionResult> ArchiveManagement()
        {
            try
            {
                ViewBag.Title = "مدیریت آرشیو لاگ‌ها";
                ViewBag.CurrentPage = "ArchiveManagement";

                var statistics = await _logRepository.GetLogStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت اطلاعات آرشیو: {ex.Message}";
                return View(new LogStatisticsViewModel());
            }
        }

        /// <summary>
        /// آرشیو کردن لاگ‌های قدیمی
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای آرشیو</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        public async Task<IActionResult> ArchiveOldLogs(DateTime beforeDate)
        {
            try
            {
                var archivedCount = await _logRepository.ArchiveOldLogsAsync(beforeDate);
                TempData["SuccessMessage"] = $"{archivedCount:N0} لاگ با موفقیت آرشیو شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در آرشیو لاگ‌ها: {ex.Message}";
            }

            return RedirectToAction(nameof(ArchiveManagement));
        }

        /// <summary>
        /// حذف لاگ‌های آرشیو شده
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای حذف</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteArchivedLogs(DateTime beforeDate)
        {
            try
            {
                var deletedCount = await _logRepository.DeleteArchivedLogsAsync(beforeDate);
                TempData["SuccessMessage"] = $"{deletedCount:N0} لاگ آرشیو شده با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در حذف لاگ‌های آرشیو: {ex.Message}";
            }

            return RedirectToAction(nameof(ArchiveManagement));
        }

        #endregion

        #region API Methods - متدهای API

        /// <summary>
        /// دریافت تعداد خطاها در بازه زمانی (API)
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تعداد خطاها</returns>
        [HttpGet]
        public async Task<IActionResult> GetErrorCount(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var errorCount = await _logRepository.GetErrorCountByDateRangeAsync(fromDate, toDate);
                return Json(new { success = true, errorCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت لیست IP های منحصر به فرد (API)
        /// </summary>
        /// <returns>لیست IP ها</returns>
        [HttpGet]
        public async Task<IActionResult> GetUniqueIps()
        {
            try
            {
                var ips = await _logRepository.GetUniqueIpAddressesAsync();
                return Json(new { success = true, ips });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت کاربر (API)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>آخرین فعالیت</returns>
        [HttpGet]
        public async Task<IActionResult> GetLastUserActivity(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Json(new { success = false, message = "شناسه کاربر مشخص نشده است." });
                }

                var lastActivity = await _logRepository.GetLastUserActivityAsync(userId);
                if (lastActivity == null)
                {
                    return Json(new { success = false, message = "هیچ فعالیتی برای این کاربر یافت نشد." });
                }

                return Json(new 
                { 
                    success = true, 
                    lastActivity = new 
                    {
                        activityDateTime = lastActivity.ActivityDateTime,
                        moduleName = lastActivity.ModuleName,
                        actionName = lastActivity.ActionName,
                        description = lastActivity.Description,
                        ipAddress = lastActivity.IpAddress
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}