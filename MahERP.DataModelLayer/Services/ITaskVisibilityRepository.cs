using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// رابط repository مدیریت قدرت مشاهده تسک‌ها بر اساس ساختار سازمانی
    /// </summary>
    public interface ITaskVisibilityRepository
    {
        #region Core Visibility Logic

        /// <summary>
        /// بررسی اینکه آیا کاربر می‌تواند تسک خاصی را مشاهده کند
        /// </summary>
        Task<bool> CanUserViewTaskAsync(string userId, int taskId);

        /// <summary>
        /// دریافت لیست شناسه تسک‌هایی که کاربر می‌تواند مشاهده کند
        /// </summary>
        Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId = null, int? teamId = null);

        #endregion

        #region Position-Based Visibility

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده بر اساس سمت
        /// </summary>
        Task<List<int>> GetPositionBasedVisibleTasksAsync(string userId, int? branchId = null, int? teamId = null);

        /// <summary>
        /// بررسی قابلیت مشاهده بر اساس سمت در تیم
        /// </summary>
        Task<bool> CanViewBasedOnPositionAsync(string userId, Tasks task);

        #endregion

        #region Team Management Visibility

        /// <summary>
        /// بررسی اینکه آیا کاربر مدیر تیم است
        /// </summary>
        Task<bool> IsUserTeamManagerAsync(string userId, int teamId);

        /// <summary>
        /// دریافت تسک‌های تیم‌های تحت مدیریت
        /// </summary>
        Task<List<int>> GetManagedTeamTasksAsync(string userId, int? branchId = null);

        /// <summary>
        /// دریافت تمام شناسه زیرتیم‌ها
        /// </summary>
        Task<List<int>> GetAllSubTeamIdsAsync(int parentTeamId);

        #endregion

        #region Special Permissions

        /// <summary>
        /// بررسی مجوزهای خاص (تبصره‌ها)
        /// </summary>
        Task<bool> HasSpecialPermissionAsync(string userId, Tasks task);

        /// <summary>
        /// دریافت تسک‌های با مجوز خاص
        /// </summary>
        Task<List<int>> GetSpecialPermissionTasksAsync(string userId);

        #endregion

        #region Chart Generation

        /// <summary>
        /// ایجاد چارت قدرت مشاهده تسک‌ها
        /// </summary>
        Task<TaskVisibilityChartViewModel> GenerateVisibilityChartAsync(int branchId);

        /// <summary>
        /// محاسبه آمار چارت قدرت مشاهده
        /// </summary>
        Task<TaskVisibilityStatsViewModel> CalculateVisibilityStatsAsync(int branchId);

        #endregion

        #region User Access Information

        /// <summary>
        /// تشخیص منابع دسترسی کاربر
        /// </summary>
        Task<List<string>> GetUserAccessSourcesAsync(string userId);

        /// <summary>
        /// دریافت اطلاعات قدرت مشاهده اعضای یک سمت
        /// </summary>
        Task<List<MemberTaskVisibilityInfo>> GetPositionMembersAsync(int positionId);

        /// <summary>
        /// دریافت اعضای بدون سمت
        /// </summary>
        Task<List<MemberTaskVisibilityInfo>> GetMembersWithoutPositionAsync(int teamId);

        #endregion

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت گروه‌بندی شده
        /// </summary>
        Task<SubTeamTasksGroupedViewModel> GetSubTeamTasksGroupedDetailedAsync(string userId, int? branchId = null);

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت Dictionary ساده
        /// </summary>
        Task<Dictionary<int, List<int>>> GetSubTeamTasksGroupedAsync(string userId, int? branchId = null);
    }
}