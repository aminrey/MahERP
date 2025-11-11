using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository برای مدیریت رونوشت تسک (ناظران دستی)
    /// </summary>
    public interface ITaskCarbonCopyRepository
    {
        /// <summary>
        /// افزودن رونوشت جدید
        /// </summary>
        Task<TaskCarbonCopy> AddCarbonCopyAsync(int taskId, string userId, string addedByUserId, string note = null);

        /// <summary>
        /// حذف رونوشت
        /// </summary>
        Task<bool> RemoveCarbonCopyAsync(int carbonCopyId, string requestingUserId);

        /// <summary>
        /// دریافت تمام رونوشت‌های یک تسک
        /// </summary>
        Task<List<TaskCarbonCopy>> GetTaskCarbonCopiesAsync(int taskId);

        /// <summary>
        /// بررسی اینکه کاربر رونوشت شده است یا خیر
        /// </summary>
        Task<bool> IsUserCarbonCopyAsync(string userId, int taskId);

        /// <summary>
        /// دریافت تسک‌های رونوشت شده به کاربر
        /// </summary>
        Task<List<int>> GetUserCarbonCopyTaskIdsAsync(string userId);

        /// <summary>
        /// بررسی مجوز حذف رونوشت
        /// </summary>
        Task<bool> CanRemoveCarbonCopyAsync(int carbonCopyId, string userId);
    }
}
