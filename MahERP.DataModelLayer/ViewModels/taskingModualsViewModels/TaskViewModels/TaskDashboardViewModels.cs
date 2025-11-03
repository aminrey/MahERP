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
        public int UnreadCount { get; set; }          // ⭐ خوانده نشده‌ها
        public int UpcomingCount { get; set; }        // آینده نزدیک
        public int TotalCount { get; set; }           // کل
        public int ReadCount { get; set; }            // ⭐ خوانده شده‌ها
        public int ActiveCount { get; set; }          // تعداد فعال
    }

    /// <summary>
    /// ViewModel برای نمایش یک یادآوری در لیست
    /// </summary>
    public class TaskReminderItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Message { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? TaskCode { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public string ScheduledDatePersian { get; set; }
        public bool IsSent { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDateTime { get; set; }
        public byte Priority { get; set; }
        public byte EventType { get; set; }
        public string NotificationChannel { get; set; }

        /// <summary>
        /// آیکون نوع رویداد
        /// </summary>
        public string EventTypeIcon => EventType switch
        {
            0 => "fa fa-clock",
            1 => "fa fa-repeat",
            2 => "fa fa-exclamation-triangle",
            3 => "fa fa-play",
            4 => "fa fa-flag-checkered",
            5 => "fa fa-user",
            _ => "fa fa-bell"
        };

        /// <summary>
        /// رنگ نوع رویداد
        /// </summary>
        public string EventTypeColor => EventType switch
        {
            0 => "primary",
            1 => "info",
            2 => "warning",
            3 => "success",
            4 => "danger",
            5 => "secondary",
            _ => "muted"
        };

        /// <summary>
        /// وضعیت یادآوری برای نمایش
        /// </summary>
        public string StatusText => (IsSent, IsRead) switch
        {
            (false, _) => "در انتظار ارسال",
            (true, false) => "ارسال شده - خوانده نشده",
            (true, true) => "خوانده شده"
            // ⭐ حذف pattern اضافی: _ => "نامشخص" چون تمام حالات bool پوشش داده شده
        };
    }
    public class TaskReminderFilterViewModel
    {
        // فیلترهای اصلی
        public string FilterType { get; set; } = "all"; 
        
        // فیلترهای جدید برای داشبورد
        public bool IncludeOverdueReminders { get; set; } = false;     // یادآوری‌های عقب افتاده
        public bool IncludeUpcomingReminders { get; set; } = false;    // یادآوری‌های آینده نزدیک
        public bool IncludeUnreadSent { get; set; } = false;           // ارسال شده ولی خوانده نشده
        public bool IncludeTodayReminders { get; set; } = false;       // یادآوری‌های امروز
        
        // فیلترهای زمانی
        public DateTime? FromDate { get; set; }                        // از تاریخ
        public DateTime? ToDate { get; set; }                          // تا تاریخ
        public int? DaysAhead { get; set; }                           // چند روز آینده
        public int? DaysBehind { get; set; }                          // چند روز گذشته
        
        // فیلترهای وضعیت
        public bool? IsSent { get; set; }                             // ارسال شده
        public bool? IsRead { get; set; }                             // خوانده شده
        public byte? Priority { get; set; }                           // اولویت
        
        // فیلترهای تسک
        public int? TaskId { get; set; }                              // شناسه تسک خاص
        public List<int>? TaskIds { get; set; }                       // شناسه تسک‌های خاص
        public string? TaskTitle { get; set; }                        // عنوان تسک
        
        // تنظیمات صفحه‌بندی
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // تنظیمات مرتب‌سازی
        public string SortBy { get; set; } = "ScheduledDateTime";      // فیلد مرتب‌سازی
        public string SortDirection { get; set; } = "desc";           // جهت مرتب‌سازی
        
        // فیلتر خاص برای داشبورد
        public bool ForDashboard { get; set; } = false;               // حالت داشبورد
        public int? DashboardLimit { get; set; } = 10;                // محدودیت تعداد در داشبورد
        
        /// <summary>
        /// تنظیمات پیش‌فرض برای داشبورد
        /// </summary>
        public static TaskReminderFilterViewModel ForDashboardReminders(int maxResults = 10, int daysAhead = 1)
        {
            return new TaskReminderFilterViewModel
            {
                ForDashboard = true,
                DashboardLimit = maxResults,
                DaysAhead = daysAhead,
                IncludeOverdueReminders = true,
                IncludeUpcomingReminders = true,
                IncludeUnreadSent = true,
                SortBy = "ScheduledDateTime",
                SortDirection = "asc"
            };
        }
        
        /// <summary>
        /// تنظیمات پیش‌فرض برای یادآوری‌های عقب افتاده
        /// </summary>
        public static TaskReminderFilterViewModel ForOverdueReminders()
        {
            return new TaskReminderFilterViewModel
            {
                FilterType = "overdue",
                IncludeOverdueReminders = true,
                IsSent = false
            };
        }
        
        /// <summary>
        /// تنظیمات پیش‌فرض برای یادآوری‌های آینده نزدیک
        /// </summary>
        public static TaskReminderFilterViewModel ForUpcomingReminders(int daysAhead = 1)
        {
            return new TaskReminderFilterViewModel
            {
                FilterType = "upcoming",
                IncludeUpcomingReminders = true,
                DaysAhead = daysAhead,
                IsSent = false
            };
        }
        
        /// <summary>
        /// تنظیمات پیش‌فرض برای یادآوری‌های خوانده نشده
        /// </summary>
        public static TaskReminderFilterViewModel ForUnreadReminders()
        {
            return new TaskReminderFilterViewModel
            {
                FilterType = "unread",
                IncludeUnreadSent = true,
                IsSent = true,
                IsRead = false
            };
        }
        /// <summary>
        /// ViewModel برای نمایش تسک‌های اخیر دریافتی
        /// </summary>
        public class RecentTaskViewModel
        {
            public int Id { get; set; }
            public string TaskCode { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public byte Priority { get; set; }
            public bool Important { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime AssignmentDate { get; set; }
            public string CreatorName { get; set; }
            public string CategoryTitle { get; set; }
            public bool IsCompleted { get; set; }
            public DateTime? CompletionDate { get; set; }
            public DateTime? DueDate { get; set; }
            public byte Status { get; set; }
            public bool IsOverdue { get; set; }

            // فیلدهای محاسباتی
            public string PriorityText => Priority switch
            {
                0 => "عادی",
                1 => "مهم",
                2 => "فوری",
                _ => "نامشخص"
            };

            public string StatusBadgeClass => IsCompleted ? "bg-success" :
                                             IsOverdue ? "bg-danger" :
                                             "bg-warning";
        }

        /// <summary>
        /// ViewModel برای نمایش تسک‌های واگذار شده
        /// </summary>
        public class RecentAssignedTaskViewModel
        {
            public int Id { get; set; }
            public string TaskCode { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public byte Priority { get; set; }
            public bool Important { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime CreateDate { get; set; }
            public string CategoryTitle { get; set; }

            // اطلاعات اعضا
            public int AssigneesCount { get; set; }
            public string AssignedToName { get; set; }
            public List<DashboardAssigneeInfo> Assignees { get; set; }

            // آمار تکمیل
            public int CompletedCount { get; set; }
            public int TotalAssignees { get; set; }
            public double CompletionPercentage => TotalAssignees > 0
                ? (double)CompletedCount / TotalAssignees * 100
                : 0;

            // وضعیت
            public DateTime? DueDate { get; set; }
            public byte Status { get; set; }
            public bool HasOverdueAssignees { get; set; }

            // فیلدهای محاسباتی
            public string ProgressBadgeClass => CompletionPercentage == 100 ? "bg-success" :
                                               CompletionPercentage >= 50 ? "bg-info" :
                                               HasOverdueAssignees ? "bg-danger" :
                                               "bg-warning";
        }

        
    }
}