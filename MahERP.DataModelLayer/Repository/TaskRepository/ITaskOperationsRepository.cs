using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Interface برای مدیریت عملیات تسک و گزارش کارهای انجام شده
    /// </summary>
    public interface ITaskOperationsRepository
    {
        #region Task Operation CRUD

        /// <summary>
        /// دریافت عملیات بر اساس شناسه
        /// </summary>
        Task<TaskOperation?> GetOperationByIdAsync(int id, bool includeWorkLogs = false);

        /// <summary>
        /// دریافت عملیات بر اساس شناسه (نسخه Sync)
        /// </summary>
        TaskOperation? GetOperationById(int id);

        /// <summary>
        /// دریافت تمام عملیات یک تسک
        /// </summary>
        Task<List<TaskOperation>> GetTaskOperationsAsync(int taskId, bool includeCompleted = true);

        /// <summary>
        /// بررسی دسترسی کاربر به عملیات
        /// </summary>
        Task<bool> CanUserAccessOperationAsync(int operationId, string userId);

        #endregion

        #region Toggle Actions (Star & Complete)

        /// <summary>
        /// تغییر وضعیت ستاره عملیات
        /// </summary>
        Task<(bool Success, string Message)> ToggleOperationStarAsync(int operationId, string userId);

        /// <summary>
        /// تغییر وضعیت تکمیل عملیات
        /// </summary>
        Task<(bool Success, string Message)> ToggleOperationCompleteAsync(
            int operationId, 
            string userId, 
            string? completionNote = null,
            bool addWorkLog = false,
            string? workDescription = null,
            int? durationMinutes = null);

        #endregion

        #region Work Log Management

        /// <summary>
        /// افزودن گزارش کار جدید
        /// </summary>
        Task<(bool Success, string Message, int? WorkLogId)> AddWorkLogAsync(OperationWorkLogViewModel model, string userId);

        /// <summary>
        /// دریافت تمام WorkLog های یک عملیات
        /// </summary>
        Task<List<OperationWorkLogViewModel>> GetOperationWorkLogsAsync(int operationId, int take = 0);

        /// <summary>
        /// دریافت آخرین WorkLog های یک تسک (برای تاریخچه)
        /// </summary>
        Task<List<OperationWorkLogSummaryViewModel>> GetTaskWorkLogsSummaryAsync(int taskId, int take = 10);

        /// <summary>
        /// حذف WorkLog
        /// </summary>
        Task<(bool Success, string Message)> DeleteWorkLogAsync(int workLogId, string userId);

        /// <summary>
        /// دریافت تعداد WorkLog های یک عملیات
        /// </summary>
        Task<int> GetWorkLogsCountAsync(int operationId);

        #endregion

        #region Statistics & Helper Methods

        /// <summary>
        /// محاسبه درصد پیشرفت عملیات بر اساس WorkLog ها
        /// </summary>
        Task<int> CalculateOperationProgressAsync(int operationId);

        /// <summary>
        /// دریافت مجموع زمان صرف شده روی یک عملیات
        /// </summary>
        Task<int> GetTotalDurationMinutesAsync(int operationId);

        /// <summary>
        /// دریافت آمار کلی WorkLog های یک تسک
        /// </summary>
        Task<TaskWorkLogsStatsViewModel> GetTaskWorkLogsStatsAsync(int taskId);
        /// <summary>
        /// افزودن عملیات جدید به تسک
        /// </summary>
        void AddTaskOperation(TaskOperation operation);



        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        Task<int> SaveChangesAsync();
        #endregion


    }

    /// <summary>
    /// ViewModel برای آمار WorkLog های تسک
    /// </summary>
    public class TaskWorkLogsStatsViewModel
    {
        public int TotalWorkLogs { get; set; }
        public int TotalOperationsWithWorkLogs { get; set; }
        public int TotalDurationMinutes { get; set; }
        public DateTime? LastWorkDate { get; set; }
        public string LastWorkDatePersian { get; set; }
        public int ActiveWorkersCount { get; set; }
    }
}