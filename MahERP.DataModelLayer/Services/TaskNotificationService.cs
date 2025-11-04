using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Repository.Tasking;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس مدیریت نوتیفیکیشن‌های مربوط به تسک‌ها
    /// این سرویس مسئول ارسال نوتیفیکیشن‌ها در رخدادهای مختلف تسک‌ها است
    /// </summary>
    public class TaskNotificationService
    {
        private readonly ICoreNotificationRepository _coreNotificationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUsers> _userManager;
        private const byte TASK_SYSTEM_ID = 7; // شناسه سیستم تسک‌ها

        public TaskNotificationService(
            ICoreNotificationRepository coreNotificationRepository,
            ITaskRepository taskRepository,
            AppDbContext context,
            UserManager<AppUsers> userManager)
        {
            _coreNotificationRepository = coreNotificationRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _context = context;
        }

        #region نوتیفیکیشن‌های ایجاد تسک - Task Creation Notifications

        /// <summary>
        /// ارسال نوتیفیکیشن ایجاد تسک جدید به کاربران منصوب
        /// </summary>
        /// <param name="taskId">شناسه تسک ایجاد شده</param>
        /// <param name="creatorUserId">شناسه کاربر ایجاد کننده</param>
        /// <param name="assignedUserIds">لیست کاربران منصوب</param>
        /// <returns>لیست شناسه نوتیفیکیشن‌های ایجاد شده</returns>
        public async Task<List<int>> NotifyTaskCreatedAsync(int taskId, string creatorUserId, List<string> assignedUserIds)
        {
            try
            {
                // دریافت اطلاعات تسک
                var task = _taskRepository.GetTaskById(taskId);
                if (task == null)
                    throw new ArgumentException("تسک یافت نشد");

                // حذف سازنده تسک از لیست (تا به خودش نوتیفikation ارسال نشود)
                var recipientUserIds = new List<string>(assignedUserIds);
                if (recipientUserIds.Contains(creatorUserId))
                {
                    recipientUserIds.Remove(creatorUserId);
                }

                if (!recipientUserIds.Any())
                    return new List<int>(); // هیچ کاربری برای ارسال نوتیفیکیشن وجود ندارد

                // تنظیم اطلاعات نوتیفیکیشن
                var title = "تسک جدید اختصاص داده شد";
                var message = $"تسک جدید '{task.Title}' به شما اختصاص داده شده است";
                var actionUrl = $"/AdminArea/Tasks/Details/{taskId}";

                // ایجاد نوتیفیکیشن برای کاربران
                var notificationIds = await _coreNotificationRepository.CreateBulkNotificationAsync(
                    recipientUserIds,
                    TASK_SYSTEM_ID,
                    1, // نوع: ایجاد رکورد جدید
                    title,
                    message,
                    creatorUserId,
                    actionUrl,
                    taskId.ToString(),
                    "Task",
                    task.Title,
                    1 // اولویت: مهم
                );

                return notificationIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ارسال نوتیفیکیشن ایجاد تسک: {ex.Message}", ex);
            }
        }

        #endregion

        #region نوتیفیکیشن‌های ویرایش تسک - Task Edit Notifications

        /// <summary>
        /// ارسال نوتیفیکیشن ویرایش تسک به کاربران منصوب
        /// </summary>
        /// <param name="taskId">شناسه تسک ویرایش شده</param>
        /// <param name="editorUserId">شناسه کاربر ویرایش کننده</param>
        /// <param name="changeDetails">جزئیات تغییرات انجام شده</param>
        /// <returns>لیست شناسه نوتیفیکیشن‌های ایجاد شده</returns>
        public async Task<List<int>> NotifyTaskEditedAsync(int taskId, string editorUserId, List<TaskChangeDetail> changeDetails)
        {
            try
            {
                // دریافت اطلاعات تسک و کاربران منصوب
                var task = _taskRepository.GetTaskById(taskId, includeAssignments: true);
                if (task == null)
                    throw new ArgumentException("تسک یافت نشد");

                // دریافت لیست کاربران منصوب (به غیر از ویرایش کننده)
                var assignedUserIds = task.TaskAssignments
                    .Where(ta => ta.AssignedUserId != editorUserId)
                    .Select(ta => ta.AssignedUserId)
                    .Distinct()
                    .ToList();

                if (!assignedUserIds.Any())
                    return new List<int>(); // هیچ کاربری برای ارسال نوتیفیکیشن وجود ندارد

                // تنظیم اطلاعات نوتیفیکیشن
                var title = "تسک ویرایش شد";
                var message = $"تسک '{task.Title}' ویرایش شده است";
                var actionUrl = $"/AdminArea/Tasks/Details/{taskId}?highlight={string.Join(",", changeDetails.Select(c => c.FieldName))}";

                // تبدیل جزئیات تغییرات
                var notificationDetails = changeDetails.Select(cd => new CoreNotificationDetailViewModel
                {
                    NotificationTypeSpecific = 1, // ویرایش
                    FieldName = cd.FieldName,
                    OldValue = cd.OldValue,
                    NewValue = cd.NewValue,
                    Description = cd.Description
                }).ToList();

                // ایجاد نوتیفیکیشن تغییر رکورد
                var notificationIds = await _coreNotificationRepository.CreateRecordChangeNotificationAsync(
                    assignedUserIds,
                    TASK_SYSTEM_ID,
                    taskId.ToString(),
                    "Task",
                    task.Title,
                    editorUserId,
                    notificationDetails
                );

                return notificationIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ارسال نوتیفیکیشن ویرایش تسک: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// تشخیص تغییرات بین تسک قدیم و جدید
        /// </summary>
        /// <param name="oldTask">تسک قبل از ویرایش</param>
        /// <param name="newTask">تسک بعد از ویرایش</param>
        /// <returns>لیست تغییرات انجام شده</returns>
        public List<TaskChangeDetail> DetectTaskChanges(object oldTask, object newTask)
        {
            var changes = new List<TaskChangeDetail>();

            try
            {
                // استفاده از Reflection برای مقایسه فیلدها
                var oldTaskType = oldTask.GetType();
                var newTaskType = newTask.GetType();

                // فیلدهای مهم برای ردیابی تغییرات
                var importantFields = new[]
                {
                    "Title", "Description", "DueDate", "Priority", "Status"
                };

                foreach (var fieldName in importantFields)
                {
                    var oldProperty = oldTaskType.GetProperty(fieldName);
                    var newProperty = newTaskType.GetProperty(fieldName);

                    if (oldProperty != null && newProperty != null)
                    {
                        var oldValue = oldProperty.GetValue(oldTask)?.ToString() ?? "";
                        var newValue = newProperty.GetValue(newTask)?.ToString() ?? "";

                        if (oldValue != newValue)
                        {
                            changes.Add(new TaskChangeDetail
                            {
                                FieldName = fieldName,
                                OldValue = oldValue,
                                NewValue = newValue,
                                Description = GetChangeDescription(fieldName, oldValue, newValue)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، تغییر کلی را ثبت می‌کنیم
                changes.Add(new TaskChangeDetail
                {
                    FieldName = "General",
                    OldValue = "",
                    NewValue = "",
                    Description = "تسک ویرایش شده است"
                });
            }

            return changes;
        }

        #endregion

        #region نوتیفیکیشن‌های اختصاص کاربر - User Assignment Notifications

        /// <summary>
        /// ارسال نوتیفیکیشن اختصاص کاربر جدید به تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="newAssignedUserId">شناسه کاربر جدید</param>
        /// <param name="assignerUserId">شناسه کاربر اختصاص دهنده</param>
        /// <returns>شناسه نوتیفیکیشن ایجاد شده</returns>
        public async Task<int> NotifyUserAssignedAsync(int taskId, string newAssignedUserId, string assignerUserId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(taskId);
                if (task == null)
                    throw new ArgumentException("تسک یافت نشد");

                // عدم ارسال نوتیفیکیشن اگر کاربر به خودش اختصاص دهد
                if (newAssignedUserId == assignerUserId)
                    return 0;

                var title = "به تسک جدیدی اختصاص یافتید";
                var message = $"شما به تسک '{task.Title}' اختصاص داده شدید";
                var actionUrl = $"/AdminArea/Tasks/Details/{taskId}";

                var notification = new CoreNotification
                {
                    SystemId = TASK_SYSTEM_ID,
                    SystemName = "سیستم مدیریت پروژه و تسک‌ها",
                    RecipientUserId = newAssignedUserId,
                    SenderUserId = assignerUserId,
                    NotificationTypeGeneral = 9, // اختصاص/انتساب
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    RelatedRecordId = taskId.ToString(),
                    RelatedRecordType = "Task",
                    RelatedRecordTitle = task.Title,
                    Priority = 1 // مهم
                };

                return await _coreNotificationRepository.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ارسال نوتیفیکیشن اختصاص کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ارسال نوتیفیکیشن حذف کاربر از تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="removedUserId">شناسه کاربر حذف شده</param>
        /// <param name="removerUserId">شناسه کاربر حذف کننده</param>
        /// <returns>شناسه نوتیفیکیشن ایجاد شده</returns>
        public async Task<int> NotifyUserRemovedAsync(int taskId, string removedUserId, string removerUserId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(taskId);
                if (task == null)
                    throw new ArgumentException("تسک یافت نشد");

                // عدم ارسال نوتیفیکیشن اگر کاربر خودش را حذف کند
                if (removedUserId == removerUserId)
                    return 0;

                var title = "از تسک حذف شدید";
                var message = $"شما از تسک '{task.Title}' حذف شدید";

                var notification = new CoreNotification
                {
                    SystemId = TASK_SYSTEM_ID,
                    SystemName = "سیستم مدیریت پروژه و تسک‌ها",
                    RecipientUserId = removedUserId,
                    SenderUserId = removerUserId,
                    NotificationTypeGeneral = 9, // اختصاص/انتساب
                    Title = title,
                    Message = message,
                    RelatedRecordId = taskId.ToString(),
                    RelatedRecordType = "Task",
                    RelatedRecordTitle = task.Title,
                    Priority = 0 // عادی
                };

                return await _coreNotificationRepository.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ارسال نوتیفیکیشن حذف کاربر: {ex.Message}", ex);
            }
        }

        #endregion

        #region نوتیفیکیشن‌های تکمیل تسک - Task Completion Notifications

        /// <summary>
        /// ارسال نوتیفیکیشن تکمیل تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="completedByUserId">شناسه کاربر تکمیل کننده</param>
        /// <returns>لیست شناسه نوتیفیکیشن‌های ایجاد شده</returns>
        public async Task<List<int>> NotifyTaskCompletedAsync(int taskId, string completedByUserId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(taskId, includeAssignments: true);
                if (task == null)
                    throw new ArgumentException("تسک یافت نشد");

                // ارسال به تمام کاربران منصوب (به غیر از تکمیل کننده)
                var recipientUserIds = task.TaskAssignments
                    .Where(ta => ta.AssignedUserId != completedByUserId)
                    .Select(ta => ta.AssignedUserId)
                    .Distinct()
                    .ToList();

                if (!recipientUserIds.Any())
                    return new List<int>();

                var title = "تسک تکمیل شد";
                var message = $"تسک '{task.Title}' تکمیل شده است";
                var actionUrl = $"/AdminArea/Tasks/Details/{taskId}";

                var notificationIds = await _coreNotificationRepository.CreateBulkNotificationAsync(
                    recipientUserIds,
                    TASK_SYSTEM_ID,
                    8, // تکمیل فرآیند
                    title,
                    message,
                    completedByUserId,
                    actionUrl,
                    taskId.ToString(),
                    "Task",
                    task.Title,
                    1 // مهم
                );

                return notificationIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ارسال نوتیفیکیشن تکمیل تسک: {ex.Message}", ex);
            }
        }

        #endregion

        #region متدهای کمکی - Helper Methods

        /// <summary>
        /// تولید توضیحات تغییر بر اساس نوع فیلد
        /// </summary>
        private string GetChangeDescription(string fieldName, string oldValue, string newValue)
        {
            return fieldName switch
            {
                "Title" => $"عنوان از '{oldValue}' به '{newValue}' تغییر یافت",
                "Description" => "توضیحات تسک ویرایش شده است",
                "DueDate" => $"تاریخ سررسید از '{oldValue}' به '{newValue}' تغییر یافت",
                "Priority" => $"اولویت از '{GetPriorityName(oldValue)}' به '{GetPriorityName(newValue)}' تغییر یافت",
                "Status" => $"وضعیت از '{GetStatusName(oldValue)}' به '{GetStatusName(newValue)}' تغییر یافت",
                _ => $"فیلد {fieldName} تغییر یافته است"
            };
        }

        /// <summary>
        /// تبدیل شماره اولویت به نام
        /// </summary>
        private string GetPriorityName(string priority)
        {
            return priority switch
            {
                "0" => "عادی",
                "1" => "مهم",
                "2" => "فوری",
                _ => priority
            };
        }

        /// <summary>
        /// تبدیل شماره وضعیت به نام
        /// </summary>
        private string GetStatusName(string status)
        {
            return status switch
            {
                "0" => "ایجاد شده",
                "1" => "در حال انجام",
                "2" => "تکمیل شده",
                "3" => "تأیید شده",
                "4" => "رد شده",
                "5" => "در انتظار",
                _ => status
            };
        }

        #endregion


        #region مدیریت نوتیفیکیشن‌های کاربر - User Notification Management

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن‌های مرتبط با تسک به عنوان خوانده شده
        /// (زمانی که کاربر وارد صفحه جزئیات تسک می‌شود)
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تعداد نوتیفیکیشن‌های علامت‌گذاری شده</returns>
        public async Task<int> MarkTaskNotificationsAsReadAsync(int taskId, string userId)
        {
            try
            {
                return await _coreNotificationRepository.MarkRelatedNotificationsAsReadAsync(
                    userId,
                    TASK_SYSTEM_ID,
                    taskId.ToString()
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در علامت‌گذاری نوتیفیکیشن‌های تسک: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت نوتیفیکیشن‌های تسک برای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="unreadOnly">فقط خوانده نشده‌ها</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد در هر صفحه</param>
        /// <returns>لیست نوتیفیکیشن‌های تسک</returns>
        public async Task<CoreNotificationListViewModel> GetUserTaskNotificationsAsync(
            string userId,
            bool unreadOnly = false,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                return await _coreNotificationRepository.GetUserNotificationsAsync(
                    userId,
                    TASK_SYSTEM_ID,
                    unreadOnly,
                    pageNumber,
                    pageSize
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت نوتیفیکیشن‌های تسک کاربر: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده تسک برای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تعداد نوتیفیکیشن‌های خوانده نشده</returns>
        public async Task<int> GetUserUnreadTaskNotificationCountAsync(string userId)
        {
            try
            {
                return await _coreNotificationRepository.GetUnreadNotificationCountAsync(userId, TASK_SYSTEM_ID);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تعداد نوتیفیکیشن‌های خوانده نشده: {ex.Message}", ex);
            }
        }

        #endregion



        /// <summary>
        /// ارسال نوتیفیکیشن برای کامنت/پیام جدید در تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="senderUserId">شناسه کاربر ارسال‌کننده کامنت</param>
        /// <param name="commentId">شناسه کامنت</param>
        public async Task NotifyNewCommentAsync(int taskId, string senderUserId, int commentId)
        {
            try
            {
                // دریافت اطلاعات تسک
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    Console.WriteLine($"⚠️ Task {taskId} not found for notification");
                    return;
                }

                // دریافت اطلاعات کاربر فرستنده
                var sender = await _userManager.FindByIdAsync(senderUserId);
                var senderName = sender != null
                    ? $"{sender.FirstName} {sender.LastName}"
                    : "کاربر";

                // ⭐ استفاده از متد موجود GetTaskAssignments
                var taskAssignments = _taskRepository.GetTaskAssignments(taskId);

                // استخراج UserId های یکتا (به جز فرستنده)
                var recipientUserIds = taskAssignments
                    .Where(a => a.AssignedUserId != senderUserId)
                    .Select(a => a.AssignedUserId)
                    .Distinct()
                    .ToList();

                if (!recipientUserIds.Any())
                {
                    Console.WriteLine($"ℹ️ No recipients found for task {taskId} comment notification");
                    return;
                }

                // ایجاد نوتیفیکیشن برای هر کاربر
                var notifications = new List<CoreNotification>();

                foreach (var recipientId in recipientUserIds)
                {
                    var notification = new CoreNotification
                    {
                        // ⭐ SystemId = 7 برای سیستم مدیریت پروژه و تسک‌ها
                        SystemId = 7,
                        SystemName = "مدیریت تسک‌ها",

                        // ⭐⭐⭐ اصلاح نام پراپرتی‌ها
                        RecipientUserId = recipientId,
                        SenderUserId = senderUserId,

                        // ⭐ NotificationTypeGeneral
                        // 0 = عمومی، 1 = ایجاد، 2 = ویرایش، 3 = حذف، 4 = تایید/رد
                        // 5 = هشدار، 6 = یادآوری، 7 = خطا، 8 = تکمیل، 9 = اختصاص، 10 = تغییر وضعیت
                        NotificationTypeGeneral = 1, // ایجاد رکورد جدید (کامنت جدید)

                        Title = "پیام جدید در تسک",
                        Message = $"{senderName} در تسک \"{task.Title}\" پیام جدیدی ارسال کرد",

                        IsRead = false,
                        CreateDate = DateTime.Now,

                        // ⭐ Priority: 0 = عادی، 1 = مهم، 2 = فوری، 3 = بحرانی
                        Priority = 0,

                        ActionUrl = $"/TaskingArea/Tasks/Details/{taskId}#chat-tab",

                        // ⭐⭐⭐ استفاده از نام‌های صحیح
                        RelatedRecordType = "TaskComment",
                        RelatedRecordId = commentId.ToString(),
                        RelatedRecordTitle = $"کامنت در تسک {task.TaskCode}",

                        IsActive = true,
                        BranchId = task.BranchId
                    };

                    notifications.Add(notification);
                }

                // ⭐ ذخیره نوتیفیکیشن‌ها
                foreach (var notification in notifications)
                {
                    await _context.CoreNotification_Tbl.AddAsync(notification);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Sent {notifications.Count} comment notifications for task {taskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in NotifyNewCommentAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // عدم throw برای جلوگیری از مختل شدن فرآیند اصلی
            }
        }
    }
        /// <summary>
        /// کلاس کمکی برای نگهداری اطلاعات تغییرات تسک
        /// </summary>
        public class TaskChangeDetail
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Description { get; set; }
    }
}