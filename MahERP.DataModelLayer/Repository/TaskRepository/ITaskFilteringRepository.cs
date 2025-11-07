using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository مسئول فیلتر کردن تسک‌ها
    /// </summary>
    public interface ITaskFilteringRepository
    {
        /// <summary>
        /// دریافت تسک‌های من (به من assign شده)
        /// </summary>
        Task<List<Tasks>> GetMyTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط من
        /// </summary>
        Task<List<Tasks>> GetAssignedByMeTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// دریافت تسک‌های نظارتی
        /// </summary>
        Task<List<Tasks>> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// دریافت همه تسک‌های قابل مشاهده
        /// </summary>
        Task<List<Tasks>> GetAllVisibleTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// دریافت تسک‌های منتصب شده به من (که خودم نساخته‌ام)
        /// </summary>
        Task<List<Tasks>> GetAssignedToMeTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// دریافت تسک‌های تیمی
        /// </summary>
        Task<List<Tasks>> GetTeamTasksAsync(string userId, TaskFilterViewModel filters = null);

        /// <summary>
        /// اعمال فیلترها روی لیست تسک‌ها
        /// </summary>
        List<Tasks> ApplyFilters(List<Tasks> tasks, TaskFilterViewModel filters);

        /// <summary>
        /// محاسبه آمار لیست
        /// </summary>
        TaskListStatsViewModel CalculateStats(List<Tasks> tasks, string userId);
    }
}