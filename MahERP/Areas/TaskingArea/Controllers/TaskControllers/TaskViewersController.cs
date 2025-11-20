using AutoMapper;
using MahERP.Areas.TaskingArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK")]
    public class TaskViewersController : BaseController
    {
        private readonly ITaskCarbonCopyRepository _carbonCopyRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IBranchRepository _branchRepository; // ⭐ اضافه شده
        private readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly ActivityLoggerService _activityLogger;

        public TaskViewersController(
            ITaskCarbonCopyRepository carbonCopyRepository,
            ITaskRepository taskRepository,
            IBranchRepository branchRepository, // ⭐ اضافه شده
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMapper mapper,
                        IMemoryCache memoryCache,
                        IUserManagerRepository userRepository,
                        IBaseRepository BaseRepository,
                        ModuleTrackingBackgroundService moduleTracking,
                         IModuleAccessService moduleAccessService
            ,ActivityLoggerService activityLogger,
            IUnitOfWork uow) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _carbonCopyRepository = carbonCopyRepository;
            _taskRepository = taskRepository;
            _branchRepository = branchRepository; // ⭐ اضافه شده
            _userManager = userManager;
            _mapper = mapper;
            _activityLogger = activityLogger;
        }

        /// <summary>
        /// نمایش ناظران تسک (سیستمی + رونوشت) - JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskViewers(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = _taskRepository.GetTaskById(taskId, includeAssignments: true);

                if (task == null)
                    return NotFound();

                // بررسی دسترسی
                //if (!_taskRepository.IsUserRelatedToTask(userId, taskId))
                //    return Forbid();

                var systemViewers = new List<object>();
                var carbonCopyViewers = new List<object>();

                // ⭐ 1. ناظران خودکار سیستمی
                var systemSupervisorIds = await _taskRepository.GetTaskSupervisorsAsync(taskId, includeCreator: true);
                
                foreach (var supervisorId in systemSupervisorIds)
                {
                    var user = await _userManager.FindByIdAsync(supervisorId);
                    if (user != null)
                    {
                        var supervisionType = await DetermineSupervisionTypeAsync(supervisorId, taskId, task);
                        
                        systemViewers.Add(new
                        {
                            userId = user.Id,
                            fullName = $"{user.FirstName} {user.LastName}",
                            profileImage = user.ProfileImagePath ?? "/images/default-avatar.png",
                            email = user.Email,
                            phoneNumber = user.PhoneNumber,
                            viewerReason = supervisionType,
                            teamName = task.Team?.Title
                        });
                    }
                }

                // ⭐ 2. ناظران رونوشت شده (دستی)
                var carbonCopies = await _carbonCopyRepository.GetTaskCarbonCopiesAsync(taskId);
                
                foreach (var cc in carbonCopies)
                {
                    carbonCopyViewers.Add(new
                    {
                        id = cc.Id,
                        userId = cc.UserId,
                        fullName = $"{cc.User.FirstName} {cc.User.LastName}",
                        profileImage = cc.User.ProfileImagePath ?? "/images/default-avatar.png",
                        email = cc.User.Email,
                        phoneNumber = cc.User.PhoneNumber,
                        addedByUserName = $"{cc.AddedByUser.FirstName} {cc.AddedByUser.LastName}",
                        addedDatePersian = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(cc.AddedDate, "yyyy/MM/dd HH:mm"),
                        note = cc.Note,
                        canRemove = await _carbonCopyRepository.CanRemoveCarbonCopyAsync(cc.Id, userId)
                    });
                }

                return Json(new
                {
                    status = "success",
                    systemViewers = systemViewers,
                    carbonCopyViewers = carbonCopyViewers,
                    canAddViewer = task.CreatorUserId == userId || User.IsInRole("Admin")
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "GetTaskViewers", "خطا در دریافت ناظران", ex, recordId: taskId.ToString());
                return Json(new
                {
                    status = "error",
                    message = "خطا در بارگذاری ناظران"
                });
            }
        }

        /// <summary>
        /// مودال افزودن رونوشت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddCarbonCopyModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = _taskRepository.GetTaskById(taskId);

                if (task == null)
                    return NotFound();

                if (!_taskRepository.IsUserRelatedToTask(userId, taskId))
                    return Forbid();

                // ⭐ استفاده از BranchRepository برای دریافت مستقیم کاربران شعبه
                var branchUsers = _branchRepository.GetBranchUsers(task.BranchId.Value, includeInactive: false);

                // حذف کاربرانی که قبلاً رONOشت شده‌اند
                var existingCarbonCopyIds = await _carbonCopyRepository.GetTaskCarbonCopiesAsync(taskId);
                var existingUserIds = existingCarbonCopyIds.Select(cc => cc.UserId).ToList();

                // ⭐ تبدیل به ViewModel
                var availableUsers = branchUsers
                    .Where(bu => bu.User != null && 
                                 bu.User.IsActive && 
                                 !bu.User.IsRemoveUser &&
                                 !existingUserIds.Contains(bu.User.Id))
                    .Select(bu => new
                    {
                        Id = bu.User.Id,
                        FullName = $"{bu.User.FirstName} {bu.User.LastName}",
                        ProfileImagePath = bu.User.ProfileImagePath ?? "/images/default-avatar.png",
                        Email = bu.User.Email
                    })
                    .OrderBy(u => u.FullName)
                    .ToList();

                ViewBag.TaskId = taskId;
                ViewBag.TaskTitle = task.Title;
                ViewBag.AvailableUsers = availableUsers;

                return PartialView("_AddCarbonCopyModal");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "AddCarbonCopyModal", "خطا در بارگذاری مودال", ex);
                return BadRequest("خطا در بارگذاری");
            }
        }

        /// <summary>
        /// ⭐⭐⭐ مودال افزودن رونوشت برای CreateTask (بدون taskId)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddCarbonCopyViewerModal(int branchId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (branchId <= 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شعبه نامعتبر است" } }
                    });
                }

                // ⭐ استفاده از BranchRepository برای دریافت مستقیم کاربران شعبه
                var branchUsers = _branchRepository.GetBranchUsers(branchId, includeInactive: false);

                // ⭐ تبدیل به ViewModel
                var availableUsers = branchUsers
                    .Where(bu => bu.User != null && bu.User.IsActive && !bu.User.IsRemoveUser)
                    .Select(bu => new
                    {
                        Id = bu.User.Id,
                        FullName = $"{bu.User.FirstName} {bu.User.LastName}",
                        ProfileImagePath = bu.User.ProfileImagePath ?? "/images/default-avatar.png",
                        Email = bu.User.Email
                    })
                    .OrderBy(u => u.FullName)
                    .ToList();

                ViewBag.BranchId = branchId;
                ViewBag.AvailableUsers = availableUsers;

                return PartialView("_AddCarbonCopyViewerModal");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "AddCarbonCopyViewerModal", "خطا در بارگذاری مودال", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بارگذاری مودال" } }
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ افزودن رونوشت از فرم CreateTask
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCarbonCopyViewerSubmit(int branchId, string userId, string note)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "کاربر انتخاب نشده است" } }
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "کاربر یافت نشد" } }
                    });
                }

                // ⭐ برگرداندن اطلاعات کاربر برای JavaScript
                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "ناظر رونوشتی اضافه شد" } },
                    data = new
                    {
                        userId = user.Id,
                        userName = $"{user.FirstName} {user.LastName}",
                        profileImage = user.ProfileImagePath ?? "/images/default-avatar.png",
                        email = user.Email,
                        note = note
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "AddCarbonCopyViewerSubmit", "خطا در افزودن رونوشت", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن ناظر" } }
                });
            }
        }

        /// <summary>
        /// افزودن رونوشت (ناظر دستی) به تسک موجود
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCarbonCopySubmit(int taskId, string userId, string note)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var task = _taskRepository.GetTaskById(taskId);

                if (task == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });
                }

                // بررسی دسترسی
                if (task.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به اضافه کردن ناظر نیستید" } }
                    });
                }

                // افزودن رونوشت
                var carbonCopy = await _carbonCopyRepository.AddCarbonCopyAsync(taskId, userId, currentUserId, note);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "TaskViewers",
                    "AddCarbonCopy",
                    $"افزودن ناظر رونوشتی به تسک {task.TaskCode}",
                    recordId: carbonCopy.Id.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "ناظر رونوشتی با موفقیت اضافه شد" } }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "warning", text = ex.Message } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "AddCarbonCopySubmit", "خطا در افزودن رونوشت", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن ناظر رونوشتی" } }
                });
            }
        }

        /// <summary>
        /// حذف رونوشت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCarbonCopy(int carbonCopyId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _carbonCopyRepository.RemoveCarbonCopyAsync(carbonCopyId, userId);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در حذف ناظر" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "TaskViewers",
                    "RemoveCarbonCopy",
                    "حذف ناظر رونوشتی",
                    recordId: carbonCopyId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "ناظر با موفقیت حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("TaskViewers", "RemoveCarbonCopy", "خطا در حذف رونوشت", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف ناظر" } }
                });
            }
        }

        #region Helper Methods

        private async Task<string> DetermineSupervisionTypeAsync(string supervisorId, int taskId, MahERP.DataModelLayer.Entities.TaskManagement.Tasks task)
        {
            var types = new List<string>();

            // مدیر تیم
            if (task.Team?.ManagerUserId == supervisorId)
                types.Add("مدیر تیم");

            // سازنده
            if (task.CreatorUserId == supervisorId)
                types.Add("سازنده");

            // منتصب
            if (task.TaskAssignments?.Any(a => a.AssignedUserId == supervisorId) == true)
                types.Add("منتصب");

            // سمت بالاتر
            var hasPosition = await _taskRepository.CanViewBasedOnPositionAsync(supervisorId, task);
            if (hasPosition)
                types.Add("سمت بالاتر");

            return types.Any() ? string.Join("، ", types) : "ناظر";
        }

        private async Task<string> RenderGetTaskViewersAsync(int taskId)
        {
            var result = await GetTaskViewers(taskId) as PartialViewResult;
            return await this.RenderViewToStringAsync(result.ViewName, result.Model);
        }

        #endregion
    }
}
