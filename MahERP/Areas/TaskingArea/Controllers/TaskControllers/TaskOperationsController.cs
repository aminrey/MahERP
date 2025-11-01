using AutoMapper;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK.OPERATIONS")]
    public class TaskOperationsController : Controller
    {
        private readonly ITaskOperationsRepository _operationsRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly UserManager<AppUsers> _userManager;
        private readonly ActivityLoggerService _activityLogger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _context;

        public TaskOperationsController(
            ITaskOperationsRepository operationsRepository,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            ActivityLoggerService activityLogger,
            IUnitOfWork context,
            IMapper mapper)
        {
            _operationsRepository = operationsRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _mapper = mapper;
            _context = context;
        }

        #region Toggle Actions (AJAX)

        /// <summary>
        /// تغییر وضعیت ستاره عملیات (AJAX)
        /// </summary>
        [HttpPost]

        public async Task<IActionResult> ToggleOperationStar(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _operationsRepository.ToggleOperationStarAsync(id, userId);

                if (result.Success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Update,
                        "TaskOperations",
                        "ToggleOperationStar",
                        $"تغییر وضعیت ستاره عملیات {id}",
                        recordId: id.ToString());
                }

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "ToggleOperationStar",
                    "خطا در تغییر وضعیت ستاره",
                    ex,
                    recordId: id.ToString());

                return Json(new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت ستاره"
                });
            }
        }

        /// <summary>
        /// تغییر وضعیت تکمیل عملیات (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleOperationComplete(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _operationsRepository.ToggleOperationCompleteAsync(id, userId);

                if (result.Success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Update,
                        "TaskOperations",
                        "ToggleOperationComplete",
                        $"تغییر وضعیت تکمیل عملیات {id}",
                        recordId: id.ToString());
                }

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "ToggleOperationComplete",
                    "خطا در تغییر وضعیت تکمیل",
                    ex,
                    recordId: id.ToString());

                return Json(new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت تکمیل"
                });
            }
        }

        #endregion
        [HttpGet]
        public async Task<IActionResult> GetOperationsGroup(int taskId, string groupType)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (!_taskRepository.IsUserRelatedToTask(userId, taskId))
                {
                    return Json(new { success = false, message = "دسترسی غیرمجاز" });
                }

                var task = _taskRepository.GetTaskById(taskId, true);
                if (task == null)
                {
                    return Json(new { success = false, message = "تسک یافت نشد" });
                }

                // ⭐ بررسی اینکه groupType نباید خالی باشد
                if (string.IsNullOrWhiteSpace(groupType))
                {
                    await _activityLogger.LogErrorAsync(
                        "TaskOperations",
                        "GetOperationsGroup",
                        "GroupType is null or empty",
                        new Exception($"TaskId: {taskId}, GroupType: '{groupType}'"));

                    return Json(new
                    {
                        success = false,
                        message = "نوع گروه مشخص نشده است",
                        count = 0
                    });
                }

                var operations = task.TaskOperations.Where(o => !o.IsDeleted);

                // ⭐ Normalize groupType
                var normalizedGroupType = groupType.Trim().ToLower();

                // فیلتر بر اساس نوع گروه
                IEnumerable<TaskOperation> filteredOps = normalizedGroupType switch
                {
                    "starred" => operations.Where(o => o.IsStarred && !o.IsCompleted),
                    "pending" => operations.Where(o => !o.IsStarred && !o.IsCompleted),
                    "completed" => operations.Where(o => o.IsCompleted),
                    _ => Enumerable.Empty<TaskOperation>()
                };

                var viewModel = _mapper.Map<List<TaskOperationViewModel>>(
                    filteredOps.OrderBy(o => o.OperationOrder)
                );

                // ⭐ ایجاد dynamic object برای ViewBag
                string groupTitle = "";
                string groupIcon = "";
                string groupColor = "";
                string groupDescription = "";

                switch (normalizedGroupType)
                {
                    case "starred":
                        groupTitle = "عملیات ستاره‌دار";
                        groupIcon = "star";
                        groupColor = "warning";
                        groupDescription = "عملیات‌های با اولویت بالا";
                        break;

                    case "pending":
                        groupTitle = "در انتظار انجام";
                        groupIcon = "clock";
                        groupColor = "primary";
                        groupDescription = "عملیات‌های در حال انجام";
                        break;

                    case "completed":
                        groupTitle = "تکمیل شده";
                        groupIcon = "check-circle";
                        groupColor = "success";
                        groupDescription = "عملیات‌های به اتمام رسیده";
                        break;

                    default:
                        // ⭐ اگر نوع نامعتبر بود
                        await _activityLogger.LogErrorAsync(
                            "TaskOperations",
                            "GetOperationsGroup",
                            $"Invalid groupType: {groupType}",
                            new Exception($"TaskId: {taskId}, GroupType: '{groupType}'"));

                        return Json(new
                        {
                            success = false,
                            message = $"نوع گروه نامعتبر است: {groupType}",
                            count = 0
                        });
                }

                // ⭐ اگر گروه خالی است، HTML خالی برگردان
                if (!viewModel.Any())
                {
                    return Json(new
                    {
                        success = true,
                        html = "", // خالی
                        count = 0,
                        groupType = normalizedGroupType
                    });
                }

                // ⭐ Log برای debug
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "TaskOperations",
                    "GetOperationsGroup",
                    $"Fetched {viewModel.Count} operations for group '{normalizedGroupType}'",
                    recordId: taskId.ToString());

                // ⭐ ایجاد dynamic object برای ViewBag
                var viewBagData = new
                {
                    GroupType = normalizedGroupType,
                    GroupTitle = groupTitle,
                    GroupIcon = groupIcon,
                    GroupColor = groupColor,
                    GroupDescription = groupDescription
                };

                // ⭐ رندر Partial View با ViewBag
                var partialView = await this.RenderViewToStringAsync(
                    "_OperationsGroup",
                    viewModel,
                    viewBagData  // ⭐ انتقال ViewBag به عنوان پارامتر سوم
                );

                return Json(new
                {
                    success = true,
                    html = partialView,
                    count = viewModel.Count,
                    groupType = normalizedGroupType
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "GetOperationsGroup",
                    $"خطا در دریافت گروه {groupType} برای تسک {taskId}",
                    ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}",
                    count = 0,
                    groupType = groupType ?? "null"
                });
            }
        }
        #region Work Log Modal & Actions

        /// <summary>
        /// نمایش مودال افزودن گزارش کار (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddWorkLogModal(int operationId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی
                if (!await _operationsRepository.CanUserAccessOperationAsync(operationId, userId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به انجام این عملیات نیستید" } }
                    });
                }

                // دریافت اطلاعات عملیات
                var operation = await _operationsRepository.GetOperationByIdAsync(operationId, includeWorkLogs: false);
                if (operation == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "عملیات یافت نشد" } }
                    });
                }

                // دریافت آخرین WorkLog ها
                var recentWorkLogs = await _operationsRepository.GetOperationWorkLogsAsync(operationId, take: 5);

                var model = new OperationWorkLogViewModel
                {
                    TaskOperationId = operationId,
                    OperationTitle = operation.Title,
                    TaskTitle = operation.Task?.Title ?? "نامشخص",
                    RecentWorkLogs = recentWorkLogs,
                    TotalWorkLogsCount = await _operationsRepository.GetWorkLogsCountAsync(operationId)
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "TaskOperations",
                    "AddWorkLogModal",
                    $"نمایش مودال ثبت کار برای عملیات {operationId}");

                return PartialView("_AddOperationWorkLogModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "AddWorkLogModal",
                    "خطا در نمایش مودال",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بارگذاری مودال" } }
                });
            }
        }

        /// <summary>
        /// ثبت گزارش کار جدید (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddWorkLog(OperationWorkLogViewModel model)
        {
            try
            {
                // ⭐ فقط بررسی فیلد WorkDescription
                if (string.IsNullOrWhiteSpace(model.WorkDescription))
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "error", text = "توضیحات کار انجام شده الزامی است" } }
                    });
                }

                if (model.WorkDescription.Length < 5)
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "error", text = "حداقل 5 کاراکتر برای توضیحات الزامی است" } }
                    });
                }

                var userId = _userManager.GetUserId(User);
                var result = await _operationsRepository.AddWorkLogAsync(model, userId);

                if (result.Success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "TaskOperations",
                        "AddWorkLog",
                        $"ثبت گزارش کار برای عملیات {model.TaskOperationId}",
                        recordId: result.WorkLogId?.ToString(),
                        entityType: "TaskOperationWorkLog");

                    // ⭐ دریافت لیست آپدیت شده WorkLog ها
                    var updatedWorkLogs = await _operationsRepository.GetOperationWorkLogsAsync(model.TaskOperationId, take: 5);

                    // ⭐ رندر Partial View برای لیست جدید
                    var workLogsHtml = await this.RenderViewToStringAsync("_WorkLogsList", updatedWorkLogs);

                    return Json(new
                    {
                        status = "update-view",
                        message = new[] { new { status = "success", text = "گزارش کار با موفقیت ثبت شد" } },
                        viewList = new[]
                        {
                            new
                            {
                                elementId = "workLogsListContainer",
                                view = new { result = workLogsHtml },
                                appendMode = false
                            }
                        },
                        workLogId = result.WorkLogId,
                        totalCount = updatedWorkLogs.Count
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = result.Message } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "AddWorkLog",
                    "خطا در ثبت گزارش کار",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ثبت گزارش کار" } }
                });
            }
        }

        /// <summary>
        /// دریافت لیست WorkLog های یک عملیات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOperationWorkLogs(int operationId, int take = 10)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (!await _operationsRepository.CanUserAccessOperationAsync(operationId, userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما مجاز به مشاهده این اطلاعات نیستید"
                    });
                }

                var workLogs = await _operationsRepository.GetOperationWorkLogsAsync(operationId, take);

                return Json(new
                {
                    success = true,
                    workLogs = workLogs
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "GetOperationWorkLogs",
                    "خطا در دریافت WorkLog ها",
                    ex);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات"
                });
            }
        }

        /// <summary>
        /// حذف WorkLog (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteWorkLog(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _operationsRepository.DeleteWorkLogAsync(id, userId);

                if (result.Success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "TaskOperations",
                        "DeleteWorkLog",
                        $"حذف گزارش کار {id}",
                        recordId: id.ToString());
                }

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "DeleteWorkLog",
                    "خطا در حذف WorkLog",
                    ex,
                    recordId: id.ToString());

                return Json(new
                {
                    success = false,
                    message = "خطا در حذف گزارش کار"
                });
            }
        }

        #endregion

        #region Statistics & Helper Actions

        /// <summary>
        /// دریافت آمار WorkLog های یک تسک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskWorkLogsStats(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی
                if (!_taskRepository.IsUserRelatedToTask(userId, taskId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما مجاز به مشاهده این اطلاعات نیستید"
                    });
                }

                var stats = await _operationsRepository.GetTaskWorkLogsStatsAsync(taskId);

                return Json(new
                {
                    success = true,
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "GetTaskWorkLogsStats",
                    "خطا در دریافت آمار",
                    ex);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت آمار"
                });
            }
        }

        #endregion

        #region Add & Delete Operation Actions

        /// <summary>
        /// افزودن عملیات جدید به تسک (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddOperation(int taskId, string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new
                    {
                        success = false,
                        message = "عنوان عملیات الزامی است"
                    });
                }

                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی کاربر به تسک
                if (!_taskRepository.IsUserRelatedToTask(userId, taskId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما مجاز به انجام این عملیات نیستید"
                    });
                }

                // دریافت تسک
                var task =  _taskRepository.GetTaskById(taskId,true);
                if (task == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "تسک یافت نشد"
                    });
                }

                // تعیین ترتیب جدید
                var maxOrder = task.TaskOperations?.Any() == true
                    ? task.TaskOperations.Max(o => o.OperationOrder)
                    : 0;

                // ایجاد عملیات جدید
                var newOperation = new TaskOperation
                {
                    TaskId = taskId,
                    Title = title.Trim(),
                    OperationOrder = maxOrder + 1,
                    IsCompleted = false,
                    IsStarred = false,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                // ذخیره در دیتابیس
                _operationsRepository.AddTaskOperation(newOperation);
                await _operationsRepository.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "TaskOperations",
                    "AddOperation",
                    $"افزودن عملیات جدید: {title}",
                    recordId: newOperation.Id.ToString(),
                    entityType: "TaskOperation");

                return Json(new
                {
                    success = true,
                    message = "عملیات با موفقیت اضافه شد",
                    operationId = newOperation.Id
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "AddOperation",
                    "خطا در افزودن عملیات",
                    ex);

                return Json(new
                {
                    success = false,
                    message = "خطا در افزودن عملیات"
                });
            }
        }

        /// <summary>
        /// حذف عملیات (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // بررسی دسترسی
                if (!await _operationsRepository.CanUserAccessOperationAsync(id, userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما مجاز به حذف این عملیات نیستید"
                    });
                }

                var operation = await _operationsRepository.GetOperationByIdAsync(id);
                if (operation == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "عملیات یافت نشد"
                    });
                }

                // Soft Delete
                operation.IsDeleted = true;
                operation.DeleteDate = DateTime.Now;

                _context.TaskOperationUW.Update(operation);
                _context.Save();
               

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "TaskOperations",
                    "DeleteOperation",
                    $"حذف عملیات: {operation.Title}",
                    recordId: id.ToString());

                return Json(new
                {
                    success = true,
                    message = "عملیات با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "DeleteOperation",
                    "خطا در حذف عملیات",
                    ex,
                    recordId: id.ToString());

                return Json(new
                {
                    success = false,
                    message = "خطا در حذف عملیات"
                });
            }
        }

        #endregion


        /// <summary>
        /// نمایش مودال لیست WorkLog ها (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewWorkLogsModal(int operationId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (!await _operationsRepository.CanUserAccessOperationAsync(operationId, userId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما مجاز به مشاهده این اطلاعات نیستید" } }
                    });
                }

                var operation = await _operationsRepository.GetOperationByIdAsync(operationId, includeWorkLogs: false);
                if (operation == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "عملیات یافت نشد" } }
                    });
                }

                var workLogs = await _operationsRepository.GetOperationWorkLogsAsync(operationId);

                var model = new OperationWorkLogViewModel
                {
                    TaskOperationId = operationId,
                    OperationTitle = operation.Title,
                    TaskTitle = operation.Task?.Title ?? "نامشخص",
                    RecentWorkLogs = workLogs,
                    TotalWorkLogsCount = workLogs.Count
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "TaskOperations",
                    "ViewWorkLogsModal",
                    $"نمایش لیست WorkLog های عملیات {operationId}");

                return PartialView("_ViewWorkLogsModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskOperations",
                    "ViewWorkLogsModal",
                    "خطا در نمایش مودال لیست",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بارگذاری لیست" } }
                });
            }
        }
    }
}