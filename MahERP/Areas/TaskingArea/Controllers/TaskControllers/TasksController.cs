using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Repository.TaskRepository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
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
using System.Text;
using TaskWorkLogViewModel = MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels.TaskWorkLogViewModel;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK")]

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
        private readonly ITaskFilterRepository _taskFilterRepository; // اضافه کردن dependency
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly ITaskVisibilityRepository _TaskVisibilityRepository;


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
            IUserManagerRepository userRepository, IBaseRepository BaseRepository,
            ITaskVisibilityRepository taskVisibilityRepository,
            ITaskFilterRepository taskFilterRepository,ITaskHistoryRepository taskHistoryRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
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
            _taskFilterRepository = taskFilterRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _TaskVisibilityRepository = taskVisibilityRepository;

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

                // ⭐ تغییر به TaskFilterRepository
                var model = await _taskFilterRepository.GetAssignedByMeTasksAsync(userId);

                ViewBag.Title = "تسک‌های واگذار شده توسط من";
                ViewBag.IsAssignedByMe = true;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AssignedByMe", "مشاهده تسک‌های واگذار شده");

                return View("Index", model);
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

        public async Task<IActionResult> Index(TaskFilterViewModel filters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (filters == null)
                {
                    filters = new TaskFilterViewModel();
                }
                if (filters.StatusFilters == null || !filters.StatusFilters.Any())
                    filters.StatusFilters = new List<byte> { 0, 1 };
                // ⭐⭐⭐ بررسی TaskViewType از query string
                if (!string.IsNullOrEmpty(Request.Query["viewType"]))
                {
                    // تبدیل string به enum
                    if (Enum.TryParse<TaskViewType>(Request.Query["viewType"], out var viewType))
                    {
                        filters.ViewType = viewType;
                    }
                    else
                    {
                        // اگر مقدار نامعتبر بود، از DataAccessLevel استفاده کن
                        var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                        filters.ViewType = dataAccessLevel switch
                        {
                            0 => TaskViewType.MyTasks,
                            1 => TaskViewType.AllTasks,
                            2 => TaskViewType.AllTasks,
                            _ => TaskViewType.MyTasks
                        };
                    }
                }
                else
                {
                    // اگر TaskViewType ارسال نشده، از DataAccessLevel استفاده کن
                    var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                    filters.ViewType = dataAccessLevel switch
                    {
                        0 => TaskViewType.MyTasks,
                        1 => TaskViewType.AllTasks,
                        2 => TaskViewType.AllTasks,
                        _ => TaskViewType.MyTasks
                    };
                }

                ViewBag.currentUserId = GetUserId();
                var model = await _taskFilterRepository.GetTasksForIndexAsync(userId, filters);

                // بررسی اینکه آیا FilterCounts null است
                if (model.FilterCounts == null)
                {
                    Console.WriteLine("⚠️ FilterCounts is NULL! Re-calculating...");
                    model.FilterCounts = await _taskFilterRepository.GetAllFilterCountsAsync(userId);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "Index",
                    $"مشاهده لیست تسک‌ها - نوع: {filters.ViewType}");

                ViewBag.FilterCounts = model.FilterCounts;

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "Index", "خطا در دریافت لیست تسک‌ها", ex);
                Console.WriteLine($"❌ Exception in TasksController.Index: {ex.Message}\n{ex.StackTrace}");
                return RedirectToAction("ErrorView", "Home");
            }
        }
        // در قسمت AJAX Actions، بعد از GetCalendarEvents


        /// <summary>
        /// دریافت تسک‌های فیلتر شده برای Quick Filters - اصلاح شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFilteredTasks(int filterType)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ ساخت TaskFilterViewModel بر اساس filterType
                var filters = new TaskFilterViewModel
                {
                    ViewType = (QuickFilterType)filterType switch
                    {
                        QuickFilterType.AllVisible => TaskViewType.AllTasks,
                        QuickFilterType.MyAssigned => TaskViewType.MyTasks,
                        QuickFilterType.AssignedByMe => TaskViewType.AssignedByMe,
                        QuickFilterType.MyTeams => TaskViewType.MyTeamsHierarchy,
                        QuickFilterType.Supervised => TaskViewType.SupervisedTasks,
                        _ => TaskViewType.AllTasks
                    },
                    StatusFilters = new List<byte> { 0, 1 } // فقط در حال انجام
                };

                // ⭐⭐⭐ استفاده از GetTasksForIndexAsync - مشابه Index
                var model = await _taskFilterRepository.GetTasksForIndexAsync(userId, filters);

                // ⭐⭐⭐ بررسی FilterCounts
                if (model.FilterCounts == null)
                {
                    Console.WriteLine("⚠️ FilterCounts is NULL in GetFilteredTasks! Re-calculating...");
                    model.FilterCounts = await _taskFilterRepository.GetAllFilterCountsAsync(userId);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetFilteredTasks",
                    $"دریافت تسک‌های فیلتر شده - نوع: {(QuickFilterType)filterType}");

                // ⭐⭐⭐ رندر Partial View با مدل کامل
                var partialHtml = await this.RenderViewToStringAsync("_TasksGroupedPartial", model);

                // ⭐⭐⭐ برگرداندن JSON با HTML و اطلاعات اضافی
                return Json(new
                {
                    success = true,
                    html = partialHtml,
                    filterName = filters.ViewType.ToString(),
                    totalCount = model.Tasks?.Count ?? 0,
                    filterCounts = new
                    {
                        allVisibleCount = model.FilterCounts.AllVisibleCount,
                        myAssignedCount = model.FilterCounts.MyAssignedCount,
                        assignedByMeCount = model.FilterCounts.AssignedByMeCount,
                        myTeamsCount = model.FilterCounts.MyTeamsCount,
                        supervisedCount = model.FilterCounts.SupervisedCount
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetFilteredTasks",
                    "خطا در دریافت تسک‌های فیلتر شده", ex);

                // ⭐⭐⭐ در صورت خطا، یک مدل خالی برگردان
                var emptyModel = new TaskIndexViewModel
                {
                    UserLoginid = _userManager.GetUserId(User),
                    Filters = new TaskFilterViewModel(),
                    Tasks = new List<TaskViewModel>(),
                    PendingTasks = new List<TaskViewModel>(),
                    CompletedTasks = new List<TaskViewModel>(),
                    FilterCounts = new TaskFilterCountsViewModel()
                };

                var errorHtml = await this.RenderViewToStringAsync("_TasksGroupedPartial", emptyModel);

                return Json(new
                {
                    success = false,
                    html = errorHtml,
                    message = "خطا در دریافت تسک‌ها"
                });
            }
        }
        /// <summary>
        /// دریافت تعداد فیلترها برای Quick Filters (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFilterCounts()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var counts = await _taskFilterRepository.GetAllFilterCountsAsync(userId);

                return Json(new
                {
                    success = true,
                    allVisibleCount = counts.AllVisibleCount,
                    myAssignedCount = counts.MyAssignedCount,
                    assignedByMeCount = counts.AssignedByMeCount,
                    myTeamsCount = counts.MyTeamsCount,
                    supervisedCount = counts.SupervisedCount
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetFilterCounts",
                    "خطا در دریافت تعداد فیلترها", ex);

                return Json(new { success = false, message = "خطا در دریافت تعداد فیلترها" });
            }
        }
        /// <summary>
        /// GET: ایجاد تسک جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateNewTask()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // آماده‌سازی مدل با سیستم جدید
                var model = await _taskRepository.PrepareCreateTaskModelAsync(userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "CreateNewTask",
                    "مشاهده فرم ایجاد تسک جدید"
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CreateNewTask",
                    "خطا در نمایش فرم ایجاد تسک",
                    ex
                );

                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction("Index");
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
                    includeAssignments: true, includeAttachments: true, includeComments: true,includeStakeHolders:true);

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
                // Get current userId
                var currentUserId = _userManager.GetUserId(User);

                // Check if user is admin (can use PermissionExtensions or your own logic)
                var isAdmin = User.IsInRole("Admin"); // Or use your IsAdmin() extension

                // Check if user is manager of the task's team
                bool isManager = false;
                if (task.TeamId.HasValue)
                {
                    // Use TaskVisibilityRepository to check
                    isManager = await _TaskVisibilityRepository.IsUserTeamManagerAsync(currentUserId, task.TeamId.Value);
                }

                // Check if user is supervisor (based on your business logic, e.g. position power)
                bool isSupervisor = false;
                if (task.TeamId.HasValue)
                {
                    isSupervisor = await _TaskVisibilityRepository.CanViewBasedOnPositionAsync(currentUserId, task);
                }

                // Pass these to the ViewBag or ViewModel
                ViewBag.IsAdmin = isAdmin;
                ViewBag.IsManager = isManager;
                ViewBag.IsSupervisor = isSupervisor; await _taskNotificationService.MarkTaskNotificationsAsReadAsync(id, currentUserId);

                // ⭐⭐⭐ بررسی اینکه آیا تسک در "روز من" کاربر فعلی است
                var isInMyDay = await _taskRepository.IsTaskInMyDayAsync(id, currentUserId);
                ViewBag.IsInMyDay = isInMyDay;

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
        /// نمایش مودال تکمیل تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت اطلاعات تسک و آماده‌سازی مودال
                var model = await _taskRepository.PrepareCompleteTaskModalAsync(id, userId);

                if (model == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد یا شما به آن دسترسی ندارید" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "CompleteTask",
                    $"نمایش مودال تکمیل تسک {model.TaskCode}",
                    recordId: id.ToString(), entityType: "Tasks", recordTitle: model.TaskTitle);

                return PartialView("_CompleteTaskModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CompleteTask", "خطا در نمایش مودال تکمیل", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بارگذاری مودال" } }
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTaskPost(CompleteTaskViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new
                    {
                        status = "validation-error",
                        message = errors
                    });
                }

                var userId = _userManager.GetUserId(User);

                // ✅ ثبت تکمیل تسک از طریق Repository
                var result = await _taskRepository.CompleteTaskAsync(model, userId);

                if (!result.IsSuccess)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = result.ErrorMessage } }
                    });
                }

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskCompletedAsync(
                    model.TaskId,
                    userId,
                    model.TaskTitle,
                    model.TaskCode
                );

                // ⭐⭐⭐ ثبت غیرفعال شدن یادآوری‌ها در تاریخچه
                await _taskHistoryRepository.LogRemindersDeactivatedOnCompletionAsync(
                    model.TaskId,
                    userId,
                    model.TaskTitle,
                    model.TaskCode
                );

                // ⭐ ارسال نوتیفیکیشن
                await _taskNotificationService.NotifyTaskCompletedAsync(model.TaskId, userId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update, "Tasks", "CompleteTask",
                    $"تکمیل تسک {model.TaskCode} - {model.TaskTitle} و غیرفعال کردن یادآوری‌ها",
                    recordId: model.TaskId.ToString(), entityType: "Tasks", recordTitle: model.TaskTitle);

                // ✅ بازگرداندن response با redirect
                return Json(new
                {
                    status = "redirect",
                    message = new[] { new { status = "success", text = "تسک با موفقیت تکمیل شد و یادآوری‌ها غیرفعال شدند" } },
                    redirectUrl = Url.Action("Details", "Tasks", new { id = model.TaskId, area = "AdminArea" })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CompleteTaskPost", "خطا در ثبت تکمیل تسک", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ثبت تکمیل تسک: " + ex.Message } }
                });
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
                    // ⭐ ثبت در تاریخچه
                    await _taskHistoryRepository.LogTaskCreatedAsync(task.Id, currentUserId, task.Title, task.TaskCode);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelect(int branchId)
        {
            try
            {
                var branchData = await _taskRepository.GetBranchTriggeredDataAsync(branchId);

                // ⭐⭐⭐ استفاده از Repository به جای متد محلی
                var teamsWithManagers = await _taskRepository.GetBranchTeamsWithManagersAsync(branchId);

                var viewList = new List<object>
        {
            new {
                elementId = "UsersDiv",
                view = new { result = await this.RenderViewToStringAsync("_BranchUsersSelect", branchData.Users) }
            },
            new {
                elementId = "TeamsDiv",
                view = new { result = await this.RenderViewToStringAsync("_BranchTeamsSelect", teamsWithManagers) }
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
        /// دریافت تیم‌های یک کاربر در شعبه مشخص (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserTeams(string userId, int branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || branchId <= 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "اطلاعات ورودی نامعتبر است"
                    });
                }

                Console.WriteLine($"🔍 GetUserTeams: UserId={userId}, BranchId={branchId}");

                // ⭐ دریافت تیم‌های کاربر از Repository
                var userTeams = await _taskRepository.GetUserTeamsByBranchAsync(userId, branchId);

                Console.WriteLine($"✅ Found {userTeams.Count} teams");
                foreach (var team in userTeams)
                {
                    Console.WriteLine($"   - {team.Title}, Manager: {team.ManagerName ?? "N/A"}");
                }

                // ⭐⭐⭐ رندر Partial View با داده‌های کامل
                var partialHtml = await this.RenderViewToStringAsync("_UserTeamsSelect", userTeams);

                // ⭐ بررسی اینکه HTML تولید شده خالی نباشد
                if (string.IsNullOrWhiteSpace(partialHtml))
                {
                    Console.WriteLine("⚠️ Warning: Partial view rendered empty HTML");
                    throw new Exception("Partial view rendering failed");
                }

                var viewList = new List<object>
                {
                    new {
                        elementId = "team-select-container",
                        view = new { result = partialHtml }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetUserTeams",
                    $"بارگذاری تیم‌های کاربر {userId} در شعبه {branchId} - تعداد: {userTeams.Count}");

                return Json(new
                {
                    status = "update-view",
                    viewList = viewList,
                    teamsCount = userTeams.Count,
                    hasNoTeam = !userTeams.Any() || userTeams.All(t => t.Id == 0)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserTeams: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                await _activityLogger.LogErrorAsync("Tasks", "GetUserTeams",
                    "خطا در دریافت تیم‌های کاربر", ex);

                // ⭐ در صورت خطا، HTML ساده برگردان
                var errorHtml = @"<select class='form-select form-select-sm team-select' disabled>
                                    <option value='0'>بدون تیم (خطا در بارگذاری)</option>
                                  </select>
                                  <small class='form-text text-danger mt-1'>
                                    <i class='fa fa-times-circle me-1'></i>
                                    خطا در بارگذاری تیم‌ها
                                  </small>";

                return Json(new
                {
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


        [HttpGet]
        public async Task<IActionResult> GetTaskHistory(int taskId)
        {
            try
            {
                var history = await _taskRepository.GetTaskHistoryAsync(taskId);
                return PartialView("_TaskHistoryTimeline", history);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskHistory", "خطا در دریافت تاریخچه", ex);
                return PartialView("_TaskHistoryTimeline", new List<TaskHistoryViewModel>());
            }
        }


        // اضافه کردن این methods به TasksController

        #region Task Reminders Management

        /// <summary>
        /// دریافت لیست یادآوری‌های تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskReminders(int taskId)
        {
            try
            {
                var currentUserId = GetUserId();
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new { status = "error", message = "تسک یافت نشد" });
                }

                var reminders = await _taskRepository.GetTaskRemindersListAsync(taskId);

                // ⭐ ارسال وضعیت قفل به View
                ViewBag.IsTaskCompleted = task.TaskAssignments?.Any(a => a.CompletionDate.HasValue && a.AssignedUserId == currentUserId) ?? false;


                return PartialView("_TaskRemindersList", new { TaskId = taskId, Reminders = reminders });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskReminders", "خطا در دریافت یادآوری‌ها", ex);
                return Json(new { status = "error", message = "خطا در دریافت یادآوری‌ها" });
            }
        }

        /// <summary>
        /// نمایش مودال افزودن یادآوری جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddReminderModal(int taskId)
        {
            try
            {
                // ✅ استفاده از Repository
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new { status = "error", message = "تسک یافت نشد" });
                }

                var model = new TaskReminderViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    ReminderType = 2, // پیش‌فرض: قبل از مهلت
                    DaysBeforeDeadline = 3,
                    NotificationTime = new TimeSpan(9, 0, 0),
                    IsActive = true
                };

                return PartialView("_AddReminderModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddReminderModal", "خطا در نمایش مودال", ex);
                return Json(new { status = "error", message = "خطا در نمایش فرم" });
            }
        }
        /// <summary>
        /// ذخیره یادآوری جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReminder(TaskReminderViewModel model)
        {
            try
            {
              
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // ✅ استفاده از Repository
                var reminderId = await _taskRepository.CreateReminderAsync(model, currentUserId);

                if (reminderId == 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری" } }
                    });
                }

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogReminderAddedAsync(
                    model.TaskId,
                    currentUserId,
                    reminderId,
                    model.Title,
                    model.ReminderType
                );

                // ✅✅✅ ساختار صحیح بر اساس main.js
                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "reminders-list-container",
                    view = new
                    {
                        result = await this.RenderViewToStringAsync(
                            "_TaskRemindersList",
                            new {
                                TaskId = model.TaskId,
                                Reminders = await _taskRepository.GetTaskRemindersListAsync(model.TaskId)
                            }
                        )
                    }
                }
            },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SaveReminder", "خطا در ذخیره یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری" } }
                });
            }
        }
        /// <summary>
        /// نمایش مودال تأیید حذف یادآوری
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteReminderConfirmModal(int reminderId)
        {
            try
            {
                var reminder = await _taskRepository.GetReminderByIdAsync(reminderId);
                if (reminder == null)
                {
                    return Json(new { status = "error", message = "یادآوری یافت نشد" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "DeleteReminderConfirmModal",
                    $"نمایش مودال تأیید حذف یادآوری {reminder.Title}");

                return PartialView("_DeleteReminderConfirmModal", reminderId);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteReminderConfirmModal", "خطا در نمایش مودال", ex);
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }
        /// <summary>
        /// حذف یادآوری تسک (برای استفاده با modal-ajax-save)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteTaskReminder(int id)
        {
            try
            {
                // ✅ استفاده از Repository
                var reminder = await _taskRepository.GetReminderByIdAsync(id);
                if (reminder == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "یادآوری یافت نشد" } }
                    });
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var taskId = reminder.TaskId;
                var reminderTitle = reminder.Title;

                // ✅ غیرفعال کردن از طریق Repository
                var result = await _taskRepository.DeactivateReminderAsync(id);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در حذف یادآوری" } }
                    });
                }

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogReminderDeletedAsync(
                    taskId,
                    currentUserId,
                    id,
                    reminderTitle
                );

                // ✅✅✅ رندر مستقیم Partial View و ارسال در viewList
                var updatedReminders = await _taskRepository.GetTaskRemindersListAsync(taskId);
                var partialHtml = await this.RenderViewToStringAsync(
                    "_TaskRemindersList",
                    new { TaskId = taskId, Reminders = updatedReminders }
                );

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "reminders-list-container",
                    view = new { result = partialHtml }
                }
            },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteTaskReminder", "خطا در حذف یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف یادآوری" } }
                });
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن یادآوری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReminderStatus(int id)
        {
            try
            {
                // ✅ استفاده از Repository
                var reminder = await _taskRepository.GetReminderByIdAsync(id);
                if (reminder == null)
                {
                    return Json(new { status = "error", message = "یادآوری یافت نشد" });
                }

                // ✅ تغییر وضعیت از طریق Repository
                var result = await _taskRepository.ToggleReminderActiveStatusAsync(id);

                if (!result)
                {
                    return Json(new { status = "error", message = "خطا در تغییر وضعیت" });
                }

                // دریافت وضعیت جدید
                var updatedReminder = await _taskRepository.GetReminderByIdAsync(id);
                var statusText = updatedReminder.IsActive ? "فعال" : "غیرفعال";

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = $"یادآوری {statusText} شد" } },
                    updateTarget = "#reminders-list-container",
                    updateUrl = Url.Action("GetTaskReminders", new { taskId = reminder.TaskId })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ToggleReminderStatus", "خطا در تغییر وضعیت", ex);
                return Json(new { status = "error", message = "خطا در تغییر وضعیت" });
            }
        }

        #endregion

        #region My Day Actions - Quick Add Without Modal

        /// <summary>
        /// نمایش مودال افزودن تسک به "روز من" (با تاریخ و یادداشت)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddToMyDayModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت اطلاعات تسک
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new { status = "error", message = "تسک یافت نشد" });
                }

                // بررسی دسترسی کاربر به تسک
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(userId, taskId);
                if (!hasAccess)
                {
                    return Json(new { status = "error", message = "شما به این تسک دسترسی ندارید" });
                }

                // بررسی اینکه آیا قبلاً در روز من است
                var isAlreadyInMyDay = await _taskRepository.IsTaskInMyDayAsync(taskId, userId);

                var model = new TaskMyDayViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd"),
                    IsAlreadyInMyDay = isAlreadyInMyDay
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AddToMyDayModal",
                    $"نمایش مودال افزودن تسک {task.TaskCode} به روز من");

                return PartialView("_AddToMyDayModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddToMyDayModal", "خطا در نمایش مودال افزودن تسک به روز من", ex);
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }

        /// <summary>
        /// افزودن تسک به "روز من" با تاریخ و یادداشت (از طریق Modal)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToMyDay(TaskMyDayViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new
                    {
                        status = "validation-error",
                        message = errors
                    });
                }

                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(userId, model.TaskId);
                if (!hasAccess)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما به این تسک دسترسی ندارید" } }
                    });
                }

                // تبدیل تاریخ شمسی به میلادی
                DateTime plannedDate = DateTime.Now.Date;
                if (!string.IsNullOrEmpty(model.PlannedDatePersian))
                {
                    plannedDate = ConvertDateTime.ConvertShamsiToMiladi(model.PlannedDatePersian);
                }

                // اضافه کردن به روز من
                var result = await _taskRepository.AddTaskToMyDayAsync(
                    model.TaskId,
                    userId,
                    plannedDate,
                    model.PlanNote);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در افزودن تسک به روز من" } }
                    });
                }

                // ⭐ دریافت اطلاعات تسک برای ثبت در تاریخچه
                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);
                
                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                    model.TaskId,
                    userId,
                    task.Title,
                    task.TaskCode,
                    plannedDate
                );

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create, "Tasks", "AddToMyDay",
                    $"افزودن تسک {task.TaskCode} به روز من - تاریخ: {model.PlannedDatePersian}",
                    recordId: model.TaskId.ToString(), entityType: "Tasks", recordTitle: task.Title);

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تسک با موفقیت به روز من اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddToMyDay", "خطا در افزودن تسک به روز من", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن تسک: " + ex.Message } }
                });
            }
        }
        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromMyDay(int taskId, string? targetDatePersian = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // تبدیل تاریخ اگر ارسال شده
                DateTime? targetDate = null;
                if (!string.IsNullOrEmpty(targetDatePersian))
                {
                    targetDate = ConvertDateTime.ConvertShamsiToMiladi(targetDatePersian);
                }

                // حذف از روز من
                var result = await _taskRepository.RemoveTaskFromMyDayAsync(taskId, userId, targetDate);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "تسک در روز من یافت نشد یا خطا در حذف"
                    });
                }

                // ⭐ ثبت در تاریخچه
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                await _taskHistoryRepository.LogTaskRemovedFromMyDayAsync(
                    taskId,
                    userId,
                    task.Title,
                    task.TaskCode
                );

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete, "Tasks", "RemoveFromMyDay",
                    $"حذف تسک {task.TaskCode} از روز من",
                    recordId: taskId.ToString(), entityType: "Tasks", recordTitle: task.Title);

                return Json(new
                {
                    success = true,
                    message = "تسک از روز من حذف شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveFromMyDay", "خطا در حذف تسک از روز من", ex);
                return Json(new
                {
                    success = false,
                    message = "خطا در حذف تسک از روز من"
                });
            }
        }

        /// <summary>
        /// دریافت صفحه "روز من"
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyDayTasks(string? date = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // تبدیل تاریخ اگر ارسال شده
                DateTime? selectedDate = null;
                if (!string.IsNullOrEmpty(date))
                {
                    selectedDate = ConvertDateTime.ConvertShamsiToMiladi(date);
                }

                // دریافت تسک‌های روز من
                var model = await _taskRepository.GetMyDayTasksAsync(userId, selectedDate);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "MyDay",
                    "مشاهده صفحه روز من");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "MyDay", "خطا در دریافت تسک‌های روز من", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// نمایش مودال افزودن کاربر به تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AssignUserToTaskModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = await _taskRepository.GetTaskByIdAsync(taskId);

                if (task == null)
                    return NotFound();

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Forbid();

                var model = new AssignUserToTaskViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    BranchId = task.BranchId ?? 0
                };

                // دریافت کاربران و تیم‌های شعبه
                if (model.BranchId > 0)
                {
                    var branchData = await _taskRepository.GetBranchTriggeredDataAsync(model.BranchId);
                    model.AvailableUsers = branchData.Users;
                    model.AvailableTeams = branchData.Teams;
                }

                return PartialView("_AssignUserToTaskModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AssignUserToTaskModal", "خطا در نمایش مودال", ex);
                return StatusCode(500, "خطا در بارگذاری مودال");
            }
        }
        /// <summary>
        /// ثبت تخصیص کاربر جدید به تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUserToTask(AssignUserToTaskViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);

                if (task == null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما دسترسی لازم را ندارید" } }
                    });

                // ⭐ بررسی اینکه آیا تسک برای کاربر جاری تکمیل شده؟
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(userId, model.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما این تسک را تکمیل کرده‌اید و نمی‌توانید کاربر جدید اضافه کنید" } }
                    });

                // بررسی تکراری نبودن
                var existingAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(
                    model.SelectedUserId,
                    model.TaskId);

                if (existingAssignment != null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "این کاربر قبلاً به تسک اختصاص داده شده است" } }
                    });

                // ایجاد Assignment
                var result = await _taskRepository.AssignUserToTaskAsync(
                    model.TaskId,
                    model.SelectedUserId,
                    userId,
                    model.SelectedTeamId,
                    model.Description);

                if (result)
                {
                    // ثبت در تاریخچه
                    var assignedUser = await _userManager.FindByIdAsync(model.SelectedUserId);
                    var assignedUserName = assignedUser != null ? $"{assignedUser.FirstName} {assignedUser.LastName}" : "نامشخص";

                    await _taskHistoryRepository.LogUserAssignedAsync(
                        model.TaskId,
                        userId,
                        assignedUserName);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Tasks",
                        "AssignUserToTask",
                        $"تخصیص کاربر {assignedUserName} به تسک {task.Title}",
                        recordId: model.TaskId.ToString());

                    // ⭐⭐⭐ دریافت لیست به‌روزرسانی شده اعضا
                    var updatedTask = _taskRepository.GetTaskById(
                        model.TaskId,
                        includeAssignments: true);

                    var assignments = updatedTask.TaskAssignments
                        .Select(a => new TaskAssignmentViewModel
                        {
                            Id = a.Id,
                            TaskId = a.TaskId,
                            AssignedUserId = a.AssignedUserId,
                            AssignedUserName = a.AssignedUser != null
                                ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                                : "نامشخص",
                            AssignDate = a.AssignmentDate,
                            CompletionDate = a.CompletionDate, // ⭐⭐⭐ از Assignment
                            Description = a.Description
                        })
                        .ToList();

                    // ⭐⭐⭐ رندر Partial View
                    var partialHtml = await this.RenderViewToStringAsync(
                        "_TaskMembersList",
                        new
                        {
                            Assignments = assignments,
                            TaskId = task.Id, // ⭐ اضافه شده
                            IsCreator = isCreator,
                            IsManager = isAdmin,
                            IsTaskCompleted = isTaskCompletedForCurrentUser // ⭐⭐⭐ برای کاربر جاری
                        });

                    // ⭐⭐⭐ برگرداندن JSON با ساختار update-view
                    return Json(new
                    {
                        status = "update-view",
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "task-members-container",
                        view = new { result = partialHtml }
                    }
                },
                        message = new[] { new { status = "success", text = "کاربر با موفقیت به تسک اختصاص داده شد" } }
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تخصیص کاربر" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AssignUserToTask", "خطا در تخصیص کاربر", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }
        /// <summary>
        /// نمایش مودال تأیید حذف Assignment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveAssignmentModal(int assignmentId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var assignment = await _taskRepository.GetTaskAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                    return NotFound();

                var task = assignment.Task;

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Forbid();

                var model = new RemoveAssignmentViewModel
                {
                    AssignmentId = assignmentId,
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    UserName = assignment.AssignedUser != null
                        ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}"
                        : "نامشخص"
                };

                return PartialView("_RemoveAssignmentModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveAssignmentModal", "خطا در نمایش مودال", ex);
                return StatusCode(500, "خطا در بارگذاری مودال");
            }
        }
        /// <summary>
        /// حذف Assignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int assignmentId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var assignment = await _taskRepository.GetTaskAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تخصیص یافت نشد" } }
                    });

                var task = assignment.Task;

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما دسترسی لازم را ندارید" } }
                    });

                // ⭐ بررسی اینکه آیا تسک برای کاربر جاری تکمیل شده؟
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(userId, task.Id);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما این تسک را تکمیل کرده‌اید و نمی‌توانید کاربر حذف کنید" } }
                    });

                // حذف Assignment
                var removedUserName = assignment.AssignedUser != null
                    ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}"
                    : "نامشخص";

                var result = await _taskRepository.RemoveTaskAssignmentAsync(assignmentId);

                if (result)
                {
                    // ثبت در تاریخچه
                    await _taskHistoryRepository.LogUserRemovedAsync(
                        task.Id,
                        userId,
                        removedUserName);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Tasks",
                        "RemoveAssignment",
                        $"حذف {removedUserName} از تسک {task.Title}",
                        recordId: task.Id.ToString());

                    // ⭐⭐⭐ دریافت لیست به‌روزرسانی شده اعضا
                    var updatedTask = _taskRepository.GetTaskById(
                        task.Id,
                        includeAssignments: true);

                    var assignments = updatedTask.TaskAssignments
                        .Select(a => new TaskAssignmentViewModel
                        {
                            Id = a.Id,
                            TaskId = a.TaskId,
                            AssignedUserId = a.AssignedUserId,
                            AssignedUserName = a.AssignedUser != null
                                ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                                : "نامشخص",
                            AssignDate = a.AssignmentDate,
                            CompletionDate = a.CompletionDate, // ⭐⭐⭐ از Assignment
                            Description = a.Description
                        })
                        .ToList();

                    // ⭐⭐⭐ رندر Partial View
                    var partialHtml = await this.RenderViewToStringAsync(
                        "_TaskMembersList",
                        new
                        {
                            Assignments = assignments,
                            TaskId = task.Id,
                            IsCreator = isCreator,
                            IsManager = isAdmin,
                            IsTaskCompleted = isTaskCompletedForCurrentUser // ⭐⭐⭐ برای کاربر جاری
                        });

                    // ⭐⭐⭐ برگرداندن JSON با ساختار update-view
                    return Json(new
                    {
                        status = "update-view",
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "task-members-container",
                        view = new { result = partialHtml }
                    }
                },
                        message = new[] { new { status = "success", text = $"{removedUserName} با موفقیت از تسک حذف شد" } }
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف کاربر" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveAssignment", "خطا در حذف کاربر", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }
        /// <summary>
        /// دریافت تیم‌های کاربر برای AJAX
        /// </summary>
    
        [HttpPost]
        public async Task<IActionResult> GetUserTeamsForAssignment(string userId, int branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || branchId <= 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "پارامترهای نامعتبر"
                    });
                }

                // ⭐ دریافت تیم‌ها با اطلاعات کامل (شامل مدیر)
                var userTeams = await _taskRepository.GetUserTeamsByBranchAsync(userId, branchId);

                var html = "";

                if (!userTeams.Any())
                {
                    html = @"<select class='form-select team-select' name='SelectedTeamId' required disabled>
                <option value='0'>بدون تیم</option>
             </select>
             <small class='form-text text-warning mt-1'>
                <i class='fa fa-exclamation-triangle me-1'></i>
                این کاربر در هیچ تیمی عضو نیست
             </small>";
                }
                else if (userTeams.Count == 1)
                {
                    var team = userTeams.First();
                    // ⭐⭐⭐ اضافه کردن data attributes برای مدیر
                    var managerInfo = !string.IsNullOrEmpty(team.ManagerName)
                        ? $" (مدیر: {team.ManagerName})"
                        : "";

                    html = $@"<select class='form-select team-select' name='SelectedTeamId' required>
                <option value='{team.Id}' 
                        data-manager-id='{team.ManagerUserId}' 
                        data-manager-name='{team.ManagerName}' 
                        data-member-count='{team.MemberCount}' 
                        selected>
                    {team.Title}{managerInfo}
                </option>
              </select>
              <small class='form-text text-success mt-1'>
                <i class='fa fa-check me-1'></i>
                تیم به صورت خودکار انتخاب شد
              </small>";
                }
                else
                {
                    html = "<select class='form-select team-select' name='SelectedTeamId' required>";
                    html += "<option value=''>انتخاب تیم...</option>";

                    foreach (var team in userTeams)
                    {
                        // ⭐⭐⭐ اضافه کردن نام مدیر به متن و data attributes
                        var managerInfo = !string.IsNullOrEmpty(team.ManagerName)
                            ? $" (مدیر: {team.ManagerName})"
                            : "";

                        html += $@"<option value='{team.Id}' 
                                  data-manager-id='{team.ManagerUserId ?? ""}' 
                                  data-manager-name='{team.ManagerName ?? ""}' 
                                  data-member-count='{team.MemberCount}'>
                              {team.Title}{managerInfo}
                          </option>";
                    }

                    html += "</select>";
                    html += @"<small class='form-text text-muted mt-1'>
                <i class='fa fa-info-circle me-1'></i>
                لطفاً تیم مربوطه را انتخاب کنید
              </small>";
                }

                // ⭐⭐⭐ لاگ برای debug
                Console.WriteLine($"✅ GetUserTeamsForAssignment: User {userId}, Teams: {userTeams.Count}");
                foreach (var team in userTeams)
                {
                    Console.WriteLine($"   - {team.Title}, Manager: {team.ManagerName ?? "N/A"}");
                }

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "TeamSelectDiv",
                    view = new { result = html }
                }
            },
                    hasNoTeam = !userTeams.Any(),
                    teamCount = userTeams.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserTeamsForAssignment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new
                {
                    status = "error",
                    message = "خطا در دریافت تیم‌ها"
                });
            }
        }


        #endregion

        #region Task Work Log

        /// <summary>
        /// مودال ثبت کار انجام شده روی تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LogTaskWorkModal(int taskId)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // ✅ استفاده از Repository به جای دسترسی مستقیم به DbContext
                var model = await _taskRepository.PrepareLogTaskWorkModalAsync(taskId, currentUserId);

                if (model == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما عضو این تسک نیستید یا تسک یافت نشد"
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "LogTaskWorkModal",
                    $"نمایش مودال ثبت کار برای تسک {model.TaskCode}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: model.TaskTitle);

                return PartialView("_LogTaskWorkModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "LogTaskWorkModal", "خطا در نمایش مودال ثبت کار", ex);
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری مودال"
                });
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogTaskWork(TaskWorkLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    status = "validation-error",
                    message = new[] {
                new {
                    status = "error",
                    text = "اطلاعات وارد شده معتبر نیست"
                }
            }
                });
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // ✅ استفاده از Repository
            var result = await _taskRepository.AddTaskWorkLogAsync(
                model.TaskId,
                currentUserId,
                model.WorkDescription,
                model.DurationMinutes,
                model.ProgressPercentage
            );

            if (result.Success)
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Tasks",
                    "LogTaskWork",
                    $"ثبت گزارش کار برای تسک {model.TaskId}",
                    recordId: model.TaskId.ToString(),
                    entityType: "Tasks");

                return Json(new
                {
                    success = true,
                    status = "success",
                    message = new[] {
                new {
                    status = "success",
                    text = result.Message
                }
            }
                });
            }

            return Json(new
            {
                success = false,
                status = "error",
                message = new[] {
            new {
                status = "error",
                text = result.Message
            }
        }
            });
        }

        /// <summary>
        /// مودال نمایش لیست گزارش کارهای یک تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewTaskWorkLogsModal(int taskId)
        {
            try
            {
                // ✅ استفاده از Repository
                var workLogs = await _taskRepository.GetTaskWorkLogsAsync(taskId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "ViewTaskWorkLogsModal",
                    $"نمایش لیست گزارش کارهای تسک {taskId}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks");

                return PartialView("_TaskWorkLogsModal", workLogs);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ViewTaskWorkLogsModal", "خطا در نمایش لیست گزارش کارها", ex);
                return PartialView("_TaskWorkLogsModal", new List<TaskWorkLogViewModel>());
            }
        }

        #endregion
        #region Task Focus

        /// <summary>
        /// تنظیم فوکوس روی تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetTaskFocus(int taskId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskRepository.SetTaskFocusAsync(taskId, currentUserId);

            return Json(new { success = result.Success, message = result.Message });
        }

        /// <summary>
        /// حذف فوکوس از تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveTaskFocus(int taskId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskRepository.RemoveTaskFocusAsync(taskId, currentUserId);

            return Json(new { success = result.Success, message = result.Message });
        }

        #endregion


        /// <summary>
        /// ⭐⭐⭐ NEW: AJAX - بروزرسانی لیست‌های Contact و Organization بر اساس شعبه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BranchTriggerSelectForStakeholders(int branchId)
        {
            try
            {
                if (branchId <= 0)
                {
                    return Json(new { success = false, message = "شعبه نامعتبر است" });
                }

                // دریافت Contacts
                var contacts = await _taskRepository.GetBranchContactsAsync(branchId);

                // دریافت Organizations
                var organizations = await _taskRepository.GetBranchOrganizationsAsync(branchId);

                // ⭐⭐⭐ رندر Partial Views
                var contactsHtml = await this.RenderViewToStringAsync("_ContactsDropdown", contacts);
                var organizationsHtml = await this.RenderViewToStringAsync("_OrganizationsDropdown", organizations);

                return Json(new
                {
                    success = true,
                    status = "update-view",
                    viewList = new[]
                    {
                        new { elementId = "ContactSelectionDiv", view = new { result = contactsHtml } },
                        new { elementId = "OrganizationSelectionDiv", view = new { result = organizationsHtml } }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "BranchTriggerSelectForStakeholders",
                    "خطا در بارگذاری Contact/Organization",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new { success = false, message = $"خطا در بارگذاری: {ex.Message}" });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: AJAX - دریافت سازمان‌های مرتبط با Contact انتخاب شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ContactTriggerSelect(int contactId)
        {
            try
            {
                if (contactId <= 0)
                {
                    return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
                }

                // دریافت سازمان‌های Contact
                var organizations = await _taskRepository.GetContactOrganizationsAsync(contactId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "ContactTriggerSelect",
                    $"دریافت سازمان‌های Contact {contactId}",
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", organizations);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ContactTriggerSelect",
                    "خطا در دریافت سازمان‌های Contact",
                    ex,
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
            }
        }
        /// <summary>
        /// بارگذاری افراد مرتبط با Organization انتخاب شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OrganizationTriggerSelect(int organizationId)
        {
            try
            {
                var contacts = await _taskRepository.GetOrganizationContactsAsync(organizationId);

                var model = new
                {
                    Contacts = contacts,
                    OrganizationId = organizationId
                };

                return PartialView("_OrganizationContactsPartial", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "OrganizationTriggerSelect",
                    "خطا در بارگذاری افراد سازمان",
                    ex
                );

                return PartialView("_OrganizationContactsPartial", new
                {
                    Contacts = new List<ContactViewModel>(),
                    OrganizationId = organizationId
                });
            }
        }
        /// <summary>
        /// دریافت آمار Hero Section
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskHeroStats(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true);

            if (task == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                progressPercentage = task.TaskOperations.Any()
                    ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                    : 0,
                completedOperations = task.TaskOperations.Count(o => o.IsCompleted),
                totalOperations = task.TaskOperations.Count
            });
        }

        /// <summary>
        /// دریافت درصد پیشرفت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskProgress(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true);

            if (task == null)
                return Json(new { success = false });

            var percentage = task.TaskOperations.Any()
                ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                : 0;

            return Json(new { success = true, percentage });
        }

        /// <summary>
        /// دریافت آمار Sidebar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskSidebarStats(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true, includeAssignments: true);

            if (task == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                completedOps = task.TaskOperations.Count(o => o.IsCompleted),
                pendingOps = task.TaskOperations.Count(o => !o.IsCompleted),
                teamMembers = task.TaskAssignments.Count,
                progress = task.TaskOperations.Any()
                    ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                    : 0
            });
        }
        #region Task Comments Management

        /// <summary>
        /// افزودن کامنت/پیام جدید به تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddTaskComment(TaskCommentViewModel model, List<IFormFile> Attachments)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new
                    {
                        success = false,
                        message = errors
                    });
                }

                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی به تسک
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(currentUserId, model.TaskId);
                if (!hasAccess)
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما به این تسک دسترسی ندارید"
                    });
                }

                // ⭐ بررسی اینکه آیا تسک تکمیل شده؟
                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, model.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new
                    {
                        success = false,
                        message = "این تسک تکمیل شده و امکان ارسال پیام وجود ندارد"
                    });
                }

                // ایجاد کامنت
                var comment = new TaskComment
                {
                    TaskId = model.TaskId,
                    CommentText = model.CommentText.Trim(),
                    IsImportant = model.IsImportant,
                    IsPrivate = model.IsPrivate,
                    CommentType = model.CommentType,
                    CreatorUserId = currentUserId,
                    CreateDate = DateTime.Now,
                    ParentCommentId = model.ParentCommentId
                };

                _uow.TaskCommentUW.Create(comment);
                _uow.Save();

                // ⭐⭐⭐ مدیریت فایل‌های پیوست
                if (Attachments != null && Attachments.Any())
                {
                    foreach (var file in Attachments)
                    {
                        if (file.Length > 0)
                        {
                            // ذخیره فایل
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "task-comments", model.TaskId.ToString());
                            Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // ایجاد رکورد پیوست
                            var attachment = new TaskCommentAttachment
                            {
                                CommentId = comment.Id,
                                FileName = file.FileName,
                                FilePath = $"/uploads/task-comments/{model.TaskId}/{uniqueFileName}",
                                FileExtension = Path.GetExtension(file.FileName),
                                FileSize = file.Length.ToString(),
                                FileUUID = uniqueFileName,
                                UploadDate = DateTime.Now,
                                UploaderUserId = currentUserId
                            };

                            _uow.TaskCommentAttachmentUW.Create(attachment);
                        }
                    }

                    _uow.Save();
                }

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogCommentAddedAsync(
                    model.TaskId,
                    currentUserId,
                    comment.Id,
                    model.CommentText.Substring(0, Math.Min(50, model.CommentText.Length))
                );

                // ⭐ ارسال نوتیفیکیشن به اعضای تسک
                await _taskNotificationService.NotifyNewCommentAsync(model.TaskId, currentUserId, comment.Id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Tasks",
                    "AddTaskComment",
                    $"افزودن کامنت به تسک {task.TaskCode}",
                    recordId: model.TaskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                return Json(new
                {
                    success = true,
                    message = "پیام با موفقیت ارسال شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddTaskComment", "خطا در افزودن کامنت", ex);
                return Json(new
                {
                    success = false,
                    message = "خطا در ارسال پیام: " + ex.Message
                });
            }
        }

        /// <summary>
        /// دریافت کامنت‌های یک تسک (برای Refresh)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskComments(int taskId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(currentUserId, taskId);
                if (!hasAccess)
                {
                    return Json(new { success = false, message = "شما به این تسک دسترسی ندارید" });
                }

                // دریافت کامنت‌ها
                var comments = await _taskRepository.GetTaskCommentsAsync(taskId);

                // رندر Partial View
                var html = await this.RenderViewToStringAsync("_TaskCommentsPartial", comments);

                return Json(new
                {
                    success = true,
                    html = html
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskComments", "خطا در دریافت کامنت‌ها", ex);
                return Json(new { success = false, message = "خطا در بارگذاری پیام‌ها" });
            }
        }

        /// <summary>
        /// حذف کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaskComment(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // دریافت کامنت
                var comment =  _uow.TaskCommentUW.GetById(id);
                if (comment == null)
                {
                    return Json(new { success = false, message = "پیام یافت نشد" });
                }

                // بررسی سازنده
                if (comment.CreatorUserId != currentUserId)
                {
                    return Json(new { success = false, message = "شما فقط می‌توانید پیام‌های خود را حذف کنید" });
                }

                // ⭐ بررسی اینکه آیا تسک تکمیل شده؟
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, comment.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new { success = false, message = "این تسک تکمیل شده و امکان حذف پیام وجود ندارد" });
                }

                // حذف فایل‌های پیوست
                var attachments =  _uow.TaskCommentAttachmentUW
                    .Get(a => a.CommentId == id).ToList();

                foreach (var attachment in attachments)
                {
                    // حذف فیزیکی فایل
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    _uow.TaskCommentAttachmentUW.Delete(attachment);
                }

                // حذف کامنت
                _uow.TaskCommentUW.Delete(comment);
                _uow.Save();

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogCommentDeletedAsync(
                    comment.TaskId,
                    currentUserId,
                    id
                );

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Tasks",
                    "DeleteTaskComment",
                    $"حذف کامنت از تسک {comment.TaskId}",
                    recordId: comment.TaskId.ToString(),
                    entityType: "Tasks"
                );

                return Json(new { success = true, message = "پیام با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteTaskComment", "خطا در حذف کامنت", ex);
                return Json(new { success = false, message = "خطا در حذف پیام" });
            }
        }

        /// <summary>
        /// ویرایش کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTaskComment(int id, string commentText)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // دریافت کامنت
                var comment =  _uow.TaskCommentUW.GetById(id);
                if (comment == null)
                {
                    return Json(new { success = false, message = "پیام یافت نشد" });
                }

                // بررسی سازنده
                if (comment.CreatorUserId != currentUserId)
                {
                    return Json(new { success = false, message = "شما فقط می‌توانید پیام‌های خود را ویرایش کنید" });
                }

                // ⭐ بررسی تکمیل تسک
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, comment.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new { success = false, message = "این تسک تکمیل شده و امکان ویرایش پیام وجود ندارد" });
                }

                // بروزرسانی
                comment.CommentText = commentText.Trim();
                comment.IsEdited = true;
                comment.EditDate = DateTime.Now;

                _uow.TaskCommentUW.Update(comment);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "Tasks",
                    "EditTaskComment",
                    $"ویرایش کامنت در تسک {comment.TaskId}",
                    recordId: comment.TaskId.ToString(),
                    entityType: "Tasks"
                );

                return Json(new { success = true, message = "پیام با موفقیت ویرایش شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "EditTaskComment", "خطا در ویرایش کامنت", ex);
                return Json(new { success = false, message = "خطا در ویرایش پیام" });
            }
        }

        #endregion
    }
}