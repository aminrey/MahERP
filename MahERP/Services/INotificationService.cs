using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;

namespace MahERP.Services
{
    public interface INotificationService
    {
        Task CreateTaskNotificationAsync(string userId, List<TaskViewModel> newTasks);
        Task CreateReminderNotificationAsync(string userId, List<TaskReminderItemViewModel> newReminders);
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 10);
        Task<int> GetUnreadNotificationsCountAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId, string userId);
        Task MarkAllNotificationsAsReadAsync(string userId);
    }
}