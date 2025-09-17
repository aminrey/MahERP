using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions; // اضافه شده
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.TaskControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly ITaskRepository _taskRepository;           
        private readonly IStakeholderRepository _stakeholderRepository;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRoleRepository _roleRepository;
        private readonly TaskNotificationService _taskNotificationService; // سرویس نوتیفیکشن تسک

        public TasksController(
            IUnitOfWork uow,
            ITaskRepository taskRepository,
            IStakeholderRepository stakeholderRepository,
            IBranchRepository branchRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            IWebHostEnvironment webHostEnvironment,
            IRoleRepository roleRepository,
            ActivityLoggerService activityLogger,
            TaskNotificationService taskNotificationService) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _uow = uow;
            _taskRepository = taskRepository;
            _stakeholderRepository = stakeholderRepository;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _roleRepository = roleRepository;
            _taskNotificationService = taskNotificationService;
        }

        // نمایش تقویم تسک‌ها
        [HttpGet]
        [Permission("Tasks", "TaskCalendar", 0)] // Read permission
        public async Task<IActionResult> TaskCalendar()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت تسک‌های کاربر برای نمایش در تقویم
                var calendarTasks = _branchRepository.GetTasksForCalendarView(userId);

                // تبدیل تاریخ‌ها به فرمت مناسب تقویم
                var calendarEvents = calendarTasks.Select(task => new
                {
                    
                    id = task.Id,
                    title = task.Title,
                    start = task.DueDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = task.DueDate?.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss"), // اضافه کردن end

                    backgroundColor = task.CalendarColor,
                    borderColor = task.CalendarColor,
                    textColor = "#ffffff",
                    description = task.Description,
                    taskCode = task.TaskCode,
                    categoryTitle = task.CategoryTitle,
                    stakeholderName = task.StakeholderName,
                    branchName = task.BranchName,
                    statusText = task.StatusText,
                    isCompleted = task.IsCompleted,
                    isOverdue = task.IsOverdue,
                    url = Url.Action("Details", "Tasks", new { id = task.Id, area = "AdminArea" })
                }).ToList();

                ViewBag.CalendarEvents = System.Text.Json.JsonSerializer.Serialize(calendarEvents);
                ViewBag.PageTitle = "تقویم تسک‌ها";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "TaskCalendar",
                    "مشاهده تقویم تسک‌ها"
                );

                return View(calendarTasks);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "TaskCalendar",
                    "خطا در نمایش تقویم تسک‌ها",
                    ex
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }

        // دریافت رویدادهای تقویم برای AJAX
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents(DateTime? start = null, DateTime? end = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت تسک‌ها بر اساس محدوده زمانی
                var calendarTasks = _branchRepository.GetTasksForCalendarView(userId, null, start, end);

                var events = calendarTasks.Select(task => new
                {
                    id = task.Id,
                    title = task.Title,
                    start = task.DueDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = task.DueDate?.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"), // یک روز اضافه می‌کنیم
                    backgroundColor = GetTaskBackgroundColor(task),
                    borderColor = GetTaskBorderColor(task),
                    textColor = "#ffffff",
                    description = task.Description ?? "",
                    extendedProps = new
                    {
                        taskCode = task.TaskCode ?? "",
                        categoryTitle = task.CategoryTitle ?? "",
                        stakeholderName = task.StakeholderName ?? "",
                        branchName = task.BranchName ?? "",
                        statusText = GetTaskStatusText(task),
                        isCompleted = task.IsCompleted,
                        isOverdue = task.IsOverdue,
                        detailUrl = Url.Action("Details", "Tasks", new { id = task.Id, area = "AdminArea" })
                    }
                }).ToList();

                return Json(events);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetCalendarEvents",
                    "خطا در دریافت رویدادهای تقویم",
                    ex
                );

                return Json(new List<object>());
            }
        }

        // متدهای کمکی برای تعیین رنگ‌ها
        private string GetTaskBackgroundColor(TaskCalendarViewModel task)
        {
            if (task.IsCompleted)
                return "#28a745"; // سبز - تکمیل شده
            else if (task.IsOverdue)
                return "#dc3545"; // قرمز - عقب افتاده
            else
                return "#007bff"; // آبی - در حال انجام
        }

        private string GetTaskBorderColor(TaskCalendarViewModel task)
        {
            return GetTaskBackgroundColor(task); // همان رنگ پس‌زمینه
        }

        private string GetTaskStatusText(TaskCalendarViewModel task)
        {
            if (task.IsCompleted)
                return "تکمیل شده";
            else if (task.IsOverdue)
                return "عقب افتاده";
            else
                return "در حال انجام";
        }


        // لیست تسک‌ها - با کنترل سطح دسترسی داده
        [Permission("Tasks", "Index", 0)] // Read permission
        public async Task<IActionResult> Index()
        {
            try
            {
                var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                var userId = _userManager.GetUserId(User);
                TaskListForIndexViewModel Filters = new TaskListForIndexViewModel
                {
                    UserLoginid = userId,
                };

                TaskListForIndexViewModel Model = new TaskListForIndexViewModel();
                
                switch (dataAccessLevel)
                {
                    case 0: // Personal - فقط تسک‌های خود کاربر
                        Model = _taskRepository.GetTaskForIndexByUser(Filters);
                        break;
                    case 1: // Branch - تسک‌های شعبه
                        break;
                    case 2: // All - همه تسک‌ها
                        break;
                    default:
                        break;
                }

                // ثبت لاگ مشاهده لیست تسک‌ها
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "Index",
                    "مشاهده لیست تسک‌ها"
                );

                return View(Model);
            }
            catch (Exception ex)
            {
                // ثبت لاگ خطا
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Index",
                    "خطا در دریافت لیست تسک‌ها",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // نمایش تسک‌های اختصاص داده شده به کاربر جاری
        [Permission("Tasks", "MyTasks", 0)] // Read permission
        public async Task<IActionResult> MyTasks()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var tasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: false);
                var viewModels = _mapper.Map<List<TaskViewModel>>(tasks);
                
                // تکمیل اطلاعات اضافی
                foreach (var viewModel in viewModels)
                {
                    var operations = _taskRepository.GetTaskOperations(viewModel.Id);
                    viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);
                }
                
                ViewBag.Title = "تسک‌های من";
                ViewBag.IsMyTasks = true;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "MyTasks",
                    "مشاهده تسک‌های شخصی"
                );
                
                return View("Index", viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "MyTasks",
                    "خطا در دریافت تسک‌های شخصی",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // جزئیات تسک
        [Permission("Tasks", "Details", 0)] // Read permission
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var task = _taskRepository.GetTaskById(id, includeOperations: true, includeAssignments: true, includeAttachments: true, includeComments: true);
                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Tasks",
                        "Details",
                        "تلاش برای مشاهده تسک غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<TaskViewModel>(task);
                
                // تکمیل اطلاعات عملیات‌ها
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(task.TaskOperations);
                
                // تکمیل اطلاعات اختصاص‌ها
                viewModel.AssignmentsTaskUser = _mapper.Map<List<TaskAssignmentViewModel>>(task.TaskAssignments);

                // علامت‌گذاری نوتیفیکشن‌های مرتبط با این تسک به عنوان خوانده شده
                var currentUserId = _userManager.GetUserId(User);
                await _taskNotificationService.MarkTaskNotificationsAsReadAsync(id, currentUserId);

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "Details",
                    $"مشاهده جزئیات تسک: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Details",
                    "خطا در دریافت جزئیات تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن تسک جدید - نمایش فرم
        // در متد Create (GET)
        [HttpGet]
        [Permission("Tasks", "CreateNewTask", 1)] // Create permission
        public async Task<IActionResult> CreateNewTask(string AddressRouteInComingUrl, int TaskTeamMember = 0)
        {
            try
            {
                if (AddressRouteInComingUrl == null)
                    AddressRouteInComingUrl = "nolink";
                string LogingUser = _userManager.GetUserId(HttpContext.User);

                PopulateDropdowns();

                TaskViewModel Model = _taskRepository.CreateTaskAndCollectData(LogingUser);

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "CreateNewTask",
                    "مشاهده فرم ایجاد تسک جدید"
                );

                return View(Model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CreateNewTask",
                    "خطا در نمایش فرم ایجاد تسک",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // در متد Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "CreateNewTask", 1)]
        public async Task<IActionResult> CreateNewTask(TaskViewModel model)
        {
            List<string> AssimentUserTask = model.AssignmentsSelectedTaskUserArraysString;
            if (model.TaskCode == null)
            {
                model.TaskCode = "0";
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // ایجاد تسک جدید
                    var task = _mapper.Map<Tasks>(model);
                    task.CreateDate = DateTime.Now;
                    task.CreatorUserId = _userManager.GetUserId(User);
                    task.IsActive = model.IsActive;
                    task.IsDeleted = false;
                    task.TaskTypeInput = 1; // کاربر عادی نرم افزار ساخته
                    task.VisibilityLevel = 0; // محرمانه به طور پیش‌فرض
                    task.Priority = 0; // عادی به طور پیش‌فرض
                    task.Important = false;
                    task.Status = 0; // ایجاد شده
                    task.CreationMode = 0; // دستی
                    task.TaskCategoryId = model.TaskCategoryIdSelected;

                    // تبدیل تاریخ شمسی به میلادی
                    if (!string.IsNullOrEmpty(model.DueDatePersian))
                    {
                        task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.DueDatePersian);
                    }

                    // ذخیره در دیتابیس
                    _uow.TaskUW.Create(task);
                    _uow.Save();

                    // ذخیره فایل‌های پیوست
                    if (model.Attachments != null && model.Attachments.Count > 0)
                    {
                        SaveTaskAttachments(task.Id, model.Attachments);
                    }

                    // اختصاص به کاربر جاری (خود کاربر ایجاد کننده)
                    if (!AssimentUserTask.Contains(_userManager.GetUserId(User)))
                    {
                        var Selfassignment = new TaskAssignment
                        {
                            TaskId = task.Id,
                            AssignedUserId = _userManager.GetUserId(User),
                            AssignerUserId = _userManager.GetUserId(User),
                            AssignmentType = 1,
                            AssignmentDate = DateTime.Now,
                            Description = "سازنده تسک"
                        };
                        _uow.TaskAssignmentUW.Create(Selfassignment);
                        _uow.Save();
                    }

                    foreach (var userId in AssimentUserTask)
                    {
                        // اختصاص تسک به کاربر
                        var assignment = new TaskAssignment
                        {
                            TaskId = task.Id,
                            AssignedUserId = userId,
                            AssignerUserId = _userManager.GetUserId(User),
                            AssignmentType = 0, // اصلی
                            AssignmentDate = DateTime.Now,
                            Description = "انتصاب تسک به کاربر",
                        };
                        _uow.TaskAssignmentUW.Create(assignment);
                        _uow.Save();
                    }

                    // ارسال نوتیفیکیشن ایجاد تسک جدید
                    try
                    {
                        await _taskNotificationService.NotifyTaskCreatedAsync(
                            task.Id, 
                            _userManager.GetUserId(User), 
                            AssimentUserTask
                        );
                    }
                    catch (Exception notificationEx)
                    {
                        // لاگ خطای نوتیفیکیشن اما عملیات اصلی را متوقف نکنیم
                        await _activityLogger.LogErrorAsync(
                            "Tasks",
                            "CreateNewTask",
                            "خطا در ارسال نوتیفیکیشن ایجاد تسک",
                            notificationEx,
                            recordId: task.Id.ToString()
                        );
                    }

                    // ثبت لاگ موفقیت
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Tasks",
                        "CreateNewTask",
                        $"ایجاد تسک جدید: {task.Title}",
                        recordId: task.Id.ToString(),
                        entityType: "Tasks",
                        recordTitle: task.Title
                    );

                    TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // ثبت لاگ خطا
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CreateNewTask",
                    "خطا در ایجاد تسک جدید",
                    ex
                );
                
                ModelState.AddModelError("", "خطایی در ثبت تسک رخ داد: " + ex.Message);
            }

            PopulateDropdowns();
            return View(model);
        }

        // ویرایش تسک - نمایش فرم
        [HttpGet]
        [Permission("Tasks", "Edit", 2)] // Edit permission
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var task = _taskRepository.GetTaskById(id, includeOperations: true);
                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Tasks",
                        "Edit",
                        "تلاش برای ویرایش تسک غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<TaskViewModel>(task);
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(task.TaskOperations);
                
                // تبدیل تاریخ میلادی به شمسی
                if (task.DueDate.HasValue)
                {
                    viewModel.DueDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy/MM/dd HH:mm");
                }
                
                PopulateDropdowns();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "Edit",
                    $"مشاهده فرم ویرایش تسک: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Edit",
                    "خطا در نمایش فرم ویرایش تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ویرایش تسک - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "Edit", 2)] // Edit permission
        public async Task<IActionResult> Edit(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // دریافت تسک از دیتابیس
                    var task = _uow.TaskUW.GetById(model.Id);
                    if (task == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "Tasks",
                            "Edit",
                            "تلاش برای ویرایش تسک غیرموجود",
                            recordId: model.Id.ToString()
                        );
                        return RedirectToAction("ErrorView", "Home");
                    }

                    // ذخیره مقادیر قبلی برای تشخیص تغییرات
                    var oldValues = new
                    {
                        task.Title,
                        task.Description,
                        task.DueDate,
                        task.Priority,
                        task.IsActive
                    };

                    // تشخیص تغییرات قبل از ویرایش
                    var changeDetails = _taskNotificationService.DetectTaskChanges(oldValues, model);

                    // به‌روزرسانی اطلاعات
                    _mapper.Map(model, task);
                    task.LastUpdateDate = DateTime.Now;
                    
                    // تبدیل تاریخ شمسی به میلادی
                    if (!string.IsNullOrEmpty(model.DueDatePersian))
                    {
                        task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.DueDatePersian);
                    }
                    else
                    {
                        task.DueDate = null;
                    }
                    
                    _uow.TaskUW.Update(task);
                    _uow.Save();
                    
                    // ذخیره فایل‌های پیوست
                    if (model.Attachments != null && model.Attachments.Count > 0)
                    {
                        SaveTaskAttachments(task.Id, model.Attachments);
                    }

                    // ارسال نوتیفیکیشن ویرایش تسک (فقط اگر تغییری وجود داشته باشد)
                    if (changeDetails.Any())
                    {
                        try
                        {
                            await _taskNotificationService.NotifyTaskEditedAsync(
                                task.Id, 
                                _userManager.GetUserId(User), 
                                changeDetails
                            );
                        }
                        catch (Exception notificationEx)
                        {
                            // لاگ خطای نوتیفیکشن اما عملیات اصلی را متوقف نکنیم
                            await _activityLogger.LogErrorAsync(
                                "Tasks",
                                "Edit",
                                "خطا در ارسال نوتیفیکیشن ویرایش تسک",
                                notificationEx,
                                recordId: task.Id.ToString()
                            );
                        }
                    }

                    // مقادیر جدید برای لاگ
                    var newValues = new
                    {
                        task.Title,
                        task.Description,
                        task.DueDate,
                        task.Priority,
                        task.IsActive
                    };

                    // ثبت لاگ تغییرات
                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Tasks",
                        "Edit",
                        $"ویرایش تسک: {task.Title}",
                        oldValues,
                        newValues,
                        recordId: task.Id.ToString(),
                        entityType: "Tasks",
                        recordTitle: task.Title
                    );

                    TempData["SuccessMessage"] = "تسک با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "Edit",
                        "خطا در ویرایش تسک",
                        ex,
                        recordId: model.Id.ToString()
                    );
                    
                    ModelState.AddModelError("", "خطایی در ویرایش تسک رخ داد: " + ex.Message);
                }
            }
            
            PopulateDropdowns();
            return View(model);
        }

        // فعال/غیرفعال کردن تسک - نمایش مودال تأیید
        [HttpGet]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                if (task.IsActive)
                {
                    // غیرفعال کردن
                    ViewBag.themeclass = "bg-gd-fruit";
                    ViewBag.ModalTitle = "غیرفعال کردن تسک";
                    ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                }
                else
                {
                    // فعال کردن
                    ViewBag.themeclass = "bg-gd-lake";
                    ViewBag.ModalTitle = "فعال کردن تسک";
                    ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "ToggleActiveStatus",
                    $"مشاهده فرم تغییر وضعیت تسک: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return PartialView("_ToggleActiveStatus", _mapper.Map<TaskViewModel>(task));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ToggleActiveStatus",
                    "خطا در نمایش فرم تغییر وضعیت",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // فعال/غیرفعال کردن تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatusPost(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                var oldStatus = task.IsActive;
                task.IsActive = !task.IsActive;
                task.LastUpdateDate = DateTime.Now;
                
                _uow.TaskUW.Update(task);
                _uow.Save();

                // ثبت لاگ
                await _activityLogger.LogChangeAsync(
                    ActivityTypeEnum.Edit,
                    "Tasks",
                    "ToggleActiveStatus",
                    $"تغییر وضعیت تسک: {task.Title} از {(oldStatus ? "فعال" : "غیرفعال")} به {(task.IsActive ? "فعال" : "غیرفعال")}]",
                    new { IsActive = oldStatus },
                    new { IsActive = task.IsActive },
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ToggleActiveStatus",
                    "خطا در تغییر وضعیت تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // حذف تسک - نمایش مودال تأیید
        [HttpGet]
        [Permission("Tasks", "Delete", 3)] // Delete permission
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ViewTitle = "حذف تسک";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "Delete",
                    $"مشاهده فرم حذف تسک: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return PartialView("_DeleteTask", _mapper.Map<TaskViewModel>(task));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Delete",
                    "خطا در نمایش فرم حذف تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // حذف تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "Delete", 3)] // Delete permission
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                var taskTitle = task.Title;
                task.IsDeleted = true;
                task.LastUpdateDate = DateTime.Now;
                
                _uow.TaskUW.Update(task);
                _uow.Save();

                // ثبت لاگ حذف
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Tasks",
                    "Delete",
                    $"حذف تسک: {taskTitle}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: taskTitle
                );

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Delete",
                    "خطا در حذف تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن عملیات به تسک - نمایش مودال
        [HttpGet]
        public async Task<IActionResult> AddOperation(int taskId)
        {
            try
            {
                var task = _uow.TaskUW.GetById(taskId);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                // تعیین ترتیب پیش‌فرض برای عملیات جدید
                var operations = _taskRepository.GetTaskOperations(taskId);
                int nextOrder = operations.Count > 0 ? operations.Max(o => o.OperationOrder) + 1 : 1;

                ViewBag.TaskId = taskId;
                ViewBag.TaskTitle = task.Title;
                ViewBag.OperationOrder = nextOrder;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "AddOperation",
                    $"مشاهده فرم افزودن عملیات برای تسک: {task.Title}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return PartialView("_AddOperation", new TaskOperationViewModel
                {
                    TaskId = taskId,
                    OperationOrder = nextOrder,
                    IsCompleted = false
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "AddOperation",
                    "خطا در نمایش فرم افزودن عملیات",
                    ex,
                    recordId: taskId.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن عملیات به تسک - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOperation(TaskOperationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = _uow.TaskUW.GetById(model.TaskId);
                    if (task == null)
                        return RedirectToAction("ErrorView", "Home");

                    var operation = _mapper.Map<TaskOperation>(model);
                    operation.CreateDate = DateTime.Now;
                    operation.CreatorUserId = _userManager.GetUserId(User);

                    _uow.TaskOperationUW.Create(operation);
                    _uow.Save();

                    // ثبت لاگ
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Tasks",
                        "AddOperation",
                        $"افزودن عملیات جدید به تسک: {task.Title} - عملیات: {operation.Title}",
                        recordId: model.TaskId.ToString(),
                        entityType: "TaskOperation",
                        recordTitle: operation.Title
                    );

                    return RedirectToAction(nameof(Details), new { id = model.TaskId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "AddOperation",
                        "خطا در افزودن عملیات جدید",
                        ex,
                        recordId: model.TaskId.ToString()
                    );
                    
                    ModelState.AddModelError("", "خطایی در افزودن عملیات رخ داد: " + ex.Message);
                }
            }

            var currentTask = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = currentTask.Title;
            ViewBag.OperationOrder = model.OperationOrder;

            return PartialView("_AddOperation", model);
        }

        // ویرایش عملیات تسک - نمایش مودال
        [HttpGet]
        public async Task<IActionResult> EditOperation(int id)
        {
            try
            {
                var operation = _taskRepository.GetTaskOperationById(id);
                if (operation == null)
                    return RedirectToAction("ErrorView", "Home");

                var task = _uow.TaskUW.GetById(operation.TaskId);
                ViewBag.TaskId = operation.TaskId;
                ViewBag.TaskTitle = task.Title;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "EditOperation",
                    $"مشاهده فرم ویرایش عملیات: {operation.Title}",
                    recordId: id.ToString(),
                    entityType: "TaskOperation",
                    recordTitle: operation.Title
                );

                return PartialView("_EditOperation", _mapper.Map<TaskOperationViewModel>(operation));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "EditOperation",
                    "خطا در نمایش فرم ویرایش عملیات",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ویرایش عملیات تسک - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOperation(TaskOperationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var operation = _uow.TaskOperationUW.GetById(model.Id);
                    if (operation == null)
                        return RedirectToAction("ErrorView", "Home");

                    // ذخیره مقادیر قبلی
                    var oldValues = new
                    {
                        operation.Title,
                        operation.Description,
                        operation.IsCompleted,
                        operation.OperationOrder
                    };

                    // اگر وضعیت تکمیل تغییر کرده است
                    bool completionChanged = operation.IsCompleted != model.IsCompleted;
                    
                    // به‌روزرسانی اطلاعات
                    _mapper.Map(model, operation);
                    
                    // اگر عملیات به تازگی تکمیل شده است
                    if (completionChanged && model.IsCompleted)
                    {
                        operation.CompletionDate = DateTime.Now;
                        operation.CompletedByUserId = _userManager.GetUserId(User);
                    }
                    // اگر عملیات از حالت تکمیل خارج شده است
                    else if (completionChanged && !model.IsCompleted)
                    {
                        operation.CompletionDate = null;
                        operation.CompletedByUserId = null;
                    }

                    _uow.TaskOperationUW.Update(operation);
                    _uow.Save();

                    // مقادیر جدید
                    var newValues = new
                    {
                        operation.Title,
                        operation.Description,
                        operation.IsCompleted,
                        operation.OperationOrder
                    };

                    // ثبت لاگ تغییرات
                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Tasks",
                        "EditOperation",
                        $"ویرایش عملیات: {operation.Title}",
                        oldValues,
                        newValues,
                        recordId: operation.Id.ToString(),
                        entityType: "TaskOperation",
                        recordTitle: operation.Title
                    );
                    
                    // بررسی اگر همه عملیات‌های تسک تکمیل شده‌اند، تسک را تکمیل کنیم
                    UpdateTaskCompletionStatus(operation.TaskId);

                    return RedirectToAction(nameof(Details), new { id = operation.TaskId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "EditOperation",
                        "خطا در ویرایش عملیات",
                        ex,
                        recordId: model.Id.ToString()
                    );
                    
                    ModelState.AddModelError("", "خطایی در ویرایش عملیات رخ داد: " + ex.Message);
                }
            }

            var task = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = task.Title;

            return PartialView("_EditOperation", model);
        }

        // حذف عملیات تسک - نمایش مودال تأیید
        [HttpGet]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            try
            {
                var operation = _taskRepository.GetTaskOperationById(id);
                if (operation == null)
                    return RedirectToAction("ErrorView", "Home");

                var task = _uow.TaskUW.GetById(operation.TaskId);
                ViewBag.TaskId = operation.TaskId;
                ViewBag.TaskTitle = task.Title;
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                ViewBag.themeclass = "bg-gd-fruit";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "DeleteOperation",
                    $"مشاهده فرم حذف عملیات: {operation.Title}",
                    recordId: id.ToString(),
                    entityType: "TaskOperation",
                    recordTitle: operation.Title
                );

                return PartialView("_DeleteOperation", _mapper.Map<TaskOperationViewModel>(operation));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "DeleteOperation",
                    "خطا در نمایش فرم حذف عملیات",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // حذف عملیات تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOperationPost(int id)
        {
            try
            {
                var operation = _uow.TaskOperationUW.GetById(id);
                if (operation == null)
                    return RedirectToAction("ErrorView", "Home");

                int taskId = operation.TaskId;
                var operationTitle = operation.Title;

                _uow.TaskOperationUW.Delete(operation);
                _uow.Save();

                // ثبت لاگ حذف
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Tasks",
                    "DeleteOperation",
                    $"حذف عملیات: {operationTitle}",
                    recordId: id.ToString(),
                    entityType: "TaskOperation",
                    recordTitle: operationTitle
                );
                
                // بازتعیین ترتیب عملیات‌ها
                ReorderOperations(taskId);
                
                // بررسی وضعیت تکمیل تسک
                UpdateTaskCompletionStatus(taskId);

                return RedirectToAction(nameof(Details), new { id = taskId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "DeleteOperation",
                    "خطا در حذف عملیات",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // تغییر وضعیت تکمیل عملیات - اکشن AJAX
        [HttpPost]
        public async Task<IActionResult> ToggleOperationCompletion(int id)
        {
            try
            {
                var operation = _uow.TaskOperationUW.GetById(id);
                if (operation == null)
                    return Json(new { success = false, message = "عملیات یافت نشد" });

                var oldStatus = operation.IsCompleted;
                operation.IsCompleted = !operation.IsCompleted;
                
                if (operation.IsCompleted)
                {
                    operation.CompletionDate = DateTime.Now;
                    operation.CompletedByUserId = _userManager.GetUserId(User);
                }
                else
                {
                    operation.CompletionDate = null;
                    operation.CompletedByUserId = null;
                }

                _uow.TaskOperationUW.Update(operation);
                _uow.Save();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Tasks",
                    "ToggleOperationCompletion",
                    $"تغییر وضعیت تکمیل عملیات: {operation.Title} از {(oldStatus ? "تکمیل شده" : "تکمیل نشده")} به {(operation.IsCompleted ? "تکمیل شده" : "تکمیل نشده")}",
                    recordId: id.ToString(),
                    entityType: "TaskOperation",
                    recordTitle: operation.Title
                );
                
                // بررسی وضعیت تکمیل تسک
                UpdateTaskCompletionStatus(operation.TaskId);

                return Json(new { 
                    success = true, 
                    isCompleted = operation.IsCompleted,
                    completionDate = operation.CompletionDate != null ? ConvertDateTime.ConvertMiladiToShamsi(operation.CompletionDate, "yyyy/MM/dd") : null
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ToggleOperationCompletion",
                    "خطا در تغییر وضعیت تکمیل عملیات",
                    ex,
                    recordId: id.ToString()
                );
                
                return Json(new { success = false, message = "خطا در تغییر وضعیت عملیات" });
            }
        }

        // اختصاص تسک به کاربر - نمایش مودال
        [HttpGet]
        public async Task<IActionResult> AssignTask(int taskId)
        {
            try
            {
                var task = _uow.TaskUW.GetById(taskId);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.TaskId = taskId;
                ViewBag.TaskTitle = task.Title;
                ViewBag.Users = new SelectList(_userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                    "Id", "FullName");

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "AssignTask",
                    $"مشاهده فرم اختصاص تسک: {task.Title}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return PartialView("_AssignTask", new TaskAssignmentViewModel
                {
                    TaskId = taskId,
                    AssignDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "AssignTask",
                    "خطا در نمایش فرم اختصاص تسک",
                    ex,
                    recordId: taskId.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // اختصاص تسک به کاربر - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTask(TaskAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = _uow.TaskUW.GetById(model.TaskId);
                    if (task == null)
                        return RedirectToAction("ErrorView", "Home");
                    
                    // بررسی آیا این کاربر قبلاً به این تسک اختصاص داده شده است
                    var existingAssignment = _taskRepository.GetTaskAssignmentByUserAndTask(model.AssignedUserId, model.TaskId);
                    if (existingAssignment != null)
                    {
                        ModelState.AddModelError("AssignedUserId", "این کاربر قبلاً به این تسک اختصاص داده شده است");
                        ViewBag.TaskId = model.TaskId;
                        ViewBag.TaskTitle = task.Title;
                        ViewBag.Users = new SelectList(_userManager.Users
                            .Where(u => u.IsActive && !u.IsRemoveUser)
                            .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                            "Id", "FullName");
                        return PartialView("_AssignTask", model);
                    }

                    var assignment = _mapper.Map<TaskAssignment>(model);
                    assignment.AssignmentDate = DateTime.Now;
                    assignment.AssignerUserId = _userManager.GetUserId(User);

                    _uow.TaskAssignmentUW.Create(assignment);
                    _uow.Save();

                    // دریافت اطلاعات کاربر اختصاص یافته
                    var assignedUser = await _userManager.FindByIdAsync(model.AssignedUserId);

                    // ارسال نوتیفیکیشن اختصاص کاربر جدید
                    try
                    {
                        await _taskNotificationService.NotifyUserAssignedAsync(
                            model.TaskId,
                            model.AssignedUserId,
                            _userManager.GetUserId(User)
                        );
                    }
                    catch (Exception notificationEx)
                    {
                        await _activityLogger.LogErrorAsync(
                            "Tasks",
                            "AssignTask",
                            "خطا در ارسال نوتیفیکیشن اختصاص کاربر",
                            notificationEx,
                            recordId: model.TaskId.ToString()
                        );
                    }

                    // ثبت لاگ
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Tasks",
                        "AssignTask",
                        $"اختصاص تسک: {task.Title} به کاربر: {assignedUser?.FirstName} {assignedUser?.LastName}",
                        recordId: model.TaskId.ToString(),
                        entityType: "TaskAssignment",
                        recordTitle: $"{task.Title} - {assignedUser?.FirstName} {assignedUser?.LastName}"
                    );

                    return RedirectToAction(nameof(Details), new { id = model.TaskId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "AssignTask",
                        "خطا در اختصاص تسک",
                        ex,
                        recordId: model.TaskId.ToString()
                    );
                    
                    ModelState.AddModelError("", "خطایی در اختصاص تسک رخ داد: " + ex.Message);
                }
            }

            var currentTask = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = currentTask.Title;
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            return PartialView("_AssignTask", model);
        }

        // حذف اختصاص تسک - نمایش مودال تأیید
        [HttpGet]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            try
            {
                var assignment = _taskRepository.GetTaskAssignmentById(id);
                if (assignment == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.TaskId = assignment.TaskId;
                ViewBag.TaskTitle = assignment.Task.Title;
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                ViewBag.themeclass = "bg-gd-fruit";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "RemoveAssignment",
                    $"مشاهده فرم حذف اختصاص تسک: {assignment.Task.Title}",
                    recordId: id.ToString(),
                    entityType: "TaskAssignment",
                    recordTitle: assignment.Task.Title
                );

                return PartialView("_RemoveAssignment", _mapper.Map<TaskAssignmentViewModel>(assignment));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "RemoveAssignment",
                    "خطا در نمایش فرم حذف اختصاص",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // حذف اختصاص تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignmentPost(int id)
        {
            try
            {
                var assignment = _uow.TaskAssignmentUW.GetById(id);
                if (assignment == null)
                    return RedirectToAction("ErrorView", "Home");

                int taskId = assignment.TaskId;
                var task = _uow.TaskUW.GetById(taskId);
                var assignedUser = await _userManager.FindByIdAsync(assignment.AssignedUserId);

                // ارسال نوتیفیکیشن حذف کاربر از تسک
                try
                {
                    await _taskNotificationService.NotifyUserRemovedAsync(
                        taskId,
                        assignment.AssignedUserId,
                        _userManager.GetUserId(User)
                    );
                }
                catch (Exception notificationEx)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "RemoveAssignment",
                        "خطا در ارسال نوتیفیکیشن حذف کاربر",
                        notificationEx,
                        recordId: id.ToString()
                    );
                }

                _uow.TaskAssignmentUW.Delete(assignment);
                _uow.Save();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Tasks",
                    "RemoveAssignment",
                    $"حذف اختصاص تسک: {task?.Title} از کاربر: {assignedUser?.FirstName} {assignedUser?.LastName}",
                    recordId: id.ToString(),
                    entityType: "TaskAssignment",
                    recordTitle: $"{task?.Title} - {assignedUser?.FirstName} {assignedUser?.LastName}"
                );

                return RedirectToAction(nameof(Details), new { id = taskId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "RemoveAssignment",
                    "خطا در حذف اختصاص تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // تکمیل تسک - نمایش مودال
        [HttpGet]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                // دریافت اطلاعات تسک از دیتابیس
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی دسترسی کاربر به این تسک (اختیاری - بر اساس نیاز سیستم)
                var currentUserId = _userManager.GetUserId(HttpContext.User);
                
                // تنظیم ViewBag برای نمایش صحیح مودال
                if (task.CompletionDate.HasValue)
                {
                    // حالت بازگشایی تسک
                    ViewBag.ModalTitle = "بازگشایی تسک";
                    ViewBag.ButtonText = "بازگشایی تسک";
                    ViewBag.ButonClass = "btn btn-warning";
                    ViewBag.themeclass = "bg-warning";
                    ViewBag.IsReopening = true;
                }
                else
                {
                    // حالت تکمیل تسک
                    ViewBag.ModalTitle = "تکمیل تسک";
                    ViewBag.ButtonText = "تکمیل تسک";
                    ViewBag.ButonClass = "btn btn-success";
                    ViewBag.themeclass = "bg-success";
                    ViewBag.IsReopening = false;
                }

                // ایجاد ViewModel با اطلاعات کامل تسک
                var viewModel = _mapper.Map<TaskViewModel>(task);
                
                // دریافت عملیات‌های تسک برای نمایش در مودال
                var operations = _taskRepository.GetTaskOperations(task.Id);
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);

                // ثبت لاگ فعالیت کاربر
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "CompleteTask",
                    $"مشاهده فرم تکمیل/بازگشایی تسک: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                // بازگشت PartialView برای نمایش در مودال
                return PartialView("_CompleteTask", viewModel);
            }
            catch (Exception ex)
            {
                // ثبت لاگ خطا
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CompleteTask",
                    "خطا در نمایش فرم تکمیل/بازگشایی تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                // در صورت بروز خطا، بازگشت به صفحه خطا
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // تکمیل تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTaskPost(int id, bool isReopening)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                var oldCompletionDate = task.CompletionDate;
                var taskTitle = task.Title;

                if (isReopening)
                {
                    task.CompletionDate = null;
                    task.SupervisorApprovedDate = null;
                    task.ManagerApprovedDate = null;

                    // ثبت لاگ بازگشایی
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Tasks",
                        "ReopenTask",
                        $"بازگشایی تسک: {taskTitle}",
                        recordId: id.ToString(),
                        entityType: "Tasks",
                        recordTitle: taskTitle
                    );
                }
                else
                {
                    task.CompletionDate = DateTime.Now;
                    
                    // اگر کاربر جاری سرپرست یا مدیر است، تایید متناظر را هم ثبت کنیم
                    var currentUser = _userManager.GetUserAsync(User).Result;
                    var isManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                    var isSupervisor = User.IsInRole("Supervisor") || isManager;
                    
                    if (isSupervisor)
                    {
                        task.SupervisorApprovedDate = DateTime.Now;
                    }
                    
                    if (isManager)
                    {
                        task.ManagerApprovedDate = DateTime.Now;
                    }
                    
                    // همه عملیات‌ها را به حالت تکمیل شده تغییر دهیم
                    var operations = _taskRepository.GetTaskOperations(id);
                    foreach (var operation in operations)
                    {
                        if (!operation.IsCompleted)
                        {
                            operation.IsCompleted = true;
                            operation.CompletionDate = DateTime.Now;
                            operation.CompletedByUserId = _userManager.GetUserId(User);
                        
                            _uow.TaskOperationUW.Update(operation);
                        }
                    }

                    // ارسال نوتیفیکیشن تکمیل تسک
                    try
                    {
                        await _taskNotificationService.NotifyTaskCompletedAsync(
                            id,
                            _userManager.GetUserId(User)
                        );
                    }
                    catch (Exception notificationEx)
                    {
                        await _activityLogger.LogErrorAsync(
                            "Tasks",
                            "CompleteTask",
                            "خطا در ارسال نوتیفیکشن تکمیل تسک",
                            notificationEx,
                            recordId: id.ToString()
                        );
                    }

                    // ثبت لاگ تکمیل
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Approve,
                        "Tasks",
                        "CompleteTask",
                        $"تکمیل تسک: {taskTitle}",
                        recordId: id.ToString(),
                        entityType: "Tasks",
                        recordTitle: taskTitle
                    );
                }
                
                task.LastUpdateDate = DateTime.Now;
                _uow.TaskUW.Update(task);
                _uow.Save();

                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CompleteTask",
                    "خطا در تکمیل/بازگشایی تسک",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // تایید تسک توسط سرپرست - اکشن AJAX
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Supervisor")]
        [Permission("Tasks  ", "ApproveTaskBySupervisor", 4)] // Approve permission
        public async Task<IActionResult> ApproveTaskBySupervisor(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Approve,
                        "Tasks",
                        "ApproveTaskBySupervisor",
                        "تلاش برای تایید تسک غیرموجود",
                        recordId: id.ToString()
                    );
                    return Json(new { success = false, message = "تسک یافت نشد" });
                }
                    
                if (!task.CompletionDate.HasValue)
                    return Json(new { success = false, message = "این تسک هنوز تکمیل نشده است" });

                task.SupervisorApprovedDate = DateTime.Now;
                task.LastUpdateDate = DateTime.Now;
                
                _uow.TaskUW.Update(task);
                _uow.Save();

                // ثبت لاگ تایید سرپرست
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Approve,
                    "Tasks",
                    "ApproveTaskBySupervisor",
                    $"تایید تسک توسط سرپرست: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return Json(new { 
                    success = true, 
                    approvalDate = ConvertDateTime.ConvertMiladiToShamsi(task.SupervisorApprovedDate,"yyyy/MM/dd")
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ApproveTaskBySupervisor",
                    "خطا در تایید تسک توسط سرپرست",
                    ex,
                    recordId: id.ToString()
                );
                
                return Json(new { success = false, message = "خطا در تایید تسک" });
            }
        }

        // تایید تسک توسط مدیر - اکشن AJAX
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [Permission("Tasks  ", "ApproveTaskByManager", 4)] // Approve permission
        public async Task<IActionResult> ApproveTaskByManager(int id)
        {
            try
            {
                var task = _uow.TaskUW.GetById(id);
                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Approve,
                        "Tasks",
                        "ApproveTaskByManager",
                        "تلاش برای تایید تسک غیرموجود توسط مدیر",
                        recordId: id.ToString()
                    );
                    return Json(new { success = false, message = "تسک یافت نشد" });
                }
                    
                if (!task.CompletionDate.HasValue)
                    return Json(new { success = false, message = "این تسک هنوز تکمیل نشده است" });
                    
                if (!task.SupervisorApprovedDate.HasValue)
                    return Json(new { success = false, message = "این تسک هنوز توسط سرپرست تایید نشده است" });

                task.ManagerApprovedDate = DateTime.Now;
                task.LastUpdateDate = DateTime.Now;
                
                _uow.TaskUW.Update(task);
                _uow.Save();

                // ثبت لاگ تایید مدیر
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Approve,
                    "Tasks",
                    "ApproveTaskByManager",
                    $"تایید تسک توسط مدیر: {task.Title}",
                    recordId: id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return Json(new { 
                    success = true, 
                    approvalDate = ConvertDateTime.ConvertMiladiToShamsi(task.ManagerApprovedDate, "yyyy/MM/dd")
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ApproveTaskByManager",
                    "خطا در تایید تسک توسط مدیر",
                    ex,
                    recordId: id.ToString()
                );
                
                return Json(new { success = false, message = "خطا در تایید تسک" });
            }
        }

        // جستجوی پیشرفته - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch()
        {
            try
            {
                ViewBag.Categories = new SelectList(_taskRepository.GetAllCategories(), "Id", "Title");
                ViewBag.Users = new SelectList(_userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                    "Id", "FullName");
                    
                ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                    .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                    "Id", "FullName");

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "AdvancedSearch",
                    "مشاهده فرم جستجوی پیشرفته تسک‌ها"
                );

                return PartialView("_AdvancedSearch", new TaskSearchViewModel());
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "AdvancedSearch",
                    "خطا در نمایش فرم جستجوی پیشرفته",
                    ex
                );
                
                return PartialView("_AdvancedSearch", new TaskSearchViewModel());
            }
        }

        // جستجوی پیشرفته - پردازش جستجو
        [HttpPost]
        public async Task<IActionResult> Search(TaskSearchViewModel model)
        {
            try
            {
                // انجام جستجو بر اساس پارامترهای دریافتی
                var tasks = _taskRepository.SearchTasks(
                    model.SearchTerm, 
                    model.CategoryId, 
                    model.AssignedUserId,
                    model.IsCompleted);
                    
                var viewModels = _mapper.Map<List<TaskViewModel>>(tasks);
                
                // تکمیل اطلاعات اضافی
                foreach (var viewModel in viewModels)
                {
                    var operations = _taskRepository.GetTaskOperations(viewModel.Id);
                    viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);
                }

                // ذخیره پارامترهای جستجو در ViewBag برای استفاده در صفحه نتایج
                ViewBag.SearchModel = model;
                ViewBag.Title = "نتایج جستجو";

                // ثبت لاگ جستجو
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Search,
                    "Tasks",
                    "Search",
                    $"جستجوی پیشرفته تسک‌ها - کلمه کلیدی: {model.SearchTerm ?? "خالی"}, تعداد نتایج: {viewModels.Count}"
                );

                return View("SearchResults", viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "Search",
                    "خطا در جستجوی پیشرفته تسک‌ها",
                    ex
                );
                
                return View("SearchResults", new List<TaskViewModel>());
            }
        }

        // توابع کمکی
        
        // پر کردن لیست‌های کشویی
        private void PopulateDropdowns()
        {
            ViewBag.Categories = new SelectList(_taskRepository.GetAllCategories(), "Id", "Title");
            ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                "Id", "FullName");
        }
        
        // ذخیره فایل‌های پیوست
        private void SaveTaskAttachments(int taskId, List<IFormFile> files)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks", taskId.ToString());
            
            // ایجاد پوشه اگر وجود ندارد
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // نام فایل یکتا
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    
                    // ذخیره اطلاعات فایل در دیتابیس
                    var attachment = new TaskAttachment
                    {
                        TaskId = taskId,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        FilePath = $"/uploads/tasks/{taskId}/{uniqueFileName}",
                        FileType = file.ContentType,
                        UploadDate = DateTime.Now,
                        UploaderUserId = _userManager.GetUserId(User)
                    };
                    
                    _uow.TaskAttachmentUW.Create(attachment);
                }
            }
            
            _uow.Save();
        }
        
        // بازتعیین ترتیب عملیات‌ها
        private void ReorderOperations(int taskId)
        {
            var operations = _taskRepository.GetTaskOperations(taskId)
                .OrderBy(o => o.OperationOrder)
                .ToList();
                
            for (int i = 0; i < operations.Count; i++)
            {
                operations[i].OperationOrder = i + 1;
                _uow.TaskOperationUW.Update(operations[i]);
            }
            
            _uow.Save();
        }
        
        // بررسی و به‌روزرسانی وضعیت تکمیل تسک
        private void UpdateTaskCompletionStatus(int taskId)
        {
            var task = _uow.TaskUW.GetById(taskId);
            var operations = _taskRepository.GetTaskOperations(taskId);
            
            // اگر عملیاتی وجود ندارد، کاری انجام نمی‌دهیم
            if (!operations.Any())
                return;
                
            // اگر همه عملیات‌ها تکمیل شده‌اند، تسک را تکمیل کنیم
            bool allCompleted = operations.All(o => o.IsCompleted);
            
            if (allCompleted && !task.CompletionDate.HasValue)
            {
                task.CompletionDate = DateTime.Now;
                task.LastUpdateDate = DateTime.Now;
                _uow.TaskUW.Update(task);
                _uow.Save();
            }
            else if (!allCompleted && task.CompletionDate.HasValue)
            {
                task.CompletionDate = null;
                task.SupervisorApprovedDate = null;
                task.ManagerApprovedDate = null;
                task.LastUpdateDate = DateTime.Now;
                _uow.TaskUW.Update(task);
                _uow.Save();
            }
        }

        // متد کمکی برای دریافت شعبه کاربر
        private int GetUserBranchId(string userId)
        {
            var branchUser = _uow.BranchUserUW.Get(bu => bu.UserId == userId && bu.IsActive).FirstOrDefault();
            return branchUser?.BranchId ?? 1; // پیش‌فرض شعبه اصلی
        }

        /// <summary>
        /// بروزرسانی لیست کاربران بر اساس شعبه انتخاب شده
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>PartialView حاوی لیست کاربران شعبه</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelect(int branchId)
        {
            try
            {
                // دریافت کاربران شعبه انتخاب شده با استفاده از Repository
                var branchUsersViewModels = _branchRepository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                var stakeholdersViewModels = _stakeholderRepository.GetStakeholdersByBranchId(branchId);

                // دریافت دسته‌بندی‌های تسک شعبه انتخاب شده
                var branchTaskCategories = _branchRepository.GetTaskCategoriesForBranchStakeholder(branchId);

                // رندر کردن partial views
                var usersPartialView = await this.RenderViewToStringAsync("_BranchUsersSelect", branchUsersViewModels);
                var stakeholdersPartialView = await this.RenderViewToStringAsync("_BranchStakeholdersSelect", stakeholdersViewModels);
                var categoriesPartialView = await this.RenderViewToStringAsync("_BranchTaskCategoriesSelect", branchTaskCategories);

                // اضافه کردن به response - بروزرسانی کاربران، طرف حساب‌ها و دسته‌بندی‌ها
                var viewList = new List<object>
        {
            new
            {
                elementId = "UsersDiv",
                view = new
                {
                    result = usersPartialView
                }
            },
            new
            {
                elementId = "StakeholdersDiv",
                view = new
                {
                    result = stakeholdersPartialView
                }
            },
            new
            {
                elementId = "TaskCategoriesDiv",
                view = new
                {
                    result = categoriesPartialView
                }
            }
        };

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "BranchTriggerSelect",
                    $"بارگذاری کاربران ({branchUsersViewModels?.Count ?? 0}), طرف حساب‌ها ({stakeholdersViewModels?.Count ?? 0}) و دسته‌بندی‌های ({branchTaskCategories?.Count ?? 0}) شعبه {branchId}"
                );

                return Json(new
                {
                    status = "update-view",
                    viewList = viewList
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "BranchTriggerSelect",
                    "خطا در بارگذاری کاربران، طرف حساب‌ها و دسته‌بندی‌های شعبه",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = "خطا در بارگذاری کاربران، طرف حساب‌ها و دسته‌بندی‌های شعبه: " + ex.Message
                });
            }
        }

        /// <summary>
        /// بروزرسانی لیست دسته‌بندی‌ها بر اساس تغییر طرف حساب
        /// این متد زمانی فراخوانی می‌شود که طرف حساب در فرم تغییر کند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>PartialView حاوی لیست دسته‌بندی‌های مربوط به طرف حساب</returns>
        [HttpPost]
        public async Task<IActionResult> StakeholderTriggerSelectTaskCategories(int stakeholderId, int BranchIdSelected)
        {
            try
            {
                // ثبت لاگ ورودی
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "StakeholderTriggerSelectTaskCategories",
                    $"درخواست cascade: شعبه {BranchIdSelected}, طرف حساب {stakeholderId}"
                );

                // اعتبارسنجی پارامترهای ورودی
                if (BranchIdSelected <= 0)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "StakeholderTriggerSelectTaskCategories",
                        "شناسه شعبه نامعتبر",
                        new ArgumentException($"شناسه شعبه نامعتبر: {BranchIdSelected}")
                    );

                    return Json(new
                    {
                        status = "error",
                        message = "شناسه شعبه نامعتبر است"
                    });
                }

                if (stakeholderId <= 0)
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "StakeholderTriggerSelectTaskCategories",
                        "شناسه طرف حساب نامعتبر",
                        new ArgumentException($"شناسه طرف حساب نامعتبر: {stakeholderId}")
                    );

                    return Json(new
                    {
                        status = "error",
                        message = "شناسه طرف حساب نامعتبر است"
                    });
                }

                // دریافت دسته‌بندی‌های تسک مربوط به شعبه و طرف حساب انتخاب شده
                var taskCategoriesViewModels = _branchRepository.GetTaskCategoriesForStakeholderChange(BranchIdSelected, stakeholderId);

                // ثبت لاگ نتیجه جستجو
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "StakeholderTriggerSelectTaskCategories",
                    $"تعداد دسته‌بندی‌های یافت شده: {taskCategoriesViewModels?.Count ?? 0}"
                );

                // رندر کردن partial view
                var partialViewHtml = await this.RenderViewToStringAsync("_TaskCategoriesSelect", taskCategoriesViewModels, true);

                // ایجاد response برای بروزرسانی div دسته‌بندی‌ها
                var viewList = new List<object>
        {
            new
            {
                elementId = "TaskCategoriesDiv",
                view = new
                {
                    result = partialViewHtml
                }
            }
        };

                // ثبت لاگ موفقیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "StakeholderTriggerSelectTaskCategories",
                    $"بارگذاری موفق دسته‌بندی‌های طرف حساب {stakeholderId} در شعبه {BranchIdSelected}"
                );

                return Json(new
                {
                    status = "update-view",
                    viewList = viewList,
                    debug = new
                    {
                        branchId = BranchIdSelected,
                        stakeholderId = stakeholderId,
                        categoriesCount = taskCategoriesViewModels?.Count ?? 0,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا با جزئیات کامل
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "StakeholderTriggerSelectTaskCategories",
                    $"خطا در بارگذاری دسته‌بندی‌های طرف حساب {stakeholderId} در شعبه {BranchIdSelected}",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = "خطا در بارگذاری دسته‌بندی‌های طرف حساب: " + ex.Message,
                    debug = new
                    {
                        branchId = BranchIdSelected,
                        stakeholderId = stakeholderId,
                        errorType = ex.GetType().Name,
                        errorMessage = ex.Message,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
        }
    }
}