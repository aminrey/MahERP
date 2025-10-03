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
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
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
                var userId = _userManager.GetUserId(User);

                if (filters == null)
                    filters = new TaskFilterViewModel { ViewType = TaskViewType.SupervisedTasks };

                var model = await _taskRepository.GetTasksForIndexAsync(userId, filters); // استفاده از همان متد

                ViewBag.Title = "تسک‌های تحت نظارت";
                ViewBag.IsSupervisedTasks = true;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "SupervisedTasks", "مشاهده تسک‌های تحت نظارت");

                return View("Index", model); // استفاده از همان Index view
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SupervisedTasks", "خطا در دریافت تسک‌های نظارتی", ex);
                return RedirectToAction("ErrorView", "Home");
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
                
                // اعتبارسنجی مقدماتی
                if (!await ValidateTaskModel(model, currentUserId))
                {
                    model = await _taskRepository.PrepareCreateTaskModelAsync(currentUserId);
                    return View(model);
                }

                // ایجاد تسک
                var task = await CreateTaskEntity(model, currentUserId);
                
                // ذخیره فایل‌های پیوست
                if (model.Attachments != null && model.Attachments.Count > 0)
                {
                    await SaveTaskAttachments(task.Id, model.Attachments);
                }

                // ذخیره عملیات‌ها و یادآوری‌ها
                await SaveTaskOperationsAndReminders(task.Id, model);

                // مدیریت انتصاب‌ها
                await HandleTaskAssignments(task, model, currentUserId);

                // ارسال نوتیفیکیشن
                await SendTaskCreatedNotification(task.Id, currentUserId, model);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create, "Tasks", "CreateNewTask",
                    $"ایجاد تسک جدید: {task.Title} با کد: {task.TaskCode}",
                    recordId: task.Id.ToString(), entityType: "Tasks", recordTitle: task.Title);

                TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
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

        /// <summary>
        /// دریافت رویدادهای تقویم
        /// </summary>
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
                        backgroundColor = GetTaskStatusColor(task),
                        borderColor = GetTaskStatusColor(task),
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

        #region Private Helper Methods

   
        private async Task<bool> ValidateTaskModel(TaskViewModel model, string userId)
        {
            var isValid = true;

            // بررسی شعبه
            if (model.BranchIdSelected <= 0)
            {
                ModelState.AddModelError("BranchIdSelected", "انتخاب شعبه الزامی است");
                isValid = false;
            }
            else
            {
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                if (!userBranches.Any(b => b.Id == model.BranchIdSelected))
                {
                    ModelState.AddModelError("BranchIdSelected", "شما به شعبه انتخاب شده دسترسی ندارید");
                    isValid = false;
                }
            }

            // بررسی کد دستی
            if (model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode))
            {
                if (!_taskCodeGenerator.ValidateTaskCode(model.ManualTaskCode))
                {
                    var settings = _taskCodeGenerator.GetTaskCodeSettings();
                    ModelState.AddModelError("ManualTaskCode", 
                        $"کد تسک نامعتبر است. نمی‌توانید از پیشوند '{settings.SystemPrefix}-' استفاده کنید");
                    isValid = false;
                }
            }

            return isValid;
        }

        private async Task<Tasks> CreateTaskEntity(TaskViewModel model, string currentUserId)
        {
            string finalTaskCode = model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode) 
                ? model.ManualTaskCode 
                : _taskCodeGenerator.GenerateTaskCode();

            var task = _mapper.Map<Tasks>(model);
            task.TaskCode = finalTaskCode;
            task.CreateDate = DateTime.Now;
            task.CreatorUserId = currentUserId;
            task.IsActive = model.IsActive;
            task.IsDeleted = false;
            task.TaskTypeInput = 1;
            task.VisibilityLevel = 0;
            task.Priority = 0;
            task.Important = false;
            task.Status = 0;
            task.CreationMode = 0;
            task.TaskCategoryId = model.TaskCategoryIdSelected;
            task.BranchId = model.BranchIdSelected;

            // تبدیل تاریخ‌های شمسی
            if (!string.IsNullOrEmpty(model.SuggestedStartDatePersian))
            {
                task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.SuggestedStartDatePersian);
            }
      
            // تبدیل تاریخ‌های شمسی
            if (!string.IsNullOrEmpty(model.StartDatePersian))
            {
                task.StartDate = ConvertDateTime.ConvertShamsiToMiladi(model.StartDatePersian);
            }
      

            _uow.TaskUW.Create(task);
            _uow.Save();

            return task;
        }

        private async Task SaveTaskOperationsAndReminders(int taskId, TaskViewModel model)
        {
            // ذخیره عملیات‌ها
            if (!string.IsNullOrEmpty(model.TaskOperationsJson))
            {
                try
                {
                    var operations = System.Text.Json.JsonSerializer.Deserialize<List<TaskOperationViewModel>>(model.TaskOperationsJson);
                    if (operations?.Any() == true)
                    {
                        _taskRepository.SaveTaskOperations(taskId, operations);
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Tasks", "SaveTaskOperations", "خطا در ذخیره عملیات‌ها", ex);
                }
            }

            // ذخیره یادآوری‌ها
            if (!string.IsNullOrEmpty(model.TaskRemindersJson))
            {
                try
                {
                    var reminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(model.TaskRemindersJson);
                    if (reminders?.Any() == true)
                    {
                        _taskRepository.SaveTaskReminders(taskId, reminders);
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Tasks", "SaveTaskReminders", "خطا در ذخیره یادآوری‌ها", ex);
                }
            }
        }

        private async Task HandleTaskAssignments(Tasks task, TaskViewModel model, string currentUserId)
        {
            var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
            var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();
            
            // کاربران از تیم‌ها
            var teamUserIds = await _taskRepository.GetUsersFromTeamsAsync(assignedTeamIds);
            var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

           
            // اختصاص به سایرین
            foreach (var assignedUserId in allAssignedUserIds)
            {
                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    AssignedUserId = assignedUserId,
                    AssignerUserId = currentUserId,
                    AssignmentType = 0,
                    AssignmentDate = DateTime.Now,
                    Description = assignedUserIds.Contains(assignedUserId) ? "انتصاب مستقیم" : "انتصاب از طریق تیم"
                };
                _uow.TaskAssignmentUW.Create(assignment);
            }

            _uow.Save();
        }

        private async Task SendTaskCreatedNotification(int taskId, string currentUserId, TaskViewModel model)
        {
            try
            {
                var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();
                var teamUserIds = await _taskRepository.GetUsersFromTeamsAsync(assignedTeamIds);
                var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                await _taskNotificationService.NotifyTaskCreatedAsync(taskId, currentUserId, allAssignedUserIds);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SendTaskCreatedNotification", "خطا در ارسال نوتیفیکیشن", ex);
            }
        }

        private async Task SaveTaskAttachments(int taskId, List<IFormFile> files)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks", taskId.ToString());
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    
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

        private string GetTaskStatusColor(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "#28a745"; // سبز
            if (task.IsOverdue) return "#dc3545";   // قرمز
            return "#007bff";                       // آبی
        }

        private string GetTaskStatusText(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "تکمیل شده";
            if (task.IsOverdue) return "عقب افتاده";
            return "در حال انجام";
        }

        #endregion
        // اضافه کردن این متدها به TasksController

        
        /// <summary>
        /// صفحه تسک‌های "روز من"
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyDayTasks()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

            

                var model = await _taskRepository.GetMyDayTasksAsync(userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "MyDayTasks",
                    "مشاهده تسک‌های روز من");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "MyDayTasks", "خطا در دریافت تسک‌های روز من", ex);
                return BadRequest("خطا در بارگذاری تسک‌های روز من");
            }
        }
        /// <summary>
        /// نمایش مودال اضافه کردن تسک به "روز من"
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddToMyDayModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = _taskRepository.GetTaskById(taskId);

                if (task == null)
                {
                    return BadRequest("تسک یافت نشد");
                }

                var model = new TaskMyDayViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd"),
                    IsAlreadyInMyDay = await _taskRepository.IsTaskInMyDayAsync(taskId, userId)
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AddToMyDayModal",
                    $"نمایش مودال اضافه به روز من برای تسک {task.TaskCode}");

                return PartialView("_AddToMyDayModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddToMyDayModal", "خطا در نمایش مودال", ex);
                return BadRequest("خطا در بارگذاری مودال");
            }
        }

        /// <summary>
        /// ذخیره تسک در "روز من"
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToMyDay(TaskMyDayViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var plannedDate = ConvertDateTime.ConvertShamsiToMiladi(model.PlannedDatePersian);

                var result = await _taskRepository.AddTaskToMyDayAsync(
                    model.TaskId, userId, plannedDate, model.PlanNote);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create, "Tasks", "AddToMyDay",
                        $"اضافه کردن تسک {model.TaskCode} به روز من",
                        recordId: model.TaskId.ToString(), entityType: "Tasks", recordTitle: model.TaskTitle);

                    return Json(new
                    {
                        status = "success",
                        message = new[] { new { status = "success", text = "تسک با موفقیت به روز شما اضافه شد" } }
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در اضافه کردن تسک" } }
                    });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddToMyDay", "خطا در اضافه کردن به روز من", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره: " + ex.Message } }
                });
            }
        }

        /// <summary>
        /// نمایش مودال ثبت کار انجام شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LogWorkModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = _taskRepository.GetTaskById(taskId);

                if (task == null)
                {
                    return BadRequest("تسک یافت نشد");
                }

                var model = new TaskWorkLogViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    IsAlreadyWorkedOn = await _taskRepository.IsTaskInMyDayAsync(taskId, userId)
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "LogWorkModal",
                    $"نمایش مودال ثبت کار برای تسک {task.TaskCode}");

                return PartialView("_LogWorkModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "LogWorkModal", "خطا در نمایش مودال", ex);
                return BadRequest("خطا در بارگذاری مودال");
            }
        }

        /// <summary>
        /// ذخیره کار انجام شده روی تسک
        /// </summary>
        /// <summary>
        /// ذخیره کار انجام شده روی تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogWork(TaskWorkLogViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var result = await _taskRepository.LogTaskWorkAsync(
                    model.TaskId, userId, model.WorkNote, model.WorkDurationMinutes);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Update, "Tasks", "LogWork",
                        $"ثبت کار انجام شده روی تسک {model.TaskCode}",
                        recordId: model.TaskId.ToString(), entityType: "Tasks", recordTitle: model.TaskTitle);

                    // دریافت داده‌های بروزرسانی شده
                    var updatedModel = await _taskRepository.GetMyDayTasksAsync(userId);

                    // رندر کردن partial views
                    var statsHtml = await this.RenderViewToStringAsync("_MyDayStats", updatedModel);
                    var tasksListHtml = await this.RenderViewToStringAsync("_MyDayTasksList", updatedModel);

                    return Json(new
                    {
                        status = "update-view",
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "myDayStatsContainer",
                        view = new { result = statsHtml }
                    },
                    new
                    {
                        elementId = "myDayTasksContainer",
                        view = new { result = tasksListHtml }
                    }
                },
                        message = new[] { new { status = "success", text = "کار انجام شده با موفقیت ثبت شد" } }
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در ثبت کار انجام شده" } }
                    });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "LogWork", "خطا در ثبت کار", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره: " + ex.Message } }
                });
            }
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای نمایش در sidebar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyDayTasksCount()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var count = await _taskRepository.GetMyDayTasksCountAsync(userId);

                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetMyDayTasksCount", "خطا در دریافت تعداد", ex);
                return Json(new { success = false, count = 0 });
            }
        }

        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromMyDay(int taskId, string date = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                DateTime? targetDate = null;

                if (!string.IsNullOrEmpty(date))
                {
                    targetDate = ConvertDateTime.ConvertShamsiToMiladi(date);
                }

                var result = await _taskRepository.RemoveTaskFromMyDayAsync(taskId, userId, targetDate);

                if (result)
                {
                    return Json(new { success = true, message = "تسک از روز شما حذف شد" });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف تسک" });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveFromMyDay", "خطا در حذف از روز من", ex);
                return Json(new { success = false, message = "خطا در حذف تسک" });
            }
        }




    }
}