using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
public class TaskViewModel
{
    public int Id { get; set; }

    public string? TaskCode { get; set; }

    [Required(ErrorMessage = "عنوان تسک الزامی است و نمی‌تواند خالی باشد")]
    [Display(Name = "عنوان تسک")]
    [MaxLength(200, ErrorMessage = "عنوان تسک حداکثر 200 کاراکتر باشد")]
    public string Title { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "نوع تسک")]
    public byte TaskType { get; set; }

    [Display(Name = "طرف حساب")]
    public int? StakeholderId { get; set; }

    [Display(Name = "قرارداد")]
    public int? ContractId { get; set; }

    [Display(Name = "شعبه")]
    public int? BranchId { get; set; }

    [Display(Name = "تسک بالادست")]
    public int? ParentTaskId { get; set; }

    [Display(Name = "شروع تاریخ مهلت انجام")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "شروع تاریخ مهلت انجام")]
    public string? StartDatePersian { get; set; }
    [Display(Name = "تاریخ مهلت انجام")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "تاریخ مهلت انجام")]
    public string? DueDatePersian { get; set; }

    /// <summary>
    /// تاریخ شروع پیشنهادی توسط سازنده تسک
    /// </summary>
    [Display(Name = "تاریخ شروع پیشنهادی")]
    public DateTime? SuggestedStartDate { get; set; }
    /// <summary>
    /// آیا تسک عقب افتاده است؟
    /// </summary>
    public bool IsOverdue => !CompletionDate.HasValue && DueDate.HasValue && DateTime.Now > DueDate.Value;

    /// <summary>
    /// تاریخ تایید سرپرست
    /// </summary>
    [Display(Name = "تاریخ تایید سرپرست")]
    public DateTime? SupervisorApproved => SupervisorApprovedDate;
    /// <summary>
    /// تاریخ شروع پیشنهادی به صورت شمسی
    /// </summary>
    [Display(Name = "تاریخ شروع پیشنهادی")]
    public string? SuggestedStartDatePersian { get; set; }

    /// <summary>
    /// مدت زمان تخمینی انجام (به ساعت)
    /// </summary>
    [Display(Name = "مدت زمان تخمینی (ساعت)")]
    public decimal? EstimatedHours { get; set; }

    /// <summary>
    /// آیا مهلت سخت است؟
    /// </summary>
    [Display(Name = "مهلت سخت")]
    public bool IsHardDeadline { get; set; }

    /// <summary>
    /// یادداشت زمان‌بندی
    /// </summary>
    [Display(Name = "یادداشت زمان‌بندی")]
    [MaxLength(500, ErrorMessage = "یادداشت زمان‌بندی حداکثر 500 کاراکتر باشد")]
    public string? TimeNote { get; set; }

    [Display(Name = "تاریخ تکمیل")]
    public DateTime? CompletionDate { get; set; }

    [Display(Name = "تاریخ تایید سرپرست")]
    public DateTime? SupervisorApprovedDate { get; set; }

    [Display(Name = "تاریخ تایید مدیر")]
    public DateTime? ManagerApprovedDate { get; set; }

    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreateDate { get; set; }

    [Display(Name = "ایجاد کننده")]
    public string? CreatorUserId { get; set; }

    [Display(Name = "وضعیت فعال")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "حذف شده")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties for display
    [Display(Name = "عنوان دسته‌بندی")]
    public string? CategoryTitle { get; set; }

    [Display(Name = "ای دی  دسته‌بندی")]
    public int? CategoryId { get; set; } = null;

    [Display(Name = "نام ایجاد کننده")]
    public string? CreatorName { get; set; }

    [Display(Name = "نام طرف حساب")]
    public string? StakeholderName { get; set; }

    [Display(Name = "عنوان قرارداد")]
    public string? ContractTitle { get; set; }

    // Attachments
    [Display(Name = "فایل‌های پیوست")]
    public List<IFormFile>? Attachments { get; set; }

    // Operations
    public List<TaskOperationViewModel>? Operations { get; set; }

    /// <summary>
    /// عملیات‌های تسک به صورت JSON (برای ذخیره در فرم)
    /// </summary>
    [Display(Name = "عملیات‌های تسک")]
    public string? TaskOperationsJson { get; set; }

    /// <summary>
    /// یادآوری‌های تسک به صورت JSON (برای ذخیره در فرم)
    /// </summary>
    [Display(Name = "یادآوری‌های تسک")]
    public string? TaskRemindersJson { get; set; }

    /// <summary>
    /// فعال‌سازی یادآوری پیش‌فرض (3 روز قبل از مهلت)
    /// </summary>
    [Display(Name = "یادآوری پیش‌فرض")]
    public bool EnableDefaultReminder { get; set; } = true;
        /// <summary>
        /// آیا این تسک در "روز من" کاربر فعلی قرار دارد؟
        /// </summary>
        [Display(Name = "در روز من")]
        public bool IsInMyDay { get; set; }

        // Progress calculation
        [Display(Name = "درصد پیشرفت")]
    public int ProgressPercentage
    {
        get
        {
            if (Operations == null || !Operations.Any()) return 0;

            int totalOperations = Operations.Count;
            int completedOperations = Operations.Count(o => o.IsCompleted);

            if (totalOperations == 0) return 0;
            return completedOperations * 100 / totalOperations;
        }
    }

    // Status display
    [Display(Name = "وضعیت")]
    public string StatusText
    {
        get
        {
            if (IsDeleted) return "حذف شده";
            if (!IsActive) return "غیرفعال";
            if (CompletionDate.HasValue && ManagerApprovedDate.HasValue) return "تکمیل و تایید شده";
            if (CompletionDate.HasValue && SupervisorApprovedDate.HasValue) return "تکمیل و تایید سرپرست";
            if (CompletionDate.HasValue) return "تکمیل شده";
            if (DueDate.HasValue && DateTime.Now > DueDate.Value) return "تاخیر";
            return "در حال انجام";
        }
    }

    /// <summary>
    /// هر جا initial هست یک لیست  جهت انتخاب هستد
    /// intial یعنی همه لیست و لود میکنه و برای نمایش تمام کاربر ها هست . 
    /// </summary>
    public List<BranchViewModel>? branchListInitial { get; set; } = new List<BranchViewModel>();
    public List<UserViewModelFull>? UsersInitial { get; set; } = new List<UserViewModelFull>();
    public List<StakeholderViewModel>? StakeholdersInitial { get; set; } = new List<StakeholderViewModel>();
    public List<TaskCategory>? TaskCategoryInitial { get; set; } = new List<TaskCategory>();

    /// <summary>
    /// لیست تیم‌های موجود برای انتخاب
    /// </summary>
    [Display(Name = "تیم‌های موجود")]
    public List<TeamViewModel>? TeamsInitial { get; set; } = new List<TeamViewModel>();

    // AssignmentsTask
    /// <summary>
    /// لیستی که کاربر انهارو انتخاب کرده 
    /// </summary>
    public List<TaskAssignmentViewModel>? AssignmentsTaskUser { get; set; }

    // AssignmentsCopyCarbon
    /// <summary>
    /// لیستی که کاربر انهارو انتخاب کرده 
    /// </summary>
    public List<TaskAssignmentViewModel>? AssignmentsCopyCarbonUsers { get; set; }

    /// <summary>
    /// لیست ارایه string برای ذخیره انتخاب ارایه مخصوص select2
    /// </summary>
    public List<string>? AssignmentsSelectedTaskUserArraysString { get; set; }

    /// <summary>
    /// لیست تیم‌های انتخاب شده برای اختصاص تسک
    /// </summary>
    [Display(Name = "تیم‌های انتخاب شده")]
    public List<int>? AssignmentsSelectedTeamIds { get; set; }

    /// <summary>
    /// لیست ارایه string برای ذخیره انتخاب ارایه مخصوص select2
    /// </summary>
    public List<string>? AssignmentsSelectedCopyCarbonUsersArraysString { get; set; }

    [Display(Name = "شعبه")]
    public int BranchIdSelected { get; set; }

    [Display(Name = "دسته‌بندی")]
    public int? TaskCategoryIdSelected { get; set; }

    /// <summary>
    /// نشان‌دهنده اینکه آیا کاربر می‌خواهد کد را دستی وارد کند
    /// </summary>
    [Display(Name = "ورود دستی کد")]
    public bool IsManualTaskCode { get; set; } = false;

    /// <summary>
    /// کد تسک دستی وارد شده توسط کاربر
    /// </summary>
    [Display(Name = "کد تسک دستی")]
    public string? ManualTaskCode { get; set; }

    /// <summary>
    /// تنظیمات کد تسک برای نمایش در view
    /// </summary>
    public TaskCodeSettings? TaskCodeSettings { get; set; }

    /// <summary>
    /// وضعیت زمان‌بندی تسک
    /// </summary>
    [Display(Name = "وضعیت زمان‌بندی")]
    public string TimeStatus
    {
        get
        {
            if (!DueDate.HasValue) return "بدون مهلت";
            if (DateTime.Now > DueDate.Value) return "منقضی";
            if (DateTime.Now.AddDays(3) >= DueDate.Value) return "نزدیک به پایان";
            return "در زمان";
        }
    }

    /// <summary>
    /// کلاس CSS برای نمایش وضعیت زمان‌بندی
    /// </summary>
    public string TimeStatusClass => TimeStatus switch
    {
        "منقضی" => "text-danger",
        "نزدیک به پایان" => "text-warning",
        "در زمان" => "text-success",
        _ => "text-muted"
    };

    /// <summary>
    /// اولویت تسک (0=عادی، 1=مهم، 2=فوری)
    /// </summary>
    [Display(Name = "اولویت")]
    public byte Priority { get; set; }

    /// <summary>
    /// آیا تسک مهم است؟
    /// </summary>
    [Display(Name = "مهم")]
    public bool Important { get; set; }

    /// <summary>
    /// وضعیت تسک
    /// </summary>
    [Display(Name = "وضعیت")]
    public byte Status { get; set; }

    /// <summary>
    /// سطح دسترسی تسک
    /// </summary>
    [Display(Name = "سطح دسترسی")]
    public byte VisibilityLevel { get; set; } = 0;

    /// <summary>
    /// تاریخ آخرین بروزرسانی
    /// </summary>
    [Display(Name = "آخرین بروزرسانی")]
    public DateTime? LastUpdateDate { get; set; }

    /// <summary>
    /// نوع ورودی تسک (0=رزرو، 1=کاربر عادی، 2=خودکار، 3=مشتری)
    /// </summary>
    [Display(Name = "نوع ورودی")]
    public byte TaskTypeInput { get; set; }

    /// <summary>
    /// نحوه ایجاد (0=دستی، 1=خودکار)
    /// </summary>
    [Display(Name = "نحوه ایجاد")]
    public byte CreationMode { get; set; }

    /// <summary>
    /// آیا تسک خصوصی است؟
    /// </summary>
    [Display(Name = "تسک خصوصی")]
    public bool IsPrivate { get; set; }

    /// <summary>
    /// لیست تیم‌های انتصاب (برای هر کاربر منتصب، تیم مرتبطش)
    /// Key: UserId, Value: TeamId
    /// </summary>
    public Dictionary<string, int> UserTeamAssignments { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// JSON برای ارسال تخصیص تیم‌ها
    /// </summary>
    public string UserTeamAssignmentsJson { get; set; }

}


/// <summary>
/// DTO برای عملیات‌های تسک در JSON
/// </summary>
public class TaskOperationDto
{
    public string Title { get; set; }
    public int OperationOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal? EstimatedHours { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO برای یادآوری‌های تسک در JSON
/// </summary>
public class TaskReminderDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public byte ReminderType { get; set; }
    public int? IntervalDays { get; set; }
    public int? DaysBeforeDeadline { get; set; }
    public TimeSpan NotificationTime { get; set; } = new TimeSpan(9, 0, 0);
    public bool IsSystemDefault { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TaskAssignmentViewModel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    [Required(ErrorMessage = "کاربر اختصاص داده شده الزامی است")]
    [Display(Name = "کاربر")]
    public string AssignedUserId { get; set; }

    [Display(Name = "نام کاربر")]
    public string? AssignedUserName { get; set; }

    [Display(Name = "تخصیص دهنده")]
    public string? AssignerUserId { get; set; }

    [Display(Name = "نام تخصیص دهنده")]
    public string? AssignerUserName { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "تاریخ تکمیل")]
    public DateTime? CompletionDate { get; set; }

    [Display(Name = "تاریخ تخصیص")]
    public DateTime AssignDate { get; set; }

    /// <summary>
    /// ⭐ متد کمکی: آیا این assignment برای خود سازنده است؟
    /// </summary>
    public bool IsSelfAssignment => AssignedUserId == AssignerUserId;

    /// <summary>
    /// ⭐ متد کمکی: آیا این یک assignment واقعی است؟ (نه self-assignment)
    /// </summary>
    public bool IsActualAssignment => !IsSelfAssignment && !string.IsNullOrEmpty(AssignedUserId);
}

public class TaskCategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است")]
    [Display(Name = "عنوان دسته‌بندی")]
    [MaxLength(100, ErrorMessage = "عنوان دسته‌بندی حداکثر 100 کاراکتر باشد")]
    public string Title { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "دسته‌بندی والد")]
    public int? ParentCategoryId { get; set; }

    [Display(Name = "عنوان دسته‌بندی والد")]
    public string? ParentCategoryTitle { get; set; }

    [Display(Name = "رنگ (کلاس CSS)")]
    public string? ColorClass { get; set; }

    [Display(Name = "آیکون")]
    public string? IconClass { get; set; }

    [Display(Name = "وضعیت فعال")]
    public bool IsActive { get; set; } = true;
}

public class TaskSearchViewModel
{
    [Display(Name = "عبارت جستجو")]
    public string? SearchTerm { get; set; }

    [Display(Name = "دسته‌بندی")]
    public int? CategoryId { get; set; }

    [Display(Name = "اختصاص داده شده به")]
    public string? AssignedUserId { get; set; }

    [Display(Name = "وضعیت تکمیل")]
    public bool? IsCompleted { get; set; }

    [Display(Name = "تاریخ شروع")]
    public string? FromDate { get; set; }

    [Display(Name = "تاریخ پایان")]
    public string? ToDate { get; set; }

    [Display(Name = "طرف حساب")]
    public int? StakeholderId { get; set; }

    [Display(Name = "نمایش حذف شده‌ها")]
    public bool IncludeDeleted { get; set; } = false;
}
/// <summary>
/// ViewModel برای بروزرسانی داده‌ها بر اساس تغییر شعبه
/// </summary>
public class BranchChangeDataViewModel
{
    public List<BranchUserViewModel> Users { get; set; } = new List<BranchUserViewModel>();
    public List<TeamViewModel> Teams { get; set; } = new List<TeamViewModel>();
    public List<StakeholderViewModel> Stakeholders { get; set; } = new List<StakeholderViewModel>();
}

/// <summary>
/// مدل پاسخ برای AJAX تغییر شعبه
/// </summary>
public class BranchSelectResponseViewModel
{
    public List<BranchUserViewModel> Users { get; set; } = new();
    public List<TeamViewModel> Teams { get; set; } = new();
    public List<StakeholderViewModel> Stakeholders { get; set; } = new();
}
/// <summary>
/// مدل آمار پروژه
/// </summary>
public class ProjectStatsViewModel
{
    public int StakeholderTasksCount { get; set; }
    public int CategoryTasksCount { get; set; }
}

/// <summary>
/// گروه‌بندی ویژه برای صفحه "تسک‌های من"
/// </summary>
public class MyTasksGroupedViewModel
{
    /// <summary>
    /// تسک‌های دریافتی (واگذار شده به من)
    /// </summary>
    public List<TaskViewModel> TasksAssignedToMe { get; set; } = new List<TaskViewModel>();

    /// <summary>
    /// تسک‌های واگذار شده (گروه‌بندی شده بر اساس انجام‌دهنده)
    /// </summary>
    public Dictionary<AssigneeInfo, List<TaskViewModel>> TasksAssignedByMe { get; set; } = new Dictionary<AssigneeInfo, List<TaskViewModel>>();
}

/// <summary>
/// ViewModel جامع برای نمایش تسک‌های کاربر به تفکیک نوع
/// </summary>
public class UserTasksComprehensiveViewModel
{
    /// <summary>
    /// تسک‌های ایجاد شده توسط کاربر
    /// </summary>
    public List<TaskViewModel> CreatedTasks { get; set; } = new List<TaskViewModel>();

    /// <summary>
    /// تسک‌های منتصب شده به کاربر
    /// </summary>
    public List<TaskViewModel> AssignedTasks { get; set; } = new List<TaskViewModel>();

    /// <summary>
    /// تسک‌های تحت نظارت کاربر
    /// </summary>
    public List<TaskViewModel> SupervisedTasks { get; set; } = new List<TaskViewModel>();

    /// <summary>
    /// تسک‌های حذف شده مرتبط با کاربر
    /// </summary>
    public List<TaskViewModel> DeletedTasks { get; set; } = new List<TaskViewModel>();

    /// <summary>
    /// تعداد کل تسک‌ها (بدون حذف‌شده‌ها)
    /// </summary>
    public int TotalActiveTasksCount => CreatedTasks.Count + AssignedTasks.Count + SupervisedTasks.Count;

    /// <summary>
    /// آمار کلی
    /// </summary>
    public UserTasksStatsViewModel Stats { get; set; } = new UserTasksStatsViewModel();
}

/// <summary>
/// آمار تفصیلی تسک‌های کاربر
/// </summary>
public class UserTasksStatsViewModel
{
    public int CreatedTasksCount { get; set; }
    public int AssignedTasksCount { get; set; }
    public int SupervisedTasksCount { get; set; }
    public int DeletedTasksCount { get; set; }
    public int CompletedTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public int TodayTasksCount { get; set; }
}

}