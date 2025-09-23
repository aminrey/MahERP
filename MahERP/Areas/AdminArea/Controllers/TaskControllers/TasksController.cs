using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
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
        private readonly TaskNotificationService _taskNotificationService;
        private readonly TaskCodeGenerator _taskCodeGenerator; // اضافه شده

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
            TaskNotificationService taskNotificationService,
            TaskCodeGenerator taskCodeGenerator) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
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
            _taskCodeGenerator = taskCodeGenerator; // اضافه شده
        }

        // نمایش تقویم تسک‌ها - با اضافه کردن فیلترها
        [HttpGet]
        //[Permission("Tasks", "TaskCalendar", 0)]
        public async Task<IActionResult> TaskCalendar()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت تسک‌های کاربر برای نمایش در تقویم (بدون فیلتر اولیه) - اصلاح شده
                var calendarTasks = _taskRepository.GetTasksForCalendarView(userId);

                // تبدیل تاریخ‌ها به فرمت مناسب تقویم
                var calendarEvents = calendarTasks.Select(task => new
                {
                    id = task.Id,
                    title = task.Title,
                    start = task.DueDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = task.DueDate?.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss"),

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

                // تهیه داده‌های فیلترها - مشابه CreateNewTask
                var filterModel = new TaskCalendarFilterViewModel();

                // دریافت شعبه‌های کاربر
                filterModel.BranchListInitial = _branchRepository.GetBrnachListByUserId(userId);

                // اضافه کردن فیلترهای پیش‌فرض (همه شعبه‌ها)
                ViewBag.FilterModel = filterModel;

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

        // دریافت رویدادهای تقویم برای AJAX - با فیلترهای جدید
        // دریافت رویدادهای تقویم برای AJAX - با فیلترهای جدید
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents(
            DateTime? start = null,
            DateTime? end = null,
            int? branchId = null,
            string assignedUserIds = null,
            int? stakeholderId = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                Console.WriteLine($"GetCalendarEvents called - UserId: {userId}, Start: {start}, End: {end}, BranchId: {branchId}, AssignedUserIds: {assignedUserIds}, StakeholderId: {stakeholderId}");

                // تبدیل رشته کاربران به لیست
                List<string> userFilterList = null;
                if (!string.IsNullOrEmpty(assignedUserIds))
                {
                    userFilterList = assignedUserIds.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }

                // دریافت تسک‌ها بر اساس محدوده زمانی و فیلترهای جدید - اصلاح شده
                var calendarTasks = _taskRepository.GetTasksForCalendarView(
                    userId,
                    branchId,
                    start,
                    end,
                    userFilterList,
                    stakeholderId);

                Console.WriteLine($"Found {calendarTasks?.Count ?? 0} tasks with filters");

                var events = calendarTasks.Select(task =>
                {
                    // اطمینان از وجود DueDate
                    if (!task.DueDate.HasValue)
                    {
                        Console.WriteLine($"Task {task.Id} has no DueDate - skipping");
                        return null;
                    }

                    // تبدیل تاریخ میلادی به شمسی برای ارسال به تقویم
                    var persianStartDate = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy-MM-dd");
                    var persianEndDate = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value.AddHours(3), "yyyy-MM-dd");

                    Console.WriteLine($"Task {task.Id}: {task.Title}");
                    Console.WriteLine($"  Original Date (Gregorian): {task.DueDate}");
                    Console.WriteLine($"  Converted Date (Persian): {persianStartDate}");

                    // تعیین رنگ پس‌زمینه بر اساس وضعیت تسک
                    string backgroundColor;
                    if (task.IsCompleted)
                        backgroundColor = "#28a745"; // سبز - تکمیل شده
                    else if (task.IsOverdue)
                        backgroundColor = "#dc3545"; // قرمز - عقب افتاده
                    else
                        backgroundColor = "#007bff"; // آبی - در حال انجام

                    // تعیین متن وضعیت
                    string statusText;
                    if (task.IsCompleted)
                        statusText = "تکمیل شده";
                    else if (task.IsOverdue)
                        statusText = "عقب افتاده";
                    else
                        statusText = "در حال انجام";

                    return new
                    {
                        id = task.Id,
                        title = task.Title,
                        start = persianStartDate, // تاریخ شمسی
                        end = persianEndDate,     // تاریخ شمسی
                        backgroundColor = backgroundColor,
                        borderColor = backgroundColor, // همان رنگ پس‌زمینه
                        textColor = "#ffffff",
                        description = task.Description ?? "",
                        extendedProps = new
                        {
                            taskCode = task.TaskCode ?? "",
                            categoryTitle = task.CategoryTitle ?? "",
                            stakeholderName = task.StakeholderName ?? "",
                            branchName = task.BranchName ?? "",
                            statusText = statusText,
                            isCompleted = task.IsCompleted,
                            isOverdue = task.IsOverdue,
                            detailUrl = Url.Action("Details", "Tasks", new { id = task.Id, area = "AdminArea" }),
                            // اضافه کردن تاریخ‌های میلادی و شمسی برای نمایش
                            gregorianDate = task.DueDate.Value.ToString("yyyy-MM-dd"),
                            persianDate = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy/MM/dd"),
                            persianDateFull = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "dddd، dd MMMM yyyy")
                        }
                    };
                }).Where(e => e != null).ToList();

                Console.WriteLine($"Returning {events.Count} filtered events to calendar with Persian dates");

                return Json(events);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCalendarEvents: {ex.Message}");

                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetCalendarEvents",
                    "خطا در دریافت رویدادهای تقویم",
                    ex
                );

                return Json(new List<object>());
            }
        }


        // لیست تسک‌ها - با کنترل سطح دسترسی داده و فیلترهای پیشرفته
        [Permission("Tasks", "Index", 0)] // Read permission
        public async Task<IActionResult> Index(TaskFilterViewModel filters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // اگر فیلتری ارسال نشده، تنظیمات پیش‌فرض بر اساس سطح دسترسی کاربر
                if (filters == null)
                {
                    var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                    filters = new TaskFilterViewModel 
                    { 
                        ViewType = dataAccessLevel switch
                        {
                            0 => TaskViewType.MyTasks,      // Personal
                            1 => TaskViewType.AllTasks,     // Branch 
                            2 => TaskViewType.AllTasks,     // All
                            _ => TaskViewType.MyTasks
                        }
                    };
                }

                var model = new TaskListForIndexViewModel
                {
                    UserLoginid = userId,
                    Filters = filters
                };

                // پر کردن لیست‌های اولیه برای فیلترها
                await PopulateFilterLists(model, userId);

                // دریافت تسک‌ها بر اساس فیلتر انتخاب شده
                await LoadTasksByFilter(model, userId);

                // محاسبه آمار
                await CalculateStatistics(model, userId);

                // ثبت لاگ مشاهده لیست تسک‌ها
                await _activityLogger.LogActivityAsync(
                  ActivityTypeEnum.View,
                  "Tasks",
                  "Index",
                  $"مشاهده لیست تسک‌ها - نوع نمایش: {filters.ViewType} - سطح دسترسی: {this.GetUserDataAccessLevel("Tasks", "Index")}"
              );
                return View(model);
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

        /// <summary>
        /// پر کردن لیست‌های مورد نیاز برای فیلترها
        /// </summary>
        private async Task PopulateFilterLists(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                // شعبه‌های کاربر
                model.branchListInitial = _branchRepository.GetBrnachListByUserId(userId);

                // تیم‌های کاربر (تیم‌هایی که مدیر آن‌هاست یا عضو آن‌هاست)
                model.TeamsInitial = await GetUserRelatedTeams(userId);

                // کاربران مرتبط (اعضای تیم‌های کاربر)
                model.UsersInitial = await GetUserRelatedUsers(userId);

                // دسته‌بندی‌ها
                model.TaskCategoryInitial = _taskRepository.GetAllCategories();

                // طرف حساب‌ها - اصلاح تبدیل نوع
                var stakeholders = _stakeholderRepository.GetStakeholders();
                model.StakeholdersInitial = stakeholders.Select(s => new StakeholderViewModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    CompanyName = s.CompanyName,
                    NationalCode = s.NationalCode,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "PopulateFilterLists",
                    "خطا در پر کردن لیست‌های فیلتر",
                    ex
                );
            }
        }

        /// <summary>
        /// دریافت تیم‌های مرتبط با کاربر
        /// </summary>
        private async Task<List<TeamViewModel>> GetUserRelatedTeams(string userId)
        {
            try
            {
                var teams = new List<TeamViewModel>();

                // تیم‌هایی که کاربر مدیر آن‌هاست
                var managedTeams = _uow.TeamUW.Get(t => t.ManagerUserId == userId && t.IsActive);
                
                // تیم‌هایی که کاربر عضو آن‌هاست
                var memberTeams = _uow.TeamMemberUW.Get(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.Team)
                    .Where(t => t != null && t.IsActive);

                // ترکیب و حذف تکرار
                var allTeams = managedTeams.Union(memberTeams).Distinct().ToList();

                foreach (var team in allTeams)
                {
                    teams.Add(new TeamViewModel
                    {
                        Id = team.Id,
                        Title = team.Title,
                        Description = team.Description,
                        BranchId = team.BranchId,
                        IsActive = team.IsActive,
                        ManagerFullName = !string.IsNullOrEmpty(team.ManagerUserId) 
                            ? _userManager.Users.FirstOrDefault(u => u.Id == team.ManagerUserId)?.FirstName + " " + 
                              _userManager.Users.FirstOrDefault(u => u.Id == team.ManagerUserId)?.LastName 
                            : "ندارد"
                    });
                }

                return teams.OrderBy(t => t.Title).ToList();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetUserRelatedTeams",
                    "خطا در دریافت تیم‌های مرتبط",
                    ex
                );
                return new List<TeamViewModel>();
            }
        }

        /// <summary>
        /// دریافت کاربران مرتبط با کاربر جاری
        /// </summary>
        private async Task<List<UserViewModelFull>> GetUserRelatedUsers(string userId)
        {
            try
            {
                var relatedUserIds = new HashSet<string>();

                // اعضای تیم‌هایی که کاربر مدیر آن‌هاست
                var managedTeams = _uow.TeamUW.Get(t => t.ManagerUserId == userId && t.IsActive);
                foreach (var team in managedTeams)
                {
                    var memberIds = _uow.TeamMemberUW.Get(tm => tm.TeamId == team.Id && tm.IsActive)
                        .Select(tm => tm.UserId);
                    foreach (var memberId in memberIds)
                        relatedUserIds.Add(memberId);
                }

                // همکاران در تیم‌هایی که کاربر عضو آن‌هاست
                var memberTeamIds = _uow.TeamMemberUW.Get(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.TeamId);
                
                foreach (var teamId in memberTeamIds)
                {
                    var teammateIds = _uow.TeamMemberUW.Get(tm => tm.TeamId == teamId && tm.IsActive)
                        .Select(tm => tm.UserId);
                    foreach (var teammateId in teammateIds)
                        relatedUserIds.Add(teammateId);
                }

                // تبدیل به UserViewModelFull
                var users = new List<UserViewModelFull>();
                foreach (var relatedUserId in relatedUserIds)
                {
                    var user = await _userManager.FindByIdAsync(relatedUserId);
                    if (user != null)
                    {
                        users.Add(new UserViewModelFull
                        {
                            Id = user.Id,
                            FullNamesString = $"{user.FirstName} {user.LastName}",
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserName = user.UserName,
                            PositionName = user.PositionName,
                            IsActive = user.IsActive
                        });
                    }
                }

                return users.OrderBy(u => u.FullNamesString).ToList();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetUserRelatedUsers",
                    "خطا در دریافت کاربران مرتبط",
                    ex
                );
                return new List<UserViewModelFull>();
            }
        }

        /// <summary>
        /// بارگذاری تسک‌ها بر اساس فیلتر انتخاب شده
        /// </summary>
        private async Task LoadTasksByFilter(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                switch (model.Filters.ViewType)
                {
                    case TaskViewType.AllTasks:
                        await LoadAllTasks(model, userId);
                        break;
                        
                    case TaskViewType.MyTeamsHierarchy:
                        await LoadMyTeamsHierarchyTasks(model, userId);
                        break;
                        
                    case TaskViewType.MyTasks:
                        await LoadMyTasks(model, userId);
                        break;
                        
                    case TaskViewType.AssignedToMe:
                        await LoadAssignedToMeTasks(model, userId);
                        break;
                        
                    default:
                        await LoadMyTasks(model, userId);
                        break;
                }

                // اعمال فیلترهای اضافی
                await ApplyAdditionalFilters(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "LoadTasksByFilter",
                    "خطا در بارگذاری تسک‌ها",
                    ex
                );
                model.Tasks = new List<TaskViewModel>();
            }
        }

        /// <summary>
        /// دریافت همه تسک‌ها (با در نظر گیری سطح دسترسی) - اصلاح شده
        /// </summary>
        private async Task LoadAllTasks(TaskListForIndexViewModel model, string userId)
        {
            var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
            
            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.View,
                "Tasks",
                "LoadAllTasks",
                $"بارگذاری تسک‌ها با سطح دسترسی: {dataAccessLevel}"
            );

            switch (dataAccessLevel)
            {
                case 0: // Personal - فقط تسک‌های خود کاربر
                    await LoadMyTasks(model, userId);
                    break;
                    
                case 1: // Branch - تسک‌های شعبه
                    await LoadBranchTasks(model, userId);
                    break;
                    
                case 2: // All - همه تسک‌ها
                    await LoadSystemTasks(model, userId);
                    break;
                    
                default:
                    await LoadMyTasks(model, userId);
                    break;
            }
        }

        /// <summary>
        /// محاسبه آمار تسک‌ها - بر اساس کل دیتابیس نه فقط لیست فعلی
        /// </summary>
        private async Task CalculateStatistics(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                // آمار کل سیستم بر اساس سطح دسترسی کاربر
                var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
                List<TaskViewModel> allAvailableTasks;

                switch (dataAccessLevel)
                {
                    case 0: // Personal
                        var personalTasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
                        allAvailableTasks = _mapper.Map<List<TaskViewModel>>(personalTasks);
                        break;
                        
                    case 1: // Branch
                        var userBranchId = GetUserBranchId(userId);
                        var branchTasks = _taskRepository.GetTasksByBranch(userBranchId);
                        allAvailableTasks = _mapper.Map<List<TaskViewModel>>(branchTasks);
                        break;
                        
                    case 2: // All
                        var systemTasks = _taskRepository.GetTasks(includeDeleted: false);
                        allAvailableTasks = _mapper.Map<List<TaskViewModel>>(systemTasks);
                        break;
                        
                    default:
                        allAvailableTasks = model.Tasks;
                        break;
                }

                // آمار بر اساس لیست فیلتر شده فعلی
                var filteredTasks = model.Tasks;

                model.Statistics = new TaskStatisticsViewModel
                {
                    // آمار کل (بر اساس سطح دسترسی)
                    TotalTasks = allAvailableTasks.Count,
                    MyTasks = allAvailableTasks.Count(t => t.CreatorUserId == userId),
                    AssignedToMe = allAvailableTasks.Count(t => t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)),
                    
                    // آمار فیلتر شده فعلی
                    CompletedTasks = filteredTasks.Count(t => t.CompletionDate.HasValue),
                    OverdueTasks = filteredTasks.Count(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now),
                    InProgressTasks = filteredTasks.Count(t => !t.CompletionDate.HasValue && t.IsActive),
                    ImportantTasks = filteredTasks.Count(t => t.TaskType == 1),
                    UrgentTasks = filteredTasks.Count(t => t.TaskType == 2),
                    
                    TeamTasks = model.GroupedTasks.TeamMemberTasks.Values.SelectMany(tasks => tasks).Count(),
                    SubTeamTasks = model.GroupedTasks.SubTeamTasks.Values.SelectMany(tasks => tasks).Count()
                };

                // اگر فیلتری فعال است، آمار را بر روی لیست فیلتر شده محاسبه کن
                if (HasActiveFilters(model.Filters))
                {
                    model.Statistics.TotalTasks = filteredTasks.Count;
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CalculateStatistics",
                    "خطا در محاسبه آمار",
                    ex
                );
                model.Statistics = new TaskStatisticsViewModel();
            }
        }

        /// <summary>
        /// بررسی وجود فیلتر فعال
        /// </summary>
        private bool HasActiveFilters(TaskFilterViewModel filters)
        {
            return filters.BranchId.HasValue ||
                   filters.TeamId.HasValue ||
                   !string.IsNullOrEmpty(filters.UserId) ||
                   filters.CategoryId.HasValue ||
                   filters.StakeholderId.HasValue ||
                   !string.IsNullOrEmpty(filters.SearchTerm) ||
                   (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All) ||
                   (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All);
        }

        /// <summary>
        /// دریافت فقط تسک‌های خود کاربر - اصلاح شده
        /// </summary>
        private async Task LoadMyTasks(TaskListForIndexViewModel model, string userId)
        {
            var tasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
            model.Tasks = _mapper.Map<List<TaskViewModel>>(tasks);
            
            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.View,
                "Tasks",
                "LoadMyTasks",
                $"بارگذاری {model.Tasks.Count} تسک شخصی برای کاربر {userId}"
            );
        }

        /// <summary>
        /// دریافت تسک‌های شعبه - اصلاح شده
        /// </summary>
        private async Task LoadBranchTasks(TaskListForIndexViewModel model, string userId)
        {
            var userBranchId = GetUserBranchId(userId);
            var branchTasks = _taskRepository.GetTasksByBranch(userBranchId);
            model.Tasks = _mapper.Map<List<TaskViewModel>>(branchTasks);
            
            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.View,
                "Tasks",
                "LoadBranchTasks",
                $"بارگذاری {model.Tasks.Count} تسک شعبه {userBranchId} برای کاربر {userId}"
            );
        }

        /// <summary>
        /// دریافت همه تسک‌های سیستم - اصلاح شده
        /// </summary>
        private async Task LoadSystemTasks(TaskListForIndexViewModel model, string userId)
        {
            var allTasks = _taskRepository.GetTasks(includeDeleted: false);
            model.Tasks = _mapper.Map<List<TaskViewModel>>(allTasks);
            
            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.View,
                "Tasks",
                "LoadSystemTasks",
                $"بارگذاری {model.Tasks.Count} تسک سیستم برای کاربر {userId}"
            );
        }

        /// <summary>
        /// اعمال فیلترهای اضافی
        /// </summary>
        private async Task ApplyAdditionalFilters(TaskListForIndexViewModel model)
        {
            var tasks = model.Tasks.ToList(); // تبدیل به List برای جلوگیری از expression tree error

            // فیلتر شعبه
            if (model.Filters.BranchId.HasValue)
            {
                tasks = tasks.Where(t => t.BranchId == model.Filters.BranchId.Value).ToList();
            }

            // فیلتر تیم
            if (model.Filters.TeamId.HasValue)
            {
                var teamUserIds = await GetUsersFromTeams(new List<int> { model.Filters.TeamId.Value });
                tasks = tasks.Where(t => t.AssignmentsTaskUser != null && 
                                        t.AssignmentsTaskUser.Any(a => teamUserIds.Contains(a.AssignedUserId))).ToList();
            }

            // فیلتر کاربر  
            if (!string.IsNullOrEmpty(model.Filters.UserId))
            {
                tasks = tasks.Where(t => 
                    (t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any(a => a.AssignedUserId == model.Filters.UserId)) ||
                    t.CreatorUserId == model.Filters.UserId).ToList();
            }

            // فیلتر اولویت
            if (model.Filters.TaskPriority.HasValue && model.Filters.TaskPriority != TaskPriorityFilter.All)
            {
                tasks = tasks.Where(t => t.TaskType == (byte)model.Filters.TaskPriority).ToList();
            }

            // فیلتر دسته‌بندی
            if (model.Filters.CategoryId.HasValue)
            {
                tasks = tasks.Where(t => t.CategoryId == model.Filters.CategoryId.Value).ToList();
            }

            // فیلتر وضعیت
            if (model.Filters.TaskStatus.HasValue && model.Filters.TaskStatus != TaskStatusFilter.All)
            {
                switch (model.Filters.TaskStatus.Value)
                {
                    case TaskStatusFilter.Completed:
                        tasks = tasks.Where(t => t.CompletionDate.HasValue).ToList();
                        break;
                    case TaskStatusFilter.InProgress:
                        tasks = tasks.Where(t => !t.CompletionDate.HasValue && t.IsActive).ToList();
                        break;
                    case TaskStatusFilter.Overdue:
                        tasks = tasks.Where(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now).ToList();
                        break;
                }
            }

            // فیلتر طرف حساب
            if (model.Filters.StakeholderId.HasValue)
            {
                tasks = tasks.Where(t => t.StakeholderId == model.Filters.StakeholderId.Value).ToList();
            }

            // فیلتر جستجو در متن
            if (!string.IsNullOrEmpty(model.Filters.SearchTerm))
            {
                tasks = tasks.Where(t => 
                    t.Title.Contains(model.Filters.SearchTerm) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(model.Filters.SearchTerm)) ||
                    t.TaskCode.Contains(model.Filters.SearchTerm)).ToList();
            }

            model.Tasks = tasks;
        }

        /// <summary>
        /// AJAX برای تغییر سریع نوع نمایش
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeViewType(TaskViewType viewType)
        {
            try
            {
                var filters = new TaskFilterViewModel { ViewType = viewType };
                return RedirectToAction("Index", filters);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ChangeViewType",
                    "خطا در تغییر نوع نمایش",
                    ex
                );
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// فیلتر سریع بر اساس آمار (فیلتر ثانویه)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FilterByStatus(TaskStatusFilter statusFilter, TaskFilterViewModel currentFilters = null)
        {
            try
            {
                currentFilters ??= new TaskFilterViewModel();
                currentFilters.TaskStatus = statusFilter;

                return RedirectToAction("Index", currentFilters);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "FilterByStatus",
                    "خطا در فیلتر سریع وضعیت",
                    ex
                );
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// فیلتر سریع بر اساس اولویت (فیلتر ثانویه)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FilterByPriority(TaskPriorityFilter priorityFilter, TaskFilterViewModel currentFilters = null)
        {
            try
            {
                currentFilters ??= new TaskFilterViewModel();
                currentFilters.TaskPriority = priorityFilter;

                return RedirectToAction("Index", currentFilters);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "FilterByPriority",
                    "خطا در فیلتر سریع اولویت",
                    ex
                );
                return RedirectToAction("Index");
            }
        }

        // متد کمکی برای دریافت کاربران تیم‌های انتخاب شده
        private async Task<List<string>> GetUsersFromTeams(List<int> teamIds)
        {
            if (teamIds == null || !teamIds.Any())
                return new List<string>();

            try
            {
                var teamUserIds = new List<string>();

                foreach (var teamId in teamIds)
                {
                    // دریافت اعضای تیم از جدول TeamMember
                    var teamMembers = _uow.TeamMemberUW.Get(tm => tm.TeamId == teamId && tm.IsActive)
                        .Select(tm => tm.UserId)
                        .ToList();

                    teamUserIds.AddRange(teamMembers);

                    // اضافه کردن مدیر تیم (اگر وجود دارد)
                    var team = _uow.TeamUW.GetById(teamId);
                    if (team != null && !string.IsNullOrEmpty(team.ManagerUserId))
                    {
                        teamUserIds.Add(team.ManagerUserId);
                    }
                }

                return teamUserIds.Distinct().ToList();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetUsersFromTeams",
                    "خطا در دریافت کاربران تیم‌ها",
                    ex
                );
                return new List<string>();
            }
        }

        // متد کمکی برای دریافت شعبه کاربر
        private int GetUserBranchId(string userId)
        {
            var branchUser = _uow.BranchUserUW.Get(bu => bu.UserId == userId && bu.IsActive).FirstOrDefault();
            return branchUser?.BranchId ?? 1; // پیش‌فرض شعبه اصلی
        }

        // افزودن تسک جدید - نمایش فرم
        // در متد Create (GET)
        [HttpGet]
        [Permission("Tasks", "CreateNewTask", 1)] // Create permission
        public async Task<IActionResult> CreateNewTask(string? AddressRouteInComingUrl, int TaskTeamMember = 0)
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

        // در متد Create (POST) - اضافه کردن logic کد تسک
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "CreateNewTask", 1)]
        public async Task<IActionResult> CreateNewTask(TaskViewModel model)
        {
            List<string> AssimentUserTask = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
            List<int> AssignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

            try
            {
                // مقداردهی اولیه کد تسک
                string finalTaskCode = _taskCodeGenerator.GenerateTaskCode();

                // حذف خطای پیش‌فرض برای BranchIdSelected اگر وجود دارد
                if (ModelState.ContainsKey("BranchIdSelected"))
                {
                    ModelState.Remove("BranchIdSelected");
                }

                // اعتبارسنجی سفارشی برای شعبه
                if (model.BranchIdSelected <= 0)
                {
                    ModelState.AddModelError("BranchIdSelected", "انتخاب شعبه الزامی است. لطفاً یک شعبه انتخاب کنید.");
                }
                else
                {
                    // بررسی دسترسی کاربر به شعبه انتخاب شده
                    var userId = _userManager.GetUserId(User);
                    var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                    if (!userBranches.Any(b => b.Id == model.BranchIdSelected))
                    {
                        ModelState.AddModelError("BranchIdSelected", "شما به شعبه انتخاب شده دسترسی ندارید.");
                    }
                }

                // تعیین کد نهایی تسک بر اساس نوع ورودی
                if (model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode))
                {
                    // اعتبارسنجی کد دستی
                    if (!_taskCodeGenerator.ValidateTaskCode(model.ManualTaskCode))
                    {
                        var settings = _taskCodeGenerator.GetTaskCodeSettings();
                        ModelState.AddModelError("ManualTaskCode", 
                            $"کد تسک نامعتبر است. نمی‌توانید از پیشوند '{settings.SystemPrefix}-' استفاده کنید یا کد تکراری است.");
                    }
                    else
                    {
                        finalTaskCode = model.ManualTaskCode;
                    }
                }
                // در غیر این صورت از کد اتوماتیک که در بالا تولید شده استفاده می‌شود

                // اگر خطای validation وجود دارد، فرم را بازگردان
                if (!ModelState.IsValid)
                {
                    await RepopulateCreateTaskModel(model);
                    return View(model);
                }

                // ایجاد تسک جدید
                var task = _mapper.Map<Tasks>(model);
                task.TaskCode = finalTaskCode; // تنظیم کد نهایی
                task.CreateDate = DateTime.Now;
                task.CreatorUserId = _userManager.GetUserId(User);
                task.IsActive = model.IsActive;
                task.IsDeleted = false;
                task.TaskTypeInput = 1;
                task.VisibilityLevel = 0;
                task.Priority = 0;
                task.Important = false;
                task.Status = 0;
                task.CreationMode = 0;
                task.TaskCategoryId = model.TaskCategoryIdSelected;

                task.BranchId = model.BranchIdSelected; // اضافه کردن BranchId

             



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

                // دریافت کاربران از تیم‌های انتخاب شده
                var teamUserIds = await GetUsersFromTeams(AssignedTeamIds);
                
                // ترکیب کاربران انتخاب شده مستقیماً و کاربران تیم‌ها (بدون تکرار)
                var allAssignedUserIds = AssimentUserTask.Union(teamUserIds).Distinct().ToList();

                // اختصاص به کاربر جاری (خود کاربر ایجاد کننده) اگر در لیست نیست
                var currentUserId = _userManager.GetUserId(User);
                if (!allAssignedUserIds.Contains(currentUserId))
                {
                    var selfAssignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = currentUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 1, // سازنده تسک
                        AssignmentDate = DateTime.Now,
                        Description = "سازنده تسک"
                    };
                    _uow.TaskAssignmentUW.Create(selfAssignment);
                }

                // اختصاص تسک به تمام کاربران (فردی + تیمی)
                foreach (var assignedUserId in allAssignedUserIds)
                {
                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = assignedUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 0, // اصلی
                        AssignmentDate = DateTime.Now,
                        Description = AssimentUserTask.Contains(assignedUserId) ? "انتصاب مستقیم" : "انتصاب از طریق تیم",
                    };
                    _uow.TaskAssignmentUW.Create(assignment);
                }
                //اموزش کاامیت کردن 
                _uow.Save();

                // ارسال نوتیفیکیشن ایجاد تسک جدید
                try
                {
                    await _taskNotificationService.NotifyTaskCreatedAsync(
                        task.Id, 
                        currentUserId, 
                        allAssignedUserIds
                    );
                }
                catch (Exception notificationEx)
                {
                    // لاگ خطای نوتیفیکشن اما عملیات اصلی را متوقف نکنیم
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
                    $"ایجاد تسک جدید: {task.Title} با کد: {task.TaskCode} - اختصاص به {allAssignedUserIds.Count} کاربر ({AssimentUserTask.Count} مستقیم + {teamUserIds.Count} از تیم‌ها)",
                    recordId: task.Id.ToString(),
                    entityType: "Tasks",
                    recordTitle: task.Title
                );

                TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "CreateNewTask",
                    "خطا در ایجاد تسک جدید",
                    ex
                );
                
                ModelState.AddModelError("", "خطایی در ثبت تسک رخ داد: " + ex.Message);
            }

            // اصلاح: بازیابی داده‌های فرم در صورت validation error
            await RepopulateCreateTaskModel(model);
            return View(model);
        }

        /// <summary>
        /// متد کمکی برای بازیابی داده‌های فرم CreateNewTask
        /// </summary>
        private async Task RepopulateCreateTaskModel(TaskViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // بازیابی لیست شعبه‌ها
                model.branchListInitial = _branchRepository.GetBrnachListByUserId(userId);
                
                // بازیابی تنظیمات کد تسک
                if (model.TaskCodeSettings == null)
                {
                    model.TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings();
                }
                
                // اگر کد تسک خالی است، کد جدید تولید کن
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // اصلاح: اگر لیست شعبه خالی است، لیست خالی ایجاد کن تا از null exception جلوگیری شود
                if (model.branchListInitial == null)
                {
                    model.branchListInitial = new List<BranchViewModel>();
                }

                // اگر دسته‌بندی‌ها خالی است، لیست خالی ایجاد کن
                if (model.TaskCategoryInitial == null)
                {
                    model.TaskCategoryInitial = _taskRepository.GetAllCategories();
                }

                // اگر کاربران خالی است، لیست خالی ایجاد کن
                if (model.UsersInitial == null)
                {
                    // برای اولین بار، لیست خالی بده تا شعبه انتخاب شود
                    model.UsersInitial = new List<UserViewModelFull>();
                }

                // اگر تیم‌ها خالی است، لیست خالی ایجاد کن
                if (model.TeamsInitial == null)
                {
                    model.TeamsInitial = new List<TeamViewModel>();
                }

                // اگر طرف حساب‌ها خالی است، لیست خالی ایجاد کن
                if (model.StakeholdersInitial == null)
                {
                    model.StakeholdersInitial = new List<StakeholderViewModel>();
                }

                // PopulateDropdowns - برای ViewBag
                PopulateDropdowns();
            }
            catch (Exception ex)
            {
                // در صورت خطا، حداقل لیست‌های خالی ایجاد کن
                model.branchListInitial ??= new List<BranchViewModel>();
                model.TaskCategoryInitial ??= new List<TaskCategory>();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= new List<TeamViewModel>();
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();
                
                model.TaskCodeSettings ??= new TaskCodeSettings
                {
                    AllowManualInput = false,
                    SystemPrefix = "TSK"
                };

                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "RepopulateCreateTaskModelFixed",
                    "خطا در بازیابی داده‌های فرم CreateTask",
                    ex
                );
            }
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

        /// <summary>
        /// بروزرسانی لیست کاربران و تیم‌ها بر اساس شعبه انتخاب شده
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>PartialView حاوی لیست کاربران و تیم‌های شعبه</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelect(int branchId)
        {
            try
            {
                // دریافت کاربران شعبه انتخاب شده با استفاده از Repository
                var branchUsersViewModels = _branchRepository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت تیم‌های شعبه انتخاب شده
                var branchTeamsViewModels = await GetBranchTeamsByBranchId(branchId);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                var stakeholdersViewModels = _stakeholderRepository.GetStakeholdersByBranchId(branchId);

                // رندر کردن partial views
                var usersPartialView = await this.RenderViewToStringAsync("_BranchUsersSelect", branchUsersViewModels);
                var teamsPartialView = await this.RenderViewToStringAsync("_BranchTeamsSelect", branchTeamsViewModels);
                var stakeholdersPartialView = await this.RenderViewToStringAsync("_BranchStakeholdersSelect", stakeholdersViewModels);

                // اضافه کردن به response - بروزرسانی کاربران، تیم‌ها، طرف حساب‌ها و دسته‌بندی‌ها
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
                        elementId = "TeamsDiv",
                        view = new
                        {
                            result = teamsPartialView
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
                            result = ""
                        }
                    }
                };

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "BranchTriggerSelect",
                    $"بارگذاری کاربران ({branchUsersViewModels?.Count ?? 0}), تیم‌ها ({branchTeamsViewModels?.Count ?? 0}), طرف حساب‌ها ({stakeholdersViewModels?.Count ?? 0}) شعبه {branchId}"
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
                    "خطا در بارگذاری کاربران، تیم‌ها، طرف حساب‌ها و دسته‌بندی‌های شعبه",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = "خطا در بارگذاری کاربران، تیم‌ها و طرف حساب‌های شعبه: " + ex.Message
                });
            }
        }

        /// <summary>
        /// متد کمکی برای دریافت تیم‌های شعبه
        /// </summary>
        private async Task<List<TeamViewModel>> GetBranchTeamsByBranchId(int branchId)
        {
            try
            {
                // دریافت تیم‌ها از طریق UnitOfWork
                var teams = _uow.TeamUW.Get(t => t.BranchId == branchId && t.IsActive)
                    .Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        BranchId = t.BranchId,
                        IsActive = t.IsActive,
                        ManagerFullName = !string.IsNullOrEmpty(t.ManagerUserId) 
                            ? _userManager.Users.FirstOrDefault(u => u.Id == t.ManagerUserId)?.FirstName + " " + 
                              _userManager.Users.FirstOrDefault(u => u.Id == t.ManagerUserId)?.LastName 
                            : "ندارد"
                    })
                    .OrderBy(t => t.Title)
                    .ToList();

                return teams;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetBranchTeamsByBranchId",
                    "خطا در دریافت تیم‌های شعبه",
                    ex
                );
                return new List<TeamViewModel>();
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

        /// <summary>
        /// بروزرسانی فیلترهای تقویم بر اساس شعبه انتخاب شده
        /// این متد مخصوص TaskCalendar است و ID های مختلف را بروزرسانی می‌کند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>PartialView حاوی فیلترهای تقویم</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelectForCalendar(int branchId)
        {
            try
            {
                // دریافت کاربران شعبه انتخاب شده با استفاده از Repository
                var branchUsersViewModels = _branchRepository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                var stakeholdersViewModels = _stakeholderRepository.GetStakeholdersByBranchId(branchId);

                // رندر کردن partial views مخصوص فیلتر
                var usersPartialView = await this.RenderViewToStringAsync("_FilterBranchUsersSelect", branchUsersViewModels);
                var stakeholdersPartialView = await this.RenderViewToStringAsync("_FilterBranchStakeholdersSelect", stakeholdersViewModels);

                // اضافه کردن به response - بروزرسانی فیلترهای تقویم
                var viewList = new List<object>
                {
                    new
                    {
                        elementId = "FilterUsersDiv",
                        view = new
                        {
                            result = usersPartialView
                        }
                    },
                    new
                    {
                        elementId = "FilterStakeholdersDiv",
                        view = new
                        {
                            result = stakeholdersPartialView
                        }
                    }
                };

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "BranchTriggerSelectForCalendar",
                    $"بارگذاری فیلترهای تقویم - کاربران ({branchUsersViewModels?.Count ?? 0}), طرف حساب‌ها ({stakeholdersViewModels?.Count ?? 0}) شعبه {branchId}"
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
                    "BranchTriggerSelectForCalendar",
                    "خطا در بارگذاری فیلترهای تقویم",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = "خطا در بارگذاری فیلترهای تقویم: " + ex.Message
                });
            }
        }

        /// <summary>
        /// اصلاح متد CreateTask برای بازیابی صحیح کاربران شعبه
        /// </summary>
        private async Task RepopulateCreateTaskModelFixed(TaskViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // بازیابی لیست شعبه‌ها (اختیاری)
                model.branchListInitial = _branchRepository.GetBrnachListByUserId(userId);
                
                // بازیابی تنظیمات کد تسک
                if (model.TaskCodeSettings == null)
                {
                    model.TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings();
                }
                
                // اگر کد تسک خالی است، کد جدید تولید کن
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // بازیابی دسته‌بندی‌ها
                model.TaskCategoryInitial ??= _taskRepository.GetAllCategories();

                // بازیابی کاربران (محدود به شعبه کاربر)
                var userBranchId = GetUserBranchId(userId);
                if (model.UsersInitial == null)
                {
                    // دریافت کاربران شعبه به صورت صحیح
                    var branchUsers = _branchRepository.GetBranchUsersByBranchId(userBranchId, includeInactive: false);
                    model.UsersInitial = branchUsers.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId, // اصلاح: استفاده از UserId
                        FullNamesString = u.UserFullName, // اصلاح: استفاده از UserFullName
                        //FirstName = u.UserFirstName, // اصلاح property names
                        //LastName = u.UserLastName,
                        //UserName = u.UserUserName,
                        //PositionName = u.UserPositionName,
                        IsActive = u.IsActive
                    }).ToList();
                }

                // بازیابی تیم‌ها (محدود به شعبه کاربر)
                model.TeamsInitial ??= await GetUserRelatedTeams(userId);

                // بازیابی طرف حساب‌ها (محدود به شعبه کاربر)
                if (model.StakeholdersInitial == null)
                {
                    var stakeholders = _stakeholderRepository.GetStakeholdersByBranchId(userBranchId);
                    model.StakeholdersInitial = stakeholders.Select(s => new StakeholderViewModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        CompanyName = s.CompanyName,
                        NationalCode = s.NationalCode,
                        IsActive = s.IsActive
                    }).ToList();
                }

                // PopulateDropdowns - برای ViewBag
                PopulateDropdowns();
            }
            catch (Exception ex)
            {
                // در صورت خطا، حداقل لیست‌های خالی ایجاد کن
                model.branchListInitial ??= new List<BranchViewModel>();
                model.TaskCategoryInitial ??= new List<TaskCategory>();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= new List<TeamViewModel>();
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();
                
                model.TaskCodeSettings ??= new TaskCodeSettings
                {
                    AllowManualInput = false,
                    SystemPrefix = "TSK"
                };

                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "RepopulateCreateTaskModelFixed",
                    "خطا در بازیابی داده‌های فرم CreateTask",
                    ex
                );
            }
        }

        /// <summary>
        /// دریافت تسک‌های من و سلسله مراتب تیم
        /// </summary>
        private async Task LoadMyTeamsHierarchyTasks(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                // 1. تسک‌های خود کاربر
                var myTasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
                model.GroupedTasks.MyTasks = _mapper.Map<List<TaskViewModel>>(myTasks);

                // 2. تسک‌های اعضای تیم‌هایی که کاربر مدیر آن‌هاست
                var managedTeams = _uow.TeamUW.Get(t => t.ManagerUserId == userId && t.IsActive);

                foreach (var team in managedTeams)
                {
                    var teamMembers = _uow.TeamMemberUW.Get(tm => tm.TeamId == team.Id && tm.IsActive);

                    foreach (var member in teamMembers)
                    {
                        var memberTasks = _taskRepository.GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                        var memberUser = await _userManager.FindByIdAsync(member.UserId);
                        var memberName = $"{memberUser?.FirstName} {memberUser?.LastName}";

                        if (memberTasks.Any())
                        {
                            model.GroupedTasks.TeamMemberTasks[memberName] = _mapper.Map<List<TaskViewModel>>(memberTasks);
                        }
                    }

                    // 3. تسک‌های تیم‌های زیرمجموعه
                    await LoadSubTeamTasks(model, team.Id);
                }

                // در نهایت، تمام تسک‌ها را در لیست اصلی نیز قرار می‌دهیم
                var allTasks = new List<TaskViewModel>();
                allTasks.AddRange(model.GroupedTasks.MyTasks);
                allTasks.AddRange(model.GroupedTasks.TeamMemberTasks.Values.SelectMany(tasks => tasks));
                allTasks.AddRange(model.GroupedTasks.SubTeamTasks.Values.SelectMany(tasks => tasks));

                // حذف تکرارها
                model.Tasks = allTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "LoadMyTeamsHierarchyTasks",
                    $"بارگذاری {model.Tasks.Count} تسک سلسله مراتبی برای کاربر {userId}"
                );
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "LoadMyTeamsHierarchyTasks",
                    "خطا در بارگذاری تسک‌های سلسله مراتبی",
                    ex
                );

                // در صورت خطا، حداقل تسک‌های شخصی را بارگذاری کن
                var myTasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
                model.Tasks = _mapper.Map<List<TaskViewModel>>(myTasks);
                model.GroupedTasks.MyTasks = model.Tasks;
            }
        }

        /// <summary>
        /// دریافت تسک‌های تیم‌های زیرمجموعه
        /// </summary>
        private async Task LoadSubTeamTasks(TaskListForIndexViewModel model, int parentTeamId)
        {
            try
            {
                var subTeams = _uow.TeamUW.Get(t => t.ParentTeamId == parentTeamId && t.IsActive);

                foreach (var subTeam in subTeams)
                {
                    var subTeamMembers = _uow.TeamMemberUW.Get(tm => tm.TeamId == subTeam.Id && tm.IsActive);
                    var subTeamTasks = new List<TaskViewModel>();

                    foreach (var member in subTeamMembers)
                    {
                        var memberTasks = _taskRepository.GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                        subTeamTasks.AddRange(_mapper.Map<List<TaskViewModel>>(memberTasks));
                    }

                    if (subTeamTasks.Any())
                    {
                        // حذف تکرارها
                        var uniqueSubTeamTasks = subTeamTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
                        model.GroupedTasks.SubTeamTasks[subTeam.Title] = uniqueSubTeamTasks;
                    }

                    // بررسی تیم‌های زیرمجموعه بیشتر (بازگشتی)
                    await LoadSubTeamTasks(model, subTeam.Id);
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "LoadSubTeamTasks",
                    "خطا در بارگذاری تسک‌های تیم‌های زیرمجموعه",
                    ex
                );
            }
        }

        /// <summary>
        /// دریافت تسک‌های منتصب به کاربر
        /// </summary>
        private async Task LoadAssignedToMeTasks(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                var assignedTasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: false);
                model.Tasks = _mapper.Map<List<TaskViewModel>>(assignedTasks);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "LoadAssignedToMeTasks",
                    $"بارگذاری {model.Tasks.Count} تسک منتصب شده برای کاربر {userId}"
                );
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "LoadAssignedToMeTasks",
                    "خطا در بارگذاری تسک‌های منتصب شده",
                    ex
                );
                model.Tasks = new List<TaskViewModel>();
            }
        }
        

    }
}