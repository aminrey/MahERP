# راهنمای تقسیم TaskRepository به Partial Classes

## ✅ فایل‌های ایجاد شده:
1. ✅ `TaskRepository.CoreCRUD.cs` - عملیات CRUD اصلی
2. ✅ `TaskRepository.Assignments.cs` - مدیریت انتساب‌ها

## 📋 فایل‌های باقیمانده برای ایجاد:

### 3. TaskRepository.Operations.cs
**محتوا:** متدهای مربوط به TaskOperation
```csharp
- GetTaskOperations
- GetTaskOperationById
- SaveTaskOperations
- CompleteRemainingOperationsAsync
```

### 4. TaskRepository.Categories.cs
**محتوا:** مدیریت دسته‌بندی‌ها
```csharp
- GetAllCategories
- GetCategoryById
- GetTaskCategoriesForStakeholderChangeAsync
```

### 5. TaskRepository.Calendar.cs
**محتوا:** تقویم و رویدادها
```csharp
- GetTasksForCalendarView
- GetCalendarEventsAsync
- AddPersonalEventsToCalendarAsync
- GetTaskStatusColor
- GetTaskStatusTextForCalendar
```

### 6. TaskRepository.Visibility.cs
**محتوا:** دسترسی‌ها و مجوزها
```csharp
- GetVisibleTasksForUserAsync
- GetTasksByUserWithPermissionsAsync
- GetSupervisedTaskIdsAsync
- GetTasksByBranchWithPermissionsAsync
- CanUserViewTaskAsync
- CanUserViewTask (قدیمی)
- GetVisibleTasksForUser (قدیمی)
```

### 7. TaskRepository.Hierarchical.cs
**محتوا:** سلسله مراتب تیمی
```csharp
- GetHierarchicalTasksForUserAsync
- LoadTeamMemberTasks
- LoadSubTeamTasks
- LoadSubTeamTasksRecursive
- MapToTaskViewModel
```

### 8. TaskRepository.Statistics.cs
**محتوا:** آمار و فیلترها
```csharp
- CalculateTaskStatisticsAsync
- ApplyFiltersAsync
- ApplyFiltersToQuery
- HasActiveFilters
- CalculateUserTasksStats
- GetUserTaskStatsAsync
```

### 9. TaskRepository.Dashboard.cs
**محتوا:** داشبورد
```csharp
- GetTaskDashboardDataAsync
- GetTasksAssignedByUserAsync
- GetSupervisedTasksAsync
- GetUrgentTasksAsync
- GetRecentTaskActivitiesAsync
```

### 10. TaskRepository.Reminders.cs
**محتوا:** یادآوری‌ها
```csharp
- SaveTaskReminders
- GetTaskRemindersAsync
- GetTaskRemindersListAsync
- GetReminderByIdAsync
- CreateReminderAsync
- DeactivateReminderAsync
- ToggleReminderActiveStatusAsync
- MarkReminderAsReadAsync
- MarkAllRemindersAsReadAsync
- DeleteReminderAsync
- GetDashboardRemindersAsync
- ApplyReminderSorting
- DeactivateTaskRemindersAsync
- DeactivateAllTaskRemindersAsync
```

### 11. TaskRepository.MyDay.cs
**محتوا:** روز من
```csharp
- AddTaskToMyDayAsync
- LogTaskWorkAsync
- GetMyDayTasksAsync
- IsTaskInMyDayAsync
- RemoveTaskFromMyDayAsync
- GetMyDayTasksCountAsync
- CalculateTaskProgress
```

### 12. TaskRepository.WorkLog.cs
**محتوا:** گزارش کار
```csharp
- AddTaskWorkLogAsync
- GetTaskWorkLogsAsync
- PrepareLogTaskWorkModalAsync
```

### 13. TaskRepository.Focus.cs
**محتوا:** فوکوس تسک
```csharp
- SetTaskFocusAsync
- RemoveTaskFocusAsync
- GetUserFocusedTaskIdAsync
```

### 14. TaskRepository.History.cs
**محتوا:** تاریخچه
```csharp
- GetTaskHistoryAsync
- GetHistoryIcon
- GetHistoryBadgeClass
```

### 15. TaskRepository.Completion.cs
**محتوا:** تکمیل تسک
```csharp
- PrepareCompleteTaskModalAsync
- CompleteTaskAsync
- CompleteRemainingOperationsAsync
```

### 16. TaskRepository.Contacts.cs
**محتوا:** مخاطبین و سازمان‌ها
```csharp
- GetBranchContactsAsync
- GetBranchOrganizationsAsync
- GetContactOrganizationsAsync
- GetOrganizationContactsAsync
- GetTaskCommentsAsync
- GetCommentAttachmentByIdAsync
```

### 17. TaskRepository.Teams.cs
**محتوا:** مدیریت تیم‌ها
```csharp
- GetUsersFromTeamsAsync
- GetUserBranchId
- GetUserBranchIds
- GetUserRelatedTeamsAsync
- GetUserRelatedUsersAsync
- GetBranchTeamsWithManagersAsync
- GetUserTeamsByBranchAsync
```

### 18. TaskRepository.Ajax.cs
**محتوا:** متدهای AJAX
```csharp
- GetBranchChangeDataAsync
- GetBranchTeamsByBranchId
- RepopulateCreateTaskModelAsync
- GetBranchTriggeredDataAsync
- PrepareCreateTaskModelAsync
- GetBranchUsersWithCurrentUserFirstAsync
- PopulateCreateTaskDataAsync
- InitializeEmptyCreateTaskLists
- GetProjectStatsAsync
```

### 19. TaskRepository.Validation.cs
**محتوا:** اعتبارسنجی
```csharp
- ValidateTaskModelAsync
- CreateTaskEntityAsync
- SaveTaskOperationsAndRemindersAsync
- SaveTaskAttachmentsAsync
```

### 20. TaskRepository.Cards.cs
**محتوا:** نمایش کارت تسک‌ها
```csharp
- GetTaskListAsync
- GetTaskCardViewModelAsync
- IsTaskCompletedForUser
- FillLegacyStatsAsync
```

### 21. TaskRepository.Helpers.cs
**محتوا:** متدهای کمکی عمومی
```csharp
- GetUserInitials
- GetTaskStatusText
- GetTaskStatusBadgeClass
- GetCategoryBadgeClass
- GetPriorityText
- GetPriorityBadgeClass
- GetTaskStakeholderName
- GetActivityTitle
- CalculateTimeAgo
- GetActiveRemindersCountAsync
- GetTaskRelatedUserIdsAsync
```

## 🔧 نحوه استفاده:

### مرحله 1: بررسی فایل اصلی
فایل `TaskRepository.cs` اصلی را به این صورت تغییر دهید:

```csharp
public partial class TaskRepository 
{
    // فقط Dependencies و Constructor
}
```

### مرحله 2: ایجاد فایل‌های Partial
هر کدام از فایل‌های بالا را ایجاد کنید با ساختار:

```csharp
namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// توضیحات بخش
    /// </summary>
    public partial class TaskRepository
    {
        // متدهای مربوط به این بخش
    }
}
```

### مرحله 3: انتقال کد
کدهای مربوط به هر بخش را از فایل اصلی کپی کرده و در فایل مربوطه paste کنید.

### مرحله 4: تست
پس از ایجاد هر فایل، Build کنید و مطمئن شوید خطایی ندارید.

## ⚠️ نکات مهم:

1. **همه فایل‌ها باید `partial class` باشند**
2. **namespace همه فایل‌ها یکسان باشد**: `MahERP.DataModelLayer.Repository.Tasking`
3. **using های لازم را در هر فایل اضافه کنید**
4. **متدهای private به public تغییر نمی‌کنند** - در همان فایلی که استفاده می‌شوند بمانند
5. **Dependencies در فایل اصلی تعریف شده‌اند** - نیازی به تکرار نیست

## ✨ مزایای این ساختار:

✅ کد منظم‌تر و قابل خواندن‌تر
✅ دسترسی آسان‌تر به بخش‌های مختلف
✅ تیم‌ها می‌توانند همزمان روی بخش‌های مختلف کار کنند
✅ merge conflict کمتر
✅ تست واحد ساده‌تر
✅ نگهداری و debug راحت‌تر

## 📝 مثال نحوه ایجاد یک فایل:

```csharp
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت دسته‌بندی‌های تسک
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Categories

        public List<TaskCategory> GetAllCategories(bool activeOnly = true)
        {
            var query = _context.TaskCategory_Tbl.AsQueryable();

            if (activeOnly)
                query = query.Where(c => c.IsActive);

            return query.OrderBy(c => c.Title).ToList();
        }

        public TaskCategory GetCategoryById(int id)
        {
            return _context.TaskCategory_Tbl.FirstOrDefault(c => c.Id == id);
        }

        #endregion
    }
}
```

## 🎯 اولویت‌بندی ایجاد فایل‌ها:

1. **اولویت بالا**: CoreCRUD, Assignments, Visibility, Completion
2. **اولویت متوسط**: Calendar, Dashboard, Statistics
3. **اولویت پایین**: بقیه فایل‌ها

---
**نکته**: این فقط یک راهنما است. شما می‌توانید بر اساس نیاز پروژه تغییراتی ایجاد کنید.
