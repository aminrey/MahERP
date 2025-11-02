using DocumentFormat.OpenXml.Spreadsheet;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.MyDayTaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private new readonly UserManager<AppUsers> _userManager;

        public MyDayTaskController(
            IMyDayTaskRepository myDayTaskRepository,
            IUnitOfWork Context,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            UserManager<AppUsers> userManager, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(Context, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
        {
            _myDayTaskRepository = myDayTaskRepository;
            _userManager = userManager;
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
        public async Task<IActionResult> AddToMyDayModal(int taskAssignmentId)
        {
            var model = new AddToMyDayViewModel
            {
                TaskAssignmentId = taskAssignmentId,
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
                    message = "داده‌های ورودی نامعتبر است"
                });
            }

            var currentUserId = _userManager.GetUserId(User);
            var result = await _myDayTaskRepository.AddTaskToMyDayAsync(
                model.TaskAssignmentId,
                currentUserId,
                model.PlannedDate,
                model.PlanNote);

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
        public async Task<IActionResult> LogWorkModal(int myDayId)
        {
            var model = new MyDayLogWorkViewModel
            {
                MyDayId = myDayId
            };

            return PartialView("_MyDayLogWorkModal", model);
        }

        /// <summary>
        /// ثبت گزارش کار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogWork(MyDayLogWorkViewModel model)
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
        public async Task<IActionResult> ViewWorkLogsModal(int taskId)
        {
            try
            {
                // ✅ استفاده از Repository به جای دسترسی مستقیم به دیتابیس
                var workLogs = await _myDayTaskRepository.GetTaskWorkLogsAsync(taskId);

                return PartialView("_TaskWorkLogsModal", workLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ViewWorkLogsModal: {ex.Message}");
                return PartialView("_TaskWorkLogsModal", new List<TaskWorkLogViewModel>());
            }
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