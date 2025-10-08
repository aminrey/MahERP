using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace MahERP.Areas.AdminArea.Controllers.TaskControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IStakeholderRepository _stakeholderRepository;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly TaskNotificationService _taskNotificationService;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        protected readonly IUserManagerRepository _userRepository;

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
            ActivityLoggerService activityLogger,
            TaskNotificationService taskNotificationService,
            TaskCodeGenerator taskCodeGenerator,
            IUserManagerRepository userRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _taskRepository = taskRepository;
            _stakeholderRepository = stakeholderRepository;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _taskNotificationService = taskNotificationService;
            _taskCodeGenerator = taskCodeGenerator;
            _userRepository = userRepository;
        }

        #region Dashboard
        /// <summary>
        /// داشبورد تسک‌ها - نمای کلی و آمارها
        /// </summary>
        //[Permission("Tasks", "TaskDashboard", 0)]
        public async Task<IActionResult> TaskDashboard()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var model = await _taskRepository.GetTaskDashboardDataAsync(userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "TaskDashboard", "مشاهده داشبورد تسک‌ها");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "TaskDashboard", "خطا در نمایش داشبورد", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// تسک‌هایی که کاربر به دیگران واگذار کرده
        /// </summary>
        //[Permission("Tasks", "AssignedByMe", 0)]
        public async Task<IActionResult> AssignedByMe(TaskFilterViewModel filters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (filters == null)
                    filters = new TaskFilterViewModel { ViewType = TaskViewType.AssignedByMe };

                var model = await _taskRepository.GetTasksForIndexAsync(userId, filters); // استفاده از همان متد

                ViewBag.Title = "تسک‌های واگذار شده توسط من";
                ViewBag.IsAssignedByMe = true;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AssignedByMe", "مشاهده تسک‌های واگذار شده");

                return View("Index", model); // استفاده از همان Index view
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AssignedByMe", "خطا در دریافت تسک‌های واگذار شده", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// تسک‌هایی که کاربر ناظر آن‌هاست
        /// </summary>
        //[Permission("Tasks", "SupervisedTasks", 0)]
        public async Task<IActionResult> SupervisedTasks(TaskFilterViewModel filters = null)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                
                // دریافت تسک‌های نظارتی
                var model = await _taskRepository.GetSupervisedTasksAsync(currentUserId, filters ?? new TaskFilterViewModel());
                
                // تنظیم ViewBag
                ViewBag.Title = "تسک‌های تحت نظارت";
                ViewBag.IsSupervisedTasks = true;
                
                return View("Index", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت تسک‌های نظارتی: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// یادآوری‌های تسک
        /// </summary>
        //[Permission("Tasks", "TaskReminders", 0)]
        public async Task<IActionResult> TaskReminders(TaskReminderFilterViewModel filters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (filters == null)
                {
                    filters = new TaskReminderFilterViewModel
                    {
                        FilterType = "all",  // Set default value
                        Page = 1,
                        PageSize = 20
                    };
                }

                // Handle query string parameter 'filter' for backward compatibility
                if (Request.Query.ContainsKey("filter"))
                {
                    filters.FilterType = Request.Query["filter"].ToString();
                }

                var model = await _taskRepository.GetTaskRemindersAsync(userId, filters);

                ViewBag.Title = "یادآوری‌های تسک";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "TaskReminders", "مشاهده یادآوری‌های تسک");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "TaskReminders", "خطا در دریافت یادآوری‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// دریافت آمار داشبورد به صورت AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var stats = await _taskRepository.GetUserTaskStatsAsync(userId);

                return Json(new
                {
                    success = true,
                    myTasksCount = stats.MyTasksCount,
                    assignedByMeCount = stats.AssignedByMeCount,
                    supervisedTasksCount = stats.SupervisedTasksCount,
                    overdueTasksCount = stats.OverdueTasksCount,
                    todayTasksCount = stats.TodayTasksCount,
                    thisWeekTasksCount = stats.ThisWeekTasksCount,
                    remindersCount = stats.RemindersCount
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetDashboardStats", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }
        #endregion
        #region Views Actions

        /// <summary>
        /// تقویم تسک‌ها
        /// </summary>
        [HttpGet]
        //[Permission("Tasks", "TaskCalendar", 0)]
        public async Task<IActionResult> TaskCalendar()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var calendarTasks = await _taskRepository.GetCalendarEventsAsync(userId);
                
                // تنظیم داده‌های ViewBag
                ViewBag.CalendarEvents = System.Text.Json.JsonSerializer.Serialize(
                    calendarTasks.Select(task => new
                    {
                        id = task.Id,
                        title = task.Title,
                        start = task.DueDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = task.DueDate?.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        backgroundColor = task.CalendarColor,
                        borderColor = task.CalendarColor,
                        textColor = "#ffffff",
                        description = task.Description ?? "",
                        url = Url.Action("Details", "Tasks", new { id = task.Id, area = "AdminArea" })
                    }));

                ViewBag.PageTitle = "تقویم تسک‌ها";
                
                // فیلترها
                var filterModel = new TaskCalendarFilterViewModel
                {
                    BranchListInitial = _branchRepository.GetBrnachListByUserId(userId)
                };
                ViewBag.FilterModel = filterModel;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "TaskCalendar", "مشاهده تقویم تسک‌ها");

                return View(calendarTasks);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "TaskCalendar", "خطا در نمایش تقویم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// لیست اصلی تسک‌ها
        /// </summary>
        //[Permission("Tasks", "Index", 0)]
        public async Task<IActionResult> Index(TaskFilterViewModel filters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // تنظیمات پیش‌فرض بر اساس سطح دسترسی
                if (filters == null)
                {
                    var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                    filters = new TaskFilterViewModel
                    {
                        ViewType = dataAccessLevel switch
                        {
                            0 => TaskViewType.MyTasks,
                            1 => TaskViewType.AllTasks,
                            2 => TaskViewType.AllTasks,
                            _ => TaskViewType.MyTasks
                        }
                    };
                }

                ViewBag.currentUserId = GetUserId();
                var model = await _taskRepository.GetTasksForIndexAsync(userId, filters);

                // ⭐⭐⭐ Debug موقت - اینجا Breakpoint بگذارید
                var debugInfo = new
                {
                    UserId = userId,
                    ViewType = model.Filters.ViewType,
                    TotalTasks = model.Tasks.Count,
                    MyCreatedTasks = model.Tasks.Count(t => t.CreatorUserId == userId),
                    AssignedToMe = model.Tasks.Count(t => t.AssignmentsTaskUser != null &&
                                                           t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)),
                    // ⭐ اضافه کردن لیست تسک‌ها
                    AllTasks = model.Tasks.Select(t => new {
                        t.Id,
                        t.TaskCode,
                        t.Title,
                        CreatedByMe = t.CreatorUserId == userId,
                        AssignedToMe = t.AssignmentsTaskUser?.Any(a => a.AssignedUserId == userId) ?? false,
                        CreatorId = t.CreatorUserId,
                        Assignments = t.AssignmentsTaskUser?.Select(a => new { a.AssignedUserId, a.AssignedUserName }).ToList()
                    }).ToList()
                };
                // ⭐ اینجا Debugger را نگه دارید و debugInfo را در Watch Window ببینید

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "Index",
                    $"مشاهده لیست تسک‌ها - نوع: {filters.ViewType}");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "Index", "خطا در دریافت لیست تسک‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد تسک جدید
        /// </summary>
        [HttpGet]
        //[Permission("Tasks", "CreateNewTask", 1)]
        public async Task<IActionResult> CreateNewTask()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var model = await _taskRepository.PrepareCreateTaskModelAsync(userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "CreateNewTask", "مشاهده فرم ایجاد تسک");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CreateNewTask", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// جزئیات تسک
        /// </summary>
        //[Permission("Tasks", "Details", 0)]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var task = _taskRepository.GetTaskById(id, includeOperations: true, 
                    includeAssignments: true, includeAttachments: true, includeComments: true);
                
                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View, "Tasks", "Details", 
                        "تلاش برای مشاهده تسک غیرموجود", recordId: id.ToString());
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<TaskViewModel>(task);
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(task.TaskOperations);
                viewModel.AssignmentsTaskUser = _mapper.Map<List<TaskAssignmentViewModel>>(task.TaskAssignments);

                // علامت‌گذاری نوتیفیکیشن‌ها به عنوان خوانده شده
                var currentUserId = _userManager.GetUserId(User);
                await _taskNotificationService.MarkTaskNotificationsAsReadAsync(id, currentUserId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "Details", 
                    $"مشاهده جزئیات تسک: {task.Title}", 
                    recordId: id.ToString(), entityType: "Tasks", recordTitle: task.Title);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "Details", "خطا در دریافت جزئیات تسک", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOperationStar(int id)
        {
            try
            {
                var operation = _taskRepository.GetTaskOperationById(id);
                if (operation == null)
                {
                    return Json(new { success = false, message = "عملیات یافت نشد" });
                }

                // بررسی دسترسی کاربر
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var hasAccess = _taskRepository.IsUserRelatedToTask(currentUserId, operation.TaskId);

                if (!hasAccess)
                {
                    return Json(new { success = false, message = "شما مجاز به انجام این عملیات نیستید" });
                }

                operation.IsStarred = !operation.IsStarred;
                _uow.TaskOperationUW.Update(operation);
                _uow.Save();

                var message = operation.IsStarred ? "عملیات ستاره‌دار شد" : "ستاره عملیات حذف شد";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در تغییر وضعیت ستاره" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOperationCompletion(int id)
        {
            try
            {
                var operation = _taskRepository.GetTaskOperationById(id);
                if (operation == null)
                {
                    return Json(new { success = false, message = "عملیات یافت نشد" });
                }

                // بررسی دسترسی کاربر
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var hasAccess = _taskRepository.IsUserRelatedToTask(currentUserId, operation.TaskId);

                if (!hasAccess)
                {
                    return Json(new { success = false, message = "شما مجاز به انجام این عملیات نیستید" });
                }

                operation.IsCompleted = !operation.IsCompleted;

                if (operation.IsCompleted)
                {
                    operation.CompletionDate = DateTime.Now;
                    operation.CompletedByUserId = currentUserId;
                }
                else
                {
                    operation.CompletionDate = null;
                    operation.CompletedByUserId = null;
                    operation.CompletionNote = null; // پاک کردن گزارش در صورت بازگشت
                }

                _uow.TaskOperationUW.Update(operation);
                _uow.Save();

                var message = operation.IsCompleted ? "عملیات تکمیل شد" : "عملیات به حالت انتظار برگشت";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در تغییر وضعیت تکمیل" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddOperationNote(int operationId)
        {
            try
            {
                var operation = _taskRepository.GetTaskOperationById(operationId);
                if (operation == null)
                {
                    return Json(new { status = "error", message = new[] { new { text = "عملیات یافت نشد" } } });
                }

                // بررسی دسترسی
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var hasAccess = _taskRepository.IsUserRelatedToTask(currentUserId, operation.TaskId);

                if (!hasAccess)
                {
                    return Json(new { status = "error", message = new[] { new { text = "شما مجاز به انجام این عملیات نیستید" } } });
                }

                // دریافت اطلاعات تسک از طریق Repository
                var task = _taskRepository.GetTaskById(operation.TaskId);

                var model = new OperationNoteViewModel
                {
                    OperationId = operationId,
                    OperationTitle = operation.Title,
                    TaskTitle = task.Title,
                    CompletionNote = operation.CompletionNote,
                    IsCompleted = operation.IsCompleted
                };

                return PartialView("_AddOperationNoteModal", model);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = new[] { new { text = "خطا در بارگذاری فرم" } } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOperationNote(OperationNoteViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { text = e.ErrorMessage })
                        .ToArray();
                    return Json(new { status = "validation-error", message = errors });
                }

                var operation = _taskRepository.GetTaskOperationById(model.OperationId);
                if (operation == null)
                {
                    return Json(new { status = "error", message = new[] { new { text = "عملیات یافت نشد" } } });
                }

                // بررسی دسترسی
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var hasAccess = _taskRepository.IsUserRelatedToTask(currentUserId, operation.TaskId);

                if (!hasAccess)
                {
                    return Json(new { status = "error", message = new[] { new { text = "شما مجاز به انجام این عملیات نیستید" } } });
                }

                operation.CompletionNote = model.CompletionNote?.Trim();

                // اگر گزارش اضافه شد، عملیات را تکمیل کن
                if (!string.IsNullOrEmpty(operation.CompletionNote) && !operation.IsCompleted)
                {
                    operation.IsCompleted = true;
                    operation.CompletionDate = DateTime.Now;
                    operation.CompletedByUserId = currentUserId;
                }

                _uow.TaskOperationUW.Update(operation);
                _uow.Save();

                return Json(new
                {
                    status = "success",
                    message = new[] { new { text = "گزارش با موفقیت ثبت شد" } },
                    refreshPage = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = new[] { new { text = "خطا در ثبت گزارش" } } });
            }
        }
        /// <summary>
        /// تسک‌های شخصی کاربر
        /// </summary>
        //[Permission("Tasks", "MyTasks", 0)]
        public async Task<IActionResult> MyTasks()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var tasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: false);
                var viewModels = _mapper.Map<List<TaskViewModel>>(tasks);

                foreach (var viewModel in viewModels)
                {
                    var operations = _taskRepository.GetTaskOperations(viewModel.Id);
                    viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);
                }

                ViewBag.Title = "تسک‌های من";
                ViewBag.IsMyTasks = true;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "MyTasks", "مشاهده تسک‌های شخصی");

                return View("Index", viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "MyTasks", "خطا در دریافت تسک‌های شخصی", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        #endregion


        /// <summary>
        /// دریافت تسک‌های فوری برای داشبورد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUrgentTasks()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var urgentTasks = await _taskRepository.GetUrgentTasksAsync(userId, take: 5);

                return PartialView("_UrgentTasksList", urgentTasks);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetUrgentTasks", "خطا در دریافت تسک‌های فوری", ex);
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
                var userId = _userManager.GetUserId(User);
                var activities = await _taskRepository.GetRecentTaskActivitiesAsync(userId, take: 10);

                return PartialView("_RecentTaskActivities", activities);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetRecentTaskActivities", "خطا در دریافت فعالیت‌ها", ex);
                return PartialView("_RecentTaskActivities", new List<RecentActivityViewModel>());
            }
        }

        /// <summary>
        /// علامت‌گذاری یادآوری به عنوان خوانده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReminderAsRead(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _taskRepository.MarkReminderAsReadAsync(id, userId);

                return Json(new { success = true, message = "یادآوری به عنوان خوانده شده علامت‌گذاری شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "MarkReminderAsRead", "خطا در علامت‌گذاری یادآوری", ex);
                return Json(new { success = false, message = "خطا در علامت‌گذاری یادآوری" });
            }
        }

        /// <summary>
        /// علامت‌گذاری همه یادآوری‌ها به عنوان خوانده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRemindersAsRead()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _taskRepository.MarkAllRemindersAsReadAsync(userId);

                return Json(new { success = true, message = "همه یادآوری‌ها به عنوان خوانده شده علامت‌گذاری شدند" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "MarkAllRemindersAsRead", "خطا در علامت‌گذاری یادآوری‌ها", ex);
                return Json(new { success = false, message = "خطا در علامت‌گذاری یادآوری‌ها" });
            }
        }

        /// <summary>
        /// حذف یادآوری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _taskRepository.DeleteReminderAsync(id, userId);

                return Json(new { success = true, message = "یادآوری حذف شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteReminder", "خطا در حذف یادآوری", ex);
                return Json(new { success = false, message = "خطا در حذف یادآوری" });
            }
        }

        /// <summary>
        /// حذف یادآوری‌های خوانده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReadReminders()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _taskRepository.DeleteReadRemindersAsync(userId);

                return Json(new { success = true, message = "یادآوری‌های خوانده شده حذف شدند" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteReadReminders", "خطا در حذف یادآوری‌ها", ex);
                return Json(new { success = false, message = "خطا در حذف یادآوری‌ها" });
            }
        }









        #region POST Actions

        /// <summary>
        /// ثبت تسک جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Permission("Tasks", "CreateNewTask", 1)]
        public async Task<IActionResult> CreateNewTask(TaskViewModel model)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                
                // ⭐ 1. اعتبارسنجی از طریق Repository
                var (isValid, errors) = await _taskRepository.ValidateTaskModelAsync(model, currentUserId);
                
                if (!isValid)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    
                    model = await _taskRepository.PrepareCreateTaskModelAsync(currentUserId);
                    return View(model);
                }

                // ⭐ شروع تراکنش
                await _uow.BeginTransactionAsync();

                try
                {
                    // ⭐ 2. ایجاد تسک از طریق Repository
                    var task = await _taskRepository.CreateTaskEntityAsync(model, currentUserId, _mapper);
                    
                    // ⭐ 3. ذخیره فایل‌های پیوست از طریق Repository
                    if (model.Attachments != null && model.Attachments.Count > 0)
                    {
                        await _taskRepository.SaveTaskAttachmentsAsync(
                            task.Id, 
                            model.Attachments, 
                            currentUserId, 
                            _webHostEnvironment.WebRootPath);
                    }

                    // ⭐ 4. ذخیره عملیات‌ها و یادآوری‌ها از طریق Repository
                    await _taskRepository.SaveTaskOperationsAndRemindersAsync(task.Id, model);

                    // ⭐ 5. مدیریت انتصاب‌ها (Bulk Insert) از طریق Repository
                    await _taskRepository.HandleTaskAssignmentsBulkAsync(task, model, currentUserId);

                    // ⭐ تأیید تراکنش
                    await _uow.CommitTransactionAsync();

                    // 6. ارسال نوتیفیکیشن (خارج از تراکنش)
                     EnqueueTaskNotification(task.Id, currentUserId, model);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create, "Tasks", "CreateNewTask",
                        $"ایجاد تسک جدید: {task.Title} با کد: {task.TaskCode}",
                        recordId: task.Id.ToString(), entityType: "Tasks", recordTitle: task.Title);

                    TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    // ⭐ Rollback خودکار
                    await _uow.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CreateNewTask", "خطا در ایجاد تسک", ex);
                ModelState.AddModelError("", $"خطا در ثبت تسک: {ex.Message}");
                
                model = await _taskRepository.PrepareCreateTaskModelAsync(_userManager.GetUserId(User));
                return View(model);
            }
        }

        /// <summary>
        /// بررسی یکتایی کد تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckTaskCodeUniqueness(string taskCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taskCode))
                    return Json(new { success = true, isUnique = true });

                bool isUnique = await _taskRepository.IsTaskCodeUniqueAsync(taskCode);
                return Json(new { success = true, isUnique = isUnique });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در بررسی کد تسک" });
            }
        }

        #endregion

        #region AJAX Actions


        // اصلاح GetCalendarEvents:
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents(
            DateTime? start = null, DateTime? end = null, int? branchId = null,
            string assignedUserIds = null, int? stakeholderId = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                List<string> userFilterList = null;
                if (!string.IsNullOrEmpty(assignedUserIds))
                {
                    userFilterList = assignedUserIds.Split(',')
                        .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }

                var calendarTasks = await _taskRepository.GetCalendarEventsAsync(
                    userId, start, end, branchId, userFilterList, stakeholderId);

                var events = calendarTasks.Where(task => task.DueDate.HasValue)
                    .Select(task => new
                    {
                        id = task.Id,
                        title = task.Title,
                        start = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy-MM-dd"),
                        end = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value.AddHours(3), "yyyy-MM-dd"),
                        // ⭐ استفاده از متدهای Repository
                        backgroundColor = _taskRepository.GetTaskStatusColor(task),
                        borderColor = _taskRepository.GetTaskStatusColor(task),
                        textColor = "#ffffff",
                        description = task.Description ?? "",
                        extendedProps = new
                        {
                            taskCode = task.TaskCode ?? "",
                            categoryTitle = task.CategoryTitle ?? "",
                            stakeholderName = task.StakeholderName ?? "",
                            branchName = task.BranchName ?? "",
                            statusText = _taskRepository.GetTaskStatusTextForCalendar(task),
                            isCompleted = task.IsCompleted,
                            isOverdue = task.IsOverdue,
                            detailUrl = Url.Action("Details", "Tasks", new { id = task.Id, area = "AdminArea" })
                        }
                    }).ToList();

                return Json(events);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetCalendarEvents", "خطا در دریافت رویدادهای تقویم", ex);
                return Json(new List<object>());
            }
        }


        /// <summary>
        /// بروزرسانی داده‌های وابسته به شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelect(int branchId)
        {
            try
            {
                var branchData = await _taskRepository.GetBranchTriggeredDataAsync(branchId);
                
                var viewList = new List<object>
                {
                    new {
                        elementId = "UsersDiv",
                        view = new { result = await this.RenderViewToStringAsync("_BranchUsersSelect", branchData.Users) }
                    },
                    new {
                        elementId = "TeamsDiv", 
                        view = new { result = await this.RenderViewToStringAsync("_BranchTeamsSelect", branchData.Teams) }
                    },
                    new {
                        elementId = "StakeholdersDiv",
                        view = new { result = await this.RenderViewToStringAsync("_BranchStakeholdersSelect", branchData.Stakeholders) }
                    },
                    new {
                        elementId = "TaskCategoriesDiv",
                        view = new { result = "" }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "BranchTriggerSelect",
                    $"بارگذاری داده‌های شعبه {branchId}");

                return Json(new { status = "update-view", viewList = viewList });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "BranchTriggerSelect", "خطا در بارگذاری داده‌های شعبه", ex);
                return Json(new { status = "error", message = "خطا در بارگذاری داده‌های شعبه" });
            }
        }

        /// <summary>
        /// بروزرسانی دسته‌بندی‌ها بر اساس طرف حساب
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> StakeholderTriggerSelectTaskCategories(int stakeholderId, int BranchIdSelected)
        {
            try
            {
                if (BranchIdSelected <= 0 || stakeholderId <= 0)
                {
                    return Json(new { status = "error", message = "پارامترهای ورودی نامعتبر است" });
                }

                var taskCategories = _branchRepository.GetTaskCategoriesForStakeholderChange(BranchIdSelected, stakeholderId);
                var partialViewHtml = await this.RenderViewToStringAsync("_TaskCategoriesSelect", taskCategories, true);

                var viewList = new List<object>
                {
                    new {
                        elementId = "TaskCategoriesDiv",
                        view = new { result = partialViewHtml }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "StakeholderTriggerSelectTaskCategories",
                    $"بارگذاری دسته‌بندی‌های طرف حساب {stakeholderId} در شعبه {BranchIdSelected}");

                return Json(new { status = "update-view", viewList = viewList });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "StakeholderTriggerSelectTaskCategories", "خطا در بارگذاری دسته‌بندی‌ها", ex);
                return Json(new { status = "error", message = "خطا در بارگذاری دسته‌بندی‌ها" });
            }
        }

        /// <summary>
        /// نمایش مودال یادآوری سفارشی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddCustomReminderModal()
        {
            try
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AddCustomReminderModal", "نمایش مودال یادآوری سفارشی");
                return PartialView("_AddCustomReminderModal");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddCustomReminderModal", "خطا در نمایش مودال", ex);
                return BadRequest("خطا در بارگذاری مودال");
            }
        }

        /// <summary>
        /// دریافت آمار پروژه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetProjectStats(int? stakeholderId, int? categoryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userBranchId = _taskRepository.GetUserBranchId(userId);
                var stats = await _taskRepository.GetProjectStatsAsync(userBranchId, stakeholderId, categoryId);
                
                return Json(new
                {
                    success = true,
                    stakeholderTasksCount = stats.StakeholderTasksCount,
                    categoryTasksCount = stats.CategoryTasksCount
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetProjectStats", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }
        /// <summary>
        /// ذخیره یادآوری سفارشی و برگرداندن partial view
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCustomReminder(TaskReminderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToList();

                    return Json(new
                    {
                        status = "validation-error",
                        message = errors
                    });
                }

                // اعتبارسنجی بر اساس نوع یادآوری
                switch (model.ReminderType)
                {
                    case 0: // یکبار
                        if (string.IsNullOrEmpty(model.StartDatePersian))
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "تاریخ یادآوری الزامی است" } }
                            });
                        }
                        break;
                    case 1: // تکراری
                        if (!model.IntervalDays.HasValue || model.IntervalDays <= 0)
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "فاصله تکرار یادآوری الزامی است" } }
                            });
                        }
                        break;
                    case 2: // قبل از مهلت
                        if (!model.DaysBeforeDeadline.HasValue || model.DaysBeforeDeadline <= 0)
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "تعداد روز قبل از مهلت الزامی است" } }
                            });
                        }
                        break;
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create, "Tasks", "SaveCustomReminder",
                    $"ایجاد یادآوری سفارشی: {model.Title}");

                // تولید ID منحصر به فرد برای یادآوری
                ViewBag.ReminderId = DateTime.Now.Ticks;

                // رندر کردن partial view با حالت append
                var partialViewHtml = await this.RenderViewToStringAsync("_ReminderItem", model, appendMode: true);

                // برگرداندن response با partial view
                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "customRemindersList",
                            appendMode = true, // اضافه کردن flag برای append
                            view = new { result = partialViewHtml }
                        }
                    },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SaveCustomReminder", "خطا در ذخیره یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری: " + ex.Message } }
                });
            }
        }

        /// <summary>
        /// نمایش مودال تنظیم تاریخ‌های شخصی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SetPersonalDatesModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی کاربر به تسک از طریق Repository
                var assignment = await _taskRepository.GetTaskAssignmentForPersonalDatesAsync(taskId, userId);

                if (assignment == null)
                {
                    return BadRequest("شما به این تسک دسترسی ندارید");
                }

                var task = assignment.Task;
                var model = new TaskPersonalDatesViewModel
                {
                    TaskId = taskId,
                    TaskAssignmentId = assignment.Id,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    OriginalStartDatePersian = task.StartDate != null ? ConvertDateTime.ConvertMiladiToShamsi(task.StartDate, "yyyy/MM/dd") : null,
                    OriginalDueDatePersian = task.DueDate != null ?  ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy/MM/dd") : null,
                    PersonalStartDatePersian = assignment.PersonalStartDate != null ? ConvertDateTime.ConvertMiladiToShamsi(assignment.PersonalStartDate, "yyyy/MM/dd") : null,
                    PersonalEndDatePersian = assignment.PersonalEndDate  != null ? ConvertDateTime.ConvertMiladiToShamsi(assignment.PersonalEndDate, "yyyy/MM/dd") : null,
                    PersonalTimeNote = assignment.PersonalTimeNote,
                    AssignedUserName = assignment.AssignedUser?.FirstName + " " + assignment.AssignedUser?.LastName,
                    LastUpdated = assignment.PersonalDatesUpdatedDate,
                    CanModifyDates = assignment.Status < 3 // فقط قبل از تکمیل
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "SetPersonalDatesModal",
                    $"نمایش مودال تنظیم تاریخ‌های شخصی برای تسک {task.TaskCode}");

                return PartialView("_SetPersonalDatesModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SetPersonalDatesModal", "خطا در نمایش مودال", ex);
                return BadRequest("خطا در بارگذاری مودال");
            }
        }

        /// <summary>
        /// ذخیره تاریخ‌های شخصی کاربر
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePersonalDates(TaskPersonalDatesViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی کاربر از طریق Repository
                var assignment = await _taskRepository.GetTaskAssignmentByIdForPersonalDatesAsync(model.TaskAssignmentId, userId);

                if (assignment == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما به این تسک دسترسی ندارید" } }
                    });
                }

                if (assignment.Status >= 3)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "امکان تغییر تاریخ‌ها وجود ندارد" } }
                    });
                }

                // تبدیل تاریخ‌های شمسی به میلادی
                DateTime? personalStartDate = null;
                DateTime? personalEndDate = null;

                if (!string.IsNullOrEmpty(model.PersonalStartDatePersian))
                {
                    personalStartDate = ConvertDateTime.ConvertShamsiToMiladi(model.PersonalStartDatePersian);
                }

                if (!string.IsNullOrEmpty(model.PersonalEndDatePersian))
                {
                    personalEndDate = ConvertDateTime.ConvertShamsiToMiladi(model.PersonalEndDatePersian);
                }

                // بروزرسانی از طریق Repository
                var updateResult = await _taskRepository.UpdatePersonalDatesAsync(
                    model.TaskAssignmentId, 
                    userId, 
                    personalStartDate, 
                    personalEndDate, 
                    model.PersonalTimeNote);

                if (!updateResult)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در بروزرسانی تاریخ‌ها" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update, "Tasks", "SavePersonalDates",
                    $"بروزرسانی تاریخ‌های شخصی تسک {assignment.Task.TaskCode}",
                    recordId: assignment.TaskId.ToString(), entityType: "Tasks", recordTitle: assignment.Task.Title);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "تاریخ‌های شخصی با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SavePersonalDates", "خطا در ذخیره تاریخ‌های شخصی", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره تاریخ‌ها: " + ex.Message } }
                });
            }
        }

        /// <summary>
        /// دریافت رویدادهای شخصی کاربران برای یک تسک
        /// </summary>
        private async Task<List<object>> GetPersonalTaskEventsAsync(int taskId, string currentUserId)
        {
            var personalEvents = new List<object>();

            // استفاده از Repository به جای UnitOfWork مستقیم
            var assignments = await _taskRepository.GetTaskAssignmentsWithPersonalDatesAsync(taskId);

            foreach (var assignment in assignments)
            {
                var isMyAssignment = assignment.AssignedUserId == currentUserId;
                // استفاده از Repository متد به جای متد محلی
                var userInitials = _taskRepository.GetUserInitials(assignment.AssignedUser?.FirstName, assignment.AssignedUser?.LastName);

                // رویداد شروع شخصی
                if (assignment.PersonalStartDate.HasValue)
                {
                    personalEvents.Add(new
                    {
                        id = $"personal-start-{assignment.Id}",
                        title = $"[شروع {userInitials}] {assignment.Task.Title}",
                        start = ConvertDateTime.ConvertMiladiToShamsi(assignment.PersonalStartDate, "yyyy-MM-dd"),
                        backgroundColor = isMyAssignment ? "#4CAF50" : "#81C784", // سبز تیره/روشن
                        borderColor = isMyAssignment ? "#388E3C" : "#66BB6A",
                        textColor = "#ffffff",
                        classNames = new[] { "personal-start-event", isMyAssignment ? "my-event" : "other-event" },
                        extendedProps = new
                        {
                            taskId = taskId,
                            assignmentId = assignment.Id,
                            eventType = "personal-start",
                            taskCode = assignment.Task.TaskCode ?? "",
                            assignedUserName = assignment.AssignedUser?.FirstName + " " + assignment.AssignedUser?.LastName,
                            isMyEvent = isMyAssignment,
                            personalNote = assignment.PersonalTimeNote,
                            detailUrl = Url.Action("Details", "Tasks", new { id = taskId, area = "AdminArea" }),
                            editUrl = isMyAssignment ? Url.Action("SetPersonalDatesModal", "Tasks", new { taskId = taskId, area = "AdminArea" }) : null
                        }
                    });
                }

                // رویداد پایان شخصی
                if (assignment.PersonalEndDate.HasValue)
                {
                    personalEvents.Add(new
                    {
                        id = $"personal-end-{assignment.Id}",
                        title = $"[پایان {userInitials}] {assignment.Task.Title}",
                        start = ConvertDateTime.ConvertMiladiToShamsi(assignment.PersonalEndDate, "yyyy-MM-dd"),
                        backgroundColor = isMyAssignment ? "#FF9800" : "#FFB74D", // نارنجی تیره/روشن
                        borderColor = isMyAssignment ? "#F57C00" : "#FFA726",
                        textColor = "#ffffff",
                        classNames = new[] { "personal-end-event", isMyAssignment ? "my-event" : "other-event" },
                        extendedProps = new
                        {
                            taskId = taskId,
                            assignmentId = assignment.Id,
                            eventType = "personal-end",
                            taskCode = assignment.Task.TaskCode ?? "",
                            assignedUserName = assignment.AssignedUser?.FirstName + " " + assignment.AssignedUser?.LastName,
                            isMyEvent = isMyAssignment,
                            personalNote = assignment.PersonalTimeNote,
                            detailUrl = Url.Action("Details", "Tasks", new { id = taskId, area = "AdminArea" }),
                            editUrl = isMyAssignment ? Url.Action("SetPersonalDatesModal", "Tasks", new { taskId = taskId, area = "AdminArea" }) : null
                        }
                    });
                }
            }

            return personalEvents;
        }

        #endregion

        /// <summary>
        /// اضافه کردن نوتیفیکشن به صف - باقی می‌ماند
        /// </summary>
        private void EnqueueTaskNotification(int taskId, string currentUserId, TaskViewModel model)
        {
            try
            {
                var notificationService = HttpContext.RequestServices
                    .GetService<NotificationBackgroundService>();

                if (notificationService != null)
                {
                    var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                    var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

                    var teamUserIds = Task.Run(async () =>
                        await _taskRepository.GetUsersFromTeamsAsync(assignedTeamIds)).Result;

                    var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                    notificationService.EnqueueTaskCreation(taskId, currentUserId, allAssignedUserIds);
                }
            }
            catch (Exception ex)
            {
                _activityLogger.LogErrorAsync("Tasks", "EnqueueTaskNotification",
                    $"خطا در اضافه کردن نوتیفیکیشن به صف", ex);
            }
        }

        /// <summary>
        /// دریافت تیم‌های یک کاربر در شعبه مشخص (AJAX) - نسخه استاندارد با Partial View و گزینه بدون تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserTeams(string userId, int branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || branchId <= 0)
                {
                    return Json(new { 
                        status = "error", 
                        message = "اطلاعات ورودی نامعتبر است" 
                    });
                }

                // ⭐ استفاده از Repository
                var userTeams = await _taskRepository.GetUserTeamsByBranchAsync(userId, branchId);

                // ⭐ رندر Partial View (مشابه BranchTriggerSelect)
                var partialHtml = await this.RenderViewToStringAsync("_UserTeamsSelect", userTeams);

                // ⭐ ID دینامیک - باید با counter هماهنگ باشد
                // در JavaScript باید counter را ارسال کنیم یا از userId استفاده کنیم
                var viewList = new List<object>
                {
                    new {
                        elementId = $"team-select-container", // ⭐ نام ثابت - JavaScript باید آن را تطبیق دهد
                        view = new { result = partialHtml }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetUserTeams",
                    $"بارگذاری تیم‌های کاربر {userId} در شعبه {branchId} - تعداد: {userTeams.Count}");

                return Json(new { 
                    status = "update-view", 
                    viewList = viewList,
                    teamsCount = userTeams.Count,
                    hasNoTeam = !userTeams.Any()
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetUserTeams", 
                    "خطا در دریافت تیم‌های کاربر", ex);
                
                // ⭐ در صورت خطا، باز هم partial view با لیست خالی برگردان
                var errorHtml = await this.RenderViewToStringAsync("_UserTeamsSelect", new List<TeamViewModel>());
                
                return Json(new { 
                    status = "update-view",
                    viewList = new List<object>
                    {
                        new {
                            elementId = "team-select-container",
                            view = new { result = errorHtml }
                        }
                    },
                    message = $"خطا: {ex.Message}",
                    hasNoTeam = true
                });
            }
        }
    }
}