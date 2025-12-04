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
using System.Text.Json;
using MahERP.DataModelLayer; // ⭐⭐⭐ برای AppDbContext

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK")]
    public class ScheduledTasksController : BaseController
    {
        private readonly ITaskRepository _taskRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context; // ⭐⭐⭐ اضافه شده

        public ScheduledTasksController(
            IUnitOfWork uow,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context) // ⭐⭐⭐ اضافه شده
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _context = context; // ⭐⭐⭐ اضافه شده
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

                // ⭐⭐⭐ استفاده از TaskRepository به جای ScheduledTaskCreationRepository
                var model = await _taskRepository.GetUserScheduledTasksAsync(userId, isAdmin);

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

                var schedule = await _taskRepository.GetScheduleByIdAsync(id);

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
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);

                if (taskModel == null)
                {
                    TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات تسک";
                    return RedirectToAction(nameof(Index));
                }

                // ⭐⭐⭐ تنظیم فلگ برای نمایش به عنوان Scheduled Task
                ViewBag.IsScheduledTaskDetails = true;
                ViewBag.ScheduleId = id;

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

                // ⭐⭐⭐ استفاده از View جدید
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

                var schedule = await _taskRepository.GetScheduleByIdAsync(id);

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
                var taskModel = await _taskRepository.GetScheduledTaskForEditAsync(id);

                if (taskModel == null)
                {
                    TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات تسک";
                    return RedirectToAction(nameof(Index));
                }

                // آماده‌سازی لیست‌ها (مثل CreateNewTask)
                taskModel = await _taskRepository.PrepareCreateTaskModelAsync(userId);
                
                // بازگرداندن اطلاعات زمان‌بندی از JSON
                var originalTaskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
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

                var schedule = await _taskRepository.GetScheduleByIdAsync(id);

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
                var result = await _taskRepository.UpdateScheduledTaskAsync(id, model, userId);

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

                var schedule = await _taskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "تسک زمان‌بندی شده یافت نشد" });
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new { success = false, message = "شما مجاز به تغییر وضعیت این تسک نیستید" });
                }

                await _taskRepository.ToggleScheduleAsync(id, !schedule.IsScheduleEnabled);

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

                var schedule = await _taskRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "تسک زمان‌بندی شده یافت نشد" });
                }

                // بررسی دسترسی
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new { success = false, message = "شما مجاز به حذف این تسک نیستید" });
                }

                await _taskRepository.DeleteScheduleAsync(id);

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

        #region Task Operations Management (for Scheduled Tasks)

        /// <summary>
        /// ⭐⭐⭐ ذخیره عملیات جدید برای Scheduled Task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTaskOperation(TaskOperationViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐ دریافت Schedule
                var schedule = await _taskRepository.GetScheduleByIdAsync(model.TaskId);
                if (schedule == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک زمان‌بندی شده یافت نشد" } }
                    });
                }

                // ⭐ بررسی دسترسی
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به ویرایش نیستید" } }
                    });
                }

                // ⭐ Deserialize کردن TaskData
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel == null)
                {
                    taskModel = new TaskViewModel { Operations = new List<TaskOperationViewModel>() };
                }

                // ⭐ اضافه کردن عملیات جدید
                if (taskModel.Operations == null)
                    taskModel.Operations = new List<TaskOperationViewModel>();

                var newOperation = new TaskOperationViewModel
                {
                    Title = model.Title,
                    Description = model.Description,
                    OperationOrder = model.OperationOrder,
                    EstimatedHours = model.EstimatedHours,
                    IsCompleted = false
                };

                taskModel.Operations.Add(newOperation);

                // ⭐ بروزرسانی JSON
                schedule.TaskDataJson = JsonSerializer.Serialize(taskModel);
                _context.ScheduledTaskCreation_Tbl.Update(schedule);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "ScheduledTasks",
                    "SaveTaskOperation",
                    $"افزودن عملیات: {model.Title}",
                    recordId: model.TaskId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "عملیات با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "SaveTaskOperation", "خطا در ذخیره عملیات", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره عملیات" } }
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ حذف عملیات از Scheduled Task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaskOperation(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐ در اینجا id همان scheduleId است (نه operationId)
                // باید از query string بگیریم
                var scheduleId = int.Parse(Request.Form["scheduleId"]);
                var operationTitle = Request.Form["operationTitle"];

                var schedule = await _taskRepository.GetScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });
                }

                // بررسی دسترسی
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به حذف نیستید" } }
                    });
                }

                // Deserialize و حذف
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel?.Operations != null)
                {
                    var operation = taskModel.Operations.FirstOrDefault(o => o.Title == operationTitle);
                    if (operation != null)
                    {
                        taskModel.Operations.Remove(operation);

                        // بروزرسانی JSON
                        schedule.TaskDataJson = JsonSerializer.Serialize(taskModel);
                        _context.ScheduledTaskCreation_Tbl.Update(schedule);
                        await _context.SaveChangesAsync();
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "ScheduledTasks",
                    "DeleteTaskOperation",
                    $"حذف عملیات: {operationTitle}",
                    recordId: scheduleId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "عملیات حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "DeleteTaskOperation", "خطا در حذف عملیات", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف عملیات" } }
                });
            }
        }

        #endregion

        #region Task Reminders Management (for Scheduled Tasks)

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست یادآوری‌های Scheduled Task
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskRemindersListPartial(int taskId)
        {
            try
            {
                var schedule = await _taskRepository.GetScheduleByIdAsync(taskId);
                if (schedule == null)
                {
                    return PartialView("_TaskRemindersList", new List<TaskReminderViewModel>());
                }

                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                var reminders = taskModel?.TaskOperationsJson != null
                    ? System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(taskModel.TaskOperationsJson)
                    : new List<TaskReminderViewModel>();

                return PartialView("_TaskRemindersList", new
                {
                    TaskId = taskId,
                    Reminders = reminders ?? new List<TaskReminderViewModel>()
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "GetTaskRemindersListPartial", "خطا در دریافت یادآوری‌ها", ex);
                return PartialView("_TaskRemindersList", new List<TaskReminderViewModel>());
            }
        }

        /// <summary>
        /// ⭐⭐⭐ ذخیره یادآوری جدید برای Scheduled Task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReminder(TaskReminderViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var schedule = await _taskRepository.GetScheduleByIdAsync(model.TaskId);
                if (schedule == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });
                }

                // بررسی دسترسی
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به ویرایش نیستید" } }
                    });
                }

                // Deserialize
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel == null)
                    taskModel = new TaskViewModel();

                // اضافه کردن یادآوری (فعلاً در TaskRemindersJson ذخیره می‌شود)
                var reminders = new List<TaskReminderViewModel>();
                if (!string.IsNullOrEmpty(taskModel.TaskRemindersJson))
                {
                    reminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(taskModel.TaskRemindersJson);
                }

                reminders.Add(model);
                taskModel.TaskRemindersJson = JsonSerializer.Serialize(reminders);

                // بروزرسانی JSON
                schedule.TaskDataJson = JsonSerializer.Serialize(taskModel);
                _context.ScheduledTaskCreation_Tbl.Update(schedule);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "ScheduledTasks",
                    "SaveReminder",
                    $"افزودن یادآوری: {model.Title}",
                    recordId: model.TaskId.ToString());

                // بارگذاری مجدد لیست
                var updatedReminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(taskModel.TaskRemindersJson);
                var html = await this.RenderViewToStringAsync("_TaskRemindersList", new
                {
                    TaskId = model.TaskId,
                    Reminders = updatedReminders
                });

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "reminders-list-container",
                            view = new { result = html }
                        }
                    },
                    message = new[] { new { status = "success", text = "یادآوری اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "SaveReminder", "خطا در ذخیره یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری" } }
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ حذف یادآوری از Scheduled Task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaskReminder(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // id اینجا scheduleId است
                var scheduleId = int.Parse(Request.Form["scheduleId"]);
                var reminderTitle = Request.Form["reminderTitle"];

                var schedule = await _taskRepository.GetScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });
                }

                // بررسی دسترسی
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به حذف نیستید" } }
                    });
                }

                // Deserialize و حذف
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel != null && !string.IsNullOrEmpty(taskModel.TaskRemindersJson))
                {
                    var reminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(taskModel.TaskRemindersJson);
                    var reminder = reminders.FirstOrDefault(r => r.Title == reminderTitle);
                    
                    if (reminder != null)
                    {
                        reminders.Remove(reminder);
                        taskModel.TaskRemindersJson = JsonSerializer.Serialize(reminders);

                        // بروزرسانی JSON
                        schedule.TaskDataJson = JsonSerializer.Serialize(taskModel);
                        _context.ScheduledTaskCreation_Tbl.Update(schedule);
                        await _context.SaveChangesAsync();
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "ScheduledTasks",
                    "DeleteTaskReminder",
                    $"حذف یادآوری: {reminderTitle}",
                    recordId: scheduleId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "یادآوری حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "DeleteTaskReminder", "خطا در حذف یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف یادآوری" } }
                });
            }
        }

        #endregion

        #region Task Assignments Management (for Scheduled Tasks)

        /// <summary>
        /// ⭐⭐⭐ حذف تخصیص کاربر از Scheduled Task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTaskAssignment(int assignmentId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // assignmentId اینجا scheduleId است
                var scheduleId = assignmentId;
                var userIdToRemove = Request.Form["userId"];

                var schedule = await _taskRepository.GetScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });
                }

                // بررسی دسترسی
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && schedule.CreatedByUserId != userId)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به حذف نیستید" } }
                    });
                }

                // Deserialize و حذف
                var taskModel = _taskRepository.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel?.AssignmentsTaskUser != null)
                {
                    var assignment = taskModel.AssignmentsTaskUser.FirstOrDefault(a => a.AssignedUserId == userIdToRemove);
                    if (assignment != null)
                    {
                        taskModel.AssignmentsTaskUser.Remove(assignment);

                        // بروزرسانی JSON
                        schedule.TaskDataJson = JsonSerializer.Serialize(taskModel);
                        _context.ScheduledTaskCreation_Tbl.Update(schedule);
                        await _context.SaveChangesAsync();
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "ScheduledTasks",
                    "RemoveTaskAssignment",
                    $"حذف کاربر از تسک زمان‌بندی شده",
                    recordId: scheduleId.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "کاربر حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ScheduledTasks", "RemoveTaskAssignment", "خطا در حذف کاربر", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف کاربر" } }
                });
            }
        }

        #endregion
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
