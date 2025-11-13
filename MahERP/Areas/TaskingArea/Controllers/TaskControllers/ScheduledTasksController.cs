using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK")]
    public class ScheduledTasksController : BaseController
    {
        private readonly IScheduledTaskCreationRepository _scheduledTaskRepository;
        private readonly ITaskRepository _taskRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScheduledTasksController(
            IUnitOfWork uow,
            IScheduledTaskCreationRepository scheduledTaskRepository,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            ModuleTrackingBackgroundService moduleTracking,
            IModuleAccessService moduleAccessService,
            IWebHostEnvironment webHostEnvironment)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// صفحه اصلی لیست تسک‌های زمان‌بندی شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                // دریافت لیست تسک‌های زمان‌بندی شده
                var model = await _scheduledTaskRepository.GetScheduledTasksListAsync(userId, isAdmin);

                ViewBag.Title = "تسک‌های زمان‌بندی شده";
                ViewBag.IsAdmin = isAdmin;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ScheduledTasks",
                    "Index",
                    "مشاهده لیست تسک‌های زمان‌بندی شده");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "Index", "خطا در دریافت لیست", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری لیست تسک‌های زمان‌بندی شده";
                return RedirectToAction("Index", "Tasks");
            }
        }

        /// <summary>
        /// مشاهده جزئیات تسک زمان‌بندی شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                var schedule = await _scheduledTaskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    TempData["ErrorMessage"] = "تسک زمان‌بندی شده یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    TempData["ErrorMessage"] = "شما به این تسک دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                // Deserialize کردن TaskViewModel
                var taskModel = _scheduledTaskRepository.DeserializeTaskData(schedule.TaskDataJson);

                if (taskModel == null)
                {
                    TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات تسک";
                    return RedirectToAction(nameof(Index));
                }

                // ساخت ViewModel
                var model = new ScheduledTaskDetailViewModel
                {
                    Schedule = schedule,
                    TaskModel = taskModel,
                    IsAdmin = isAdmin,
                    CanEdit = isAdmin || schedule.CreatedByUserId == userId
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ScheduledTasks",
                    "Details",
                    $"مشاهده جزئیات: {schedule.ScheduleTitle}",
                    recordId: id.ToString());

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "Details", "خطا در دریافت جزئیات", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش تسک زمان‌بندی شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                var schedule = await _scheduledTaskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    TempData["ErrorMessage"] = "تسک زمان‌بندی شده یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    TempData["ErrorMessage"] = "شما مجاز به ویرایش این تسک نیستید";
                    return RedirectToAction(nameof(Index));
                }

                // بازیابی TaskViewModel از JSON
                var taskModel = await _scheduledTaskRepository.GetScheduledTaskForEditAsync(id);

                if (taskModel == null)
                {
                    TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات تسک";
                    return RedirectToAction(nameof(Index));
                }

                // آماده‌سازی لیست‌ها (مثل CreateNewTask)
                taskModel = await _taskRepository.PrepareCreateTaskModelAsync(userId);
                
                // بازگرداندن اطلاعات زمان‌بندی از JSON
                var originalTaskModel = _scheduledTaskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (originalTaskModel != null)
                {
                    taskModel.Title = originalTaskModel.Title;
                    taskModel.Description = originalTaskModel.Description;
                    taskModel.BranchIdSelected = originalTaskModel.BranchIdSelected;
                    taskModel.Priority = originalTaskModel.Priority;
                    taskModel.Important = originalTaskModel.Important;
                    taskModel.TaskType = originalTaskModel.TaskType;
                    taskModel.AssignmentsTaskUser = originalTaskModel.AssignmentsTaskUser;
                    taskModel.Operations = originalTaskModel.Operations;
                }

                // اطلاعات زمان‌بندی
                taskModel.TaskSchedule = new TaskScheduleViewModel
                {
                    IsScheduled = true,
                    ScheduleTitle = schedule.ScheduleTitle,
                    ScheduleDescription = schedule.ScheduleDescription,
                    ScheduleType = schedule.ScheduleType,
                    ScheduledTime = schedule.ScheduledTime,
                    ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                    ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                    StartDatePersian = schedule.StartDate.HasValue 
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(schedule.StartDate.Value, "yyyy/MM/dd")
                        : null,
                    EndDatePersian = schedule.EndDate.HasValue
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(schedule.EndDate.Value, "yyyy/MM/dd")
                        : null,
                    MaxOccurrences = schedule.MaxOccurrences,
                    IsRecurring = schedule.IsRecurring
                };

                ViewBag.ScheduleId = id;
                ViewBag.IsEditMode = true;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ScheduledTasks",
                    "Edit",
                    $"نمایش فرم ویرایش: {schedule.ScheduleTitle}",
                    recordId: id.ToString());

                return View("Edit", taskModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "Edit", "خطا در نمایش فرم ویرایش", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم ویرایش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ذخیره تغییرات تسک زمان‌بندی شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                var schedule = await _scheduledTaskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    TempData["ErrorMessage"] = "تسک زمان‌بندی شده یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    TempData["ErrorMessage"] = "شما مجاز به ویرایش این تسک نیستید";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    model = await _taskRepository.PrepareCreateTaskModelAsync(userId);
                    ViewBag.ScheduleId = id;
                    ViewBag.IsEditMode = true;
                    return View(model);
                }

                // بروزرسانی
                var result = await _scheduledTaskRepository.UpdateScheduledTaskAsync(id, model, userId);

                if (!result)
                {
                    TempData["ErrorMessage"] = "خطا در بروزرسانی تسک زمان‌بندی شده";
                    model = await _taskRepository.PrepareCreateTaskModelAsync(userId);
                    ViewBag.ScheduleId = id;
                    ViewBag.IsEditMode = true;
                    return View(model);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "ScheduledTasks",
                    "Edit",
                    $"بروزرسانی تسک زمان‌بندی شده: {model.TaskSchedule?.ScheduleTitle ?? model.Title}",
                    recordId: id.ToString());

                TempData["SuccessMessage"] = "تسک زمان‌بندی شده با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "Edit", "خطا در ذخیره تغییرات", ex);
                TempData["ErrorMessage"] = $"خطا در بروزرسانی: {ex.Message}";
                model = await _taskRepository.PrepareCreateTaskModelAsync(_userManager.GetUserId(User));
                ViewBag.ScheduleId = id;
                ViewBag.IsEditMode = true;
                return View(model);
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن تسک زمان‌بندی شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                var schedule = await _scheduledTaskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "تسک زمان‌بندی شده یافت نشد" });
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new { success = false, message = "شما مجاز به تغییر وضعیت این تسک نیستید" });
                }

                await _scheduledTaskRepository.ToggleScheduleAsync(id, !schedule.IsScheduleEnabled);

                var statusText = !schedule.IsScheduleEnabled ? "فعال" : "غیرفعال";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "ScheduledTasks",
                    "ToggleStatus",
                    $"تغییر وضعیت به {statusText}: {schedule.ScheduleTitle}",
                    recordId: id.ToString());

                return Json(new
                {
                    success = true,
                    message = $"وضعیت تسک به {statusText} تغییر یافت",
                    isEnabled = !schedule.IsScheduleEnabled
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "ToggleStatus", "خطا در تغییر وضعیت", ex);
                return Json(new { success = false, message = "خطا در تغییر وضعیت" });
            }
        }

        /// <summary>
        /// حذف تسک زمان‌بندی شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                var schedule = await _scheduledTaskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "تسک زمان‌بندی شده یافت نشد" });
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new { success = false, message = "شما مجاز به حذف این تسک نیستید" });
                }

                await _scheduledTaskRepository.DeleteScheduleAsync(id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "ScheduledTasks",
                    "Delete",
                    $"حذف تسک زمان‌بندی شده: {schedule.ScheduleTitle}",
                    recordId: id.ToString());

                return Json(new
                {
                    success = true,
                    message = "تسک زمان‌بندی شده با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف تسک" });
            }
        }
    }

    /// <summary>
    /// ViewModel برای صفحه جزئیات
    /// </summary>
    public class ScheduledTaskDetailViewModel
    {
        public DataModelLayer.Entities.TaskManagement.ScheduledTaskCreation Schedule { get; set; }
        public TaskViewModel TaskModel { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanEdit { get; set; }
    }
}
