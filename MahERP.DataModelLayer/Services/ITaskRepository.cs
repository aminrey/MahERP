using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
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
        Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, 
            bool includeAttachments = false, bool includeComments = false);

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
        /// دریافت تسک‌های قابل مشاهده برای کاربر (نسخه Async)
        /// </summary>
        Task<List<Tasks>> GetVisibleTasksForUserAsync(string userId, bool includeDeleted = false);

        /// <summary>
        /// دریافت تسک‌های کاربر با در نظر گیری مجوزهای جدید (نسخه Async)
        /// </summary>
        Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(string userId, bool includeAssigned = true, 
            bool includeCreated = false, bool includeDeleted = false);

        /// <summary>
        /// دریافت تسک‌های شعبه با در نظر گیری مجوزهای جدید (نسخه Async)
        /// </summary>
        Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false);

        /// <summary>
        /// بررسی مجوز مشاهده تسک (نسخه Async)
        /// </summary>
        Task<bool> CanUserViewTaskAsync(string userId, int taskId);

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده برای کاربر (نسخه Sync - برای سازگاری)
        /// </summary>
        List<Tasks> GetVisibleTasksForUser(string userId, bool includeDeleted = false);

        /// <summary>
        /// بررسی مجوز مشاهده تسک (نسخه Sync - برای سازگاری)
        /// </summary>
        bool CanUserViewTask(string userId, int taskId);

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

        #endregion

        #region Statistics and Filter Methods

        /// <summary>
        /// محاسبه آمار تسک‌ها بر اساس سطح دسترسی کاربر
        /// </summary>
        Task<TaskStatisticsViewModel> CalculateTaskStatisticsAsync(string userId, int dataAccessLevel, 
            List<TaskViewModel> filteredTasks);

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
        /// تغییر نوع نمایش
        /// </summary>
        Task<TaskFilterViewModel> ChangeViewTypeAsync(TaskViewType viewType);

        /// <summary>
        /// فیلتر سریع بر اساس وضعیت
        /// </summary>
        Task<TaskFilterViewModel> FilterByStatusAsync(TaskStatusFilter statusFilter, 
            TaskFilterViewModel currentFilters = null);

        /// <summary>
        /// فیلتر سریع بر اساس اولویت
        /// </summary>
        Task<TaskFilterViewModel> FilterByPriorityAsync(TaskPriorityFilter priorityFilter, 
            TaskFilterViewModel currentFilters = null);

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
        /// دریافت تسک‌ها برای صفحه Index
        /// </summary>
        TaskListForIndexViewModel GetTaskForIndexByUser(TaskListForIndexViewModel filterModel);

        /// <summary>
        /// دریافت تسک‌ها برای Index با فیلترها (نسخه Async جدید)
        /// </summary>
        Task<TaskListForIndexViewModel> GetTasksForIndexAsync(string userId, TaskFilterViewModel filters);

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
        /// دریافت تسک‌های تحت نظارت کاربر
        /// </summary>
        Task<TasksListViewModel> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters);

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

        #endregion

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
    }
}