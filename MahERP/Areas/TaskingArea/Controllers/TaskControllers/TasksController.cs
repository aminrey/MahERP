using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.Extentions;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
        private readonly TaskCodeGenerator _taskCodeGenerator;
        protected readonly IUserManagerRepository _userRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ITeamRepository _teamRepository;
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
    TaskCodeGenerator taskCodeGenerator,
    IUserManagerRepository userRepository,
    IBaseRepository BaseRepository,
    ITaskHistoryRepository taskHistoryRepository,
    ModuleTrackingBackgroundService moduleTracking,
    IModuleAccessService moduleAccessService,
    IOrganizationRepository organizationRepository, // ⭐⭐⭐ اضافه شده
    ITeamRepository teamRepository) // ⭐⭐⭐ اضافه شده
    : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _taskRepository = taskRepository;
            _stakeholderRepository = stakeholderRepository;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _taskCodeGenerator = taskCodeGenerator;
            _userRepository = userRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _organizationRepository = organizationRepository; // ⭐⭐⭐ اضافه شده
            _teamRepository = teamRepository; // ⭐⭐⭐ اضافه شده
        }

        #region Dashboard
        /// <summary>
        /// صفحه اصلی لیست تسک‌ها - نسخه جدید
        /// </summary>
        public async Task<IActionResult> Index(
            TaskViewType viewType = TaskViewType.MyTasks,
            TaskGroupingType grouping = TaskGroupingType.Team,
            QuickStatusFilter? statusFilter = null,
            TaskFilterViewModel advancedFilters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ اعمال فیلتر پیش‌فرض (فقط در حال انجام)
                if (!statusFilter.HasValue && advancedFilters == null)
                {
                    statusFilter = QuickStatusFilter.Pending;
                }

                // ⭐⭐⭐ ایجاد filters برای ارسال به Repository
                var filters = advancedFilters ?? new TaskFilterViewModel();

                // ⭐⭐⭐ ذخیره viewType و grouping در فیلتر
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                // اعمال فیلتر وضعیت سریع
                if (statusFilter.HasValue)
                {
                    filters.TaskStatus = statusFilter.Value switch
                    {
                        QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                        QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                        QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                        QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                        _ => TaskStatusFilter.InProgress
                    };
                }
                else
                {
                    filters.TaskStatus = TaskStatusFilter.InProgress;
                    statusFilter = QuickStatusFilter.Pending;
                }


                    var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                // ⭐⭐⭐ مرتب‌سازی گروه‌ها از جدید به قدیم
                if (model.TaskGroups != null && model.TaskGroups.Any())
                {
                    model.TaskGroups = model.TaskGroups
                        .OrderByDescending(g =>
                        {
                            var allTasks = new List<TaskCardViewModel>();
                            allTasks.AddRange(g.PendingTasks);
                            allTasks.AddRange(g.CompletedTasks);
                            return allTasks.Any() ? allTasks.Max(t => t.CreateDate) : DateTime.MinValue;
                        })
                        .ToList();
                }

                ViewBag.CurrentViewType = viewType;
                ViewBag.CurrentGrouping = grouping;
                ViewBag.CurrentStatusFilter = statusFilter ?? QuickStatusFilter.Pending;
                ViewBag.UserId = userId;
                ViewBag.HasAdvancedFilters = advancedFilters != null;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "Index",
                    $"مشاهده لیست تسک‌ها - نمایش: {viewType}, گروه‌بندی: {grouping}, فیلتر: {statusFilter}");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "Index", "خطا در دریافت لیست تسک‌ها", ex);
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


        #endregion

        /// <summary>
        /// اعمال فیلترهای پیشرفته - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ApplyAdvancedFilters(TaskFilterViewModel filters)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ حفظ viewType و grouping از فیلتر
                var viewType = filters.ViewType ?? TaskViewType.MyTasks;
                var grouping = filters.Grouping ?? TaskGroupingType.Team;

                // ⭐ اعمال فیلترهای پیشرفته
                var model = await _taskRepository.GetTaskListAsync(
                    userId,
                    viewType,
                    grouping,
                    filters);

                // ⭐⭐⭐ مرتب‌سازی گروه‌ها از جدید به قدیم
                if (model.TaskGroups != null && model.TaskGroups.Any())
                {
                    model.TaskGroups = model.TaskGroups
                        .OrderByDescending(g =>
                        {
                            var allTasks = new List<TaskCardViewModel>();
                            allTasks.AddRange(g.PendingTasks);
                            allTasks.AddRange(g.CompletedTasks);
                            return allTasks.Any() ? allTasks.Max(t => t.CreateDate) : DateTime.MinValue;
                        })
                        .ToList();
                }

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "ApplyAdvancedFilters",
                    $"اعمال فیلترهای پیشرفته - نتایج: {model.Tasks.Count}");

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "task-groups-container",
                    view = new { result = html }
                }
            },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent,
                        total = model.Tasks.Count
                    },
                    // ⭐⭐⭐ اضافه کردن viewType و grouping به response
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    message = new[] { new { status = "success", text = $"{model.Tasks.Count} تسک یافت شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ApplyAdvancedFilters", "خطا در اعمال فیلترها", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در اعمال فیلترها" } }
                });
            }
        }
        /// <summary>
        /// دریافت داده‌های اولیه برای فیلترهای پیشرفته - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetAdvancedFilterData()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت شعبه‌های کاربر
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                var branchIds = userBranches.Select(b => b.Id).ToList();

                // ⭐ دریافت کاربران شعبه‌ها
                var branchUsers = new List<UserViewModelFull>();
                foreach (var branchId in branchIds)
                {
                    var users = _branchRepository.GetBranchUsersByBranchId(branchId, includeInactive: false);
                    foreach (var branchUser in users)
                    {
                        if (!branchUsers.Any(u => u.Id == branchUser.UserId))
                        {
                            branchUsers.Add(new UserViewModelFull
                            {
                                Id = branchUser.UserId,
                                FullNamesString = branchUser.UserFullName
                            });
                        }
                    }
                }

                // حذف تکرار و مرتب‌سازی
                var uniqueUsers = branchUsers
                    .OrderBy(u => u.FullNamesString)
                    .ToList();

                // ⭐⭐⭐ دریافت سازمان‌ها (Organizations) به جای Stakeholders
                var organizations = new List<object>();
                var allOrganizations = await _organizationRepository.GetOrganizationsAsViewModelAsync(includeInactive: false);

                foreach (var org in allOrganizations)
                {
                    organizations.Add(new
                    {
                        Id = org.Id,
                        Name = org.DisplayName ?? org.Name,
                        Type = org.OrganizationType == 0 ? "شخص حقیقی" : "شخص حقوقی",
                        MemberCount = org.TotalMembers
                    });
                }

                // حذف تکرار و مرتب‌سازی
                var uniqueOrganizations = organizations
                    .OrderBy(o => ((dynamic)o).Name)
                    .ToList();

                // ⭐⭐⭐ دریافت تیم‌های عضو کاربر
                var userTeams = new List<object>();
                foreach (var branchId in branchIds)
                {
                    var branchTeams = _teamRepository.GetTeamsByBranchId(branchId, includeInactive: false);

                    foreach (var team in branchTeams)
                    {
                        // بررسی اینکه کاربر عضو این تیم است یا نه
                        var isMember = _teamRepository.GetTeamMembers(team.Id, includeInactive: false)
                            .Any(tm => tm.UserId == userId);

                        if (isMember && !userTeams.Any(t => ((dynamic)t).Id == team.Id))
                        {
                            userTeams.Add(new
                            {
                                Id = team.Id,
                                Title = team.Title,
                                BranchId = team.BranchId,
                                ManagerName = team.ManagerFullName,
                                MemberCount = _teamRepository.GetTeamMembers(team.Id, includeInactive: false).Count
                            });
                        }
                    }
                }

                // حذف تکرار و مرتب‌سازی
                var uniqueTeams = userTeams
                    .OrderBy(t => ((dynamic)t).Title)
                    .ToList();

                // ⭐ دریافت دسته‌بندی‌ها
                var categories = _taskRepository.GetAllCategories(activeOnly: true);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetAdvancedFilterData",
                    $"دریافت داده‌های فیلتر پیشرفته - {uniqueUsers.Count} کاربر، {uniqueOrganizations.Count} سازمان، {uniqueTeams.Count} تیم");

                return Json(new
                {
                    status = "success",
                    users = uniqueUsers.Select(u => new { Id = u.Id, FullName = u.FullNamesString }).ToList(),
                    organizations = uniqueOrganizations,
                    teams = uniqueTeams,
                    categories = categories.Select(c => new { c.Id, c.Title }).ToList()
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetAdvancedFilterData", "خطا در دریافت داده‌های فیلتر", ex);
                return Json(new { status = "error", message = "خطا در دریافت داده‌ها: " + ex.Message });
            }
        }
        /// <summary>
        /// پاک کردن فیلترهای پیشرفته
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearAdvancedFilters(
            TaskViewType viewType = TaskViewType.MyTasks,
            TaskGroupingType grouping = TaskGroupingType.Team,
            QuickStatusFilter statusFilter = QuickStatusFilter.Pending) // ⭐ اضافه شده
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ ایجاد فیلتر خالی با حفظ statusFilter
                var filters = new TaskFilterViewModel
                {
                    ViewType = viewType,
                    Grouping = grouping,
                    TaskStatus = statusFilter switch
                    {
                        QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                        QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                        QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                        QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                        _ => TaskStatusFilter.All
                    }
                };

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                // ⭐⭐⭐ مرتب‌سازی گروه‌ها از جدید به قدیم
                if (model.TaskGroups != null && model.TaskGroups.Any())
                {
                    model.TaskGroups = model.TaskGroups
                        .OrderByDescending(g =>
                        {
                            var allTasks = new List<TaskCardViewModel>();
                            allTasks.AddRange(g.PendingTasks);
                            allTasks.AddRange(g.CompletedTasks);
                            return allTasks.Any() ? allTasks.Max(t => t.CreateDate) : DateTime.MinValue;
                        })
                        .ToList();
                }

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "task-groups-container",
                    view = new { result = html }
                }
            },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    // ⭐⭐⭐ برگرداندن اطلاعات فعلی
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    currentStatusFilter = (int)statusFilter,
                    message = new[] { new { status = "success", text = "فیلترها پاک شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ClearAdvancedFilters", "خطا در پاک کردن فیلترها", ex);
                return Json(new { status = "error", message = "خطا در پاک کردن فیلترها" });
            }
        }
        /// <summary>
        /// تغییر گروه‌بندی - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeGrouping(
            TaskViewType viewType,
            TaskGroupingType grouping,
            TaskFilterViewModel currentFilters = null) // ⭐ اضافه شده
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ استفاده از فیلترهای فعلی
                var filters = currentFilters ?? new TaskFilterViewModel();
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "task-groups-container",
                    view = new { result = html }
                }
            },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    // ⭐⭐⭐ برگرداندن اطلاعات فعلی
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "خطا در تغییر گروه‌بندی" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangeQuickStatusFilter(
    TaskViewType viewType,
    TaskGroupingType grouping,
    int statusFilter, // ⭐⭐⭐ تغییر از QuickStatusFilter به int
    TaskFilterViewModel currentFilters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ تبدیل int به QuickStatusFilter
                var statusFilterEnum = (QuickStatusFilter)statusFilter;

                var filters = currentFilters ?? new TaskFilterViewModel();
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                // ⭐⭐⭐ اعمال فیلتر وضعیت سریع
                filters.TaskStatus = statusFilterEnum switch
                {
                    QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                    QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                    QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                    QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                    _ => TaskStatusFilter.All
                };

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                // ⭐⭐⭐ مرتب‌سازی گروه‌ها
                if (model.TaskGroups != null && model.TaskGroups.Any())
                {
                    model.TaskGroups = model.TaskGroups
                        .OrderByDescending(g =>
                        {
                            var allTasks = new List<TaskCardViewModel>();
                            allTasks.AddRange(g.PendingTasks);
                            allTasks.AddRange(g.CompletedTasks);
                            return allTasks.Any() ? allTasks.Max(t => t.CreateDate) : DateTime.MinValue;
                        })
                        .ToList();
                }

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "ChangeQuickStatusFilter",
                    $"تغییر فیلتر وضعیت به: {statusFilterEnum}");

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "task-groups-container",
                    view = new { result = html }
                }
            },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    currentStatusFilter = statusFilter // ⭐⭐⭐ برگرداندن همون int که اومده
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ChangeQuickStatusFilter", "خطا در تغییر فیلتر", ex);
                return Json(new { status = "error", message = "خطا در تغییر فیلتر" });
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
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // ⭐⭐⭐ بارگذاری تسک با تمام assignments
                var task = _taskRepository.GetTaskById(id,
                    includeOperations: true,
                    includeAssignments: true,
                    includeAttachments: true,
                    includeComments: true,
                    includeStakeHolders: true,
                    includeTaskWorkLog: true);

                if (task == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View, "Tasks", "Details",
                        "تلاش برای مشاهده تسک غیرموجود", recordId: id.ToString());
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<TaskViewModel>(task);

                // ⭐⭐⭐ اضافه کردن IsIndependentCompletion به ViewModel
                viewModel.IsIndependentCompletion = task.IsIndependentCompletion;

                var currentUserId = _userManager.GetUserId(User);

                var isAdmin = User.IsInRole("Admin");

                bool isManager = false;
                if (task.TeamId.HasValue)
                {
                    isManager = await _taskRepository.IsUserTeamManagerAsync(currentUserId, task.TeamId.Value);
                }

                bool isSupervisor = false;
                if (task.TeamId.HasValue)
                {
                    isSupervisor = await _taskRepository.CanViewBasedOnPositionAsync(currentUserId, task);
                }

                ViewBag.IsAdmin = isAdmin;
                ViewBag.IsManager = isManager;
                ViewBag.IsSupervisor = isSupervisor;
                viewModel.SetUserContext(currentUserId, isAdmin, isManager, isSupervisor);

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
        [HttpGet]
        public async Task<IActionResult> CompleteTask(int id, int rowNum, bool fromList = false)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound();

                var userId = _userManager.GetUserId(User);

                var model = await _taskRepository.PrepareCompleteTaskModalAsync(id, userId);

                model.rowNum = rowNum;
                model.FromList = fromList;
                model.TaskId = id; // ⭐ مطمئن شویم که TaskId تنظیم شده

                return PartialView("_CompleteTask", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CompleteTask", "خطا در نمایش فرم تکمیل", ex);
                return BadRequest();
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

                    return Json(new { status = "validation-error", message = errors });
                }

                var userId = _userManager.GetUserId(User);

                // ثبت تکمیل تسک
                var result = await _taskRepository.CompleteTaskAsync(model, userId);

                if (!result.IsSuccess)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = result.ErrorMessage } }
                    });
                }

                // ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskCompletedAsync(
                    model.TaskId,
                    userId,
                    model.TaskTitle,
                    model.TaskCode
                );

                await _taskHistoryRepository.LogRemindersDeactivatedOnCompletionAsync(
                    model.TaskId,
                    userId,
                    model.TaskTitle,
                    model.TaskCode
                );

                // ⭐⭐⭐ ارسال به صف - فوری و بدون Blocking
                NotificationProcessingBackgroundService.EnqueueTaskNotification(
                    model.TaskId,
                    userId,
                    NotificationEventType.TaskCompleted,
                    priority: 2
                );

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "Tasks",
                    "CompleteTask",
                    $"تکمیل تسک {model.TaskCode} - {model.TaskTitle}",
                    recordId: model.TaskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: model.TaskTitle
                );

                // ⭐⭐⭐ اگر از لیست آمده، پارشیال ردیف را برگردان
                if (model.FromList)
                {
                    var updatedTask = await _taskRepository.GetTaskCardViewModelAsync(model.TaskId, userId);
                    
                    if (updatedTask != null)
                    {

                        updatedTask.CardNumber= model.rowNum;  // حفظ شماره ردیف
                        // رندر پارشیال ردیف
                        var partialView = await this.RenderViewToStringAsync("_TaskRowPartial", updatedTask);

                        return Json(new
                        {
                            status = "update-view",
                            viewList = new[]
                            {
                        new
                        {
                            elementId = $"task-row-{model.TaskId}",
                            view = new { result = partialView },
                            appendMode = false
                        }
                    },
                            message = new[] { new { status = "success", text = "تسک با موفقیت تکمیل شد" } }
                        });
                    }
                }

                // ⭐ حالت پیش‌فرض: redirect
                return Json(new
                {
                    status = "redirect",
                    message = new[] { new { status = "success", text = "تسک با موفقیت تکمیل شد" } },
                    redirectUrl = Url.Action("Details", "Tasks", new { id = model.TaskId, area = "TaskingArea" })
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










        #region POST Actions
        /// <summary>
        /// ثبت تسک جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewTask(TaskViewModel model)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ اعتبارسنجی
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
                    // ⭐⭐⭐ بررسی: آیا زمان‌بندی فعال است؟
                    if (model.TaskSchedule?.IsScheduled == true)
                    {
                        // ⭐ زمان‌بندی فعال - ساخت Schedule
                        var (scheduleId, immediateTask) = await _taskRepository.CreateScheduledTaskAsync(
                            model,
                            currentUserId);

                        // ⭐⭐⭐ اگر CreateImmediately = true، تسک ساخته شده
                        if (immediateTask != null)
                        {
                            // ذخیره پیوست‌ها، عملیات، یادآوری‌ها
                            if (model.Attachments != null && model.Attachments.Count > 0)
                            {
                                await _taskRepository.SaveTaskAttachmentsAsync(
                                    immediateTask.Id,
                                    model.Attachments,
                                    currentUserId,
                                    _webHostEnvironment.WebRootPath);
                            }

                            await _taskRepository.SaveTaskOperationsAndRemindersAsync(immediateTask.Id, model);
                            await _taskRepository.HandleTaskAssignmentsBulkAsync(immediateTask, model, currentUserId);

                            // ⭐ ارسال نوتیفیکیشن برای تسک فوری
                            NotificationProcessingBackgroundService.EnqueueTaskNotification(
                                immediateTask.Id,
                                currentUserId,
                                NotificationEventType.TaskAssigned,
                                priority: 1);

                            await _taskHistoryRepository.LogTaskCreatedAsync(
                                immediateTask.Id,
                                currentUserId,
                                immediateTask.Title,
                                immediateTask.TaskCode);
                        }

                        await _uow.CommitTransactionAsync();

                        // ⭐ پیام موفقیت
                        if (immediateTask != null)
                        {
                            TempData["SuccessMessage"] =
                                "زمان‌بندی با موفقیت ایجاد شد و یک تسک فوری نیز ساخته شد";
                        }
                        else
                        {
                            TempData["SuccessMessage"] =
                                "زمان‌بندی با موفقیت ایجاد شد. تسک‌ها در زمان مشخص شده ساخته خواهند شد";
                        }

                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Tasks",
                            "CreateNewTask",
                            $"ایجاد زمان‌بندی تسک: {model.TaskSchedule.ScheduleTitle ?? model.Title}",
                            recordId: scheduleId.ToString(),
                            entityType: "ScheduledTasks");

                        // ⭐ Redirect به لیست Schedule ها
                        return RedirectToAction("Index", "ScheduledTasks");
                    }
                    else
                    {
                        // ⭐ تسک معمولی (بدون زمان‌بندی)
                        var task = await _taskRepository.CreateTaskEntityAsync(model, currentUserId, _mapper);
                        task.CreationMode = 0; // ⭐ دستی

                        _uow.TaskUW.Update(task);
                        await _uow.SaveAsync();

                        // ذخیره پیوست‌ها
                        if (model.Attachments != null && model.Attachments.Count > 0)
                        {
                            await _taskRepository.SaveTaskAttachmentsAsync(
                                task.Id,
                                model.Attachments,
                                currentUserId,
                                _webHostEnvironment.WebRootPath);
                        }

                        await _taskRepository.SaveTaskOperationsAndRemindersAsync(task.Id, model);
                        await _taskRepository.HandleTaskAssignmentsBulkAsync(task, model, currentUserId);

                        await _uow.CommitTransactionAsync();

                        // ارسال نوتیفیکیشن
                        NotificationProcessingBackgroundService.EnqueueTaskNotification(
                            task.Id,
                            currentUserId,
                            NotificationEventType.TaskAssigned,
                            priority: 1);

                        await _taskHistoryRepository.LogTaskCreatedAsync(
                            task.Id,
                            currentUserId,
                            task.Title,
                            task.TaskCode);

                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Tasks",
                            "CreateNewTask",
                            $"ایجاد تسک جدید: {task.Title} با کد: {task.TaskCode}",
                            recordId: task.Id.ToString(),
                            entityType: "Tasks",
                            recordTitle: task.Title
                        );

                        TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch
                {
                    await _uow.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CreateNewTask", "خطا در ایجاد تسک", ex);
                ModelState.AddModelError("", $" خطا در ثبت تسک. لطفا با پشتیبان نرم افزار تماس بگیرید.");

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

        #endregion

        

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



        #region Task Work Log

        /// <summary>
        /// مودال ثبت کار انجام شده روی تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SubmitTaskWorkLogModal(int taskId)
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

                return PartialView("_SubmitTaskWorkLogModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SubmitTaskWorkLogModal", "خطا در نمایش مودال ثبت کار", ex);
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
                // ⭐⭐⭐ ارسال اعلان به صف - فوری و بدون Blocking
                NotificationProcessingBackgroundService.EnqueueTaskNotification(
                    model.TaskId,
                    currentUserId,
                    NotificationEventType.TaskWorkLog, 
                    priority: 1
                );
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
        public async Task<IActionResult> SetTaskFocus(int taskId, bool fromList = false)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // ⭐⭐⭐ ابتدا شناسه تسک فوکوس قبلی را دریافت کن
            var previousFocusedTaskId = await _taskRepository.GetUserFocusedTaskIdAsync(currentUserId);

            var result = await _taskRepository.SetTaskFocusAsync(taskId, currentUserId);

            if (result.Success)
            {
                // ⭐⭐⭐ اگر از لیست آمده، پارشیال ردیف‌ها را برگردان
                if (fromList)
                {
                    var viewList = new List<object>();

                    // ⭐ 1. بروزرسانی ردیف تسک جدید (فوکوس شده)
                    var newFocusedTaskCard = await _taskRepository.GetTaskCardViewModelAsync(taskId, currentUserId);
                    if (newFocusedTaskCard != null)
                    {
                        newFocusedTaskCard.IsFocused = true;
                        var newPartialView = await this.RenderViewToStringAsync("_TaskRowPartial", newFocusedTaskCard);

                        viewList.Add(new
                        {
                            elementId = $"task-row-{taskId}",
                            view = new { result = newPartialView },
                            appendMode = false
                        });
                    }

                    // ⭐ 2. بروزرسانی ردیف تسک قبلی (اگر وجود داشت)
                    if (previousFocusedTaskId.HasValue && previousFocusedTaskId.Value != taskId)
                    {
                        var previousTaskCard = await _taskRepository.GetTaskCardViewModelAsync(previousFocusedTaskId.Value, currentUserId);
                        if (previousTaskCard != null)
                        {
                            previousTaskCard.IsFocused = false;
                            var previousPartialView = await this.RenderViewToStringAsync("_TaskRowPartial", previousTaskCard);

                            viewList.Add(new
                            {
                                elementId = $"task-row-{previousFocusedTaskId.Value}",
                                view = new { result = previousPartialView },
                                appendMode = false
                            });
                        }
                    }

                    // ⭐⭐⭐ برگرداندن هر دو ردیف
                    return Json(new
                    {
                        status = "update-view",
                        viewList = viewList.ToArray(),
                        message = new[] { new { status = "success", text = result.Message } }
                    });
                }
            }

            return Json(new { success = result.Success, message = result.Message });
        }

        /// <summary>
        /// حذف فوکوس از تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveTaskFocus(int taskId, bool fromList = false)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskRepository.RemoveTaskFocusAsync(taskId, currentUserId);

            if (result.Success)
            {
                // ⭐⭐⭐ اگر از لیست آمده، پارشیال ردیف را برگردان
                if (fromList)
                {
                    // دریافت اطلاعات تسک به‌روز شده
                    var taskCard = await _taskRepository.GetTaskCardViewModelAsync(taskId, currentUserId);

                    if (taskCard != null)
                    {
                        // ⭐ تنظیم IsFocused به false
                        taskCard.IsFocused = false;

                        // رندر پارشیال ردیف
                        var partialView = await this.RenderViewToStringAsync("_TaskRowPartial", taskCard);

                        return Json(new
                        {
                            status = "update-view",
                            viewList = new[]
                            {
                        new
                        {
                            elementId = $"task-row-{taskId}",
                            view = new { result = partialView },
                            appendMode = false
                        }
                    },
                            message = new[] { new { status = "success", text = result.Message } }
                        });
                    }
                }
            }

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
        /// افزودن کامنت/پیام جدید به تسک - نسخه بهینه شده ⚡
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

                // ⭐⭐⭐ بررسی‌های موازی برای کاهش زمان
                var accessTask = _taskRepository.CanUserViewTaskAsync(currentUserId, model.TaskId);
                var taskTask = _taskRepository.GetTaskByIdAsync(model.TaskId);
                var assignmentTask = _taskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, model.TaskId);

                await Task.WhenAll(accessTask, taskTask, assignmentTask);

                var hasAccess = await accessTask;
                var task = await taskTask;
                var currentUserAssignment = await assignmentTask;

                if (!hasAccess)
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما به این تسک دسترسی ندارید"
                    });
                }

                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new
                    {
                        success = false,
                        message = "این تسک تکمیل شده و امکان ارسال پیام وجود ندارد"
                    });
                }

                // ⭐⭐⭐ ایجاد کامنت (فقط یکبار Save)
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
                await _uow.SaveAsync();

                // ⭐⭐⭐ پردازش موازی فایل‌ها (اگر وجود دارند)
                if (Attachments != null && Attachments.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "task-comments", model.TaskId.ToString());
                    Directory.CreateDirectory(uploadsFolder);

                    var attachmentTasks = Attachments
                        .Where(f => f.Length > 0)
                        .Select(file => SaveAttachmentAsync(file, comment.Id, uploadsFolder, currentUserId, model.TaskId))
                        .ToList();

                    if (attachmentTasks.Any())
                    {
                        var attachments = await Task.WhenAll(attachmentTasks);

                        foreach (var attachment in attachments)
                        {
                            _uow.TaskCommentAttachmentUW.Create(attachment);
                        }

                        await _uow.SaveAsync();
                    }
                }

                // ⭐⭐⭐ کارهای بعدی را Fire-and-Forget کن (بدون await)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // ثبت در تاریخچه
                        await _taskHistoryRepository.LogCommentAddedAsync(
                            model.TaskId,
                            currentUserId,
                            comment.Id,
                            model.CommentText.Substring(0, Math.Min(50, model.CommentText.Length))
                        );

                        // ارسال نوتیفیکیشن (خودش async است)
                        NotificationProcessingBackgroundService.EnqueueTaskNotification(
                            model.TaskId,
                            currentUserId,
                            NotificationEventType.TaskCommentAdded,
                            priority: 1
                        );

                        // لاگ فعالیت
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Tasks",
                            "AddTaskComment",
                            $"افزودن کامنت به تسک {task.TaskCode}",
                            recordId: model.TaskId.ToString(),
                            entityType: "Tasks",
                            recordTitle: task.Title
                        );
                    }
                    catch (Exception bgEx)
                    {
                        // لاگ خطا در Background (بدون متوقف کردن response)
                        await _activityLogger.LogErrorAsync("Tasks", "AddTaskComment_Background", "خطا در عملیات پس‌زمینه", bgEx);
                    }
                });

                // ⭐⭐⭐ فوراً response برگردان
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
        /// ⭐⭐⭐ متد کمکی برای ذخیره موازی فایل‌ها
        /// </summary>
        private async Task<TaskCommentAttachment> SaveAttachmentAsync(
            IFormFile file,
            int commentId,
            string uploadsFolder,
            string currentUserId,
            int taskId)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return new TaskCommentAttachment
            {
                TaskCommentId = commentId,
                FileName = file.FileName,
                FilePath = $"/uploads/task-comments/{taskId}/{uniqueFileName}",
                FileExtension = Path.GetExtension(file.FileName),
                FileSize = file.Length.ToString(),
                FileUUID = uniqueFileName,
                UploadDate = DateTime.Now,
                UploaderUserId = currentUserId
            };
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
                var comment = _uow.TaskCommentUW.GetById(id);
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
                var attachments = _uow.TaskCommentAttachmentUW
                    .Get(a => a.TaskCommentId == id).ToList();

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
        /// دانلود فایل پیوست شده به کامنت تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                // ⭐ استفاده از Repository به جای دسترسی مستقیم به DbContext
                var attachment = await _taskRepository.GetCommentAttachmentByIdAsync(id);

                if (attachment == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Tasks",
                        "DownloadAttachment",
                        $"تلاش برای دانلود فایل غیرموجود با ID: {id}");

                    return NotFound(new { success = false, message = "فایل یافت نشد" });
                }

                // بررسی دسترسی کاربر به تسک
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // ⭐ استفاده از متد Repository برای بررسی دسترسی
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(currentUserId, attachment.Comment.TaskId);
                var isCreator = attachment.Comment.Task.CreatorUserId == currentUserId;
                var isAdmin = User.IsInRole("Admin");

                if (!hasAccess && !isCreator && !isAdmin)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Tasks",
                        "DownloadAttachment",
                        $"تلاش ناموفق برای دانلود فایل {attachment.FileName} - عدم دسترسی",
                        recordId: attachment.Comment.TaskId.ToString());

                    return Forbid();
                }

                // بررسی وجود فایل فیزیکی
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    await _activityLogger.LogErrorAsync(
                        "Tasks",
                        "DownloadAttachment",
                        $"فایل در مسیر {filePath} یافت نشد",
                        null,
                        recordId: id.ToString());

                    return NotFound(new { success = false, message = "فایل در سرور یافت نشد" });
                }

                // خواندن فایل
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(attachment.FileName);

                // ⭐ ثبت لاگ دانلود موفق
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "DownloadAttachment",
                    $"دانلود فایل {attachment.FileName} از تسک {attachment.Comment.Task.TaskCode}",
                    recordId: attachment.Comment.TaskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: attachment.Comment.Task.Title);

                // تنظیم هدرهای دانلود
                Response.Headers.Add("Content-Disposition",
                    $"attachment; filename=\"{Uri.EscapeDataString(attachment.FileName)}\"");

                return File(fileBytes, contentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "DownloadAttachment",
                    $"خطا در دانلود فایل با ID: {id}",
                    ex,
                    recordId: id.ToString());

                return StatusCode(500, new { success = false, message = "خطا در دانلود فایل" });
            }
        }

        /// <summary>
        /// تعیین Content-Type بر اساس پسوند فایل
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
        #endregion

        /// <summary>
        /// دریافت آمار بروز شده Hero Section
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskHeroStatsPartial(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                // رندر Partial View
                var html = await this.RenderViewToStringAsync("_TaskHeroStats", viewModel);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskHeroStatsPartial", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// دریافت آمار بروز شده Sidebar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskSidebarStatsPartial(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                // رندر Partial View
                var html = await this.RenderViewToStringAsync("_TaskSidebarStats", viewModel);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskSidebarStatsPartial", "خطا در دریافت آمار Sidebar", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// ⭐ بروزرسانی تمام آمارها با یک درخواست
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RefreshAllTaskStats(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                // رندر تمام Partial View ها
                var heroHtml = await this.RenderViewToStringAsync("_TaskHeroStats", viewModel);
                var sidebarHtml = await this.RenderViewToStringAsync("_TaskSidebarStats", viewModel);

                return Json(new
                {
                    success = true,
                    status = "update-view",
                    viewList = new[]
                    {
                new
                {
                    elementId = "hero-stats-container",
                    view = new { result = heroHtml }
                },
                new
                {
                    elementId = "sidebar-stats-container",
                    view = new { result = sidebarHtml }
                }
            },
                    // ⭐ اطلاعات اضافی برای Badge
                    totalOperations = viewModel.Operations?.Count ?? 0,
                    completedOperations = viewModel.Operations?.Count(o => o.IsCompleted) ?? 0
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RefreshAllTaskStats", "خطا در بروزرسانی آمار", ex);
                return Json(new { success = false, message = "خطا در بروزرسانی آمار" });
            }
        }
    }
}