using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;

namespace MahERP.DataModelLayer.Repository.TaskRepository.Tasking
{
    public interface ITaskFilterRepository
    {
        /// <summary>
        /// دریافت تسک‌ها برای صفحه Index با فیلترهای مختلف
        /// </summary>
        Task<TaskIndexViewModel> GetTasksForIndexAsync(string userId, TaskFilterViewModel filters);

        /// <summary>
        /// دریافت همه تسک‌های قابل مشاهده
        /// </summary>
        Task<TaskFilterResultViewModel> GetAllVisibleTasksAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های منتصب شده به کاربر
        /// </summary>
        Task<TaskFilterResultViewModel> GetMyAssignedTasksAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های واگذار شده به دیگران
        /// </summary>
        Task<TaskFilterResultViewModel> GetAssignedByMeTasksAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های تیم‌های کاربر
        /// </summary>
        Task<TaskFilterResultViewModel> GetMyTeamsTasksAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های نظارتی
        /// </summary>
        Task<TaskFilterResultViewModel> GetSupervisedTasksAsync(string userId);

        /// <summary>
        /// دریافت تعداد تسک‌ها برای همه فیلترها
        /// </summary>
        Task<TaskFilterCountsViewModel> GetAllFilterCountsAsync(string userId);
    }
}