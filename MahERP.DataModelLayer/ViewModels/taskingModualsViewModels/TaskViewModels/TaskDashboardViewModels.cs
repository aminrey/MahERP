using MahERP.DataModelLayer.ViewModels.Core;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    public class TaskDashboardViewModel
    {
        public UserTaskStatsViewModel UserStats { get; set; } = new();
        public List<TaskSummaryViewModel> UrgentTasks { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
        public int CompletedThisWeek { get; set; }
        public int InProgressThisWeek { get; set; }
    }

    public class UserTaskStatsViewModel
    {
        public int MyTasksCount { get; set; }
        public int AssignedByMeCount { get; set; }
        public int SupervisedTasksCount { get; set; }
        public int TodayTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }
        public int ThisWeekTasksCount { get; set; }
        public int RemindersCount { get; set; }
    }

    public class TasksListViewModel
    {
        public List<TaskViewModel> Tasks { get; set; } = new();
        public TasksListStatsViewModel Stats { get; set; } = new();
        public TaskFilterViewModel Filters { get; set; } = new();
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class TasksListStatsViewModel
    {
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public int NeedsAttentionCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public int InProgressCount { get; set; }
        public int RequiresApprovalCount { get; set; }
        public int DelayedCount { get; set; }
    }

    public class TaskRemindersViewModel
    {
        public List<TaskReminderItemViewModel> Reminders { get; set; } = new();
        public TaskRemindersStatsViewModel Stats { get; set; } = new();
        public TaskReminderFilterViewModel Filters { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class TaskRemindersStatsViewModel
    {
        public int PendingCount { get; set; }
        public int SentCount { get; set; }
        public int OverdueCount { get; set; }
        public int TodayCount { get; set; }
    }

    public class TaskReminderItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskCode { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public string ScheduledDatePersian { get; set; }
        public bool IsSent { get; set; }
        public bool IsRead { get; set; }
        public byte Priority { get; set; }
        public string EventTypeIcon { get; set; }
        public string EventTypeColor { get; set; }
    }

    public class TaskReminderFilterViewModel
    {
        public string FilterType { get; set; } = "all"; 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}