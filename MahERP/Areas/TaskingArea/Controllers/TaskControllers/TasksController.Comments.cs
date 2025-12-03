using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Comments Management - مدیریت کامنت‌ها و پیام‌های تسک
    /// </summary>
    public partial class TasksController
    {
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

                    return Json(new { success = false, message = errors });
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
                    return Json(new { success = false, message = "شما به این تسک دسترسی ندارید" });
                }

                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new { success = false, message = "این تسک تکمیل شده و امکان ارسال پیام وجود ندارد" });
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

                // ⭐⭐⭐ کپی داده‌های لازم برای Background Task (قبل از DbContext Dispose شدن)
                var taskId = model.TaskId;
                var commentId = comment.Id;
                var taskCode = task?.TaskCode ?? "N/A";
                var taskTitle = task?.Title ?? "N/A";
                var commentPreview = model.CommentText.Substring(0, Math.Min(50, model.CommentText.Length));

                // ⭐⭐⭐ کارهای بعدی را Fire-and-Forget کن (با Scope جدید)
                Task.Run(async () =>
                {
                    try
                    {
                        // ⭐ ایجاد Scope جدید برای DbContext جدید
                        using var scope = _serviceScopeFactory.CreateScope();
                        var scopedHistoryRepo = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();
                        var scopedActivityLogger = scope.ServiceProvider.GetRequiredService<ActivityLoggerService>();

                        // ⭐ ثبت History با DbContext جدید
                        await scopedHistoryRepo.LogCommentAddedAsync(
                            taskId,
                            currentUserId,
                            commentId,
                            commentPreview
                        );

                        // ⭐ ارسال Notification (این یکی مشکلی نداره چون Static است)
                        NotificationProcessingBackgroundService.EnqueueTaskNotification(
                            taskId,
                            currentUserId,
                            NotificationEventType.TaskCommentAdded,
                            priority: 1
                        );

                        // ⭐ ثبت Activity Log با DbContext جدید
                        await scopedActivityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Tasks",
                            "AddTaskComment",
                            $"افزودن کامنت به تسک {taskCode}",
                            recordId: taskId.ToString(),
                            entityType: "Tasks",
                            recordTitle: taskTitle
                        );
                    }
                    catch (Exception bgEx)
                    {
                        // ⭐ لاگ کنسول برای دیباگ
                        Console.WriteLine($"❌ Error in AddTaskComment Background: {bgEx.Message}");
                        Console.WriteLine($"   Stack: {bgEx.StackTrace}");
                    }
                }).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"❌ Background task faulted: {t.Exception?.Flatten().Message}");
                    }
                }, TaskScheduler.Default);

                return Json(new { success = true, message = "پیام با موفقیت ارسال شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddTaskComment", "خطا در افزودن کامنت", ex);
                return Json(new { success = false, message = "خطا در ارسال پیام: " + ex.Message });
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

                var hasAccess = await _taskRepository.CanUserViewTaskAsync(currentUserId, taskId);
                if (!hasAccess)
                {
                    return Json(new { success = false, message = "شما به این تسک دسترسی ندارید" });
                }

                var comments = await _taskRepository.GetTaskCommentsAsync(taskId);

                var html = await this.RenderViewToStringAsync("_TaskCommentsPartial", comments);

                return Json(new { success = true, html = html });
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

                var comment = _uow.TaskCommentUW.GetById(id);
                if (comment == null)
                {
                    return Json(new { success = false, message = "پیام یافت نشد" });
                }

                if (comment.CreatorUserId != currentUserId)
                {
                    return Json(new { success = false, message = "شما فقط می‌توانید پیام‌های خود را حذف کنید" });
                }

                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(currentUserId, comment.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                {
                    return Json(new { success = false, message = "این تسک تکمیل شده و امکان حذف پیام وجود ندارد" });
                }

                // حذف فایل‌های پیوست
                var attachments = _uow.TaskCommentAttachmentUW.Get(a => a.TaskCommentId == id).ToList();

                foreach (var attachment in attachments)
                {
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    _uow.TaskCommentAttachmentUW.Delete(attachment);
                }

                _uow.TaskCommentUW.Delete(comment);
                _uow.Save();

                await _taskHistoryRepository.LogCommentDeletedAsync(comment.TaskId, currentUserId, id);

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

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(attachment.FileName);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "DownloadAttachment",
                    $"دانلود فایل {attachment.FileName} از تسک {attachment.Comment.Task.TaskCode}",
                    recordId: attachment.Comment.TaskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: attachment.Comment.Task.Title);

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

        #endregion
    }
}
