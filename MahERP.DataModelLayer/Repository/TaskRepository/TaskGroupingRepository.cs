using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository مسئول گروه‌بندی تسک‌ها
    /// </summary>
    public class TaskGroupingRepository : ITaskGroupingRepository
    {
        private readonly AppDbContext _context;

        public TaskGroupingRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// گروه‌بندی تسک‌ها بر اساس نوع انتخابی
        /// </summary>
        public async Task<List<TaskGroupViewModel>> GroupTasksAsync(
            List<Tasks> tasks,
            TaskGroupingType grouping,
            string currentUserId)
        {
            var groups = new List<TaskGroupViewModel>();

            switch (grouping)
            {
                case TaskGroupingType.Team:
                    groups = await GroupByTeamAsync(tasks, currentUserId);
                    break;

                case TaskGroupingType.Creator:
                    groups = GroupByCreator(tasks);
                    break;

                case TaskGroupingType.CreateDate:
                    groups = GroupByCreateDate(tasks);
                    break;

                case TaskGroupingType.DueDate:
                    groups = GroupByDueDate(tasks);
                    break;

                case TaskGroupingType.Priority:
                    groups = GroupByPriority(tasks);
                    break;
            }

            // ⭐ تبدیل به کارت برای هر گروه
            foreach (var group in groups)
            {
                var groupTasks = tasks.Where(t => IsTaskInGroup(t, group.GroupKey, grouping)).ToList();

                group.PendingTasks = groupTasks
                    .Where(t => !IsTaskCompletedForUser(t.Id, currentUserId))
                    .Select((t, index) => MapToTaskCard(t, index + 1, currentUserId))
                    .ToList();

                group.CompletedTasks = groupTasks
                    .Where(t => IsTaskCompletedForUser(t.Id, currentUserId))
                    .Select((t, index) => MapToTaskCard(t, index + 1, currentUserId))
                    .ToList();
            }

            return groups.Where(g => g.TotalTasks > 0).ToList();
        }

        /// <summary>
        /// گروه‌بندی بر اساس تیم
        /// </summary>
        public async Task<List<TaskGroupViewModel>> GroupByTeamAsync(List<Tasks> tasks, string userId)
        {
            var groups = new List<TaskGroupViewModel>
            {
                new TaskGroupViewModel
                {
                    GroupKey = "no-team",
                    GroupTitle = "بدون تیم",
                    GroupIcon = "fa-user",
                    GroupBadgeClass = "bg-secondary"
                }
            };

            var userTeams = await _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Team)
                .Distinct()
                .ToListAsync();

            foreach (var team in userTeams)
            {
                groups.Add(new TaskGroupViewModel
                {
                    GroupKey = $"team-{team.Id}",
                    GroupTitle = team.Title,
                    GroupIcon = "fa-users",
                    GroupBadgeClass = "bg-primary"
                });
            }

            return groups;
        }

        /// <summary>
        /// گروه‌بندی بر اساس سازنده
        /// </summary>
        public List<TaskGroupViewModel> GroupByCreator(List<Tasks> tasks)
        {
            return tasks
                .GroupBy(t => t.CreatorUserId)
                .Select(g => new TaskGroupViewModel
                {
                    GroupKey = $"creator-{g.Key}",
                    GroupTitle = GetUserFullName(g.Key),
                    GroupIcon = "fa-user-tie",
                    GroupBadgeClass = "bg-info"
                })
                .ToList();
        }

        /// <summary>
        /// گروه‌بندی بر اساس زمان ساخت
        /// </summary>
        public List<TaskGroupViewModel> GroupByCreateDate(List<Tasks> tasks)
        {
            return new List<TaskGroupViewModel>
            {
                new TaskGroupViewModel { GroupKey = "today", GroupTitle = "امروز", GroupIcon = "fa-calendar-day", GroupBadgeClass = "bg-success" },
                new TaskGroupViewModel { GroupKey = "this-week", GroupTitle = "این هفته", GroupIcon = "fa-calendar-week", GroupBadgeClass = "bg-primary" },
                new TaskGroupViewModel { GroupKey = "this-month", GroupTitle = "این ماه", GroupIcon = "fa-calendar-alt", GroupBadgeClass = "bg-info" },
                new TaskGroupViewModel { GroupKey = "older", GroupTitle = "قدیمی‌تر", GroupIcon = "fa-calendar", GroupBadgeClass = "bg-secondary" }
            };
        }

        /// <summary>
        /// گروه‌بندی بر اساس زمان پایان
        /// </summary>
        public List<TaskGroupViewModel> GroupByDueDate(List<Tasks> tasks)
        {
            return new List<TaskGroupViewModel>
            {
                new TaskGroupViewModel { GroupKey = "overdue", GroupTitle = "عقب افتاده", GroupIcon = "fa-exclamation-triangle", GroupBadgeClass = "bg-danger" },
                new TaskGroupViewModel { GroupKey = "today", GroupTitle = "امروز", GroupIcon = "fa-calendar-check", GroupBadgeClass = "bg-warning" },
                new TaskGroupViewModel { GroupKey = "this-week", GroupTitle = "این هفته", GroupIcon = "fa-calendar-week", GroupBadgeClass = "bg-primary" },
                new TaskGroupViewModel { GroupKey = "later", GroupTitle = "بعداً", GroupIcon = "fa-calendar", GroupBadgeClass = "bg-info" },
                new TaskGroupViewModel { GroupKey = "no-deadline", GroupTitle = "بدون مهلت", GroupIcon = "fa-calendar-times", GroupBadgeClass = "bg-secondary" }
            };
        }

        /// <summary>
        /// گروه‌بندی بر اساس اولویت
        /// </summary>
        public List<TaskGroupViewModel> GroupByPriority(List<Tasks> tasks)
        {
            return new List<TaskGroupViewModel>
            {
                new TaskGroupViewModel { GroupKey = "urgent", GroupTitle = "فوری", GroupIcon = "fa-fire", GroupBadgeClass = "bg-danger" },
                new TaskGroupViewModel { GroupKey = "important", GroupTitle = "مهم", GroupIcon = "fa-star", GroupBadgeClass = "bg-warning" },
                new TaskGroupViewModel { GroupKey = "normal", GroupTitle = "عادی", GroupIcon = "fa-circle", GroupBadgeClass = "bg-primary" }
            };
        }

        /// <summary>
        /// بررسی تعلق تسک به گروه
        /// </summary>
        public bool IsTaskInGroup(Tasks task, string groupKey, TaskGroupingType grouping)
        {
            switch (grouping)
            {
                case TaskGroupingType.Team:
                    return groupKey == (task.TeamId.HasValue ? $"team-{task.TeamId}" : "no-team");

                case TaskGroupingType.Creator:
                    return groupKey == $"creator-{task.CreatorUserId}";

                case TaskGroupingType.CreateDate:
                    var createDate = task.CreateDate.Date;
                    var today = DateTime.Now.Date;
                    var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                    var thisMonthStart = new DateTime(today.Year, today.Month, 1);

                    return groupKey switch
                    {
                        "today" => createDate == today,
                        "this-week" => createDate >= thisWeekStart && createDate < today,
                        "this-month" => createDate >= thisMonthStart && createDate < thisWeekStart,
                        "older" => createDate < thisMonthStart,
                        _ => false
                    };

                case TaskGroupingType.DueDate:
                    if (!task.DueDate.HasValue)
                        return groupKey == "no-deadline";

                    var dueDate = task.DueDate.Value.Date;
                    var todayDue = DateTime.Now.Date;
                    var thisWeekEndDue = todayDue.AddDays(7);

                    return groupKey switch
                    {
                        "overdue" => dueDate < todayDue,
                        "today" => dueDate == todayDue,
                        "this-week" => dueDate > todayDue && dueDate <= thisWeekEndDue,
                        "later" => dueDate > thisWeekEndDue,
                        _ => false
                    };

                case TaskGroupingType.Priority:
                    if (task.Priority == 2) return groupKey == "urgent";
                    if (task.Important || task.Priority == 1) return groupKey == "important";
                    return groupKey == "normal";

                default:
                    return false;
            }
        }

        /// <summary>
        /// تبدیل Task به TaskCard
        /// </summary>
        public TaskCardViewModel MapToTaskCard(Tasks task, int cardNumber, string currentUserId)
        {
            var card = new TaskCardViewModel
            {
                Id = task.Id,
                CardNumber = cardNumber,
                TaskCode = task.TaskCode,
                Title = task.Title,
                ShortDescription = task.Description?.Length > 100
                    ? task.Description.Substring(0, 100) + "..."
                    : task.Description,

                CategoryTitle = task.TaskCategory?.Title ?? "عمومی",
                CategoryBadgeClass = "bg-info",

                StakeholderName = GetStakeholderName(task),
                CreatorName = GetUserFullName(task.CreatorUserId),
                CreatorAvatar = GetUserAvatar(task.CreatorUserId),

                Status = task.Status,
                StatusText = GetTaskStatusText(task.Status),
                StatusBadgeClass = GetTaskStatusBadgeClass(task.Status),
                IsCompleted = IsTaskCompletedForUser(task.Id, currentUserId),
                IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.Now && !IsTaskCompletedForUser(task.Id, currentUserId),

                Priority = task.Priority,
                Important = task.Important,
                PriorityText = GetPriorityText(task.Priority, task.Important),
                PriorityBadgeClass = GetPriorityBadgeClass(task.Priority, task.Important),

                CreateDate = task.CreateDate,
                CreateDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.CreateDate, "yyyy/MM/dd"),
                DueDate = task.DueDate,
                DueDatePersian = task.DueDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy/MM/dd")
                    : null,
                DaysRemaining = task.DueDate.HasValue
                    ? (int)(task.DueDate.Value.Date - DateTime.Now.Date).TotalDays
                    : null,

                TotalOperations = task.TaskOperations?.Count ?? 0,
                CompletedOperations = task.TaskOperations?.Count(o => o.IsCompleted) ?? 0,
                ProgressPercentage = CalculateProgress(task),

                CanEdit = task.CreatorUserId == currentUserId,
                CanDelete = task.CreatorUserId == currentUserId,
                CanComplete = task.TaskAssignments?.Any(a => a.AssignedUserId == currentUserId) ?? false
            };

            // ⭐ اعضای تسک
            if (task.TaskAssignments != null)
            {
                card.TotalMembers = task.TaskAssignments.Count;
                card.Members = task.TaskAssignments
                    .Take(4)
                    .Select(a => new TaskMemberAvatarViewModel
                    {
                        UserId = a.AssignedUserId,
                        FullName = GetUserFullName(a.AssignedUserId),
                        Initials = GetUserInitials(a.AssignedUser?.FirstName, a.AssignedUser?.LastName),
                        ProfileImagePath = GetUserAvatar(a.AssignedUserId),
                        TooltipText = GetUserFullName(a.AssignedUserId)
                    })
                    .ToList();
            }

            return card;
        }

        #region Helper Methods

        private string GetStakeholderName(Tasks task)
        {
            if (task.Contact != null)
                return $"{task.Contact.FirstName} {task.Contact.LastName}";

            if (task.Organization != null)
                return task.Organization.DisplayName;

            return "ندارد";
        }

        private string GetUserFullName(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user != null ? $"{user.FirstName} {user.LastName}" : "نامشخص";
        }

        private string GetUserAvatar(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.ProfileImagePath ?? "/images/default-avatar.png";
        }

        private string GetUserInitials(string firstName, string lastName)
        {
            var initials = "";
            if (!string.IsNullOrEmpty(firstName)) initials += firstName[0];
            if (!string.IsNullOrEmpty(lastName)) initials += lastName[0];
            return string.IsNullOrEmpty(initials) ? "کاربر" : initials;
        }

        private bool IsTaskCompletedForUser(int taskId, string userId)
        {
            return _context.TaskAssignment_Tbl
                .Any(a => a.TaskId == taskId &&
                         a.AssignedUserId == userId &&
                         a.CompletionDate.HasValue);
        }

        private int CalculateProgress(Tasks task)
        {
            if (task.TaskOperations == null || !task.TaskOperations.Any())
                return 0;

            var completed = task.TaskOperations.Count(o => o.IsCompleted);
            return (int)((double)completed / task.TaskOperations.Count * 100);
        }

        private string GetPriorityText(byte priority, bool important)
        {
            if (priority == 2) return "فوری";
            if (important || priority == 1) return "مهم";
            return "عادی";
        }

        private string GetPriorityBadgeClass(byte priority, bool important)
        {
            if (priority == 2) return "bg-danger";
            if (important || priority == 1) return "bg-warning";
            return "bg-primary";
        }

        private string GetTaskStatusText(byte status)
        {
            return status switch
            {
                0 => "ایجاد شده",
                1 => "در حال انجام",
                2 => "تکمیل شده",
                3 => "تأیید شده",
                4 => "رد شده",
                5 => "در انتظار",
                _ => "نامشخص"
            };
        }

        private string GetTaskStatusBadgeClass(byte status)
        {
            return status switch
            {
                0 => "bg-secondary",
                1 => "bg-warning",
                2 => "bg-success",
                3 => "bg-info",
                4 => "bg-danger",
                5 => "bg-primary",
                _ => "bg-dark"
            };
        }

        #endregion
    }
}