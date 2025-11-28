using AutoMapper;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    public interface ITaskRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت تمام تسک‌ها با فیلترهای اختیاری
        /// </summary>
        List<Tasks> GetTasks(bool includeDeleted = false, int? categoryId = null, string assignedUserId = null);

        /// <summary>
        /// دریافت تسک بر اساس شناسه با امکان include کردن related entities
        /// </summary>
        Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, bool includeAttachments = false,
            bool includeComments = false, bool includeStakeHolders = false, bool includeTaskWorkLog = false);


        /// <summary>
        /// دریافت تسک‌های مرتبط با طرف حساب
        /// </summary>
        List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false);

        /// <summary>
        /// دریافت تسک‌های کاربر (منتصب شده یا ایجاد شده توسط)
        /// </summary>
        List<Tasks> GetTasksByUser(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false);

        /// <summary>
        /// دریافت تسک‌های شعبه
        /// </summary>
        List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false);

        #endregion

        #region Task Validation Methods

        /// <summary>
        /// بررسی یکتایی کد تسک (نسخه Async)
        /// </summary>
        Task<bool> IsTaskCodeUniqueAsync(string taskCode, int? excludeTaskId = null);

        /// <summary>
        /// بررسی یکتایی کد تسک (نسخه Sync)
        /// </summary>
        bool IsTaskCodeUnique(string taskCode, int? excludeId = null);

        /// <summary>
        /// اعتبارسنجی کد تسک
        /// </summary>
        bool ValidateTaskCode(string taskCode, int? excludeId = null);

        /// <summary>
        /// بررسی ارتباط کاربر با تسک
        /// </summary>
        bool IsUserRelatedToTask(string userId, int taskId);

        /// <summary>
        /// بررسی تعلق تسک به شعبه
        /// </summary>
        bool IsTaskInBranch(int taskId, int branchId);

        #endregion

        #region Task Operations

        /// <summary>
        /// دریافت عملیات‌های تسک
        /// </summary>
        List<TaskOperation> GetTaskOperations(int taskId, bool includeCompleted = true);

        /// <summary>
        /// دریافت عملیات تسک بر اساس شناسه
        /// </summary>
        TaskOperation GetTaskOperationById(int id);

        /// <summary>
        /// ذخیره عملیات‌های تسک
        /// </summary>
        void SaveTaskOperations(int taskId, List<TaskOperationViewModel> operations);

        /// <summary>
        /// ذخیره یادآوری‌های تسک
        /// </summary>
        void SaveTaskReminders(int taskId, List<TaskReminderViewModel> reminders);

        #endregion

        #region Task Assignments

        /// <summary>
        /// دریافت انتساب‌های تسک
        /// </summary>
        List<TaskAssignment> GetTaskAssignments(int taskId);

        /// <summary>
        /// دریافت انتساب تسک بر اساس شناسه
        /// </summary>
        TaskAssignment GetTaskAssignmentById(int id);

        /// <summary>
        /// دریافت انتساب تسک برای کاربر و تسک مشخص
        /// </summary>
        TaskAssignment GetTaskAssignmentByUserAndTask(string userId, int taskId);

        /// <summary>
        /// تخصیص کاربر جدید به تسک
        /// </summary>
        Task<bool> AssignUserToTaskAsync(int taskId, string userId, string assignerUserId, int? teamId = null, string description = null);

        /// <summary>
        /// حذف Assignment
        /// </summary>
        Task<bool> RemoveTaskAssignmentAsync(int assignmentId);

        /// <summary>
        /// دریافت Assignment با اطلاعات کامل
        /// </summary>
        Task<TaskAssignment> GetTaskAssignmentByIdAsync(int assignmentId);

        /// <summary>
        /// بررسی تکراری نبودن Assignment
        /// </summary>
        Task<TaskAssignment> GetTaskAssignmentByUserAndTaskAsync(string userId, int taskId);

        #endregion

        #region Task Categories

        /// <summary>
        /// دریافت تمام دسته‌بندی‌ها
        /// </summary>
        List<TaskCategory> GetAllCategories(bool activeOnly = true);

        /// <summary>
        /// دریافت دسته‌بندی بر اساس شناسه
        /// </summary>
        TaskCategory GetCategoryById(int id);

        #endregion

        #region Task Search and Filter

        /// <summary>
        /// جستجوی تسک‌ها بر اساس معیارهای مختلف
        /// </summary>
        List<Tasks> SearchTasks(string searchTerm, int? categoryId = null, string assignedUserId = null, bool? isCompleted = null);

        #endregion

        #region Calendar and View Methods

        /// <summary>
        /// دریافت تسک‌های شعبه برای نمایش در تقویم بر اساس تاریخ مهلت انجام
        /// </summary>
        List<TaskCalendarViewModel> GetTasksForCalendarView(
            string userId,
            int? branchId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null);

        /// <summary>
        /// دریافت رویدادهای تقویم (نسخه Async)
        /// </summary>
        Task<List<TaskCalendarViewModel>> GetCalendarEventsAsync(
            string userId,
            DateTime? start = null,
            DateTime? end = null,
            int? branchId = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null);

        #endregion

        #region Visibility Methods


        /// <summary>
        /// دریافت تسک‌های کاربر با در نظر گیری مجوزهای جدید (نسخه Async)
        /// </summary>
        Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(string userId, bool includeAssigned = true, bool includeCreated = false,
            bool includeDeleted = false, bool includeSupervisedTasks = false);


        /// <summary>
        /// دریافت تسک‌های شعبه با در نظر گیری مجوزهای جدید (نسخه Async)
        /// </summary>
        Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false);

        /// <summary>
        /// بررسی مجوز مشاهده تسک (نسخه Async)
        /// </summary>
        Task<bool> CanUserViewTaskAsync(string userId, int taskId);

        /// <summary>
        /// دریافت لیست شناسه تسک‌هایی که کاربر می‌تواند مشاهده کند
        /// </summary>
        Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId = null, int? teamId = null);

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده بر اساس سمت
        /// </summary>
        Task<List<int>> GetPositionBasedVisibleTasksAsync(string userId, int? branchId = null, int? teamId = null);

        /// <summary>
        /// بررسی قابلیت مشاهده بر اساس سمت در تیم
        /// </summary>
        Task<bool> CanViewBasedOnPositionAsync(string userId, Tasks task);

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

        /// <summary>
        /// بررسی مجوزهای خاص (تبصره‌ها)
        /// </summary>
        Task<bool> HasSpecialPermissionAsync(string userId, Tasks task);

        /// <summary>
        /// دریافت تسک‌های با مجوز خاص
        /// </summary>
        Task<List<int>> GetSpecialPermissionTasksAsync(string userId);

        /// <summary>
        /// دریافت ناظران یک تسک خاص
        /// </summary>
        Task<List<string>> GetTaskSupervisorsAsync(int taskId, bool includeCreator = false);

        /// <summary>
        /// دریافت ناظران یک کاربر در تیم خاص
        /// </summary>
        Task<List<string>> GetUserSupervisorsInTeamAsync(string userId, int teamId, int branchId);

        /// <summary>
        /// دریافت ناظران یک کاربر در تمام تیم‌های او
        /// </summary>
        Task<List<string>> GetUserAllSupervisorsAsync(string userId, int? branchId = null);

        /// <summary>
        /// دریافت ناظران تسک با جزئیات کامل
        /// </summary>
        Task<List<SupervisorInfoViewModel>> GetTaskSupervisorsWithDetailsAsync(int taskId);

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت گروه‌بندی شده
        /// </summary>
        Task<SubTeamTasksGroupedViewModel> GetSubTeamTasksGroupedDetailedAsync(string userId, int? branchId = null);

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت Dictionary ساده
        /// </summary>
        Task<Dictionary<int, List<int>>> GetSubTeamTasksGroupedAsync(string userId, int? branchId = null);

        /// <summary>
        /// ایجاد چارت قدرت مشاهده تسک‌ها
        /// </summary>
        Task<TaskVisibilityChartViewModel> GenerateVisibilityChartAsync(int branchId);

        /// <summary>
        /// محاسبه آمار چارت قدرت مشاهده
        /// </summary>
        Task<TaskVisibilityStatsViewModel> CalculateVisibilityStatsAsync(int branchId);

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

        #region Hierarchical Task Methods

        /// <summary>
        /// دریافت تسک‌ها گروه‌بندی شده بر اساس سلسله مراتب تیمی
        /// </summary>
        Task<TaskGroupedViewModel> GetHierarchicalTasksForUserAsync(string userId);

        #endregion


        #region Team and User Helper Methods

        /// <summary>
        /// دریافت کاربران از تیم‌های انتخاب شده
        /// </summary>
        Task<List<string>> GetUsersFromTeamsAsync(List<int> teamIds);

        /// <summary>
        /// دریافت شعبه کاربر
        /// </summary>
        int GetUserBranchId(string userId);

        /// <summary>
        /// دریافت تیم‌های مرتبط با کاربر
        /// </summary>
        Task<List<TeamViewModel>> GetUserRelatedTeamsAsync(string userId);

        /// <summary>
        /// دریافت کاربران مرتبط با کاربر
        /// </summary>
        Task<List<UserViewModelFull>> GetUserRelatedUsersAsync(string userId);

        /// <summary>
        /// دریافت تیم‌های یک کاربر در شعبه مشخص
        /// </summary>
        Task<List<TeamViewModel>> GetUserTeamsByBranchAsync(string userId, int branchId);

        #endregion

        #region Statistics and Filter Methods



        /// <summary>
        /// اعمال فیلترهای اضافی بر روی لیست تسک‌ها
        /// </summary>
        Task<List<TaskViewModel>> ApplyFiltersAsync(List<TaskViewModel> tasks, TaskFilterViewModel filters);

        /// <summary>
        /// بررسی وجود فیلتر فعال
        /// </summary>
        bool HasActiveFilters(TaskFilterViewModel filters);

        #endregion

        #region AJAX and Helper Methods



        /// <summary>
        /// دریافت داده‌های مربوط به تغییر شعبه
        /// </summary>
        Task<BranchChangeDataViewModel> GetBranchChangeDataAsync(int branchId);

        /// <summary>
        /// دریافت دسته‌بندی‌ها بر اساس تغییر طرف حساب
        /// </summary>
        Task<List<TaskCategory>> GetTaskCategoriesForStakeholderChangeAsync(int branchId, int stakeholderId);

        /// <summary>
        /// بازیابی داده‌های فرم CreateTask
        /// </summary>
        Task<TaskViewModel> RepopulateCreateTaskModelAsync(TaskViewModel model, string userId);

        #endregion

        #region Main Form Methods

        /// <summary>
        /// ایجاد تسک و جمع‌آوری داده‌های مربوطه
        /// </summary>
        TaskViewModel CreateTaskAndCollectData(string UserId);




        /// <summary>
        /// آماده‌سازی مدل برای ایجاد تسک جدید (نسخه Async جدید)
        /// </summary>
        Task<TaskViewModel> PrepareCreateTaskModelAsync(string userId);

        /// <summary>
        /// دریافت داده‌های شعبه برای AJAX
        /// </summary>
        Task<BranchSelectResponseViewModel> GetBranchTriggeredDataAsync(int branchId);

        /// <summary>
        /// دریافت آمار پروژه
        /// </summary>
        Task<ProjectStatsViewModel> GetProjectStatsAsync(int branchId, int? stakeholderId = null, int? categoryId = null);

        #endregion

        #region Dashboard Methods

        /// <summary>
        /// دریافت داده‌های داشبورد تسک‌ها برای کاربر
        /// </summary>
        Task<TaskDashboardViewModel> GetTaskDashboardDataAsync(string userId);

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط کاربر
        /// </summary>
        Task<TasksListViewModel> GetTasksAssignedByUserAsync(string userId, TaskFilterViewModel filters);



        /// <summary>
        /// دریافت یادآوری‌های تسک برای کاربر
        /// </summary>
        Task<TaskRemindersViewModel> GetTaskRemindersAsync(string userId, TaskReminderFilterViewModel filters);

        /// <summary>
        /// دریافت آمار تسک‌ها برای کاربر
        /// </summary>
        Task<UserTaskStatsViewModel> GetUserTaskStatsAsync(string userId);

        #endregion

        #region Task Summary and Activities

        /// <summary>
        /// دریافت تسک‌های فوری کاربر
        /// </summary>
        Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5);

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک کاربر
        /// </summary>
        Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(string userId, int take = 10);

        #endregion

        #region Reminder Management

        /// <summary>
        /// علامت‌گذاری یادآوری به عنوان خوانده شده
        /// </summary>
        Task MarkReminderAsReadAsync(int reminderId, string userId);

        #endregion

        /// <summary>
        /// علامت‌گذاری همه یادآوری‌ها به عنوان خوانده شده
        /// </summary>
        Task MarkAllRemindersAsReadAsync(string userId);

        /// <summary>
        /// حذف یادآوری
        /// </summary>
        Task DeleteReminderAsync(int reminderId, string userId);

        /// <summary>
        /// حذف یادآوری‌های خوانده شده
        /// </summary>
        Task DeleteReadRemindersAsync(string userId);

        /// <summary>
        /// دریافت انتصاب‌های تسک همراه با تاریخ‌های شخصی
        /// </summary>
        Task<List<TaskAssignment>> GetTaskAssignmentsWithPersonalDatesAsync(int taskId);

        /// <summary>
        /// دریافت حروف اول نام کاربر
        /// </summary>
        string GetUserInitials(string firstName, string lastName);


        #region Missing Methods - Added from TaskRepository Implementation

        /// <summary>
        /// دریافت متن وضعیت تسک - پیاده‌سازی شده در TaskRepository
        /// </summary>
        string GetTaskStatusText(byte status);

        /// <summary>
        /// دریافت کلاس CSS برای وضعیت تسک - پیاده‌سازی شده در TaskRepository
        /// </summary>
        string GetTaskStatusBadgeClass(byte status);

        #endregion

        #region Personal Dates Management Methods

        /// <summary>
        /// دریافت انتساب تسک برای تنظیم تاریخ‌های شخصی
        /// </summary>
        Task<TaskAssignment> GetTaskAssignmentForPersonalDatesAsync(int taskId, string userId);

        /// <summary>
        /// دریافت انتساب تسک بر اساس شناسه انتساب برای تنظیم تاریخ‌های شخصی
        /// </summary>
        Task<TaskAssignment> GetTaskAssignmentByIdForPersonalDatesAsync(int assignmentId, string userId);

        /// <summary>
        /// بروزرسانی تاریخ‌های شخصی انتساب تسک
        /// </summary>
        Task<bool> UpdatePersonalDatesAsync(int assignmentId, string userId, DateTime? personalStartDate, DateTime? personalEndDate, string personalTimeNote);

        #endregion

        #region TaskMyDay Methods

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های "روز من" با گروه‌بندی پیشرفته
        /// </summary>
        Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های یک روز خاص
        /// </summary>
        Task<List<MyDayTaskItemViewModel>> GetTasksForDateAsync(string userId, DateTime date);

        /// <summary>
        /// ⭐⭐⭐ افزودن تسک به "روز من" با پشتیبانی از GroupTitle
        /// </summary>
        Task<(bool Success, string Message, int? MyDayId)> AddTaskToMyDayAsync(
            int taskId,
            string userId,
            DateTime plannedDate,
            string? planNote = null,
            string? groupTitle = null);

        /// <summary>
        /// ⭐⭐⭐ شروع کار روی تسک
        /// </summary>
        Task<(bool Success, string Message)> StartWorkOnTaskAsync(int myDayId, string userId);

        /// <summary>
        /// ⭐⭐⭐ ثبت گزارش کار انجام شده
        /// </summary>
        Task<(bool Success, string Message)> LogWorkAsync(
            int myDayId,
            string userId,
            string workNote,
            int? durationMinutes = null);

        /// <summary>
        /// ⭐⭐⭐ تنظیم تسک به عنوان متمرکز
        /// </summary>
        Task<(bool Success, string Message)> SetTaskAsFocusedAsync(int myDayId, string userId);

        /// <summary>
        /// ⭐⭐⭐ حذف تمرکز از تسک
        /// </summary>
        Task<(bool Success, string Message)> RemoveFocusFromTaskAsync(int myDayId, string userId);

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک متمرکز فعلی
        /// </summary>
        Task<MyDayTaskItemViewModel?> GetFocusedTaskAsync(string userId);

        /// <summary>
        /// بررسی وجود تسک در "روز من"
        /// </summary>
        Task<bool> IsTaskInMyDayAsync(int taskId, string userId, DateTime? targetDate = null);

        /// <summary>
        /// ⭐⭐⭐ حذف تسک از "روز من"
        /// </summary>
        Task<(bool Success, string Message)> RemoveTaskFromMyDayAsync(int myDayId, string userId);

        /// <summary>
        /// ⭐⭐⭐ دریافت آمار "روز من"
        /// </summary>
        Task<MyDayStatsViewModel> GetMyDayStatsAsync(string userId, DateTime? date = null);

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای کاربر
        /// </summary>
        Task<int> GetMyDayTasksCountAsync(string userId, DateTime? targetDate = null);

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست گزارش کارها
        /// </summary>
        Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId);

        /// <summary>
        /// ⭐⭐⭐ دریافت اطلاعات تسک برای مودال حذف
        /// </summary>
        Task<(string TaskTitle, string TaskCode)?> GetMyDayTaskInfoForRemovalAsync(int myDayId);

        /// <summary>
        /// ⭐⭐⭐ دریافت MyDayTask بر اساس taskId
        /// </summary>
        Task<TaskMyDay?> GetMyDayTaskByTaskIdAsync(int taskId, string userId);

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست عناوین گروه‌های موجود برای یک کاربر
        /// </summary>
        Task<List<string>> GetMyDayGroupTitlesAsync(string userId);

        #endregion

        #region Comprehensive User Tasks

        /// <summary>
        /// دریافت همه انواع تسک‌های کاربر به تفکیک نوع
        /// </summary>
        Task<UserTasksComprehensiveViewModel> GetUserTasksComprehensiveAsync(
           string userId,
           bool includeCreatedTasks = true,
           bool includeAssignedTasks = true,
           bool includeSupervisedTasks = false,
           bool includeDeletedTasks = false);

        /// <summary>
        /// دریافت یادآوری‌های داشبورد
        /// </summary>
        Task<List<TaskReminderItemViewModel>> GetDashboardRemindersAsync(string userId, int maxResults = 10, int daysAhead = 1);

        #endregion

        #region Task Creation and Validation Helper Methods

        /// <summary>
        /// اعتبارسنجی مدل تسک قبل از ایجاد یا ویرایش
        /// </summary>
        Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateTaskModelAsync(TaskViewModel model, string userId);

        /// <summary>
        /// ایجاد entity تسک از ViewModel
        /// </summary>
        Task<Tasks> CreateTaskEntityAsync(TaskViewModel model, string currentUserId, IMapper mapper);

        /// <summary>
        /// ذخیره عملیات‌ها و یادآوری‌های تسک
        /// </summary>
        Task SaveTaskOperationsAndRemindersAsync(int taskId, TaskViewModel model);

        /// <summary>
        /// مدیریت انتصاب‌های تسک (نسخه ساده)
        /// </summary>
        Task HandleTaskAssignmentsAsync(Tasks task, TaskViewModel model, string currentUserId);

        /// <summary>
        /// مدیریت انتساب‌های تسک با Bulk Insert
        /// </summary>
        Task HandleTaskAssignmentsBulkAsync(Tasks task, TaskViewModel model, string currentUserId);

        /// <summary>
        /// ذخیره فایل‌های پیوست تسک
        /// </summary>
        Task SaveTaskAttachmentsAsync(int taskId, List<IFormFile> files, string uploaderUserId, string webRootPath);

        #endregion

        #region Task Status Helper Methods

        /// <summary>
        /// دریافت رنگ وضعیت تسک برای تقویم
        /// </summary>
        string GetTaskStatusColor(TaskCalendarViewModel task);

        /// <summary>
        /// دریافت متن وضعیت تسک برای تقویم
        /// </summary>
        string GetTaskStatusTextForCalendar(TaskCalendarViewModel task);

        #endregion

        #region Task History Methods

        /// <summary>
        /// دریافت تاریخچه کامل تسک
        /// </summary>
        Task<List<TaskHistoryViewModel>> GetTaskHistoryAsync(int taskId);

        #endregion

        #region Task Reminders Management

        /// <summary>
        /// دریافت لیست یادآوری‌های تسک
        /// </summary>
        Task<List<TaskReminderViewModel>> GetTaskRemindersListAsync(int taskId);

        /// <summary>
        /// دریافت یادآوری بر اساس شناسه
        /// </summary>
        Task<TaskReminderSchedule> GetReminderByIdAsync(int reminderId);

        /// <summary>
        /// ایجاد یادآوری جدید
        /// </summary>
        Task<int> CreateReminderAsync(TaskReminderViewModel model, string userId);

        /// <summary>
        /// غیرفعال کردن یادآوری
        /// </summary>
        Task<bool> DeactivateReminderAsync(int reminderId);

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال یادآوری
        /// </summary>
        Task<bool> ToggleReminderActiveStatusAsync(int reminderId);

        /// <summary>
        /// دریافت تسک از طریق شناسه (async)
        /// </summary>
        Task<Tasks> GetTaskByIdAsync(int taskId);

        #endregion

        #region Complete Task Methods

        /// <summary>
        /// آماده‌سازی مودال تکمیل تسک
        /// </summary>
        Task<CompleteTaskViewModel> PrepareCompleteTaskModalAsync(int taskId, string userId);

        /// <summary>
        /// ثبت تکمیل تسک
        /// </summary>
        Task<(bool IsSuccess, string ErrorMessage, bool IsFullyCompleted)> CompleteTaskAsync(
            CompleteTaskViewModel model,
            string userId);

        /// <summary>
        /// غیرفعال کردن یادآوری‌های یک تسک
        /// </summary>
        Task<bool> DeactivateAllTaskRemindersAsync(int taskId);

        #endregion

        #region Task Work Log Management - ⭐ جدید اضافه شده

        /// <summary>
        /// ثبت گزارش کار انجام شده روی تسک (سطح کلی تسک)
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر ثبت‌کننده</param>
        /// <param name="workDescription">توضیحات کار انجام شده</param>
        /// <param name="durationMinutes">مدت زمان کار (اختیاری)</param>
        /// <param name="progressPercentage">درصد پیشرفت (اختیاری)</param>
        /// <returns>موفقیت، پیام، و شناسه WorkLog</returns>
        Task<(bool Success, string Message, int? WorkLogId)> AddTaskWorkLogAsync(
            int taskId,
            string userId,
            string workDescription,
            int? durationMinutes = null,
            int? progressPercentage = null);

        /// <summary>
        /// دریافت لیست گزارش کارهای یک تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="take">تعداد رکورد (0 = همه)</param>
        /// <returns>لیست گزارش کارها</returns>
        Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId, int take = 0);

        #endregion

        #region Task Focus Management - ⭐ جدید اضافه شده

        /// <summary>
        /// تنظیم فوکوس کاربر روی یک تسک (فقط یک تسک می‌تواند فوکوس باشد)
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>موفقیت و پیام</returns>
        Task<(bool Success, string Message)> SetTaskFocusAsync(int taskId, string userId);

        /// <summary>
        /// حذف فوکوس از یک تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>موفقیت و پیام</returns>
        Task<(bool Success, string Message)> RemoveTaskFocusAsync(int taskId, string userId);

        /// <summary>
        /// دریافت شناسه تسک فوکوس شده کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>شناسه تسک فوکوس شده یا null</returns>
        Task<int?> GetUserFocusedTaskIdAsync(string userId);

        #endregion
        #region Task Work Log Management

        /// <summary>
        /// آماده‌سازی مودال ثبت کار انجام شده روی تسک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>ViewModel برای مودال یا null در صورت عدم دسترسی</returns>
        Task<TaskWorkLogViewModel?> PrepareLogTaskWorkModalAsync(int taskId, string userId);

        #endregion

        #region Branch and Organization Related Methods

        /// <summary>
        /// دریافت Contactهای شعبه (افراد مرتبط با شعبه)
        /// </summary>
        Task<List<ContactViewModel>> GetBranchContactsAsync(int branchId);

        /// <summary>
        /// دریافت Organizationهای شعبه (سازمان‌های مرتبط با شعبه)
        /// </summary>
        Task<List<OrganizationViewModel>> GetBranchOrganizationsAsync(int branchId);

        /// <summary>
        /// دریافت سازمان‌هایی که یک Contact در آن‌ها عضو است
        /// </summary>
        Task<List<OrganizationViewModel>> GetContactOrganizationsAsync(int contactId);

        #endregion

        Task<List<ContactViewModel>> GetOrganizationContactsAsync(int organizationId);
        /// <summary>
        /// ⭐⭐⭐ دریافت تیم‌های شعبه با اطلاعات کامل مدیر و اعضا
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>لیست تیم‌های شعبه با اطلاعات کامل</returns>
        Task<List<TeamViewModel>> GetBranchTeamsWithManagersAsync(int branchId);
        /// <summary>
        /// دریافت کامنت‌های یک تسک
        /// </summary>
        Task<List<TaskCommentViewModel>> GetTaskCommentsAsync(int taskId);
        Task<TaskCommentAttachment?> GetCommentAttachmentByIdAsync(int attachmentId);


        Task<TaskListViewModel> GetTaskListAsync(
        string userId,
        TaskViewType viewType,
        TaskGroupingType grouping,
        TaskFilterViewModel filters = null);
        /// <summary>
        /// دریافت ViewModel کارت تسک برای نمایش در لیست
        /// </summary>
        Task<TaskCardViewModel> GetTaskCardViewModelAsync(int taskId, string userId);
        /// <summary>
        /// دریافت لیست UserId های اختصاص داده شده به تسک
        /// </summary>
        Task<List<string>> GetTaskAssignedUserIdsAsync(int taskId);

        /// <summary>
        /// دریافت همه کاربران مرتبط با تسک (اعضا + سازنده)
        /// </summary>
        Task<List<string>> GetTaskRelatedUserIdsAsync(int taskId);
        Task<List<Tasks>> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters = null);

        #region Scheduled Tasks Management

        /// <summary>
        /// ایجاد زمان‌بندی تسک و بررسی CreateImmediately
        /// </summary>
        Task<(int ScheduleId, Tasks? ImmediateTask)> CreateScheduledTaskAsync(
            TaskViewModel model,
            string userId);

        /// <summary>
        /// دریافت لیست زمان‌بندی‌های کاربر
        /// </summary>
        Task<ScheduledTaskListViewModel> GetUserScheduledTasksAsync(
            string userId,
            bool isAdmin = false);

        /// <summary>
        /// دریافت جزئیات یک زمان‌بندی
        /// </summary>
        Task<ScheduledTaskCreation?> GetScheduleByIdAsync(int scheduleId);

        /// <summary>
        /// فعال/غیرفعال کردن زمان‌بندی
        /// </summary>
        Task ToggleScheduleAsync(int scheduleId, bool isEnabled);

        /// <summary>
        /// حذف زمان‌بندی (نرم)
        /// </summary>
        Task DeleteScheduleAsync(int scheduleId);

        /// <summary>
        /// دریافت تسک زمان‌بندی شده برای ویرایش
        /// </summary>
        Task<TaskViewModel?> GetScheduledTaskForEditAsync(int scheduleId);

        /// <summary>
        /// بروزرسانی زمان‌بندی موجود
        /// </summary>
        Task<bool> UpdateScheduledTaskAsync(
            int scheduleId,
            TaskViewModel taskModel,
            string userId);

        /// <summary>
        /// دریافت زمان‌بندی‌های آماده برای اجرا توسط Background Service
        /// </summary>
        Task<List<ScheduledTaskCreation>> GetDueScheduledTasksAsync();

        /// <summary>
        /// بروزرسانی وضعیت اجرا پس از ساخت تسک
        /// </summary>
        Task UpdateExecutionStatusAsync(
            int scheduleId,
            bool success,
            string? notes = null);

        /// <summary>
        /// Deserialize کردن JSON به TaskViewModel
        /// </summary>
        TaskViewModel? DeserializeTaskData(string taskDataJson);

        #endregion
    }
}