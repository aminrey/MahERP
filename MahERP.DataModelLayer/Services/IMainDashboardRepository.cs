using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using System.Threading.Tasks;
using static MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels.TaskReminderFilterViewModel;

namespace MahERP.DataModelLayer.Services
{
    public interface IMainDashboardRepository
    {
        /// <summary>
        /// آماده‌سازی مدل داشبورد برای کاربر
        /// </summary>
        Task<DashboardViewModel> PrepareDashboardModelAsync(string userId, string userName);

        /// <summary>
        /// دریافت آمار کلی کاربر
        /// </summary>
        Task<DashboardStatsViewModel> GetUserDashboardStatsAsync(string userId);

        /// <summary>
        /// محاسبه آمار تسک‌ها
        /// </summary>
        Task<TasksStatsViewModel> CalculateTaskStatsAsync(string userId);

        /// <summary>
        /// محاسبه آمار قراردادها
        /// </summary>
        Task<ContractsStatsViewModel> CalculateContractStatsAsync(string userId);

        /// <summary>
        /// محاسبه آمار طرف حساب‌ها
        /// </summary>
        Task<StakeholdersStatsViewModel> CalculateStakeholderStatsAsync(string userId);

        /// <summary>
        /// دریافت آخرین فعالیت‌های کاربر
        /// </summary>
        Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(string userId);

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک
        /// </summary>
        Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(string userId, int take = 10);

        /// <summary>
        /// محاسبه تعداد یادآوری‌های فعال
        /// </summary>
        Task<int> CalculateActiveRemindersCountAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های فوری
        /// </summary>
        Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5);

        /// <summary>
        /// محاسبه زمان گذشته از فعالیت
        /// </summary>
        string CalculateTimeAgo(DateTime activityDate);

        /// <summary>
        /// دریافت متن وضعیت تسک
        /// </summary>
        string GetTaskStatusText(byte status);

        /// <summary>
        /// دریافت کلاس badge برای وضعیت تسک
        /// </summary>
        string GetTaskStatusBadgeClass(byte status);

        Task<List<RecentTaskViewModel>> GetRecentReceivedTasksAsync(string userId, int take = 5);
        Task<List<RecentAssignedTaskViewModel>> GetRecentAssignedTasksAsync(string userId, int take = 5);
    }
}
