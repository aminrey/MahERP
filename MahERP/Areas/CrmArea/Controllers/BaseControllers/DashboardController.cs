using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers.BaseControllers
{
    [Authorize]
    [Area("CrmArea")]
    [PermissionRequired("DASHBOARD.VIEW")]
    public class DashboardController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly IMapper _Mapper;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUsers> _UserManager;
        private new readonly PersianDateHelper _persianDateHelper;
        private readonly IMemoryCache _memoryCache;
        private readonly IMainDashboardRepository _mainDashboardRepository;
        private readonly ITaskRepository _taskrepository;
        private readonly INotificationService _notify;
        protected readonly IUserManagerRepository _userRepository;


        public DashboardController(
            IWebHostEnvironment env,
            IUnitOfWork Context,
            IMapper Mapper,
            UserManager<AppUsers> UserManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ITaskRepository taskRepository,
            INotificationService notify,
            ActivityLoggerService activityLogger, IBaseRepository BaseRepository,
            IMainDashboardRepository mainDashboardRepository,
                    IUserManagerRepository userRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(Context, UserManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
        {
            _Context = Context;
            _UserManager = UserManager;
            _Mapper = Mapper;
            _env = env;
            _persianDateHelper = persianDateHelper;
            _memoryCache = memoryCache;
            _mainDashboardRepository = mainDashboardRepository;
            _taskrepository = taskRepository;
            _notify = notify;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var model = await _mainDashboardRepository.PrepareDashboardModelAsync(userId, User.Identity.Name);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Dashboard", "Index", "مشاهده داشبورد اصلی");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "Index", "خطا در نمایش داشبورد", ex);
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// دریافت آمار کلی برای داشبورد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                var stats = await _mainDashboardRepository.GetUserDashboardStatsAsync(userId);

                return Json(new
                {
                    success = true,
                    tasksStats = new
                    {
                        myTasksCount = stats.TasksStats.MyTasksCount,
                        ActivePersonalTasksCount = stats.TasksStats.MyTasksCount, // ⭐ اضافه کنید این خط را
                        onTimeTasksCount = stats.TasksStats.OnTimeTasksCount,
                        overdueTasksCount = stats.TasksStats.OverdueTasksCount,
                        supervisedTasksCount = stats.TasksStats.SupervisedTasksCount,
                        assignedByMeCount = stats.TasksStats.AssignedByMeCount,
                        todayTasksCount = stats.TasksStats.TodayTasksCount,
                        remindersCount = stats.TasksStats.RemindersCount
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetDashboardSummary", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }
        /// <summary>
        /// دریافت آخرین تسک‌های دریافتی کاربر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentReceivedTasks()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);

                // دریافت آخرین 5 تسک دریافتی با جزئیات کامل
                var userTasks = await _taskrepository.GetUserTasksComprehensiveAsync(
                    userId,
                    includeCreatedTasks: false,
                    includeAssignedTasks: true,
                    includeSupervisedTasks: false,
                    includeDeletedTasks: false
                );

                var recentTasks = userTasks.AssignedTasks
                    .OrderByDescending(t => t.CreateDate)
                    .Take(5)
                    .Select(t => new {
                        id = t.Id,
                        taskCode = t.TaskCode,
                        title = t.Title,
                        description = t.Description,
                        priority = t.Priority,
                        important = t.Important,
                        startDate = t.StartDate?.ToString("yyyy-MM-dd"),
                        createDate = t.CreateDate.ToString("yyyy-MM-dd"),
                        // ⭐ نام کسی که تسک را به من داده (سازنده تسک)
                        creatorName = GetTaskCreatorName(t.CreatorUserId)
                    })
                    .ToList();

                return Json(new { success = true, tasks = recentTasks });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetRecentReceivedTasks", "خطا در دریافت تسک‌های دریافتی", ex);
                return Json(new { success = false, message = "خطا در دریافت داده‌ها" });
            }
        }
        /// <summary>
        /// دریافت آخرین تسک‌های واگذار شده توسط کاربر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentAssignedTasks()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);

                // دریافت آخرین 5 تسک واگذار شده با جزئیات کامل
                var userTasks = await _taskrepository.GetUserTasksComprehensiveAsync(
                    userId,
                    includeCreatedTasks: true,    // ⭐ تغییر: باید CreatedTasks باشد نه AssignedTasks
                    includeAssignedTasks: false,  // ⭐ تغییر: برای تسک‌های واگذار شده
                    includeSupervisedTasks: false,
                    includeDeletedTasks: false
                );

                var recentTasks = userTasks.CreatedTasks  // ⭐ درست است - تسک‌هایی که من ایجاد کرده‌ام
                    .Where(t => t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any())
                    .OrderByDescending(t => t.CreateDate)
                    .Take(5)
                    .Select(t => new {
                        id = t.Id,
                        taskCode = t.TaskCode,
                        title = t.Title,
                        description = t.Description,
                        priority = t.Priority,
                        important = t.Important,
                        startDate = t.StartDate?.ToString("yyyy-MM-dd"),
                        createDate = t.CreateDate.ToString("yyyy-MM-dd"),
                        // ⭐ اصلاح: حذف assignments مربوط به خود سازنده
                        assignedToName = GetMainAssigneeName(t.AssignmentsTaskUser, userId)
                    })
                    .ToList();

                return Json(new { success = true, tasks = recentTasks });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetRecentAssignedTasks", "خطا در دریافت تسک‌های واگذار شده", ex);
                return Json(new { success = false, message = "خطا در دریافت داده‌ها" });
            }
        }
        /// <summary>
        /// دریافت یادآوری‌های مهم برای داشبورد - اصلاح شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardReminders()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);

                // ⭐ استفاده از متد ساده برای داشبورد
                var reminders = await _taskrepository.GetDashboardRemindersAsync(userId, maxResults: 10, daysAhead: 1);

                var dashboardReminders = reminders
                    .Select(r => new {
                        id = r.Id,
                        title = r.Title,
                        message = r.Message,
                        taskId = r.TaskId,
                        taskTitle = r.TaskTitle,
                        taskCode = r.TaskCode,
                        scheduledDateTime = r.ScheduledDateTime.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDatePersian = r.ScheduledDatePersian,
                        isSent = r.IsSent,
                        isRead = r.IsRead,
                        priority = r.Priority,
                        status = GetReminderStatus(r.ScheduledDateTime, r.IsSent, r.IsRead, DateTime.Now),
                        timeInfo = GetReminderTimeInfo(r.ScheduledDateTime, DateTime.Now)
                    })
                    .ToList();

                return Json(new { success = true, reminders = dashboardReminders });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Dashboard", "GetDashboardReminders", "خطا در دریافت یادآوری‌ها", ex);
                return Json(new { success = false, message = "خطا در دریافت داده‌ها" });
            }
        }
        /// <summary>
        /// دریافت آمار کاربر برای نمایش در header
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserHeaderStats()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "کاربر شناسایی نشد" });
                }

                // دریافت آمار تسک‌ها
                var taskStats = await _taskrepository.GetUserTaskStatsAsync(userId);

                // دریافت تعداد نوتیفیکیشن‌های خوانده نشده
                var notificationsCount = await _notify.GetUnreadNotificationsCountAsync(userId);

                var stats = new
                {
                    success = true,
                    myTasksCount = taskStats?.MyTasksCount ?? 0,
                    remindersCount = taskStats?.RemindersCount ?? 0,
                    notificationsCount = notificationsCount
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }




        /// <summary>
        /// دریافت نام اصلی کسی که تسک به او واگذار شده (بدون سازنده)
        /// </summary>
        private string GetMainAssigneeName(List<TaskAssignmentViewModel> assignments, string creatorUserId)
        {
            if (assignments == null || !assignments.Any())
                return "نامشخص";

            // ⭐ حذف assignment مربوط به خود سازنده
            var actualAssignees = assignments
                .Where(a => a.AssignedUserId != creatorUserId)  // حذف self-assignment
                .Where(a => !string.IsNullOrEmpty(a.AssignedUserId))  // فقط assignments معتبر
                .ToList();

            if (!actualAssignees.Any())
            {
                // اگر فقط به خودش واگذار شده، "خودم" بنویس
                return "خودم";
            }

            // اگر چند نفر هست، اولین نفر + تعداد بقیه
            if (actualAssignees.Count == 1)
            {
                return actualAssignees.First().AssignedUserName ?? "نامشخص";
            }
            else
            {
                var firstName = actualAssignees.First().AssignedUserName ?? "نامشخص";
                return $"{firstName} و {actualAssignees.Count - 1} نفر دیگر";
            }
        }
        /// <summary>
        /// دریافت نام سازنده تسک
        /// </summary>
        private string GetTaskCreatorName(string creatorUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(creatorUserId))
                    return "نامشخص";

                var user = _Context.UserManagerUW.GetById(creatorUserId);
                return user != null ? $"{user.FirstName} {user.LastName}".Trim() : "نامشخص";
            }
            catch (Exception)
            {
                return "نامشخص";
            }
        }

        /// <summary>
        /// تعیین وضعیت یادآوری
        /// </summary>
        private string GetReminderStatus(DateTime scheduledDateTime, bool isSent, bool isRead, DateTime now)
        {
            if (!isSent && scheduledDateTime < now)
                return "overdue"; // عقب افتاده
            if (!isSent && scheduledDateTime >= now && scheduledDateTime <= now.AddDays(1))
                return "upcoming"; // آینده نزدیک
            if (isSent && !isRead)
                return "unread"; // ارسال شده ولی خوانده نشده
            if (isSent && isRead)
                return "read"; // خوانده شده

            return "normal";
        }

        /// <summary>
        /// محاسبه اطلاعات زمانی یادآوری
        /// </summary>
        private string GetReminderTimeInfo(DateTime scheduledDateTime, DateTime now)
        {
            var timeSpan = scheduledDateTime - now;

            if (scheduledDateTime < now)
            {
                // گذشته
                var pastTime = now - scheduledDateTime;
                if (pastTime.TotalMinutes < 60)
                    return $"{(int)pastTime.TotalMinutes} دقیقه پیش";
                if (pastTime.TotalHours < 24)
                    return $"{(int)pastTime.TotalHours} ساعت پیش";
                return $"{(int)pastTime.TotalDays} روز پیش";
            }
            else
            {
                // آینده
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} دقیقه دیگر";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} ساعت دیگر";
                return $"{(int)timeSpan.TotalDays} روز دیگر";
            }
        }
    }
}