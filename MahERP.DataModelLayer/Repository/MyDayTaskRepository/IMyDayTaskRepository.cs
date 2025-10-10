using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.MyDayTaskRepository
{
    /// <summary>
    /// رابط مخزن مدیریت تسک‌های "روز من"
    /// </summary>
    public interface IMyDayTaskRepository
    {
        /// <summary>
        /// دریافت تمام تسک‌های "روز من" برای یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startDate">تاریخ شروع (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان (اختیاری)</param>
        /// <returns>لیست تسک‌های روز من</returns>
        Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// دریافت تسک‌های یک روز خاص
        /// </summary>
        Task<List<MyDayTaskItemViewModel>> GetTasksForDateAsync(string userId, DateTime date);

        /// <summary>
        /// افزودن تسک به "روز من"
        /// </summary>
        /// <param name="taskAssignmentId">شناسه TaskAssignment</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="plannedDate">تاریخ برنامه‌ریزی</param>
        /// <param name="planNote">یادداشت برنامه‌ریزی (اختیاری)</param>
        /// <returns>موفقیت عملیات</returns>
        Task<(bool Success, string Message, int? MyDayId)> AddTaskToMyDayAsync(
            int taskAssignmentId, 
            string userId, 
            DateTime plannedDate, 
            string? planNote = null);

        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        Task<(bool Success, string Message)> RemoveTaskFromMyDayAsync(int myDayId, string userId);

        /// <summary>
        /// ثبت شروع کار روی تسک
        /// </summary>
        Task<(bool Success, string Message)> StartWorkOnTaskAsync(int myDayId, string userId);

        /// <summary>
        /// ثبت گزارش کار انجام شده
        /// </summary>
        Task<(bool Success, string Message)> LogWorkAsync(
            int myDayId, 
            string userId, 
            string workNote, 
            int? durationMinutes = null);

        /// <summary>
        /// بررسی وجود تسک در "روز من" برای یک تاریخ خاص
        /// </summary>
        Task<bool> IsTaskInMyDayAsync(int taskAssignmentId, string userId, DateTime date);

        /// <summary>
        /// دریافت آمار "روز من"
        /// </summary>
        Task<MyDayStatsViewModel> GetMyDayStatsAsync(string userId, DateTime? date = null);

        /// <summary>
        /// تنظیم تسک به عنوان "متمرکز"
        /// فقط یک تسک می‌تواند در یک زمان متمرکز باشد
        /// </summary>
        Task<(bool Success, string Message)> SetTaskAsFocusedAsync(int myDayId, string userId);

        /// <summary>
        /// حذف تمرکز از تسک
        /// </summary>
        Task<(bool Success, string Message)> RemoveFocusFromTaskAsync(int myDayId, string userId);

        /// <summary>
        /// دریافت تسک متمرکز فعلی کاربر
        /// </summary>
        Task<MyDayTaskItemViewModel?> GetFocusedTaskAsync(string userId);

        /// <summary>
        /// دریافت لیست گزارش کارهای یک تسک
        /// </summary>
        Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId);

        /// <summary>
        /// دریافت اطلاعات تسک در "روز من" برای مودال حذف
        /// </summary>
        Task<(string TaskTitle, string TaskCode)?> GetMyDayTaskInfoForRemovalAsync(int myDayId);
    }
}