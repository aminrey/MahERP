using DocumentFormat.OpenXml.Spreadsheet;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using MahERP.Helpers; // ⭐⭐⭐ اضافه شد
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK.MYDAY")]
    public class MyDayTaskController : BaseController
    {
        private readonly ITaskRepository _TaskRepository;
        private new readonly UserManager<AppUsers> _userManager;

        public MyDayTaskController(
            IUnitOfWork Context,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            UserManager<AppUsers> userManager, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking,
            IModuleAccessService moduleAccessService , ITaskRepository taskRepository)


 : base(Context, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _userManager = userManager;
            _TaskRepository = taskRepository;
        }

        /// <summary>
        /// صفحه اصلی "روز من"
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? date = null)
        {
            var currentUserId = _userManager.GetUserId(User);

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = ConvertDateTime.ConvertShamsiToMiladi(date);
                startDate = parsedDate;
                endDate = parsedDate;
            }

            var model = await _TaskRepository.GetMyDayTasksAsync(currentUserId, startDate, endDate);

            return View(model);
        }

        /// <summary>
        /// مودال افزودن تسک به روز من
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddToMyDayModal(
            int taskId, 
            bool fromList = false,
            string? returnUrl = null,
            string? sourcePage = null)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                
                // ✅ دریافت لیست گروه‌های موجود برای Autocomplete
                var groupTitles = await _TaskRepository.GetMyDayGroupTitlesAsync(currentUserId);
                ViewBag.GroupTitles = groupTitles;
                
                // ✅ استفاده از TaskRepository به جای UnitOfWork
                var taskAssignment = await _TaskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, taskId);

                if (taskAssignment == null)
                {
                    TempData["ErrorMessage"] = "این تسک به شما تخصیص داده نشده است یا دسترسی ندارید";
                    
                    var errorModel = new AddToMyDayViewModel
                    {
                        TaskId = taskId,
                        FromList = fromList,
                        PlannedDate = DateTime.Now.Date,
                        ReturnUrl = returnUrl,
                        SourcePage = sourcePage
                    };
                    
                    return PartialView("_AddToMyDayModal", errorModel);
                }

                var model = new AddToMyDayViewModel
                {
                    TaskAssignmentId = taskAssignment.Id,
                    TaskId = taskId,
                    FromList = fromList,
                    PlannedDate = DateTime.Now.Date,
                    PlannedDateString = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.Date, "yyyy/MM/dd"),
                    ReturnUrl = returnUrl,
                    SourcePage = sourcePage
                };

                return PartialView("_AddToMyDayModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddToMyDayModal: {ex.Message}");
                TempData["ErrorMessage"] = "خطا در بارگذاری مودال: " + ex.Message;
                
                var errorModel = new AddToMyDayViewModel
                {
                    TaskId = taskId,
                    FromList = fromList,
                    PlannedDate = DateTime.Now.Date,
                    ReturnUrl = returnUrl,
                    SourcePage = sourcePage
                };
                
                return PartialView("_AddToMyDayModal", errorModel);
            }
        }

        /// <summary>
        /// افزودن تسک به روز من
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToMyDay(AddToMyDayViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "داده‌های ورودی نامعتبر است" } }
                });
            }
            
            model.PlannedDate = ConvertDateTime.ConvertShamsiToMiladi(model.PlannedDateString);
            var currentUserId = _userManager.GetUserId(User);
            
            // ⭐⭐⭐ پاس دادن GroupTitle به Repository
            var result = await _TaskRepository.AddTaskToMyDayAsync(
                model.TaskId,
                currentUserId,
                model.PlannedDate,
                model.PlanNote,
                model.GroupTitle);

            if (result.Success)
            {
                // ⭐⭐⭐ اگر از لیست آمده، پارشیال ردیف را برگردان
                if (model.FromList && model.TaskId != 0)
                {
                    // دریافت اطلاعات تسک به‌روز شده
                    var taskCard = await _TaskRepository.GetTaskCardViewModelAsync(model.TaskId, currentUserId);
                    
                    if (taskCard != null)
                    {
                        // ⭐ تنظیم IsInMyDay به true
                        taskCard.IsInMyDay = true;
                        
                        // رندر پارشیال ردیف
                        var partialView = await this.RenderViewToStringAsync("../Tasks/_TaskRowPartial", taskCard);

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
                            message = new[] { new { status = "success", text = result.Message } }
                        });
                    }
                }

                // ⭐⭐⭐ NEW: استفاده از ReturnUrlHelper برای تعیین URL بازگشت
                var returnUrl = this.GetSafeReturnUrl(
                    model.ReturnUrl,
                    defaultAction: "Index",
                    defaultController: "MyDayTask",
                    defaultArea: "TaskingArea"
                );

                // ⭐⭐⭐ لاگ برای Debug
                Console.WriteLine($"✅ AddToMyDay Success - SourcePage: {model.SourcePage}, ReturnUrl: {returnUrl}");

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = returnUrl,
                    message = new[] { new { status = "success", text = result.Message } }
                });
            }

            return Json(new
            {
                status = "error",
                message = new[] { new { status = "error", text = result.Message } }
            });
        }

        /// <summary>
        /// حذف تسک از روز من
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromMyDay(RemoveFromMyDayViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "داده‌های ورودی نامعتبر است" } }
                });
            }

            var currentUserId = _userManager.GetUserId(User);
            var result = await _TaskRepository.RemoveTaskFromMyDayAsync(model.MyDayId, currentUserId);

            if (result.Success)
            {
                // ⭐⭐⭐ استفاده از ReturnUrlHelper برای تعیین URL بازگشت
                var returnUrl = this.GetSafeReturnUrl(
                    model.ReturnUrl,
                    defaultAction: "Index",
                    defaultController: "MyDayTask",
                    defaultArea: "TaskingArea"
                );

                // ⭐⭐⭐ لاگ برای Debug
                Console.WriteLine($"✅ RemoveFromMyDay Success - SourcePage: {model.SourcePage}, ReturnUrl: {returnUrl}");

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = returnUrl,
                    message = new[] { new { status = "success", text = result.Message } }
                });
            }

            return Json(new
            {
                status = "error",
                message = new[] { new { status = "error", text = result.Message } }
            });
        }

        /// <summary>
        /// مودال ثبت گزارش کار
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SubmitWorkNote(int myDayId)
        {
            var model = new MyDayLogWorkViewModel
            {
                MyDayId = myDayId
            };

            return PartialView("_SubmitWorkNoteModal", model);
        }

        /// <summary>
        /// ثبت گزارش کار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWorkNote(MyDayLogWorkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    status = "error",
                    message = "لطفاً توضیحات کار را وارد کنید"
                });
            }

            var currentUserId = _userManager.GetUserId(User);
            var result = await _TaskRepository.LogWorkAsync(
                model.MyDayId,
                currentUserId,
                model.WorkNote,
                model.DurationMinutes);

            if (result.Success)
            {
                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index"),
                    message = result.Message
                });
            }

            return Json(new
            {
                status = "error",
                message = result.Message
            });
        }

        /// <summary>
        /// تنظیم تسک به عنوان متمرکز
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetFocused(int myDayId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var result = await _TaskRepository.SetTaskAsFocusedAsync(myDayId, currentUserId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        /// <summary>
        /// حذف تمرکز
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveFocused(int myDayId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var result = await _TaskRepository.RemoveFocusFromTaskAsync(myDayId, currentUserId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        /// <summary>
        /// دریافت تسک متمرکز فعلی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFocusedTask()
        {
            var currentUserId = _userManager.GetUserId(User);
            var task = await _TaskRepository.GetFocusedTaskAsync(currentUserId);

            return Json(new
            {
                success = task != null,
                task = task
            });
        }

        /// <summary>
        /// ⭐⭐⭐ مودال نمایش لیست کارهای انجام شده - استفاده از Repository
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SubmitAndShowTaskWorkLogs(int taskId)
        {
            try
            {
                // ✅ استفاده از Repository به جای دسترسی مستقیم به دیتابیس
                var workLogs = await _TaskRepository.GetTaskWorkLogsAsync(taskId);
                ViewBag.WorkLogs = workLogs;
                return  PartialView("_SubmitAndShowTaskWorkLogs", new TaskWorkLogViewModel());
            }
            catch (Exception ex)
            {

                 
return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// ثبت گزارش کار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAndShowTaskWorkLogs(TaskWorkLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    status = "error",
                    message = "لطفاً توضیحات کار را وارد کنید"
                });
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // ✅ استفاده از Repository
            var result = await _TaskRepository.AddTaskWorkLogAsync(
                model.TaskId,
                currentUserId,
                model.WorkDescription,
                model.DurationMinutes,
                model.ProgressPercentage
            );

            if (result.Success)
            { // ⭐⭐⭐ ارسال اعلان به صف - فوری و بدون Blocking
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
                    status = "redirect",
                    redirectUrl = Url.Action("Index"),
                    message = result.Message
                });
            }

            return Json(new
            {
                status = "error",
                message = result.Message
            });
        }

        /// <summary>
        /// ⭐⭐⭐ مودال تایید حذف از روز من - استفاده از taskId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveFromMyDayModal(
            int taskId,
            string? returnUrl = null,
            string? sourcePage = null)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ✅ استفاده از Repository - دریافت MyDayTask بر اساس taskId و userId
                var myDayTask = await _TaskRepository.GetMyDayTaskByTaskIdAsync(taskId, currentUserId);

                if (myDayTask == null)
                {
                    TempData["ErrorMessage"] = "این تسک در روز شما قرار ندارد";
                    return NotFound();
                }

                // ✅ دریافت اطلاعات تسک برای نمایش
                var taskInfo = await _TaskRepository.GetMyDayTaskInfoForRemovalAsync(myDayTask.Id);

                if (taskInfo == null)
                {
                    TempData["ErrorMessage"] = "تسک یافت نشد";
                    return NotFound();
                }

                var model = new RemoveFromMyDayViewModel
                {
                    MyDayId = myDayTask.Id,
                    TaskId = taskId,
                    TaskTitle = taskInfo.Value.TaskTitle,
                    TaskCode = taskInfo.Value.TaskCode,
                    ReturnUrl = returnUrl,
                    SourcePage = sourcePage
                };

                return PartialView("_RemoveFromMyDayModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveFromMyDayModal: {ex.Message}");
                TempData["ErrorMessage"] = "خطا در بارگذاری مودال: " + ex.Message;
                return NotFound();
            }
        }
    }
}