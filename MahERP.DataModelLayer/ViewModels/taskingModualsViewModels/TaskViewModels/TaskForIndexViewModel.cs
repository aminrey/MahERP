using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    public class TaskListForIndexViewModel
    {
        public string? UserLoginid { get; set; }
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
        public int TotalCount { get; set; }

        // لیست‌های اولیه برای فیلترها
        public List<BranchViewModel>? branchListInitial { get; set; }
        public List<UserViewModelFull>? UsersInitial { get; set; }
        public List<StakeholderViewModel>? StakeholdersInitial { get; set; }
        public List<TaskCategory>? TaskCategoryInitial { get; set; }
        public List<TeamViewModel>? TeamsInitial { get; set; }

        // فیلترهای انتخاب شده
        public TaskFilterViewModel Filters { get; set; } = new TaskFilterViewModel();

        // آمار تسک‌ها
        public TaskStatisticsViewModel Statistics { get; set; } = new TaskStatisticsViewModel();

        // گروه‌بندی تسک‌ها بر اساس نوع نمایش
        public TaskGroupedViewModel GroupedTasks { get; set; } = new TaskGroupedViewModel();

        // خاصیت برای تشخیص وجود فیلتر فعال
        public bool HasActiveFilters => 
            Filters.BranchId.HasValue ||
            Filters.TeamId.HasValue ||
            !string.IsNullOrEmpty(Filters.UserId) ||
            Filters.CategoryId.HasValue ||
            Filters.StakeholderId.HasValue ||
            !string.IsNullOrEmpty(Filters.SearchTerm) ||
            (Filters.TaskPriority.HasValue && Filters.TaskPriority != TaskPriorityFilter.All) ||
            (Filters.TaskStatus.HasValue && Filters.TaskStatus != TaskStatusFilter.All);
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

        [Display(Name = "دسته‌بندی")]
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
        public int MyTasks { get; set; }
        public int AssignedToMe { get; set; }
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
}