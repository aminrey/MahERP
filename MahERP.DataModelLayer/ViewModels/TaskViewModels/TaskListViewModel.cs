using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels
{
    /// <summary>
    /// ViewModel اصلی برای نمایش لیست تسک‌ها - نسخه یکپارچه
    /// ادغام TaskIndexViewModel قدیمی + TaskListViewModel جدید
    /// </summary>
    public class TaskListViewModel
    {
        // ========================================
        // ⭐ بخش 1: اطلاعات کاربر
        // ========================================

        /// <summary>
        /// شناسه کاربر لاگین شده
        /// </summary>
        public string UserLoginid { get; set; } = string.Empty;

        // ========================================
        // ⭐ بخش 2: نمایش جدید (Card-Based)
        // ========================================

        /// <summary>
        /// نوع نمایش فعلی (تسک‌های من، واگذار شده، نظارتی)
        /// </summary>
        public TaskViewType CurrentViewType { get; set; } = TaskViewType.MyTasks;

        /// <summary>
        /// نوع گروه‌بندی فعلی
        /// </summary>
        public TaskGroupingType CurrentGrouping { get; set; } = TaskGroupingType.Team;

        /// <summary>
        /// گروه‌های تسک (نمایش کارتی)
        /// </summary>
        public List<TaskGroupViewModel> TaskGroups { get; set; } = new List<TaskGroupViewModel>();

        // ========================================
        // ⭐ بخش 3: نمایش قدیمی (Compatibility)
        // ========================================

        /// <summary>
        /// لیست تسک‌ها (نمایش ساده - برای سازگاری با کدهای قدیمی)
        /// </summary>
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();

        /// <summary>
        /// تسک‌های گروه‌بندی شده (نمایش سلسله مراتبی قدیمی)
        /// </summary>
        public TaskGroupedViewModel GroupedTasks { get; set; } = new TaskGroupedViewModel();

        /// <summary>
        /// تسک‌های انجام نشده (Status < 2)
        /// </summary>
        public List<TaskViewModel> PendingTasks { get; set; } = new List<TaskViewModel>();

        /// <summary>
        /// تسک‌های انجام شده 5 روز اخیر (Status >= 2)
        /// </summary>
        public List<TaskViewModel> CompletedTasks { get; set; } = new List<TaskViewModel>();

        /// <summary>
        /// نتیجه فیلتر فعلی (برای نمایش گروه‌بندی شده قدیمی)
        /// </summary>
        public TaskFilterResultViewModel FilterResult { get; set; } = new TaskFilterResultViewModel();

        // ========================================
        // ⭐ بخش 4: فیلترها و آمار
        // ========================================

        /// <summary>
        /// فیلترهای اعمال شده
        /// </summary>
        public TaskFilterViewModel Filters { get; set; } = new TaskFilterViewModel();

        /// <summary>
        /// آیا فیلتر فعالی اعمال شده است؟
        /// </summary>
        public bool HasActiveFilters { get; set; }

        /// <summary>
        /// آمار تسک‌ها (سازگار با قدیمی + جدید)
        /// </summary>
        public TaskListStatsViewModel Stats { get; set; } = new TaskListStatsViewModel();

        /// <summary>
        /// آمار قدیمی (برای سازگاری)
        /// </summary>
        public TaskStatisticsViewModel Statistics
        {
            get => MapToOldStats(Stats);
            set => Stats = MapToNewStats(value);
        }

        /// <summary>
        /// تعداد تسک‌ها برای هر فیلتر سریع
        /// </summary>
        public TaskFilterCountsViewModel FilterCounts { get; set; } = new TaskFilterCountsViewModel();

        /// <summary>
        /// فیلتر سریع فعلی (قدیمی)
        /// </summary>
        public QuickFilterType CurrentFilter { get; set; } = QuickFilterType.AllVisible;

        // ========================================
        // ⭐ بخش 5: لیست‌های اولیه برای فیلترها
        // ========================================

        #region Initial Lists

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
        /// لیست طرف‌حساب‌ها برای فیلتر (قدیمی)
        /// </summary>
        public List<StakeholderViewModel> StakeholdersInitial { get; set; } = new List<StakeholderViewModel>();

        /// <summary>
        /// لیست Contact‌ها برای فیلتر (جدید)
        /// </summary>
        public List<ContactViewModel> ContactsInitial { get; set; } = new List<ContactViewModel>();

        /// <summary>
        /// لیست Organization‌ها برای فیلتر (جدید)
        /// </summary>
        public List<OrganizationViewModel> OrganizationsInitial { get; set; } = new List<OrganizationViewModel>();

        #endregion

        // ========================================
        // ⭐ بخش 6: متدهای کمکی
        // ========================================

        #region Helper Methods


 
        /// <summary>
        /// آیا نمایش کارتی جدید فعال است؟
        /// </summary>
        public bool IsCardView => TaskGroups.Any();

        /// <summary>
        /// ⭐⭐⭐ فیلتر سریعstatus  فعلی
        /// </summary>
        public QuickStatusFilter CurrentStatusFilter { get; set; } = QuickStatusFilter.All;

        /// <summary>
        /// دریافت متن نوع نمایش فعلی
        /// </summary>
        public string GetCurrentViewTypeText()
        {
            return CurrentViewType switch
            {
                TaskViewType.MyTasks => "تسک‌های من",
                TaskViewType.AssignedByMe => "واگذار شده",
                TaskViewType.Supervised => "نظارتی",
     
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

        /// <summary>
        /// تبدیل آمار جدید به قدیمی (برای سازگاری)
        /// </summary>
        private static TaskStatisticsViewModel MapToOldStats(TaskListStatsViewModel newStats)
        {
            return new TaskStatisticsViewModel
            {
                TotalTasks = newStats.TotalPending + newStats.TotalCompleted,
                AssignedToMe = 0, // محاسبه می‌شود
                AssignedByMe = 0, // محاسبه می‌شود
                CompletedTasks = newStats.TotalCompleted,
                OverdueTasks = newStats.TotalOverdue,
                InProgressTasks = newStats.TotalPending,
                ImportantTasks = newStats.TotalImportant,
                UrgentTasks = newStats.TotalUrgent,
                TeamTasks = 0,
                SubTeamTasks = 0
            };
        }

        /// <summary>
        /// تبدیل آمار قدیمی به جدید
        /// </summary>
        private static TaskListStatsViewModel MapToNewStats(TaskStatisticsViewModel oldStats)
        {
            return new TaskListStatsViewModel
            {
                TotalPending = oldStats.InProgressTasks,
                TotalCompleted = oldStats.CompletedTasks,
                TotalOverdue = oldStats.OverdueTasks,
                TotalUrgent = oldStats.UrgentTasks,
                TotalImportant = oldStats.ImportantTasks
            };
        }

        #endregion
    }

    // ========================================
    // ⭐ کلاس‌های جدید (Card-Based Design)
    // ========================================

    #region New Card-Based Models

    /// <summary>
    /// گروه تسک‌ها بر اساس نوع گروه‌بندی
    /// </summary>
    public class TaskGroupViewModel
    {
        public string GroupKey { get; set; } = string.Empty;
        public string GroupTitle { get; set; } = string.Empty;
        public string GroupIcon { get; set; } = "fa-folder";
        public string GroupBadgeClass { get; set; } = "bg-primary";

        public List<TaskCardViewModel> PendingTasks { get; set; } = new List<TaskCardViewModel>();
        public List<TaskCardViewModel> CompletedTasks { get; set; } = new List<TaskCardViewModel>();

        public int TotalTasks => PendingTasks.Count + CompletedTasks.Count;
        public int PendingCount => PendingTasks.Count;
        public int CompletedCount => CompletedTasks.Count;
    }

    /// <summary>
    /// کارت تسک - برای نمایش در لیست
    /// </summary>
    public class TaskCardViewModel
    {
        public int Id { get; set; }
        public int CardNumber { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }

        // اطلاعات اصلی
        public string? CategoryTitle { get; set; }
        public string CategoryBadgeClass { get; set; } = "bg-info";
        public string? StakeholderName { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorAvatar { get; set; }

        // وضعیت
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = "bg-secondary";
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }

        // اولویت
        public byte Priority { get; set; }
        public bool Important { get; set; }
        public string PriorityText { get; set; } = "عادی";
        public string PriorityBadgeClass { get; set; } = "bg-primary";

        // زمان
        public DateTime CreateDate { get; set; }
        public string CreateDatePersian { get; set; } = string.Empty;
        
        /// <summary>
        /// ⭐⭐⭐ تاریخ شروع تسک
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        public string? DueDatePersian { get; set; }
        public int? DaysRemaining { get; set; }

        // اعضا
        public List<TaskMemberAvatarViewModel> Members { get; set; } = new List<TaskMemberAvatarViewModel>();
        public int TotalMembers { get; set; }
        public int DisplayMemberCount => Math.Min(Members.Count, 4);
        public int RemainingMembers => Math.Max(0, TotalMembers - 4);

        // عملیات
        public int TotalOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int ProgressPercentage { get; set; }

        // دسترسی‌ها
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanComplete { get; set; }

        // ⭐⭐⭐ ویژگی‌های تسک
        /// <summary>
        /// آیا این تسک در "روز من" کاربر فعلی قرار دارد؟
        /// </summary>
        public bool IsInMyDay { get; set; }

        /// <summary>
        /// آیا این تسک فوکوس کاربر فعلی است؟
        /// </summary>
        public bool IsFocused { get; set; }

        // ⭐⭐⭐ نوع نظارت (برای تسک‌های نظارتی)
        /// <summary>
        /// نوع نظارت: "system" (سیستمی) یا "carbon-copy" (رونوشت)
        /// </summary>
        public string? SupervisionType { get; set; }

        /// <summary>
        /// متن نمایشی نوع نظارت
        /// </summary>
        public string? SupervisionTypeText => SupervisionType switch
        {
            "system" => "ناظر سیستمی",
            "carbon-copy" => "رونوشت شده",
            _ => null
        };

        /// <summary>
        /// کلاس Badge برای نوع نظارت
        /// </summary>
        public string? SupervisionBadgeClass => SupervisionType switch
        {
            "system" => "bg-primary",
            "carbon-copy" => "bg-success",
            _ => null
        };

        /// <summary>
        /// آیکون نوع نظارت
        /// </summary>
        public string? SupervisionIcon => SupervisionType switch
        {
            "system" => "fa-eye",
            "carbon-copy" => "fa-copy",
            _ => null
        };

        /// <summary>
        /// ⭐⭐⭐ دلیل نظارت (برای نظارت سیستمی)
        /// مثلاً: "شما ناظر تیم فروش هستید و این تسک برای علی رضایی در همان تیم است"
        /// </summary>
        public string? SupervisionReason { get; set; }
    }

    /// <summary>
    /// اطلاعاتMember برای نمایش آواتار
    /// </summary>
    public class TaskMemberAvatarViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string ProfileImagePath { get; set; } = "/images/default-avatar.png";
        public string TooltipText { get; set; } = string.Empty;
    }

    /// <summary>
    /// آمار لیست تسک‌ها (نسخه جدید)
    /// </summary>
    public class TaskListStatsViewModel
    {
        public int TotalPending { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalOverdue { get; set; }
        public int TotalUrgent { get; set; }
        public int TotalImportant { get; set; }
    }

    /// <summary>
    /// نوع نمایش تسک‌ها (ادغام شده)
    /// </summary>
    public enum TaskViewType
    {
        MyTasks = 0,           // تسک‌های من (جدید)
        AssignedByMe = 1,      // واگذار شده (جدید)
        Supervised = 2,        // نظارتی (جدید)
       
    }

    /// <summary>
    /// نوع گروه‌بندی (جدید)
    /// </summary>
    public enum TaskGroupingType
    {
        Team = 0,        // بر اساس تیم
        Creator = 1,     // بر اساس سازنده
        CreateDate = 2,  // بر اساس زمان ساخت
        DueDate = 3,     // بر اساس زمان پایان
        Priority = 4,    // بر اساس اولویت
        Stakeholder = 5, // ⭐⭐⭐ بر اساس طرف حساب (Contact/Organization)
        AssignedUser = 6 // ⭐⭐⭐ بر اساس اعضا (Assigned Users) - NEW
    }

    /// <summary>
    /// ⭐⭐⭐ فیلتر سریع وضعیت تسک‌ها (برای آمار)
    /// </summary>
    public enum QuickStatusFilter
    {
        All = 0,         // همه
        Pending = 1,     // در حال انجام
        Completed = 2,   // تکمیل شده
        Overdue = 3,     // عقب افتاده
        Urgent = 4       // فوری
    }

    #endregion

    // ========================================
    // ⭐ کلاس‌های قدیمی (برای سازگاری)
    // ========================================

    #region Legacy Models - Kept for Backward Compatibility

    /// <summary>
    /// آمار تسک‌ها (نسخه قدیمی)
    /// </summary>
    public class TaskStatisticsViewModel
    {
        public int TotalTasks { get; set; }
        public int AssignedToMe { get; set; }
        public int AssignedByMe { get; set; }
        public int MyTasks => AssignedToMe + AssignedByMe;
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int ImportantTasks { get; set; }
        public int UrgentTasks { get; set; }
        public int TeamTasks { get; set; }
        public int SubTeamTasks { get; set; }
    }

    /// <summary>
    /// تسک‌های گروه‌بندی شده (نسخه قدیمی)
    /// </summary>
    public class TaskGroupedViewModel
    {
        public List<TaskViewModel> MyTasks { get; set; } = new List<TaskViewModel>();
        public List<TaskViewModel> AssignedToMe { get; set; } = new List<TaskViewModel>();
        public Dictionary<string, List<TaskViewModel>> TeamMemberTasks { get; set; } = new Dictionary<string, List<TaskViewModel>>();
        public Dictionary<string, List<TaskViewModel>> SubTeamTasks { get; set; } = new Dictionary<string, List<TaskViewModel>>();
        public MyTasksGroupedViewModel MyTasksGrouped { get; set; } = new MyTasksGroupedViewModel();
        public Dictionary<string, Dictionary<string, List<TaskViewModel>>> TeamTasksGrouped { get; set; } = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();
    }

    /// <summary>
    /// نتیجه فیلتر (قدیمی)
    /// </summary>
    public class TaskFilterResultViewModel
    {
        public Dictionary<string, Dictionary<string, List<TaskViewModel>>> GroupedTasks { get; set; } = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();
    }

    #endregion


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
    /// <summary>
    /// اطلاعات عضو منتصب شده - ویژه Dashboard
    /// </summary>
    public class DashboardAssigneeInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
    public class TaskFilterViewModel
    {

        [Display(Name = "نوع نمایش")]
        public TaskViewType? ViewType { get; set; }

        [Display(Name = "گروه‌بندی")]
        public TaskGroupingType? Grouping { get; set; }

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
        
        public List<byte>? StatusFilters { get; set; }

        // ⭐⭐⭐ فیلدهای جدید برای فیلتر پیشرفته

        /// <summary>
        /// تاریخ ساخت تسک (از) - شمسی
        /// </summary>
        [Display(Name = "تاریخ ساخت (از)")]
        public string? CreateDateFromPersian { get; set; }

        /// <summary>
        /// تاریخ ساخت تسک (تا) - شمسی
        /// </summary>
        [Display(Name = "تاریخ ساخت (تا)")]
        public string? CreateDateToPersian { get; set; }

        /// <summary>
        /// نام تسک برای جستجو
        /// </summary>
        [Display(Name = "نام تسک")]
        public string? TaskTitle { get; set; }

        /// <summary>
        /// کد تسک برای جستجو
        /// </summary>
        [Display(Name = "کد تسک")]
        public string? TaskCode { get; set; }

        /// <summary>
        /// سازنده تسک
        /// </summary>
        [Display(Name = "سازنده تسک")]
        public string? CreatorUserId { get; set; }

        /// <summary>
        /// اعضای تسک
        /// </summary>
        [Display(Name = "عضو تسک")]
        public string? AssignedUserId { get; set; }


        // خاصیت کمکی برای تشخیص وجود فیلتر فعال
        public bool HasActiveFilters =>
            BranchId.HasValue ||
            TeamId.HasValue ||
            !string.IsNullOrEmpty(UserId) ||
            CategoryId.HasValue ||
            StakeholderId.HasValue ||
            !string.IsNullOrEmpty(SearchTerm) ||
            !string.IsNullOrEmpty(TaskTitle) || // ⭐ جدید
            !string.IsNullOrEmpty(TaskCode) || // ⭐ جدید
            !string.IsNullOrEmpty(CreatorUserId) || // ⭐ جدید
            !string.IsNullOrEmpty(AssignedUserId) || // ⭐ جدید
            !string.IsNullOrEmpty(CreateDateFromPersian) || // ⭐ جدید
            !string.IsNullOrEmpty(CreateDateToPersian) || // ⭐ جدید
            (TaskPriority.HasValue && TaskPriority != TaskPriorityFilter.All) ||
            (TaskStatus.HasValue && TaskStatus != TaskStatusFilter.All);
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