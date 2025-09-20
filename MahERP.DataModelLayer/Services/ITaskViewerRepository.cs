using MahERP.DataModelLayer.Entities.TaskManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    public interface ITaskViewerRepository
    {
        /// <summary>
        /// بررسی اینکه آیا کاربر مجوز مشاهده تسک خاصی را دارد
        /// </summary>
        bool CanUserViewTask(string userId, int taskId);

        /// <summary>
        /// دریافت لیست تسک‌هایی که کاربر مجوز مشاهده آن‌ها را دارد
        /// </summary>
        List<int> GetVisibleTaskIds(string userId, bool includeDeleted = false);

        /// <summary>
        /// اضافه کردن مجوز مشاهده تسک به کاربر
        /// </summary>
        Task<int> AddTaskViewerAsync(TaskViewer taskViewer);

        /// <summary>
        /// حذف مجوز مشاهده تسک از کاربر
        /// </summary>
        Task<bool> RemoveTaskViewerAsync(int taskViewerId);

        /// <summary>
        /// دریافت تمام کاربرانی که مجوز مشاهده تسک خاصی را دارند
        /// </summary>
        List<TaskViewer> GetTaskViewers(int taskId, bool activeOnly = true);

        /// <summary>
        /// دریافت تمام مجوزهای مشاهده یک کاربر
        /// </summary>
        List<TaskViewer> GetUserTaskViewers(string userId, bool activeOnly = true);

        /// <summary>
        /// اضافه کردن مجوز خاص برای مشاهده تسک‌های کاربر یا تیم
        /// </summary>
        Task<int> GrantSpecialPermissionAsync(string granteeUserId, string targetUserId = null, int? targetTeamId = null, byte permissionType = 0, string grantedByUserId = null, string description = null);

        /// <summary>
        /// بروزرسانی وضعیت مشاهده تسک توسط کاربر
        /// </summary>
        Task<bool> MarkTaskAsViewedAsync(string userId, int taskId);

        /// <summary>
        /// ایجاد خودکار TaskViewers بر اساس قوانین سیستم
        /// </summary>
        Task GenerateAutomaticViewersForTask(int taskId);

        /// <summary>
        /// پاک‌سازی مجوزهای منقضی شده
        /// </summary>
        Task<int> CleanupExpiredPermissionsAsync();
    }
}