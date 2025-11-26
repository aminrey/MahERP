using DocumentFormat.OpenXml.Spreadsheet;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.MyDayTaskRepository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
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
        private readonly IMyDayTaskRepository _myDayTaskRepository;
        private readonly ITaskRepository _TaskRepository;
        private new readonly UserManager<AppUsers> _userManager;

        public MyDayTaskController(
            IMyDayTaskRepository myDayTaskRepository,
            IUnitOfWork Context,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            UserManager<AppUsers> userManager, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking,
            IModuleAccessService moduleAccessService , ITaskRepository taskRepository)


 : base(Context, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _myDayTaskRepository = myDayTaskRepository;
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

            var model = await _myDayTaskRepository.GetMyDayTasksAsync(currentUserId, startDate, endDate);

            return View(model);
        }

        /// <summary>
        /// مودال افزودن تسک به روز من
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddToMyDayModal(int taskId, bool fromList = false)
        {
            // ⭐⭐⭐ دریافت taskAssignment برای کاربر فعلی
            var currentUserId = _userManager.GetUserId(User);
            
            var taskAssignment =  _uow.TaskAssignmentUW.Get()
                .Where(ta => ta.TaskId == taskId && ta.AssignedUserId == currentUserId)
                .FirstOrDefault();

            if (taskAssignment == null)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "تخصیص تسک یافت نشد" } }
                });
            }

            var model = new AddToMyDayViewModel
            {
                TaskAssignmentId = taskAssignment.Id,
                TaskId = taskId,
                FromList = fromList,
                PlannedDate = DateTime.Now.Date
            };

            return PartialView("_AddToMyDayModal", model);
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
            var result = await _myDayTaskRepository.AddTaskToMyDayAsync(
                model.TaskAssignmentId,
                currentUserId,
                model.PlannedDate,
                model.PlanNote,
                model.GroupTitle); // ⭐ اضافه شد

            if (result.Success)
            {
                // ⭐⭐⭐ اگر از لیست آمده، پارشیال ردیف را برگردان
                if (model.FromList && model.TaskId.HasValue)
                {
                    // دریافت اطلاعات تسک به‌روز شده
                    var taskCard = await _TaskRepository.GetTaskCardViewModelAsync(model.TaskId.Value, currentUserId);
                    
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

                // ⭐ حالت پیش‌فرض: redirect
                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "MyDayTask"),
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
        public async Task<IActionResult> RemoveFromMyDay(int myDayId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var result = await _myDayTaskRepository.RemoveTaskFromMyDayAsync(myDayId, currentUserId);

            if (result.Success)
            {
                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "MyDayTask"),
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
            var result = await _myDayTaskRepository.LogWorkAsync(
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
            var result = await _myDayTaskRepository.SetTaskAsFocusedAsync(myDayId, currentUserId);

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
            var result = await _myDayTaskRepository.RemoveFocusFromTaskAsync(myDayId, currentUserId);

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
            var task = await _myDayTaskRepository.GetFocusedTaskAsync(currentUserId);

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
                var workLogs = await _myDayTaskRepository.GetTaskWorkLogsAsync(taskId);
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
        /// ⭐⭐⭐ مودال تایید حذف از روز من - استفاده از Repository
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveFromMyDayModal(int myDayId)
        {
            try
            {
                // ✅ استفاده از Repository به جای دسترسی مستقیم به دیتابیس
                var taskInfo = await _myDayTaskRepository.GetMyDayTaskInfoForRemovalAsync(myDayId);

                if (taskInfo == null)
                    return NotFound();

                var model = new
                {
                    MyDayId = myDayId,
                    TaskTitle = taskInfo.Value.TaskTitle,
                    TaskCode = taskInfo.Value.TaskCode
                };

                return PartialView("_RemoveFromMyDayModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveFromMyDayModal: {ex.Message}");
                return NotFound();
            }
        }
    }
}