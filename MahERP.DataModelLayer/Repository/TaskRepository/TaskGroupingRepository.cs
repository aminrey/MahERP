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
            string currentUserId,
            TaskViewType? viewType = null)
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

                case TaskGroupingType.Stakeholder:  // ⭐⭐⭐ جدید
                    groups = GroupByStakeholder(tasks);
                    break;
            }

            // ⭐ تبدیل به کارت برای هر گروه
            foreach (var group in groups)
            {
                var groupTasks = tasks.Where(t => IsTaskInGroup(t, group.GroupKey, grouping, currentUserId)).ToList();  // ⭐ پاس دادن currentUserId

                group.PendingTasks = groupTasks
                    .Where(t => !IsTaskCompletedForUser(t.Id, currentUserId))
                    .Select((t, index) => MapToTaskCard(t, index + 1, currentUserId, viewType))  // ⭐ پاس دادن viewType
                    .ToList();

                group.CompletedTasks = groupTasks
                    .Where(t => IsTaskCompletedForUser(t.Id, currentUserId))
                    .Select((t, index) => MapToTaskCard(t, index + 1, currentUserId, viewType))  // ⭐ پاس دادن viewType
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
        /// ⭐⭐⭐ گروه‌بندی بر اساس طرف حساب (Contact/Organization)
        /// </summary>
        public List<TaskGroupViewModel> GroupByStakeholder(List<Tasks> tasks)
        {
            var groups = new List<TaskGroupViewModel>
            {
                new TaskGroupViewModel
                {
                    GroupKey = "no-stakeholder",
                    GroupTitle = "بدون طرف حساب",
                    GroupIcon = "fa-user-slash",
                    GroupBadgeClass = "bg-secondary"
                }
            };

            // گروه‌بندی بر اساس Contact
            var contactGroups = tasks
                .Where(t => t.ContactId.HasValue && t.Contact != null)
                .GroupBy(t => t.ContactId.Value)
                .Select(g =>
                {
                    var contact = g.First().Contact;
                    return new TaskGroupViewModel
                    {
                        GroupKey = $"contact-{g.Key}",
                        GroupTitle = $"{contact.FirstName} {contact.LastName}",
                        GroupIcon = "fa-user",
                        GroupBadgeClass = "bg-primary"
                    };
                });

            // گروه‌بندی بر اساس Organization
            var organizationGroups = tasks
                .Where(t => t.OrganizationId.HasValue && 
                           t.Organization != null && 
                           !t.ContactId.HasValue) // فقط تسک‌هایی که Contact ندارند
                .GroupBy(t => t.OrganizationId.Value)
                .Select(g =>
                {
                    var org = g.First().Organization;
                    return new TaskGroupViewModel
                    {
                        GroupKey = $"organization-{g.Key}",
                        GroupTitle = org.DisplayName,
                        GroupIcon = "fa-building",
                        GroupBadgeClass = "bg-info"
                    };
                });

            groups.AddRange(contactGroups);
            groups.AddRange(organizationGroups);

            return groups.OrderBy(g => g.GroupTitle).ToList();
        }

        /// <summary>
        /// بررسی تعلق تسک به گروه
        /// </summary>
        public bool IsTaskInGroup(Tasks task, string groupKey, TaskGroupingType grouping, string currentUserId = null)
        {
            switch (grouping)
            {
                case TaskGroupingType.Team:
                    // ⭐⭐⭐ اصلاح: گرفتن تیم از AssignedInTeamId نه از task.TeamId
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        var userAssignment = task.TaskAssignments?
                            .FirstOrDefault(a => a.AssignedUserId == currentUserId);

                        if (userAssignment != null)
                        {
                            var teamId = userAssignment.AssignedInTeamId;
                            return groupKey == (teamId.HasValue ? $"team-{teamId}" : "no-team");
                        }
                    }

                    // اگر کاربر فعالی assignment نداشت، از TeamId اصلی تسک استفاده کن
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

                case TaskGroupingType.Stakeholder:  // ⭐⭐⭐ جدید
                    // بدون طرف حساب
                    if (groupKey == "no-stakeholder")
                        return !task.ContactId.HasValue && !task.OrganizationId.HasValue;

                    // Contact
                    if (groupKey.StartsWith("contact-"))
                    {
                        var contactId = int.Parse(groupKey.Replace("contact-", ""));
                        return task.ContactId == contactId;
                    }

                    // Organization
                    if (groupKey.StartsWith("organization-"))
                    {
                        var orgId = int.Parse(groupKey.Replace("organization-", ""));
                        return task.OrganizationId == orgId && !task.ContactId.HasValue;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// تبدیل Task به TaskCard
        /// </summary>
        public TaskCardViewModel MapToTaskCard(Tasks task, int cardNumber, string currentUserId, TaskViewType? viewType = null)
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
                PriorityText = GetTaskTypeText(task.TaskType),
                PriorityBadgeClass = GetTaskTypeBadgeClass(task.TaskType),

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
                CanComplete = task.TaskAssignments?.Any(a => a.AssignedUserId == currentUserId) ?? false,

                // ⭐⭐⭐ دریافت اطلاعات IsInMyDay و IsFocused
                IsInMyDay = IsTaskInMyDay(task.Id, currentUserId),
                IsFocused = IsTaskFocused(task.Id, currentUserId)
            };

            // ⭐⭐⭐ تشخیص نوع نظارت برای تسک‌های نظارتی
            if (viewType == TaskViewType.Supervised)
            {
                var (supervisionType, supervisionReason) = GetSupervisionTypeAndReason(task.Id, currentUserId);
                card.SupervisionType = supervisionType;
                card.SupervisionReason = supervisionReason;
            }

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

        /// <summary>
        /// ⭐⭐⭐ تشخیص نوع نظارت و دلیل آن
        /// </summary>
        private (string supervisionType, string supervisionReason) GetSupervisionTypeAndReason(int taskId, string userId)
        {
            // ========================================
            // 1️⃣ بررسی رونوشت
            // ========================================
            var carbonCopy = _context.TaskViewer_Tbl
                .Include(tv => tv.AddedByUser)
                .FirstOrDefault(tv => tv.TaskId == taskId &&
                          tv.UserId == userId &&
                          tv.IsActive &&
                          (tv.StartDate == null || tv.StartDate <= DateTime.Now) &&
                          (tv.EndDate == null || tv.EndDate > DateTime.Now));

            if (carbonCopy != null)
            {
                var adderName = carbonCopy.AddedByUser != null
                    ? $"{carbonCopy.AddedByUser.FirstName} {carbonCopy.AddedByUser.LastName}"
                    : "نامشخص";

                var addedDatePersian = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                    carbonCopy.AddedDate, "yyyy/MM/dd");

                return ("carbon-copy", $"شما توسط {adderName} در تاریخ {addedDatePersian} به این تسک رونوشت شده‌اید");
            }

            // ========================================
            // 2️⃣ نظارت سیستمی - تشخیص دلیل
            // ========================================
            var task = _context.Tasks_Tbl
                .Include(t => t.Team)
                .Include(t => t.Creator)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedInTeam)
                .FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                return ("system", "ناظر سیستمی");

            var reasons = new List<string>();

            // ⭐ 1. سازنده تسک
            if (task.CreatorUserId == userId)
            {
                reasons.Add("شما سازنده این تسک هستید");
            }

            // ⭐ 2. مدیر تیم تسک
            if (task.Team != null && task.Team.ManagerUserId == userId)
            {
                reasons.Add($"شما مدیر تیم «{task.Team.Title}» هستید");
            }

            // ⭐ 3. دریافت تیم‌های کاربر
            var userTeamMemberships = _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .Include(tm => tm.Position)
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .ToList();

            // ⭐⭐⭐ 4. بررسی نظارت بر اساس تیم assigned users
            foreach (var assignment in task.TaskAssignments ?? new List<TaskAssignment>())
            {
                if (assignment.AssignedInTeamId.HasValue)
                {
                    var assignedTeam = assignment.AssignedInTeam;
                    var assignedUserName = assignment.AssignedUser != null
                        ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}"
                        : "نامشخص";

                    // بررسی مدیر تیم عضو
                    if (assignedTeam != null && assignedTeam.ManagerUserId == userId)
                    {
                        reasons.Add($"شما مدیر تیم «{assignedTeam.Title}» هستید که {assignedUserName} در آن عضو است");
                    }

                    // بررسی سمت بالاتر در تیم
                    var userMembershipInSameTeam = userTeamMemberships.FirstOrDefault(tm => tm.TeamId == assignment.AssignedInTeamId.Value);

                    if (userMembershipInSameTeam != null && userMembershipInSameTeam.Position != null)
                    {
                        var assignedUserMembership = _context.TeamMember_Tbl
                            .Include(tm => tm.Position)
                            .FirstOrDefault(tm => tm.UserId == assignment.AssignedUserId && tm.TeamId == assignment.AssignedInTeamId.Value && tm.IsActive);

                        if (assignedUserMembership != null && assignedUserMembership.Position != null)
                        {
                            // ⭐ اصلاح: PowerLevel کمتر = قدرت بیشتر
                            if (userMembershipInSameTeam.Position.PowerLevel < assignedUserMembership.Position.PowerLevel)
                            {
                                reasons.Add($"شما در تیم «{assignedTeam.Title}» سمت بالاتر از {assignedUserName} دارید");
                            }
                        }
                    }

                    // ⭐⭐⭐ بررسی ناظر رسمی (MembershipType = 1)
                    var isFormalSupervisorInTeam = userTeamMemberships.Any(tm => tm.TeamId == assignment.AssignedInTeamId.Value && tm.MembershipType == 1);

                    if (isFormalSupervisorInTeam)
                    {
                        reasons.Add($"شما ناظر رسمی تیم «{assignedTeam?.Title}» هستید که {assignedUserName} در آن عضو است");
                    }
                }
            }

            // ⭐⭐⭐ 5. بررسی نظارت بر اساس همه اعضای تیم‌های کاربر (حتی اگر assigned نباشند)
            foreach (var userMembership in userTeamMemberships)
            {
                // 5.1 - اگر سمت بالاتر دارید
                if (userMembership.Position != null && userMembership.Position.CanViewSubordinateTasks)
                {
                    // پیدا کردن زیردستان در این تیم
                    var subordinatesInTeam = _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == userMembership.TeamId &&
                                    tm.IsActive &&
                                    tm.UserId != userId &&
                                    tm.Position != null &&
                                    tm.Position.PowerLevel > userMembership.Position.PowerLevel)
                        .ToList();

                    // بررسی آیا هیچ‌کدام از زیردستان در این تسک assign شده‌اند؟
                    var assignedSubordinates = task.TaskAssignments?
                        .Where(a => subordinatesInTeam.Any(s => s.UserId == a.AssignedUserId))
                        .ToList();

                    if (assignedSubordinates != null && assignedSubordinates.Any())
                    {
                        var subNames = assignedSubordinates
                            .Select(a => a.AssignedUser != null 
                                ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" 
                                : "نامشخص")
                            .Distinct()
                            .Take(3); // فقط 3 نفر اول

                        var namesList = string.Join("، ", subNames);
                        if (assignedSubordinates.Count > 3)
                            namesList += $" و {assignedSubordinates.Count - 3} نفر دیگر";

                        reasons.Add($"شما در تیم «{userMembership.Team.Title}» سمت بالاتر از {namesList} دارید");
                    }
                }

                // 5.2 - اگر ناظر رسمی هستید
                if (userMembership.MembershipType == 1)
                {
                    // پیدا کردن اعضای عادی تیم
                    var normalMembersInTeam = _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == userMembership.TeamId &&
                                    tm.IsActive &&
                                    tm.UserId != userId &&
                                    tm.MembershipType == 0)
                        .Select(tm => tm.UserId)
                        .ToList();

                    // بررسی آیا هیچ‌کدام از اعضای عادی در این تسک assign شده‌اند؟
                    var assignedMembers = task.TaskAssignments?
                        .Where(a => normalMembersInTeam.Contains(a.AssignedUserId))
                        .ToList();

                    if (assignedMembers != null && assignedMembers.Any())
                    {
                        reasons.Add($"شما ناظر رسمی تیم «{userMembership.Team.Title}» هستید");
                    }
                }
            }

            // برگرداندن دلایل
            if (reasons.Any())
            {
                return ("system", string.Join(" و ", reasons));
            }

            return ("system", "ناظر سیستمی");
        }


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

        private string GetTaskTypeText(byte taskType)
        {
            if (taskType == 2) return "اضطراری";
            if (taskType == 1) return "مهم";
            return "عادی";
        }

        private string GetTaskTypeBadgeClass(byte taskType)
        {
            if (taskType == 2) return "bg-danger";
            if (taskType == 1) return "bg-warning";
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

        /// <summary>
        /// ⭐⭐⭐ بررسی آیا تسک در "روز من" کاربر است
        /// </summary>
        private bool IsTaskInMyDay(int taskId, string userId)
        {
            var today = DateTime.Now.Date;
            
            return _context.TaskMyDay_Tbl
                .Any(tmd => tmd.TaskAssignment.TaskId == taskId &&
                           tmd.TaskAssignment.AssignedUserId == userId &&
                           !tmd.IsRemoved &&
                           tmd.PlannedDate.Date == today);
        }

        /// <summary>
        /// ⭐⭐⭐ بررسی آیا تسک فوکوس کاربر است
        /// </summary>
        private bool IsTaskFocused(int taskId, string userId)
        {
            return _context.TaskAssignment_Tbl
                .Any(ta => ta.TaskId == taskId &&
                          ta.AssignedUserId == userId &&
                          ta.IsFocused);
        }

        #endregion
    }
}