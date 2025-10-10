using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Interface Repository برای مدیریت تاریخچه تسک‌ها
    /// </summary>
    public interface ITaskHistoryRepository
    {
        #region Core Methods

        /// <summary>
        /// ثبت تغییر در تاریخچه
        /// </summary>
        Task<int> LogHistoryAsync(
            int taskId,
            string userId,
            TaskHistoryActionType actionType,
            string title,
            string description = null,
            int? relatedItemId = null,
            string relatedItemType = null,
            object oldValue = null,
            object newValue = null);

        /// <summary>
        /// دریافت تاریخچه کامل تسک
        /// </summary>
        Task<List<TaskHistoryViewModel>> GetTaskHistoryAsync(int taskId);

        /// <summary>
        /// دریافت آخرین تغییرات تسک
        /// </summary>
        Task<List<TaskHistoryViewModel>> GetRecentTaskHistoryAsync(int taskId, int take = 10);

        /// <summary>
        /// حذف تاریخچه تسک (Soft Delete)
        /// </summary>
        Task<bool> DeleteTaskHistoryAsync(int taskId);

        #endregion

        #region Specific History Methods

        /// <summary>
        /// ثبت ایجاد تسک
        /// </summary>
        Task LogTaskCreatedAsync(int taskId, string userId, string taskTitle, string taskCode);

        /// <summary>
        /// ثبت تکمیل تسک
        /// </summary>
        Task LogTaskCompletedAsync(int taskId, string userId, string taskTitle);

        /// <summary>
        /// ثبت ویرایش تسک
        /// </summary>
        Task LogTaskEditedAsync(int taskId, string userId, string taskTitle, object oldValues, object newValues);

        /// <summary>
        /// ثبت افزودن عملیات
        /// </summary>
        Task LogOperationAddedAsync(int taskId, string userId, int operationId, string operationTitle);

        /// <summary>
        /// ثبت ویرایش عملیات
        /// </summary>
        Task LogOperationEditedAsync(int taskId, string userId, int operationId, string operationTitle);

        /// <summary>
        /// ثبت تکمیل عملیات
        /// </summary>
        Task LogOperationCompletedAsync(int taskId, string userId, int operationId, string operationTitle);

        /// <summary>
        /// ثبت حذف عملیات
        /// </summary>
        Task LogOperationDeletedAsync(int taskId, string userId, int operationId, string operationTitle);

        /// <summary>
        /// ثبت گزارش کار روی عملیات
        /// </summary>
        Task LogWorkLogAddedAsync(
            int taskId,
            string userId,
            int operationId,
            string operationTitle,
            int workLogId,
            string workDescription,
            int? durationMinutes = null);

        /// <summary>
        /// ثبت افزودن یادآوری
        /// </summary>
        Task LogReminderAddedAsync(
            int taskId,
            string userId,
            int reminderId,
            string reminderTitle,
            byte reminderType);

        /// <summary>
        /// ثبت حذف یادآوری
        /// </summary>
        Task LogReminderDeletedAsync(int taskId, string userId, int reminderId, string reminderTitle);

        /// <summary>
        /// ثبت تایید سرپرست
        /// </summary>
        Task LogSupervisorApprovedAsync(int taskId, string userId, string taskTitle);

        /// <summary>
        /// ثبت تایید مدیر
        /// </summary>
        Task LogManagerApprovedAsync(int taskId, string userId, string taskTitle);

        /// <summary>
        /// ثبت رد تسک
        /// </summary>
        Task LogTaskRejectedAsync(int taskId, string userId, string taskTitle, string reason);

        /// <summary>
        /// ثبت افزودن کاربر به تسک
        /// </summary>
        Task LogUserAssignedAsync(int taskId, string userId, string assignedUserName);

        /// <summary>
        /// ثبت حذف کاربر از تسک
        /// </summary>
        Task LogUserRemovedAsync(int taskId, string userId, string removedUserName);

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت آیکون برای نوع تغییر
        /// </summary>
        string GetHistoryIcon(byte actionType);

        /// <summary>
        /// دریافت رنگ Badge برای نوع تغییر
        /// </summary>
        string GetHistoryBadgeClass(byte actionType);

        /// <summary>
        /// دریافت متن نوع تغییر
        /// </summary>
        string GetActionTypeText(byte actionType);
        /// <summary>
        /// ثبت افزودن تسک به "روز من"
        /// </summary>
        Task LogTaskAddedToMyDayAsync(int taskId, string userId, string taskTitle, string taskCode, DateTime plannedDate);

        /// <summary>
        /// ثبت حذف تسک از "روز من"
        /// </summary>
        Task LogTaskRemovedFromMyDayAsync(int taskId, string userId, string taskTitle, string taskCode);
        #endregion
    }
}