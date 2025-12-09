using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using MahERP.DataModelLayer.Extensions;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// CRUD Operations - ایجاد، خواندن، بروزرسانی، حذف تسک‌ها
    /// </summary>
    public partial class TasksController 
    {
        #region Create

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
        /// POST: ثبت تسک جدید
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

        #region Read

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

                // ⭐⭐⭐ اضافه کردن نام Contact و Organization به ViewModel
                if (task.ContactId.HasValue && task.Contact != null)
                {
                    viewModel.ContactFullName = $"{task.Contact.FirstName} {task.Contact.LastName}";
                }

                if (task.OrganizationId.HasValue && task.Organization != null)
                {
                    viewModel.OrganizationName = task.Organization.DisplayName;
                }

                // ⭐⭐⭐ اضافه کردن فایل‌های پیوست
                viewModel.ExistingAttachments = task.TaskAttachments?.ToList();
                if (viewModel.ExistingAttachments == null)
                {
                    viewModel.ExistingAttachments = new List<MahERP.DataModelLayer.Entities.TaskManagement.TaskAttachment>();
                }

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

                // ⭐⭐⭐ تنظیم URL بازگشت
                SetBackUrlInViewBag("Index", "Tasks", "TaskingArea");

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
        /// دانلود فایل پیوست تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadTaskAttachment(int id)
        {
            try
            {
                // دریافت اطلاعات Attachment
                var attachment = _uow.TaskAttachmentUW.GetById(id);

                if (attachment == null)
                {
                    return NotFound();
                }

                // بررسی دسترسی کاربر
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var hasAccess = await _taskRepository.CanUserViewTaskAsync(currentUserId, attachment.TaskId);

                if (!hasAccess)
                {
                    return Forbid();
                }

                // بررسی وجود فایل
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                // خواندن فایل
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(attachment.FileName);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "DownloadTaskAttachment",
                    $"دانلود فایل {attachment.FileName}",
                    recordId: attachment.TaskId.ToString());

                return File(fileBytes, contentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DownloadTaskAttachment", $"خطا: {ex.Message}", ex);
                return StatusCode(500);
            }
        }

    

        #endregion

        #region Complete

        /// <summary>
        /// نمایش مودال تکمیل تسک
        /// </summary>
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
                model.TaskId = id;

                return PartialView("_CompleteTask", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "CompleteTask", "خطا در نمایش فرم تکمیل", ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// ثبت تکمیل تسک
        /// </summary>
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
                        updatedTask.CardNumber = model.rowNum;  // حفظ شماره ردیف
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

        #endregion
    }
}
