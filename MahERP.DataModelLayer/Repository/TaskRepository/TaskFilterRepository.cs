using AutoMapper;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository.Tasking;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using static NuGet.Packaging.PackagingConstants;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository برای فیلترینگ و گروه‌بندی تسک‌ها
    /// </summary>
    public class TaskFilterRepository : ITaskFilterRepository
    {
        private readonly AppDbContext _context;
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskVisibilityRepository _visibilityRepository;
        private readonly IMapper _mapper; 

        public TaskFilterRepository(
            AppDbContext context,
            ITaskRepository taskRepository,
            ITaskVisibilityRepository visibilityRepository, IMapper mapper)
        {
            _context = context;
            _taskRepository = taskRepository;
            _visibilityRepository = visibilityRepository;
            _mapper = mapper; 

        }
        /// <summary>
        /// دریافت تسک‌ها برای صفحه Index با فیلترهای مختلف - اصلاح شده
        /// </summary>
        public async Task<TaskIndexViewModel> GetTasksForIndexAsync(string userId, TaskFilterViewModel filters)
        {
            try
            {
                Console.WriteLine($"🔍 GetTasksForIndexAsync START - UserId: {userId}");

                var model = new TaskIndexViewModel
                {
                    UserLoginid = userId,
                    Filters = filters ?? new TaskFilterViewModel()
                };

              
                // بارگذاری تسک‌ها بر اساس ViewType
                await LoadTasksByViewTypeAsync(model, userId);
                Console.WriteLine($"   ✅ Tasks loaded: {model.Tasks?.Count ?? 0}");

                // اعمال فیلترهای اضافی
                if (_taskRepository.HasActiveFilters(model.Filters))
                {
                    model.Tasks = await _taskRepository.ApplyFiltersAsync(model.Tasks, model.Filters);
                    model.HasActiveFilters = true;
                }

                // محاسبه آمار
                model.Statistics = await CalculateStatisticsAsync(userId, model.Tasks);
                Console.WriteLine($"   ✅ Statistics calculated");

                // دریافت تعداد فیلترها
                Console.WriteLine($"   🔍 Calling GetAllFilterCountsAsync...");
                model.FilterCounts = await GetAllFilterCountsAsync(userId);
                Console.WriteLine($"   ✅ FilterCounts received: All={model.FilterCounts.AllVisibleCount}");

                // تعیین فیلتر فعلی
                model.CurrentFilter = DetermineCurrentFilter(model.Filters.ViewType);
                // ⭐⭐⭐ پر کردن FilterResult برای نمایش گروه‌بندی شده

                model.FilterResult = await BuildFilterResultAsync(model, userId);

                // بررسی نهایی
                if (model.FilterCounts == null)
                {
                    Console.WriteLine("⚠️ WARNING: FilterCounts is still NULL!");
                    model.FilterCounts = new TaskFilterCountsViewModel();
                }

                Console.WriteLine($"🔍 GetTasksForIndexAsync END");
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetTasksForIndexAsync: {ex.Message}\n{ex.StackTrace}");
                return new TaskIndexViewModel
                {
                    UserLoginid = userId,
                    Filters = filters ?? new TaskFilterViewModel(),
                    Tasks = new List<TaskViewModel>(),
                    Statistics = new TaskStatisticsViewModel(),
                    FilterCounts = new TaskFilterCountsViewModel(),
                    HasActiveFilters = false,
                    // ⭐ لیست‌های خالی برای جلوگیری از خطا
                    branchListInitial = new List<BranchViewModel>(),
                    TeamsInitial = new List<TeamViewModel>(),
                    UsersInitial = new List<UserViewModelFull>(),
                    TaskCategoryInitial = new List<TaskCategory>(),
                    StakeholdersInitial = new List<StakeholderViewModel>()
                };
            }
        }
        /// <summary>
        /// ساخت FilterResult برای نمایش گروه‌بندی شده تسک‌ها
        /// </summary>
        private async Task<TaskFilterResultViewModel> BuildFilterResultAsync(TaskIndexViewModel model, string userId)
        {
            try
            {
                // اگر گروه‌بندی وجود دارد، از آن استفاده کن
                if (model.GroupedTasks?.TeamTasksGrouped?.Any() == true)
                {
                    return new TaskFilterResultViewModel
                    {
                        FilterName = model.Filters.ViewType switch
                        {
                            TaskViewType.AllTasks => "همه تسک‌ها",
                            TaskViewType.MyTasks => "تسک‌های من",
                            TaskViewType.AssignedToMe => "منتصب به من",
                            TaskViewType.AssignedByMe => "واگذار شده توسط من",
                            TaskViewType.MyTeamsHierarchy => "تسک‌های تیمی",
                            TaskViewType.SupervisedTasks => "تسک‌های نظارتی",
                            _ => "تسک‌ها"
                        },
                        TotalCount = model.Tasks?.Count ?? 0,
                        GroupedTasks = model.GroupedTasks.TeamTasksGrouped
                    };
                }

                // اگر گروه‌بندی وجود ندارد، یک گروه‌بندی ساده بساز
                var tasks =  model.Tasks ?? new List<TaskViewModel>();

                // تبدیل به Entity برای استفاده از متد موجود
                var taskEntities = await _context.Tasks_Tbl
                    .Where(t => tasks.Select(tv => tv.Id).Contains(t.Id))
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Creator)
                    .ToListAsync();

                var groupedTasks = await GroupTasksByTeamAndPersonAsync(taskEntities, userId);

                return new TaskFilterResultViewModel
                {
                    FilterName = model.Filters.ViewType switch
                    {
                        TaskViewType.AllTasks => "همه تسک‌ها",
                        TaskViewType.MyTasks => "تسک‌های من",
                        TaskViewType.AssignedToMe => "منتصب به من",
                        TaskViewType.AssignedByMe => "واگذار شده توسط من",
                        TaskViewType.MyTeamsHierarchy => "تسک‌های تیمی",
                        TaskViewType.SupervisedTasks => "تسک‌های نظارتی",
                        _ => "تسک‌ها"
                    },
                    TotalCount = tasks.Count,
                    GroupedTasks = groupedTasks
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in BuildFilterResultAsync: {ex.Message}");
                return new TaskFilterResultViewModel
                {
                    FilterName = "خطا",
                    TotalCount = 0,
                    GroupedTasks = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
                };
            }
        }
        /// <summary>
        /// بارگذاری تسک‌ها بر اساس نوع نمایش
        /// </summary>
        private async Task LoadTasksByViewTypeAsync(TaskIndexViewModel model, string userId)
        {
            switch (model.Filters.ViewType)
            {
                case TaskViewType.AllTasks:
                    await LoadAllTasksAsync(model, userId);
                    break;

                case TaskViewType.MyTasks:
                    await LoadMyTasksAsync(model, userId);
                    break;

                case TaskViewType.AssignedToMe:
                    await LoadAssignedToMeAsync(model, userId);
                    break;

                case TaskViewType.AssignedByMe:
                    await LoadAssignedByMeAsync(model, userId);
                    break;

                case TaskViewType.MyTeamsHierarchy:
                    await LoadTeamTasksAsync(model, userId);
                    break;

                default:
                    await LoadMyTasksAsync(model, userId);
                    break;
            }
        }
        /// <summary>
         /// بارگذاری تسک‌های منتصب به من
         /// </summary>
        private async Task LoadAssignedToMeAsync(TaskIndexViewModel model, string userId)
        {
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            // ⭐ دریافت Assignments کاربر
            var userAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            !ta.Task.IsDeleted &&
                            ta.Task.CreatorUserId != userId && // ⭐ فقط تسک‌هایی که من نساخته‌ام
                            (
                                !ta.CompletionDate.HasValue ||
                                (ta.CompletionDate.HasValue && ta.CompletionDate >= fiveDaysAgo)
                            ))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var tasks = await _context.Tasks_Tbl
                .Where(t => userAssignments.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = new List<TaskViewModel>();
            foreach (var task in tasks)
            {
                var taskViewModel = await MapToViewModelAsync(task, userId);
                taskViewModels.Add(taskViewModel);
            }

            model.Tasks = taskViewModels;
            model.PendingTasks = taskViewModels.Where(t => !t.CompletionDate.HasValue).ToList();
            model.CompletedTasks = taskViewModels.Where(t => t.CompletionDate.HasValue).ToList();
        }

        /// <summary>
        /// بارگذاری همه تسک‌های مرتبط با کاربر (اصلاح شده - بر اساس CompletionDate)
        /// </summary>
        private async Task LoadAllTasksAsync(TaskIndexViewModel model, string userId)
        {
            Console.WriteLine($"🔍 LoadAllTasksAsync START - UserId: {userId}");

            var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);

            Console.WriteLine($"   ✅ Visible task IDs: {visibleTaskIds.Count}");

            if (!visibleTaskIds.Any())
            {
                model.Tasks = new List<TaskViewModel>();
                return;
            }

            // ⭐⭐⭐ محاسبه 5 روز قبل
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            // 🔹 دریافت تمام TaskAssignments کاربر
            var userAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            visibleTaskIds.Contains(ta.TaskId))
                .Select(ta => new
                {
                    ta.TaskId,
                    ta.CompletionDate
                })
                .ToListAsync();

            // ⭐ تفکیک بر اساس CompletionDate
            var pendingTaskIds = userAssignments
                .Where(a => !a.CompletionDate.HasValue)
                .Select(a => a.TaskId)
                .Distinct()
                .ToList();

            var completedTaskIds = userAssignments
                .Where(a => a.CompletionDate.HasValue && a.CompletionDate >= fiveDaysAgo)
                .Select(a => a.TaskId)
                .Distinct()
                .ToList();

            var allRelevantTaskIds = pendingTaskIds.Union(completedTaskIds).ToList();

            Console.WriteLine($"   📊 Pending: {pendingTaskIds.Count}, Completed (5d): {completedTaskIds.Count}");

            // 🔹 دریافت تسک‌ها
            var allTasks = await _context.Tasks_Tbl
                .Where(t => allRelevantTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            // تبدیل به ViewModel
            var taskViewModels = new List<TaskViewModel>();
            foreach (var task in allTasks)
            {
                var taskViewModel = await MapToViewModelAsync(task, userId);
                taskViewModels.Add(taskViewModel);
            }

            model.Tasks = taskViewModels;

            // ⭐⭐⭐ جداسازی بر اساس CompletionDate کاربر فعلی
            model.PendingTasks = taskViewModels
                .Where(t => !t.CompletionDate.HasValue) // CompletionDate از MapToViewModelAsync
                .ToList();

            model.CompletedTasks = taskViewModels
                .Where(t => t.CompletionDate.HasValue)
                .ToList();

            Console.WriteLine($"📊 END - Pending: {model.PendingTasks.Count}, Completed: {model.CompletedTasks.Count}");
        }


        /// <summary>
        /// بارگذاری تسک‌های من (اصلاح شده)
        /// </summary>
        private async Task LoadMyTasksAsync(TaskIndexViewModel model, string userId)
        {
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            // ⭐ دریافت Assignments کاربر
            var userAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            !ta.Task.IsDeleted &&
                            (
                                !ta.CompletionDate.HasValue ||
                                (ta.CompletionDate.HasValue && ta.CompletionDate >= fiveDaysAgo)
                            ))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var tasks = await _context.Tasks_Tbl
                .Where(t => userAssignments.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = new List<TaskViewModel>();
            foreach (var task in tasks)
            {
                var taskViewModel = await MapToViewModelAsync(task, userId);
                taskViewModels.Add(taskViewModel);
            }

            model.Tasks = taskViewModels;
            model.PendingTasks = taskViewModels.Where(t => !t.CompletionDate.HasValue).ToList();
            model.CompletedTasks = taskViewModels.Where(t => t.CompletionDate.HasValue).ToList();

            await GroupMyTasksAsync(model, userId);
        }

        /// <summary>
        /// بارگذاری تسک‌های واگذار شده توسط من (اصلاح شده)
        /// </summary>
        private async Task LoadAssignedByMeAsync(TaskIndexViewModel model, string userId)
        {
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            // ⭐ تسک‌هایی که من ساخته‌ام و به دیگران داده‌ام
            var myCreatedTasks = await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                           t.CreatorUserId == userId &&
                           t.TaskAssignments.Any(ta => ta.AssignedUserId != null && ta.AssignedUserId != userId))
                .Select(t => t.Id)
                .ToListAsync();

            // ⭐ بررسی CompletionDate برای هر Assignment
            var relevantAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => myCreatedTasks.Contains(ta.TaskId) &&
                            ta.AssignedUserId != userId &&
                            (
                                !ta.CompletionDate.HasValue ||
                                (ta.CompletionDate.HasValue && ta.CompletionDate >= fiveDaysAgo)
                            ))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantAssignments.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = new List<TaskViewModel>();
            foreach (var task in tasks)
            {
                var taskViewModel = await MapToViewModelAsync(task, userId);
                taskViewModels.Add(taskViewModel);
            }

            model.Tasks = taskViewModels;

            // ⭐ برای "واگذار شده توسط من"، باید CompletionDate افراد دیگر را بررسی کنیم
            model.PendingTasks = taskViewModels
                .Where(t => t.AssignmentsTaskUser?.Any(a => 
                    a.AssignedUserId != userId && !a.CompletionDate.HasValue) == true)
                .ToList();

            model.CompletedTasks = taskViewModels
                .Where(t => t.AssignmentsTaskUser?.Any(a => 
                    a.AssignedUserId != userId && a.CompletionDate.HasValue) == true)
                .ToList();

            await GroupAssignedByMeTasksAsync(model, userId);
        }

        /// <summary>
        /// بارگذاری تسک‌های تیم (اصلاح شده)
        /// </summary>
        private async Task LoadTeamTasksAsync(TaskIndexViewModel model, string userId)
        {
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            var userTeamIds = await _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            if (!userTeamIds.Any())
            {
                model.Tasks = new List<TaskViewModel>();
                model.PendingTasks = new List<TaskViewModel>();
                model.CompletedTasks = new List<TaskViewModel>();
                return;
            }

            // ⭐ دریافت تمام Assignments تیمی
            var teamAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => userTeamIds.Contains(ta.AssignedInTeamId ?? 0) &&
                            !ta.Task.IsDeleted &&
                            (
                                !ta.CompletionDate.HasValue ||
                                (ta.CompletionDate.HasValue && ta.CompletionDate >= fiveDaysAgo)
                            ))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var tasks = await _context.Tasks_Tbl
                .Where(t => teamAssignments.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            var taskViewModels = new List<TaskViewModel>();
            foreach (var task in tasks)
            {
                var taskViewModel = await MapToViewModelAsync(task, userId);
                taskViewModels.Add(taskViewModel);
            }

            model.Tasks = taskViewModels;

            // ⭐ برای تسک‌های تیمی، بررسی CompletionDate کاربر فعلی
            model.PendingTasks = taskViewModels.Where(t => !t.CompletionDate.HasValue).ToList();
            model.CompletedTasks = taskViewModels.Where(t => t.CompletionDate.HasValue).ToList();

            await GroupTeamTasksAsync(model, userId);
        }






        /// <summary>
        /// گروه‌بندی تسک‌های من
        /// </summary>
        private async Task GroupMyTasksAsync(TaskIndexViewModel model, string userId)
        {
            model.GroupedTasks = new TaskGroupedViewModel
            {
                MyTasksGrouped = new MyTasksGroupedViewModel()
            };

            model.GroupedTasks.MyTasksGrouped.TasksAssignedToMe = model.Tasks
                .Where(t => t.AssignmentsTaskUser?.Any(a => a.AssignedUserId == userId) == true
                         && t.CreatorUserId != userId)
                .ToList();

            var createdByMe = model.Tasks.Where(t => t.CreatorUserId == userId).ToList();
            var grouped = new Dictionary<AssigneeInfo, List<TaskViewModel>>();

            foreach (var task in createdByMe)
            {
                if (task.AssignmentsTaskUser == null) continue;

                foreach (var assignment in task.AssignmentsTaskUser)
                {
                    var assigneeInfo = new AssigneeInfo
                    {
                        Id = assignment.AssignedUserId,
                        FullName = assignment.AssignedUserName,
                        Type = "User"
                    };

                    if (!grouped.ContainsKey(assigneeInfo))
                        grouped[assigneeInfo] = new List<TaskViewModel>();

                    if (!grouped[assigneeInfo].Any(t => t.Id == task.Id))
                        grouped[assigneeInfo].Add(task);
                }
            }

            model.GroupedTasks.MyTasksGrouped.TasksAssignedByMe = grouped;
        }

        /// <summary>
        /// گروه‌بندی تسک‌های واگذار شده توسط من
        /// </summary>
        private async Task GroupAssignedByMeTasksAsync(TaskIndexViewModel model, string userId)
        {
            model.GroupedTasks = new TaskGroupedViewModel
            {
                MyTasksGrouped = new MyTasksGroupedViewModel()
            };

            var grouped = new Dictionary<AssigneeInfo, List<TaskViewModel>>();

            foreach (var task in model.Tasks)
            {
                if (task.AssignmentsTaskUser == null) continue;

                foreach (var assignment in task.AssignmentsTaskUser)
                {
                    var assigneeInfo = new AssigneeInfo
                    {
                        Id = assignment.AssignedUserId,
                        FullName = assignment.AssignedUserName,
                        Type = "User"
                    };

                    if (!grouped.ContainsKey(assigneeInfo))
                        grouped[assigneeInfo] = new List<TaskViewModel>();

                    if (!grouped[assigneeInfo].Any(t => t.Id == task.Id))
                        grouped[assigneeInfo].Add(task);
                }
            }

            model.GroupedTasks.MyTasksGrouped.TasksAssignedByMe = grouped;
        }

        /// <summary>
        /// گروه‌بندی تسک‌های تیمی
        /// </summary>
        private async Task GroupTeamTasksAsync(TaskIndexViewModel model, string userId)
        {
            model.GroupedTasks = new TaskGroupedViewModel
            {
                TeamTasksGrouped = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
            };

            var userTeams = await _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Team)
                .Distinct()
                .ToListAsync();

            foreach (var team in userTeams)
            {
                var teamMembers = await _context.TeamMember_Tbl
                    .Include(tm => tm.User)
                    .Where(tm => tm.TeamId == team.Id && tm.IsActive)
                    .ToListAsync();

                var teamMemberIds = teamMembers.Select(tm => tm.UserId).ToList();

                var teamTasks = model.Tasks
                    .Where(t => t.AssignmentsTaskUser?.Any(a => teamMemberIds.Contains(a.AssignedUserId)) == true)
                    .ToList();

                if (!teamTasks.Any()) continue;

                var tasksByPerson = new Dictionary<string, List<TaskViewModel>>();

                foreach (var member in teamMembers)
                {
                    var memberTasks = teamTasks
                        .Where(t => t.AssignmentsTaskUser?.Any(a => a.AssignedUserId == member.UserId) == true)
                        .ToList();

                    if (memberTasks.Any())
                    {
                        var memberName = $"{member.User.FirstName} {member.User.LastName}";
                        tasksByPerson[memberName] = memberTasks;
                    }
                }

                if (tasksByPerson.Any())
                {
                    model.GroupedTasks.TeamTasksGrouped[team.Title] = tasksByPerson;
                }
            }
        }


        /// <summary>
        /// محاسبه آمار - اصلاح شده
        /// </summary>
        private async Task<TaskStatisticsViewModel> CalculateStatisticsAsync(string userId, List<TaskViewModel> tasks)
        {
            return new TaskStatisticsViewModel
            {
                TotalTasks = tasks.Count,

                AssignedToMe = tasks.Count(t =>
                    t.AssignmentsTaskUser?.Any(a => a.AssignedUserId == userId) == true
                    && t.CreatorUserId != userId),

                AssignedByMe = tasks.Count(t => t.CreatorUserId == userId),

                // ⭐⭐⭐ اصلاح شده: بررسی CompletionDate کاربر فعالی
                CompletedTasks = tasks.Count(t =>
                    t.AssignmentsTaskUser?.Any(a =>
                        a.AssignedUserId == userId &&
                        a.CompletionDate.HasValue) == true),

                // ⭐⭐⭐ اصلاح شده: عقب افتاده‌ها
                OverdueTasks = tasks.Count(t =>
                {
                    // بررسی اینکه آیا کاربر فعالی تسک را تکمیل کرده؟
                    var myAssignment = t.AssignmentsTaskUser?
                        .FirstOrDefault(a => a.AssignedUserId == userId);

                    var isCompletedByMe = myAssignment?.CompletionDate.HasValue ?? false;

                    return !isCompletedByMe
                           && t.DueDate.HasValue
                           && t.DueDate < DateTime.Now;
                }),

                // ⭐⭐⭐ اصلاح شده: در حال انجام
                InProgressTasks = tasks.Count(t =>
                {
                    var myAssignment = t.AssignmentsTaskUser?
                        .FirstOrDefault(a => a.AssignedUserId == userId);

                    var isCompletedByMe = myAssignment?.CompletionDate.HasValue ?? false;

                    return !isCompletedByMe && t.IsActive;
                }),

                ImportantTasks = tasks.Count(t => t.Important || t.Priority == 1),
                UrgentTasks = tasks.Count(t => t.Priority == 2)
            };
        }

        /// <summary>
        /// تعیین فیلتر سریع فعلی
        /// </summary>
        private QuickFilterType DetermineCurrentFilter(TaskViewType viewType)
        {
            return viewType switch
            {
                TaskViewType.MyTasks => QuickFilterType.MyAssigned,
                TaskViewType.AssignedByMe => QuickFilterType.AssignedByMe,
                TaskViewType.MyTeamsHierarchy => QuickFilterType.MyTeams,
                TaskViewType.AllTasks => QuickFilterType.AllVisible,
                _ => QuickFilterType.AllVisible
            };
        }

        /// <summary>
        /// تبدیل Entity به ViewModel - اصلاح شده برای استفاده از TaskAssignment
        /// </summary>
        private TaskViewModel MapToViewModel(Tasks task, string? currentUserId = null)
        {
            var assignments = _context.TaskAssignment_Tbl
                .Include(ta => ta.AssignedUser)
                .Where(ta => ta.TaskId == task.Id)
                .ToList();

            var category = _context.TaskCategory_Tbl.FirstOrDefault(c => c.Id == task.TaskCategoryId);

            var taskViewModel = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                IsActive = task.IsActive,
                IsDeleted = task.IsDeleted,
                BranchId = task.BranchId,
                CreatorUserId = task.CreatorUserId,
                CategoryId = task.TaskCategoryId,
                CategoryTitle = category?.Title,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,

                AssignmentsTaskUser = assignments.Select(a => new TaskAssignmentViewModel
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}",
                    AssignerUserId = a.AssignerUserId,
                    AssignDate = a.AssignmentDate,
                    CompletionDate = a.CompletionDate
                }).ToList()
            };

            // ⭐ بررسی "روز من" و فوکوس برای کاربر فعالی
            if (!string.IsNullOrEmpty(currentUserId))
            {
                // ⭐⭐⭐ اصلاح شده: بررسی IsInMyDay از طریق TaskAssignment
                var currentUserAssignment = assignments.FirstOrDefault(a => a.AssignedUserId == currentUserId);

                if (currentUserAssignment != null)
                {
                    // بررسی وجود رکورد در TaskMyDay برای این Assignment
                    taskViewModel.IsInMyDay = _context.TaskMyDay_Tbl
                        .AsNoTracking()
                        .Any(tmd => tmd.TaskAssignmentId == currentUserAssignment.Id &&
                                   !tmd.IsRemoved &&
                                   tmd.PlannedDate.Date == DateTime.Now.Date); // ⭐ فقط امروز

                    // بررسی IsFocused از TaskAssignment
                    taskViewModel.IsFocused = currentUserAssignment.IsFocused;
                    taskViewModel.CompletionDate = currentUserAssignment.CompletionDate;
                }
                else
                {
                    // کاربر فعالی عضو این تسک نیست
                    taskViewModel.IsInMyDay = false;
                    taskViewModel.IsFocused = false;
                }
            }

            return taskViewModel;
        }

        /// <summary>
        /// تبدیل Entity به ViewModel (نسخه Async) - اصلاح شده
        /// </summary>
        private async Task<TaskViewModel> MapToViewModelAsync(Tasks task, string? currentUserId = null)
        {
            var assignments = await _context.TaskAssignment_Tbl
                .Include(ta => ta.AssignedUser)
                .Where(ta => ta.TaskId == task.Id)
                .ToListAsync();

            // ⭐ دریافت نام سازنده
            var category = await _context.TaskCategory_Tbl
                .FirstOrDefaultAsync(c => c.Id == task.TaskCategoryId);

            var creator = await _context.Users.FirstOrDefaultAsync(u => u.Id == task.CreatorUserId);

            var taskViewModel = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                IsActive = task.IsActive,
                IsDeleted = task.IsDeleted,
                BranchId = task.BranchId,
                CreatorUserId = task.CreatorUserId,
                CategoryId = task.TaskCategoryId,
                CategoryTitle = category?.Title,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,

                AssignmentsTaskUser = assignments.Select(a => new TaskAssignmentViewModel
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}",
                    AssignerUserId = a.AssignerUserId,
                    AssignedInTeamName = a.AssignedInTeam?.Title, 

                    AssignDate = a.AssignmentDate,
                    CompletionDate = a.CompletionDate
                }).ToList()
            };

            // ⭐ بررسی "روز من" و فوکوس برای کاربر فعالی (نسخه Async)
            if (!string.IsNullOrEmpty(currentUserId))
            {
                // ⭐⭐⭐ اصلاح شده: بررسی IsInMyDay از طریق TaskAssignment
                var currentUserAssignment = assignments.FirstOrDefault(a => a.AssignedUserId == currentUserId);

                if (currentUserAssignment != null)
                {
                    // بررسی وجود رکورد در TaskMyDay برای این Assignment (نسخه Async)
                    taskViewModel.IsInMyDay = await _context.TaskMyDay_Tbl
                        .AsNoTracking()
                        .AnyAsync(tmd => tmd.TaskAssignmentId == currentUserAssignment.Id &&
                                        !tmd.IsRemoved &&
                                        tmd.PlannedDate.Date == DateTime.Now.Date); // ⭐ فقط امروز

                    // بررسی IsFocused از TaskAssignment
                    taskViewModel.IsFocused = currentUserAssignment.IsFocused;
                    taskViewModel.CompletionDate = currentUserAssignment.CompletionDate;
                }
                else
                {
                    // کاربر فعلی عضو این تسک نیست
                    taskViewModel.IsInMyDay = false;
                    taskViewModel.IsFocused = false;
                }
            }

            return taskViewModel;
        }

        #region Helper Methods

        private async Task<List<int>> GetUserBranchIdsAsync(string userId)
        {
            try
            {
                var branchIds = await _context.BranchUser_Tbl
                    .Where(bu => bu.UserId == userId && bu.IsActive)
                    .Select(bu => bu.BranchId)
                    .Distinct()
                    .ToListAsync();

                return branchIds.Any() ? branchIds : new List<int> { 1 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در GetUserBranchIdsAsync: {ex.Message}");
                return new List<int> { 1 };
            }
        }

        private async Task<List<Tasks>> GetTasksByBranchesAsync(List<int> branchIds, bool includeDeleted = false)
        {
            try
            {
                var query = _context.Tasks_Tbl
                    .Where(t => branchIds.Contains(t.BranchId ?? 0))
                    .AsQueryable();

                if (!includeDeleted)
                    query = query.Where(t => !t.IsDeleted);

                return await query
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در GetTasksByBranchesAsync: {ex.Message}");
                return new List<Tasks>();
            }
        }

        #endregion

        #region Implementation of Interface Methods

        public async Task<TaskFilterResultViewModel> GetAllVisibleTasksAsync(string userId)
        {
            try
            {
                var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);

                if (!visibleTaskIds.Any())
                {
                    return new TaskFilterResultViewModel
                    {
                        FilterName = "همه تسک‌ها",
                        TotalCount = 0,
                        GroupedTasks = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
                    };
                }

                var tasks = await _context.Tasks_Tbl
                    .Where(t => visibleTaskIds.Contains(t.Id) && !t.IsDeleted)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Creator)
                    .ToListAsync();

                // ⭐⭐⭐ تبدیل به ViewModel با بررسی IsInMyDay
                var taskViewModels = new List<TaskViewModel>();
                foreach (var task in tasks)
                {
                    var taskViewModel = await MapToViewModelAsync(task, userId);
                    taskViewModels.Add(taskViewModel);
                }

                // گروه‌بندی بر اساس تیم و شخص
                var grouped = await GroupTasksByTeamAndPersonFromViewModelsAsync(taskViewModels, userId);

                return new TaskFilterResultViewModel
                {
                    FilterName = "همه تسک‌ها",
                    TotalCount = tasks.Count,
                    GroupedTasks = grouped
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در GetAllVisibleTasksAsync: {ex.Message}");
                return new TaskFilterResultViewModel();
            }
        }

        /// <summary>
        /// گروه‌بندی تسک‌های ViewModel بر اساس تیم و شخص
        /// </summary>
        private async Task<Dictionary<string, Dictionary<string, List<TaskViewModel>>>> GroupTasksByTeamAndPersonFromViewModelsAsync(
            List<TaskViewModel> tasks,
            string? highlightUserId = null)
        {
            var result = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();

            foreach (var task in tasks)
            {
                var assignments = task.AssignmentsTaskUser?.Where(ta => !string.IsNullOrEmpty(ta.AssignedUserId)).ToList();

                if (!assignments.Any())
                {
                    AddTaskToGroupViewModel(result, "بدون انتساب", "بدون کاربر", task);
                    continue;
                }

                foreach (var assignment in assignments)
                {
                    // دریافت اطلاعات تیم از دیتابیس (اگر نیاز باشد)
                    var teamName = "بدون تیم";

                    // اگر assignment دارای تیم باشد
                    if (!string.IsNullOrEmpty(assignment.AssignedUserId))
                    {
                        // می‌توانید از cache یا repository استفاده کنید
                        var teamInfo = await _context.TaskAssignment_Tbl
                            .Include(ta => ta.AssignedInTeam)
                            .FirstOrDefaultAsync(ta => ta.Id == assignment.Id);

                        teamName = teamInfo?.AssignedInTeam?.Title ?? "بدون تیم";
                    }

                    var personName = assignment.AssignedUserName ?? "نامشخص";

                    if (string.IsNullOrEmpty(personName))
                        personName = "نامشخص";

                    AddTaskToGroupViewModel(result, teamName, personName, task);
                }
            }

            return result;
        }

        /// <summary>
        /// افزودن تسک به گروه (نسخه ViewModel)
        /// </summary>
        private void AddTaskToGroupViewModel(
            Dictionary<string, Dictionary<string, List<TaskViewModel>>> groups,
            string teamName,
            string personName,
            TaskViewModel task)
        {
            if (!groups.ContainsKey(teamName))
            {
                groups[teamName] = new Dictionary<string, List<TaskViewModel>>();
            }

            if (!groups[teamName].ContainsKey(personName))
            {
                groups[teamName][personName] = new List<TaskViewModel>();
            }

            if (!groups[teamName][personName].Any(t => t.Id == task.Id))
            {
                groups[teamName][personName].Add(task);
            }
        }
        public async Task<TaskFilterResultViewModel> GetAssignedByMeTasksAsync(string userId)
        {
            try
            {
                var filters = new TaskFilterViewModel { StatusFilters = new List<byte> { 0, 1 } };

                var tasks = await _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted && t.CreatorUserId == userId && filters.StatusFilters.Contains(t.Status) &&

                               t.TaskAssignments.Any(ta => ta.AssignedUserId != null && ta.AssignedUserId != userId))
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .ToListAsync();

                var grouped = await GroupTasksByTeamAndPersonAsync(tasks);

                return new TaskFilterResultViewModel
                {
                    FilterName = "تسک‌های واگذار شده",
                    TotalCount = tasks.Count,
                    GroupedTasks = grouped
                };
            }
            catch
            {
                return new TaskFilterResultViewModel();
            }
        }

        public async Task<TaskFilterResultViewModel> GetMyAssignedTasksAsync(string userId)
        {
            try
            {
                var filters = new TaskFilterViewModel { StatusFilters = new List<byte> { 0, 1 } };

                var tasks = await _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted &&
                                       filters.StatusFilters.Contains(t.Status) &&

                               t.TaskAssignments.Any(ta => ta.AssignedUserId == userId))
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .ToListAsync();

                var grouped = await GroupTasksByTeamAndPersonAsync(tasks, userId);

                return new TaskFilterResultViewModel
                {
                    FilterName = "تسک‌های من",
                    TotalCount = tasks.Count,
                    GroupedTasks = grouped
                };
            }
            catch
            {
                return new TaskFilterResultViewModel();
            }
        }

        public async Task<TaskFilterResultViewModel> GetMyTeamsTasksAsync(string userId)
        {
            try
            {
                var userTeamIds = await _context.TeamMember_Tbl
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.TeamId)
                    .ToListAsync();

                if (!userTeamIds.Any())
                    return new TaskFilterResultViewModel();

                var tasks = await _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted &&
                               t.TaskAssignments.Any(ta =>
                                   ta.AssignedInTeamId.HasValue &&
                                   userTeamIds.Contains(ta.AssignedInTeamId.Value)))
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .ToListAsync();

                var grouped = await GroupTasksByTeamAndPersonAsync(tasks);

                return new TaskFilterResultViewModel
                {
                    FilterName = "تسک‌های تیمی",
                    TotalCount = tasks.Count,
                    GroupedTasks = grouped
                };
            }
            catch
            {
                return new TaskFilterResultViewModel();
            }
        }
        /// <summary>
        /// دریافت تسک‌های نظارتی - اصلاح شده برای نمایش فقط تسک‌های تیمی
        /// </summary>
        public async Task<TaskFilterResultViewModel> GetSupervisedTasksAsync(string userId)
        {
            try
            {
                Console.WriteLine($"🔍 GetSupervisedTasksAsync START - UserId: {userId}");

                var allVisibleTaskIds = new List<int>();

                // 1️⃣ تسک‌هایی که با AssignmentType = 2 مستقیماً به من منتصب شده‌اند
                var directSupervisedTaskIds = await _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted &&
                               t.TaskAssignments.Any(ta =>
                                   ta.AssignedUserId == userId &&
                                   ta.AssignmentType == 2))
                    .Select(t => t.Id)
                    .ToListAsync();

                allVisibleTaskIds.AddRange(directSupervisedTaskIds);
                Console.WriteLine($"   ✅ Direct supervised: {directSupervisedTaskIds.Count}");

                // 2️⃣ تسک‌های قابل مشاهده بر اساس سمت در تیم (فقط تسک‌های تیمی)
                var userManagedTeamIds = await _context.Team_Tbl
                    .AsNoTracking()
                    .Where(t => t.ManagerUserId == userId && t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                var userTeamIds = await _context.TeamMember_Tbl
                    .AsNoTracking()
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.TeamId)
                    .ToListAsync();

                // تیم‌هایی که عضو هستم اما مدیر نیستم
                var supervisedTeamIds = userTeamIds.Except(userManagedTeamIds).ToList();
                Console.WriteLine($"   📊 Supervised teams: {supervisedTeamIds.Count}");

                if (supervisedTeamIds.Any())
                {
                    foreach (var teamId in supervisedTeamIds)
                    {
                        Console.WriteLine($"   🔍 Processing team: {teamId}");

                        // ⭐⭐⭐ اصلاح شده: فقط تسک‌هایی که به این تیم منتصب شده‌اند
                        var teamTaskIds = await _context.TaskAssignment_Tbl
                            .AsNoTracking()
                            .Where(ta => ta.AssignedInTeamId == teamId &&  // ⭐ کلید اصلی: فقط تسک‌های این تیم
                                        ta.AssignedUserId != userId)        // به جز خودم
                            .Select(ta => ta.TaskId)
                            .Distinct()
                            .ToListAsync();

                        Console.WriteLine($"      📋 Team {teamId} tasks: {teamTaskIds.Count}");

                        if (teamTaskIds.Any())
                        {
                            // دریافت اعضای این تیم (برای فیلتر سمت)
                            var teamMemberUserIds = await _context.TeamMember_Tbl
                                .AsNoTracking()
                                .Where(tm => tm.TeamId == teamId &&
                                            tm.IsActive &&
                                            tm.UserId != userId)
                                .Select(tm => tm.UserId)
                                .Distinct()
                                .ToListAsync();

                            // فیلتر بر اساس سمت (فقط زیردستان و همسطح)
                            var positionFilteredTaskIds = await GetSupervisedVisibleTasksAsync(
                                userId,
                                new List<int> { teamId },
                                teamTaskIds
                            );

                            allVisibleTaskIds.AddRange(positionFilteredTaskIds);
                            Console.WriteLine($"      ✅ Position-filtered tasks: {positionFilteredTaskIds.Count}");
                        }
                    }
                }

                // 3️⃣ تسک‌های قابل مشاهده بر اساس سمت (اضافی از GetPositionBasedVisibleTasksAsync)
                // ⭐ اصلاح: فقط تسک‌های تیمی
                var positionBasedTaskIds = await _visibilityRepository.GetPositionBasedVisibleTasksAsync(userId);
                Console.WriteLine($"   ✅ Additional position-based: {positionBasedTaskIds.Count}");

                // فیلتر: فقط تسک‌هایی که به تیم‌های نظارتی منتصب شده‌اند و خودم نساخته‌ام
                var positionTasksFiltered = await _context.TaskAssignment_Tbl
                    .AsNoTracking()
                    .Where(ta => positionBasedTaskIds.Contains(ta.TaskId) &&
                                supervisedTeamIds.Contains(ta.AssignedInTeamId ?? 0) // ⭐ فقط تیم‌های نظارتی
                                && ta.Task.CreatorUserId != userId &&
                                !ta.Task.IsDeleted)
                    .Select(ta => ta.TaskId)
                    .Distinct()
                    .ToListAsync();

                allVisibleTaskIds.AddRange(positionTasksFiltered);

                // حذف تکراری‌ها
                var uniqueTaskIds = allVisibleTaskIds.Distinct().ToList();
                Console.WriteLine($"   📊 Total unique supervised tasks: {uniqueTaskIds.Count}");

                // دریافت تسک‌های کامل
                var tasks = await _context.Tasks_Tbl
                    .Where(t => uniqueTaskIds.Contains(t.Id) && !t.IsDeleted)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedInTeam)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Creator)
                    .ToListAsync();

                var grouped = await GroupTasksByTeamAndPersonAsync(tasks);

                Console.WriteLine($"🔍 GetSupervisedTasksAsync END - Total: {tasks.Count}");

                return new TaskFilterResultViewModel
                {
                    FilterName = "تسک‌های نظارتی",
                    TotalCount = tasks.Count,
                    GroupedTasks = grouped
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در GetSupervisedTasksAsync: {ex.Message}\n{ex.StackTrace}");
                return new TaskFilterResultViewModel
                {
                    FilterName = "تسک‌های نظارتی",
                    TotalCount = 0,
                    GroupedTasks = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
                };
            }
        }
        /// <summary>
        /// دریافت تعداد تسک‌ها برای همه فیلترها - اصلاح شده و تست شده
        /// </summary>
        public async Task<TaskFilterCountsViewModel> GetAllFilterCountsAsync(string userId)
        {
            try
            {
                var counts = new TaskFilterCountsViewModel();

                // ⭐ AllVisibleCount: همه تسک‌هایی که کاربر می‌تواند ببیند
                var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);
                counts.AllVisibleCount = visibleTaskIds.Count;

                // ⭐ MyAssignedCount: تسک‌هایی که به من منتصب شده‌اند
                counts.MyAssignedCount = await _context.TaskAssignment_Tbl
                    .AsNoTracking()
                    .Where(ta => ta.AssignedUserId == userId)
                    .Select(ta => ta.TaskId)
                    .Distinct()
                    .CountAsync(taskId => _context.Tasks_Tbl.Any(t => t.Id == taskId && !t.IsDeleted));

                // ⭐ AssignedByMeCount: تسک‌هایی که من ساخته‌ام و به دیگران داده‌ام
                counts.AssignedByMeCount = await _context.Tasks_Tbl
                    .AsNoTracking()
                    .Where(t => !t.IsDeleted &&
                               t.CreatorUserId == userId &&
                               t.TaskAssignments.Any(ta => ta.AssignedUserId != null && ta.AssignedUserId != userId))
                    .CountAsync();

                // ⭐ MyTeamsCount: تسک‌های اعضای تیم‌های من که من می‌توانم ببینم
                var userManagedTeamIds = await _context.Team_Tbl
                    .AsNoTracking()
                    .Where(t => t.ManagerUserId == userId && t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                if (userManagedTeamIds.Any())
                {
                    // دریافت اعضای تیم‌های تحت مدیریت (به جز خود کاربر)
                    var teamMemberUserIds = await _context.TeamMember_Tbl
                        .AsNoTracking()
                        .Where(tm => userManagedTeamIds.Contains(tm.TeamId) &&
                                    tm.IsActive &&
                                    tm.UserId != userId)
                        .Select(tm => tm.UserId)
                        .Distinct()
                        .ToListAsync();

                    if (teamMemberUserIds.Any())
                    {
                        // تسک‌های منتصب شده به اعضای تیم
                        var teamTaskIds = await _context.TaskAssignment_Tbl
                            .AsNoTracking()
                            .Where(ta => teamMemberUserIds.Contains(ta.AssignedUserId))
                            .Select(ta => ta.TaskId)
                            .Distinct()
                            .ToListAsync();

                        // فیلتر بر اساس تسک‌هایی که کاربر می‌تواند ببیند
                        counts.MyTeamsCount = teamTaskIds.Intersect(visibleTaskIds).Count();
                    }
                }

                // ⭐ SupervisedCount: تسک‌هایی که من به عنوان ناظر می‌توانم ببینم
                // (اعضای تیم‌هایی که من در آن‌ها عضو هستم، نه مدیر)
                var userTeamIds = await _context.TeamMember_Tbl
                    .AsNoTracking()
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Select(tm => tm.TeamId)
                    .ToListAsync();

                if (userTeamIds.Any())
                {
                    // حذف تیم‌هایی که کاربر مدیر آن‌هاست (برای جلوگیری از تکرار با MyTeamsCount)
                    var supervisedTeamIds = userTeamIds.Except(userManagedTeamIds).ToList();

                    if (supervisedTeamIds.Any())
                    {
                        // ⭐⭐⭐ اصلاح شده: فقط تسک‌های منتصب شده به تیم‌های نظارتی
                        var supervisedTaskIds = await _context.TaskAssignment_Tbl
                            .AsNoTracking()
                            .Where(ta => supervisedTeamIds.Contains(ta.AssignedInTeamId ?? 0) &&  // ⭐ کلید اصلی
                                        ta.AssignedUserId != userId)
                            .Select(ta => ta.TaskId)
                            .Distinct()
                            .ToListAsync();

                        Console.WriteLine($"   📋 Supervised team tasks: {supervisedTaskIds.Count}");

                        if (supervisedTaskIds.Any())
                        {
                            // فیلتر بر اساس سمت و سلسله‌مراتب
                            var supervisedVisibleTaskIds = await GetSupervisedVisibleTasksAsync(
                                userId,
                                supervisedTeamIds,
                                supervisedTaskIds
                            );
                            counts.SupervisedCount = supervisedVisibleTaskIds.Count;
                        }
                    }
                }

                return counts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در GetAllFilterCountsAsync: {ex.Message}");
                return new TaskFilterCountsViewModel();
            }
        }

        /// <summary>
        /// دریافت تسک‌های قابل نظارت بر اساس سمت و سلسله‌مراتب
        /// </summary>
        private async Task<List<int>> GetSupervisedVisibleTasksAsync(string userId, List<int> teamIds, List<int> candidateTaskIds)
        {
            var visibleTaskIds = new List<int>();

            // دریافت عضویت‌های کاربر با سمت
            var memberships = await _context.TeamMember_Tbl
                .AsNoTracking()
                .Include(tm => tm.Position)
                .Where(tm => tm.UserId == userId &&
                            teamIds.Contains(tm.TeamId) &&
                            tm.IsActive &&
                            tm.PositionId.HasValue)
                .ToListAsync();

            foreach (var membership in memberships.Where(m => m.Position != null))
            {
                // تسک‌های زیردستان
                if (membership.Position.CanViewSubordinateTasks)
                {
                    var subordinateTaskIds = await GetSubordinateTaskIdsForSupervisionAsync(membership, candidateTaskIds);
                    visibleTaskIds.AddRange(subordinateTaskIds);
                }

                // تسک‌های همسطح
                if (membership.Position.CanViewPeerTasks)
                {
                    var peerTaskIds = await GetPeerTaskIdsForSupervisionAsync(membership, candidateTaskIds);
                    visibleTaskIds.AddRange(peerTaskIds);
                }
            }

            return visibleTaskIds.Distinct().ToList();
        }

        /// <summary>
        /// دریافت تسک‌های زیردستان برای نظارت
        /// </summary>
        private async Task<List<int>> GetSubordinateTaskIdsForSupervisionAsync(TeamMember membership, List<int> candidateTaskIds)
        {
            var subordinateUserIds = await _context.TeamMember_Tbl
                .AsNoTracking()
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel > membership.Position.PowerLevel)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!subordinateUserIds.Any()) return new List<int>();

            return await _context.TaskAssignment_Tbl
                .AsNoTracking()
                .Where(ta => subordinateUserIds.Contains(ta.AssignedUserId) &&
                            candidateTaskIds.Contains(ta.TaskId))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های همسطح برای نظارت
        /// </summary>
        private async Task<List<int>> GetPeerTaskIdsForSupervisionAsync(TeamMember membership, List<int> candidateTaskIds)
        {
            var peerUserIds = await _context.TeamMember_Tbl
                .AsNoTracking()
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel == membership.Position.PowerLevel &&
                            tm.UserId != membership.UserId)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!peerUserIds.Any()) return new List<int>();

            return await _context.TaskAssignment_Tbl
                .AsNoTracking()
                .Where(ta => peerUserIds.Contains(ta.AssignedUserId) &&
                            candidateTaskIds.Contains(ta.TaskId))
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();
        }
        #endregion

        #region Grouping Helper Methods

        private async Task<Dictionary<string, Dictionary<string, List<TaskViewModel>>>> GroupTasksByTeamAndPersonAsync(
            List<Tasks> tasks,
            string? highlightUserId = null)
        {
            var result = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();

            foreach (var task in tasks)
            {
                var assignments = task.TaskAssignments?.Where(ta => !string.IsNullOrEmpty(ta.AssignedUserId)).ToList()
                    ?? new List<TaskAssignment>();

                if (!assignments.Any())
                {
                    AddTaskToGroup(result, "بدون انتساب", "بدون کاربر", task);
                    continue;
                }

                foreach (var assignment in assignments)
                {
                    var teamName = assignment.AssignedInTeam?.Title ?? "بدون تیم";
                    var personName = assignment.AssignedUser != null
                        ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}".Trim()
                        : "نامشخص";

                    if (string.IsNullOrEmpty(personName))
                        personName = "نامشخص";


                    AddTaskToGroup(result, teamName, personName, task, highlightUserId);
                }
            }

            return result;
        }

        private void AddTaskToGroup(
            Dictionary<string, Dictionary<string, List<TaskViewModel>>>
            groups,
            string teamName,
            string personName,
            Tasks task, string highlightUserId = null)
        {
            if (!groups.ContainsKey(teamName))
            {
                groups[teamName] = new Dictionary<string, List<TaskViewModel>>();
            }

            if (!groups[teamName].ContainsKey(personName))
            {
                groups[teamName][personName] = new List<TaskViewModel>();
            }

            var taskViewModel = MapToViewModel(task, highlightUserId);

            if (!groups[teamName][personName].Any(t => t.Id == taskViewModel.Id))
            {
                groups[teamName][personName].Add(taskViewModel);
            }
        }

        #endregion

        /// <summary>
/// اعمال فیلتر وضعیت در سطح دیتابیس (بهینه‌سازی شده)
/// </summary>
private IQueryable<Tasks> ApplyStatusFilter(IQueryable<Tasks> query, List<byte> statusFilters)
{
    // اگر فیلتر وضعیت تنظیم شده باشد، آن را در Query اعمال کن
    if (statusFilters != null && statusFilters.Any())
    {
        query = query.Where(t => statusFilters.Contains(t.Status));
    }

    return query;
}

/// <summary>
/// دریافت تسک‌ها با فیلتر وضعیت در سطح دیتابیس
/// </summary>
public async Task<TaskFilterResultViewModel> GetAllVisibleTasksAsync(string userId, TaskFilterViewModel filters = null)
{
    try
    {
        // پیش‌فرض: فقط تسک‌های Created (0) و InProgress (1)
        if (filters == null)
        {
            filters = new TaskFilterViewModel { StatusFilters = new List<byte> { 0, 1 } };
        }
        else if (filters.StatusFilters == null || !filters.StatusFilters.Any())
        {
            filters.StatusFilters = new List<byte> { 0, 1 };
        }

        // ⭐ دریافت IDs قابل مشاهده
        var visibleTaskIds = await _visibilityRepository.GetVisibleTaskIdsAsync(userId);

        // ⭐⭐⭐ کوئری اصلی با فیلتر وضعیت در Database Level
        var query = _context.Tasks_Tbl
            .Where(t => visibleTaskIds.Contains(t.Id) && 
                       !t.IsDeleted &&
                       filters.StatusFilters.Contains(t.Status)) // ⭐ فیلتر قبل از ToList
            .Include(t => t.Creator)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.AssignedUser)
            .Include(t => t.TaskCategory)
            .Include(t => t.Team)
            .Include(t => t.Contact)
            .Include(t => t.Organization);

        // ⭐ اجرای Query و گروه‌بندی
        var tasks = await query.OrderBy(t => t.CreateDate).ToListAsync();
                var taskViewModels = new List<TaskViewModel>();
                foreach (var task in tasks)
                {
                    var taskViewModel = await MapToViewModelAsync(task, userId);
                    taskViewModels.Add(taskViewModel);
                }

                // ⭐ گروه‌بندی
                var grouped = await GroupTasksByTeamAndPersonFromViewModelsAsync(taskViewModels, userId);


                return new TaskFilterResultViewModel
        {
            FilterName = "تسک‌های قابل مشاهده",
            TotalCount = tasks.Count,
            GroupedTasks = grouped
                };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR in GetAllVisibleTasksAsync: {ex.Message}\n{ex.StackTrace}");
        return new TaskFilterResultViewModel
        {
            FilterName = "خطا",
            TotalCount = 0,
            GroupedTasks = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
        };
    }
}
    }
}