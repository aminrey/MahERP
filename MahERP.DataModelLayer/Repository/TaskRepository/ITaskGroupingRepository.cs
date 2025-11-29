using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository مسئول گروه‌بندی تسک‌ها
    /// </summary>
    public interface ITaskGroupingRepository
    {
        /// <summary>
        /// گروه‌بندی تسک‌ها بر اساس نوع انتخابی
        /// </summary>
        Task<List<TaskGroupViewModel>> GroupTasksAsync(
            List<Tasks> tasks,
            TaskGroupingType grouping,
            string currentUserId,
            TaskViewType? viewType = null);  // ⭐ اضافه شده

        /// <summary>
        /// گروه‌بندی بر اساس تیم
        /// </summary>
        Task<List<TaskGroupViewModel>> GroupByTeamAsync(List<Tasks> tasks, string userId);

        /// <summary>
        /// گروه‌بندی بر اساس سازنده
        /// </summary>
        List<TaskGroupViewModel> GroupByCreator(List<Tasks> tasks);

        /// <summary>
        /// گروه‌بندی بر اساس زمان ساخت
        /// </summary>
        List<TaskGroupViewModel> GroupByCreateDate(List<Tasks> tasks);

        /// <summary>
        /// گروه‌بندی بر اساس زمان پایان
        /// </summary>
        List<TaskGroupViewModel> GroupByDueDate(List<Tasks> tasks);

        /// <summary>
        /// گروه‌بندی بر اساس اولویت
        /// </summary>
        List<TaskGroupViewModel> GroupByPriority(List<Tasks> tasks);

        /// <summary>
        /// ⭐⭐⭐ گروه‌بندی بر اساس طرف حساب (Contact/Organization)
        /// </summary>
        List<TaskGroupViewModel> GroupByStakeholder(List<Tasks> tasks);

        /// <summary>
        /// ⭐⭐⭐ گروه‌بندی بر اساس اعضای منتصب شده (Assigned Users)
        /// </summary>
        List<TaskGroupViewModel> GroupByAssignedUser(List<Tasks> tasks);

        /// <summary>
        /// بررسی تعلق تسک به گروه
        /// </summary>
        bool IsTaskInGroup(Tasks task, string groupKey, TaskGroupingType grouping, string currentUserId = null);

        /// <summary>
        /// تبدیل Task به TaskCard
        /// </summary>
        TaskCardViewModel MapToTaskCard(Tasks task, int cardNumber, string currentUserId, TaskViewType? viewType = null);  // ⭐ اضافه شده
    }
}