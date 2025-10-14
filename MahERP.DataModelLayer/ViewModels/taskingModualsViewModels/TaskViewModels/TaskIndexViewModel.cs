using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel صفحه Index تسک‌ها - نسخه کامل
    /// </summary>
    public class TaskIndexViewModel
    {
        /// <summary>
        /// شناسه کاربر لاگین شده
        /// </summary>
        public string UserLoginid { get; set; } = string.Empty;

        /// <summary>
        /// لیست تسک‌ها (نمایش ساده)
        /// </summary>
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();

        /// <summary>
        /// تسک‌های گروه‌بندی شده (نمایش سلسله مراتبی)
        /// </summary>
        public TaskGroupedViewModel GroupedTasks { get; set; } = new TaskGroupedViewModel();

        /// <summary>
        /// فیلترهای اعمال شده
        /// </summary>
        public TaskFilterViewModel Filters { get; set; } = new TaskFilterViewModel();

        /// <summary>
        /// آمار تسک‌ها
        /// </summary>
        public TaskStatisticsViewModel Statistics { get; set; } = new TaskStatisticsViewModel();

        /// <summary>
        /// آیا فیلتر فعالی اعمال شده است؟
        /// </summary>
        public bool HasActiveFilters { get; set; }

        /// <summary>
        /// تعداد تسک‌ها برای هر فیلتر سریع
        /// </summary>
        public TaskFilterCountsViewModel FilterCounts { get; set; } = new TaskFilterCountsViewModel();

        /// <summary>
        /// فیلتر سریع فعلی
        /// </summary>
        public QuickFilterType CurrentFilter { get; set; } = QuickFilterType.AllVisible;

        /// <summary>
        /// نتیجه فیلتر فعلی (برای نمایش گروه‌بندی شده)
        /// </summary>
        public TaskFilterResultViewModel FilterResult { get; set; } = new TaskFilterResultViewModel();

        #region لیست‌های اولیه برای فیلترها

        /// <summary>
        /// لیست شعبه‌ها برای فیلتر
        /// </summary>
        public List<BranchViewModel> branchListInitial { get; set; } = new List<BranchViewModel>();

        /// <summary>
        /// لیست تیم‌ها برای فیلتر
        /// </summary>
        public List<TeamViewModel> TeamsInitial { get; set; } = new List<TeamViewModel>();

        /// <summary>
        /// لیست کاربران برای فیلتر
        /// </summary>
        public List<UserViewModelFull> UsersInitial { get; set; } = new List<UserViewModelFull>();

        /// <summary>
        /// لیست دسته‌بندی‌ها برای فیلتر
        /// </summary>
        public List<TaskCategory> TaskCategoryInitial { get; set; } = new List<TaskCategory>();

        /// <summary>
        /// لیست طرف‌حساب‌ها برای فیلتر
        /// </summary>
        public List<StakeholderViewModel> StakeholdersInitial { get; set; } = new List<StakeholderViewModel>();

        #endregion

        #region متدهای کمکی

        /// <summary>
        /// آیا نمایش گروه‌بندی شده فعال است؟
        /// </summary>
        public bool IsGroupedView => Filters?.ViewType == TaskViewType.MyTeamsHierarchy;

        /// <summary>
        /// آیا نمایش لیست ساده فعال است؟
        /// </summary>
        public bool IsListView => !IsGroupedView;

        /// <summary>
        /// دریافت متن نوع نمایش فعلی
        /// </summary>
        public string GetCurrentViewTypeText()
        {
            return Filters?.ViewType switch
            {
                TaskViewType.AllTasks => "همه تسک‌ها",
                TaskViewType.MyTasks => "تسک‌های من",
                TaskViewType.AssignedToMe => "منتصب شده به من",
                TaskViewType.AssignedByMe => "واگذار شده توسط من",
                TaskViewType.MyTeamsHierarchy => "تسک‌های تیمی",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت کلاس badge برای فیلتر فعال
        /// </summary>
        public string GetFilterBadgeClass(TaskViewType viewType)
        {
            return Filters?.ViewType == viewType ? "badge-primary" : "badge-secondary";
        }

        /// <summary>
        /// آیا فیلتر خاصی فعال است؟
        /// </summary>
        public bool IsFilterActive(TaskViewType viewType)
        {
            return Filters?.ViewType == viewType;
        }

        #endregion
    }

    public class TaskFilterViewModel
    {
        [Display(Name = "نوع نمایش")]
        public TaskViewType ViewType { get; set; } = TaskViewType.MyTasks;

        [Display(Name = "شعبه")]
        public int? BranchId { get; set; }

        [Display(Name = "تیم")]
        public int? TeamId { get; set; }

        [Display(Name = "کاربر")]
        public string? UserId { get; set; }

        [Display(Name = "نوع تسک")]
        public TaskPriorityFilter? TaskPriority { get; set; }

        public int? CategoryId { get; set; }

        [Display(Name = "وضعیت تسک")]
        public TaskStatusFilter? TaskStatus { get; set; }

        [Display(Name = "طرف حساب")]
        public int? StakeholderId { get; set; }

        [Display(Name = "تاریخ شروع")]
        public string? FromDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string? ToDate { get; set; }

        [Display(Name = "جستجو در متن")]
        public string? SearchTerm { get; set; }
        public List<byte>? StatusFilters { get; set; }

        // خاصیت کمکی برای تشخیص وجود فیلتر فعال
        public bool HasActiveFilters =>
            BranchId.HasValue ||
            TeamId.HasValue ||
            !string.IsNullOrEmpty(UserId) ||
            CategoryId.HasValue ||
            StakeholderId.HasValue ||
            !string.IsNullOrEmpty(SearchTerm) ||
            (TaskPriority.HasValue && TaskPriority != TaskPriorityFilter.All) ||
            (TaskStatus.HasValue && TaskStatus != TaskStatusFilter.All);
    }

    public class TaskStatisticsViewModel
    {
        public int TotalTasks { get; set; }

        /// <summary>
        /// تسک‌هایی که به من واگذار شده‌اند
        /// </summary>
        public int AssignedToMe { get; set; }

        /// <summary>
        /// تسک‌هایی که من به دیگران واگذار کرده‌ام
        /// </summary>
        public int AssignedByMe { get; set; }

        /// <summary>
        /// مجموع تسک‌های من = دریافتی + واگذار شده
        /// </summary>
        public int MyTasks => AssignedToMe + AssignedByMe;

        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int ImportantTasks { get; set; }
        public int UrgentTasks { get; set; }
        public int TeamTasks { get; set; }
        public int SubTeamTasks { get; set; }
    }

    public class TaskGroupedViewModel
    {
        public List<TaskViewModel> MyTasks { get; set; } = new List<TaskViewModel>();
        public List<TaskViewModel> AssignedToMe { get; set; } = new List<TaskViewModel>();
        public Dictionary<string, List<TaskViewModel>> TeamMemberTasks { get; set; } = new Dictionary<string, List<TaskViewModel>>();
        public Dictionary<string, List<TaskViewModel>> SubTeamTasks { get; set; } = new Dictionary<string, List<TaskViewModel>>();

        /// <summary>
        /// گروه‌بندی ویژه برای صفحه "تسک‌های من"
        /// </summary>
        public MyTasksGroupedViewModel MyTasksGrouped { get; set; } = new MyTasksGroupedViewModel();
        /// <summary>
        /// تسک‌های تیمی گروه‌بندی شده: Dictionary<TeamName, Dictionary<PersonName, List<TaskViewModel>>>
        /// </summary>
        public Dictionary<string, Dictionary<string, List<TaskViewModel>>> TeamTasksGrouped { get; set; }
            = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();
    }

    public enum TaskViewType
    {
        [Display(Name = "همه تسک‌ها")]
        AllTasks = 0,

        [Display(Name = "تسک‌های من و تیم")]
        MyTeamsHierarchy = 1,

        [Display(Name = "تسک‌های من")]
        MyTasks = 2,

        [Display(Name = "منتصب به من")]
        AssignedToMe = 3,

        // اضافه کردن مقادیر مفقود
        [Display(Name = "واگذار شده توسط من")]
        AssignedByMe = 4,

        [Display(Name = "تحت نظارت")]
        SupervisedTasks = 5
    }

    public enum TaskPriorityFilter
    {
        [Display(Name = "همه")]
        All = -1,

        [Display(Name = "عادی")]
        Normal = 0,

        [Display(Name = "مهم")]
        Important = 1,

        [Display(Name = "فوری")]
        Urgent = 2
    }

    public enum TaskStatusFilter
    {
        [Display(Name = "همه")]
        All = -1,

        [Display(Name = "ایجاد شده")]
        Created = 0,

        [Display(Name = "در حال انجام")]
        InProgress = 1,

        [Display(Name = "تکمیل شده")]
        Completed = 2,

        [Display(Name = "تأیید شده")]
        Approved = 3,

        [Display(Name = "رد شده")]
        Rejected = 4,

        [Display(Name = "در انتظار")]
        Pending = 5,

        [Display(Name = "تاخیردار")]
        Overdue = 99
    }

    /// <summary>
    /// اطلاعات شخص یا تیم منتصب شده
    /// </summary>
    public class AssigneeInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }

        /// <summary>
        /// شناسه تیم (برای گروه‌بندی)
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// نام تیم (برای نمایش)
        /// </summary>
        public string? TeamName { get; set; }

        public string Type { get; set; } // "User" or "Team"
        public bool IsTeam { get; set; }

        // برای استفاده در Dictionary
        public override bool Equals(object? obj)
        {
            if (obj is AssigneeInfo other)
            {
                return Id == other.Id && Type == other.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Type);
        }
    }


    public class TaskSummaryViewModel
    {
        public int Id { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public byte Priority { get; set; }
        public bool Important { get; set; }
        public DateTime? DueDate { get; set; }
        public byte Status { get; set; }
        public bool IsOverdue { get; set; }
        public string StatusText { get; set; }
        public string StatusBadgeClass { get; set; }
        public string StakeholderName { get; set; }

        /// <summary>
        /// تعداد روزهای باقی‌مانده تا مهلت
        /// </summary>
        public int? DaysUntilDue
        {
            get
            {
                if (!DueDate.HasValue) return null;
                return (DueDate.Value.Date - DateTime.Now.Date).Days;
            }
        }
    }

    public class RecentActivityViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string IconClass { get; set; }
        public DateTime ActivityDate { get; set; }
        public string TimeAgo { get; set; }
        public string Url { get; set; }
    }
}