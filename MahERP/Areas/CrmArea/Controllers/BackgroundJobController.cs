using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers
{
    [Area("CrmArea")]
    [Authorize]
    public class BackgroundJobController : BaseController
    {
        private readonly IBackgroundJobRepository _jobRepo;

        public BackgroundJobController(
            IBackgroundJobRepository jobRepo,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository BaseRepository,
            ModuleTrackingBackgroundService moduleTracking,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _jobRepo = jobRepo;
        }

        /// <summary>
        /// دریافت Job های فعال کاربر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetActiveJobs()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var jobs = await _jobRepo.GetUserActiveJobsAsync(currentUser.Id);

            return Json(new
            {
                success = true,
                jobs = jobs.Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    description = j.Description,
                    jobType = j.JobType,
                    jobTypeText = j.JobTypeText,
                    status = j.Status,
                    statusText = j.StatusText,
                    statusBadgeClass = j.StatusBadgeClass,
                    progress = j.Progress,
                    totalItems = j.TotalItems,
                    processedItems = j.ProcessedItems,
                    successCount = j.SuccessCount,
                    failedCount = j.FailedCount,
                    startDate = j.StartDate.ToString("yyyy/MM/dd HH:mm"),
                    errorMessage = j.ErrorMessage
                })
            });
        }

        /// <summary>
        /// دریافت تاریخچه Job ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetJobHistory()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var jobs = await _jobRepo.GetUserJobsAsync(currentUser.Id, 20);

            return Json(new
            {
                success = true,
                jobs = jobs.Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    description = j.Description,
                    jobTypeText = j.JobTypeText,
                    statusText = j.StatusText,
                    statusBadgeClass = j.StatusBadgeClass,
                    progress = j.Progress,
                    successCount = j.SuccessCount,
                    failedCount = j.FailedCount,
                    startDate = j.StartDate.ToString("yyyy/MM/dd HH:mm"),
                    completedDate = j.CompletedDate?.ToString("yyyy/MM/dd HH:mm"),
                    duration = j.Duration?.ToString(@"mm\:ss")
                })
            });
        }

        /// <summary>
        /// دریافت جزئیات یک Job
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetJobDetails(int id)
        {
            var job = await _jobRepo.GetJobByIdAsync(id);
            if (job == null)
                return Json(new { success = false, message = "Job یافت نشد" });

            var currentUser = await _userManager.GetUserAsync(User);
            if (job.CreatedByUserId != currentUser.Id)
                return Json(new { success = false, message = "دسترسی غیرمجاز" });

            return Json(new
            {
                success = true,
                job = new
                {
                    id = job.Id,
                    title = job.Title,
                    description = job.Description,
                    jobTypeText = job.JobTypeText,
                    statusText = job.StatusText,
                    statusBadgeClass = job.StatusBadgeClass,
                    progress = job.Progress,
                    totalItems = job.TotalItems,
                    processedItems = job.ProcessedItems,
                    successCount = job.SuccessCount,
                    failedCount = job.FailedCount,
                    startDate = job.StartDate.ToString("yyyy/MM/dd HH:mm"),
                    completedDate = job.CompletedDate?.ToString("yyyy/MM/dd HH:mm"),
                    duration = job.Duration?.ToString(@"mm\:ss"),
                    errorMessage = job.ErrorMessage
                }
            });
        }

        /// <summary>
        /// Partial View برای Dropdown
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetJobsPartial()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var activeJobs = await _jobRepo.GetUserActiveJobsAsync(currentUser.Id);
            var recentJobs = await _jobRepo.GetUserJobsAsync(currentUser.Id, 5);

            ViewBag.ActiveJobs = activeJobs;
            ViewBag.RecentJobs = recentJobs;

            return PartialView("_BackgroundJobsPartial");
        }
    }
}
