using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        private readonly IBranchRepository _BranchRipository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStakeholderRepository _StakeholderRepo;
        private readonly IUserManagerRepository _userManagerRepository;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        private readonly ITaskVisibilityRepository _taskVisibilityRepository;

        public TaskRepository(AppDbContext context, IBranchRepository branchRipository, IUnitOfWork unitOfWork, 
            IUserManagerRepository userManagerRepository, IStakeholderRepository stakeholderRepo, 
            TaskCodeGenerator taskCodeGenerator, ITaskVisibilityRepository taskVisibilityRepository)
        {
            _context = context;
            _BranchRipository = branchRipository;
            _unitOfWork = unitOfWork;
            _userManagerRepository = userManagerRepository;
            _StakeholderRepo = stakeholderRepo;
            _taskCodeGenerator = taskCodeGenerator;
            _taskVisibilityRepository = taskVisibilityRepository;
        }

        #region Core CRUD Operations

        public TaskListForIndexViewModel GetTaskForIndexByUser(TaskListForIndexViewModel filterModel)
        {
            string userId = filterModel.UserLoginid;

            var taskForIndexViewModel = new TaskListForIndexViewModel
            {
                branchListInitial = _BranchRipository.GetBrnachListByUserId(userId),
                TaskCategoryInitial = GetAllCategories(),
                UsersInitial = _userManagerRepository.GetUserListBybranchId(0),
                StakeholdersInitial = _StakeholderRepo.GetStakeholdersByBranchId(0),
                Tasks = new List<TaskViewModel>()
            };

            // اصلاح کوئری LINQ برای جلوگیری از مشکل anonymous type
            var tasksQuery = from ts in _context.TaskAssignment_Tbl
                             join t in _context.Tasks_Tbl on ts.TaskId equals t.Id
                             join cate in _context.TaskCategory_Tbl on t.TaskCategoryId equals cate.Id into categoryJoin
                             from category in categoryJoin.DefaultIfEmpty()
                             where ts.AssignedUserId == userId && !t.IsDeleted
                             select new { t, category };

            var tasks = tasksQuery.ToList().Select(item => new TaskViewModel
            {
                Id = item.t.Id,
                Title = item.t.Title ?? string.Empty,
                Description = item.t.Description,
                TaskCode = item.t.TaskCode,
                CreateDate = item.t.CreateDate,
                DueDate = item.t.DueDate,
                CompletionDate = item.t.CompletionDate,
                ManagerApprovedDate = item.t.ManagerApprovedDate,
                SupervisorApprovedDate = item.t.SupervisorApprovedDate,
                IsActive = item.t.IsActive,
                IsDeleted = item.t.IsDeleted,
                BranchId = item.t.BranchId,
                CategoryId = item.category?.Id,
                CategoryTitle = item.category?.Title,
                CreatorUserId = item.t.CreatorUserId,
                StakeholderId = item.t.StakeholderId,
                TaskType = item.t.TaskType
            }).ToList();

            taskForIndexViewModel.Tasks = tasks;
            taskForIndexViewModel.TotalCount = tasks.Count;

            return taskForIndexViewModel;
        }

        public TaskViewModel CreateTaskAndCollectData(string UserId)
        {
            var Tasks = new TaskViewModel();
            Tasks.branchListInitial = _BranchRipository.GetBrnachListByUserId(UserId);
            Tasks.TaskCategoryInitial = GetAllCategories();
            Tasks.UsersInitial = _userManagerRepository.GetUserListBybranchId(0);

            // تولید کد تسک اتوماتیک
            Tasks.TaskCode = _taskCodeGenerator.GenerateTaskCode();

            // تنظیمات کد تسک
            Tasks.TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings();

            // مقدار پیش‌فرض برای ورود دستی کد
            Tasks.IsManualTaskCode = false;

            return Tasks;
        }

        public List<Tasks> GetTasks(bool includeDeleted = false, int? categoryId = null, string assignedUserId = null)
        {
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            if (categoryId.HasValue)
                query = query.Where(t => t.TaskCategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(assignedUserId))
            {
                query = query.Where(t => _context.TaskAssignment_Tbl
                    .Any(a => a.TaskId == t.Id && a.AssignedUserId == assignedUserId));
            }

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, bool includeAttachments = false, bool includeComments = false)
        {
            var query = _context.Tasks_Tbl.AsQueryable();

            if (includeOperations)
                query = query.Include(t => t.TaskOperations);

            if (includeAssignments)
                query = query.Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser);

            if (includeAttachments)
                query = query.Include(t => t.TaskAttachments);

            if (includeComments)
                query = query.Include(t => t.TaskComments)
                    .ThenInclude(c => c.Creator);

            return query.FirstOrDefault(t => t.Id == id);
        }

        public List<Tasks> GetTasksByUser(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId);
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId));
            }
            else if (includeCreated)
            {
                query = query.Where(t => t.CreatorUserId == userId);
            }

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.BranchId == branchId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.StakeholderId == stakeholderId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        #endregion

        #region Task Validation Methods

        public bool IsTaskCodeUnique(string taskCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return true;

            var query = _context.Tasks_Tbl.Where(t => t.TaskCode == taskCode);

            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);

            return !query.Any();
        }

        public bool ValidateTaskCode(string taskCode, int? excludeId = null)
        {
            return _taskCodeGenerator.ValidateTaskCode(taskCode, excludeId);
        }

        public bool IsUserRelatedToTask(string userId, int taskId)
        {
            return _context.Tasks_Tbl.Any(t => t.Id == taskId &&
                (t.CreatorUserId == userId ||
                 _context.TaskAssignment_Tbl.Any(a => a.TaskId == taskId && a.AssignedUserId == userId)));
        }

        public bool IsTaskInBranch(int taskId, int branchId)
        {
            return _context.Tasks_Tbl.Any(t => t.Id == taskId && t.BranchId == branchId);
        }

        #endregion

        #region Task Operations

        public List<TaskOperation> GetTaskOperations(int taskId, bool includeCompleted = true)
        {
            var query = _context.TaskOperation_Tbl.Where(o => o.TaskId == taskId);

            if (!includeCompleted)
                query = query.Where(o => !o.IsCompleted);

            return query.OrderBy(o => o.OperationOrder).ToList();
        }

        public TaskOperation GetTaskOperationById(int id)
        {
            return _context.TaskOperation_Tbl.FirstOrDefault(o => o.Id == id);
        }

        #endregion

        #region Task Assignments

        public List<TaskAssignment> GetTaskAssignments(int taskId)
        {
            return _context.TaskAssignment_Tbl
                .Include(a => a.AssignedUser)
                .Where(a => a.TaskId == taskId)
                .ToList();
        }

        public TaskAssignment GetTaskAssignmentById(int id)
        {
            return _context.TaskAssignment_Tbl
                .Include(a => a.AssignedUser)
                .Include(a => a.Task)
                .FirstOrDefault(a => a.Id == id);
        }

        public TaskAssignment GetTaskAssignmentByUserAndTask(string userId, int taskId)
        {
            return _context.TaskAssignment_Tbl
                .FirstOrDefault(a => a.AssignedUserId == userId && a.TaskId == taskId);
        }

        #endregion

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

        #region Task Search and Filter

        public List<Tasks> SearchTasks(string searchTerm, int? categoryId = null, string assignedUserId = null, bool? isCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) && !categoryId.HasValue && string.IsNullOrEmpty(assignedUserId) && !isCompleted.HasValue)
                return GetTasks();

            var query = _context.Tasks_Tbl.Where(t => !t.IsDeleted);

            // Search in title and description
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)) ||
                    t.TaskCode.Contains(searchTerm));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.TaskCategoryId == categoryId.Value);
            }

            // Filter by assigned user
            if (!string.IsNullOrEmpty(assignedUserId))
            {
                query = query.Where(t => _context.TaskAssignment_Tbl
                    .Any(a => a.TaskId == t.Id && a.AssignedUserId == assignedUserId));
            }

            // Filter by completion status
            if (isCompleted.HasValue)
            {
                query = query.Where(t => (t.CompletionDate != null) == isCompleted.Value);
            }

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        #endregion

        #region Calendar Methods

        public List<TaskCalendarViewModel> GetTasksForCalendarView(
            string userId,
            int? branchId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            try
            {
                var query = _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted &&
                               t.IsActive &&
                               t.DueDate.HasValue)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Stakeholder)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.DueDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.DueDate <= endDate.Value);
                }

                if (branchId.HasValue)
                {
                    query = query.Where(t =>
                        t.TaskAssignments.Any(ta =>
                            _context.BranchUser_Tbl.Any(bu =>
                                bu.UserId == ta.AssignedUserId &&
                                bu.BranchId == branchId.Value &&
                                bu.IsActive)) ||
                        (t.StakeholderId.HasValue &&
                         _context.StakeholderBranch_Tbl.Any(sb =>
                            sb.StakeholderId == t.StakeholderId &&
                            sb.BranchId == branchId.Value))
                    );
                }
                else
                {
                    query = query.Where(t =>
                        t.CreatorUserId == userId ||
                        t.TaskAssignments.Any(ta => ta.AssignedUserId == userId));
                }

                if (assignedUserIds != null && assignedUserIds.Any())
                {
                    query = query.Where(t =>
                        t.TaskAssignments.Any(ta => assignedUserIds.Contains(ta.AssignedUserId)));
                }

                if (stakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == stakeholderId.Value);
                }

                var tasksData = query.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.TaskCode,
                    t.DueDate,
                    t.CompletionDate,
                    t.CreateDate,
                    t.CreatorUserId,
                    t.StakeholderId,
                    StakeholderFirstName = t.Stakeholder != null ? t.Stakeholder.FirstName : null,
                    StakeholderLastName = t.Stakeholder != null ? t.Stakeholder.LastName : null,
                    CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : null
                }).ToList();

                var tasks = tasksData.Select(t => new TaskCalendarViewModel
                {
                    Id = t.Id,
                    Title = t.Title ?? string.Empty,
                    Description = t.Description,
                    TaskCode = t.TaskCode,
                    DueDate = t.DueDate,
                    IsCompleted = t.CompletionDate.HasValue,
                    IsOverdue = !t.CompletionDate.HasValue && t.DueDate < DateTime.Now,
                    StakeholderId = t.StakeholderId,
                    StakeholderName = t.StakeholderFirstName != null && t.StakeholderLastName != null ?
                        $"{t.StakeholderFirstName} {t.StakeholderLastName}" : "ندارد",
                    CategoryTitle = t.CategoryTitle ?? "ندارد",
                    BranchName = "ندارد",
                    CalendarColor = t.CompletionDate.HasValue ? "#28a745" :
                                   (!t.CompletionDate.HasValue && t.DueDate < DateTime.Now) ? "#dc3545" :
                                   "#007bff",
                    StatusText = t.CompletionDate.HasValue ? "تکمیل شده" :
                                (!t.CompletionDate.HasValue && t.DueDate < DateTime.Now) ? "عقب افتاده" :
                                "در حال انجام",
                    CreateDate = t.CreateDate,
                    CreatorUserId = t.CreatorUserId ?? string.Empty
                })
                .OrderBy(t => t.DueDate)
                .ToList();

                return tasks;
            }
            catch (Exception ex)
            {
                return new List<TaskCalendarViewModel>();
            }
        }

        #endregion

        #region Visibility Methods

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده برای کاربر بر اساس سیستم سلسله مراتبی
        /// </summary>
        public async Task<List<Tasks>> GetVisibleTasksForUserAsync(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
            
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های کاربر با در نظر گیری سیستم مجوزهای جدید
        /// </summary>
        public async Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) || // تسک‌های قابل مشاهده
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId);
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) || // تسک‌های قابل مشاهده
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId));
            }
            else if (includeCreated)
            {
                query = query.Where(t => t.CreatorUserId == userId);
            }

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های شعبه با در نظر گیری سیستم مجوزهای جدید
        /// </summary>
        public async Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId, branchId);
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر مجوز مشاهده تسک خاصی را دارد - بروزرسانی شده
        /// </summary>
        public async Task<bool> CanUserViewTaskAsync(string userId, int taskId)
        {
            return await _taskVisibilityRepository.CanUserViewTaskAsync(userId, taskId);
        }

        /// <summary>
        /// متد قدیمی - حفظ شده برای سازگاری
        /// </summary>
        public List<Tasks> GetVisibleTasksForUser(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId && tv.IsActive)
                .Where(tv => !tv.StartDate.HasValue || tv.StartDate <= DateTime.Now)
                .Where(tv => !tv.EndDate.HasValue || tv.EndDate >= DateTime.Now)
                .Select(tv => tv.TaskId)
                .ToList();

            var query = _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        /// <summary>
        /// متد قدیمی - حفظ شده برای سازگاری
        /// </summary>
        public bool CanUserViewTask(string userId, int taskId)
        {
            var now = DateTime.Now;
            
            return _context.TaskViewer_Tbl
                .Any(tv => tv.UserId == userId && 
                          tv.TaskId == taskId && 
                          tv.IsActive &&
                          (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                          (!tv.EndDate.HasValue || tv.EndDate >= now));
        }

        #endregion

        #region Hierarchical Task Methods

        /// <summary>
        /// دریافت تسک‌ها گروه‌بندی شده بر اساس سلسله مراتب تیمی
        /// </summary>
        public async Task<TaskGroupedViewModel> GetHierarchicalTasksForUserAsync(string userId)
        {
            var result = new TaskGroupedViewModel();

            // 1. تسک‌های شخصی کاربر
            var myTasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
            result.MyTasks = myTasks.Select(MapToTaskViewModel).ToList();

            // 2. تسک‌های اعضای تیم‌هایی که کاربر مدیر آن‌هاست
            await LoadTeamMemberTasks(result, userId);

            // 3. تسک‌های تیم‌های زیرمجموعه
            await LoadSubTeamTasks(result, userId);

            return result;
        }

        /// <summary>
        /// بارگذاری تسک‌های اعضای تیم
        /// </summary>
        private async Task LoadTeamMemberTasks(TaskGroupedViewModel result, string userId)
        {
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive).ToList();

            foreach (var team in managedTeams)
            {
                var teamMembers = _context.TeamMember_Tbl
                    .Include(tm => tm.User)
                    .Where(tm => tm.TeamId == team.Id && tm.IsActive && tm.UserId != userId)
                    .ToList();

                foreach (var member in teamMembers)
                {
                    var memberTasks = GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                    if (memberTasks.Any())
                    {
                        var memberName = $"{member.User.FirstName} {member.User.LastName}";
                        result.TeamMemberTasks[memberName] = memberTasks.Select(MapToTaskViewModel).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// بارگذاری تسک‌های زیرتیم‌ها
        /// </summary>
        private async Task LoadSubTeamTasks(TaskGroupedViewModel result, string userId)
        {
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive).ToList();

            foreach (var parentTeam in managedTeams)
            {
                await LoadSubTeamTasksRecursive(result, parentTeam.Id, parentTeam.Title);
            }
        }

        /// <summary>
        /// بارگذاری بازگشتی تسک‌های زیرتیم‌ها
        /// </summary>
        private async Task LoadSubTeamTasksRecursive(TaskGroupedViewModel result, int parentTeamId, string parentTeamTitle)
        {
            var subTeams = _context.Team_Tbl.Where(t => t.ParentTeamId == parentTeamId && t.IsActive).ToList();

            foreach (var subTeam in subTeams)
            {
                var subTeamTasks = new List<TaskViewModel>();

                // تسک‌های مدیر زیرتیم
                if (!string.IsNullOrEmpty(subTeam.ManagerUserId))
                {
                    var managerTasks = GetTasksByUser(subTeam.ManagerUserId, includeAssigned: true, includeCreated: true);
                    subTeamTasks.AddRange(managerTasks.Select(MapToTaskViewModel));
                }

                // تسک‌های اعضای زیرتیم
                var subTeamMembers = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == subTeam.Id && tm.IsActive)
                    .ToList();

                foreach (var member in subTeamMembers)
                {
                    var memberTasks = GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                    subTeamTasks.AddRange(memberTasks.Select(MapToTaskViewModel));
                }

                if (subTeamTasks.Any())
                {
                    // حذف تکرارها
                    var uniqueTasks = subTeamTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
                    result.SubTeamTasks[$"{parentTeamTitle} > {subTeam.Title}"] = uniqueTasks;
                }

                // بررسی زیرتیم‌های بیشتر (بازگشتی)
                await LoadSubTeamTasksRecursive(result, subTeam.Id, $"{parentTeamTitle} > {subTeam.Title}");
            }
        }
        /// <summary>
        /// تبدیل Task Entity به TaskViewModel
        /// </summary>
        private TaskViewModel MapToTaskViewModel(Tasks task)
        {
            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                CompletionDate = task.CompletionDate,
                ManagerApprovedDate = task.ManagerApprovedDate,
                SupervisorApprovedDate = task.SupervisorApprovedDate,
                IsActive = task.IsActive,
                IsDeleted = task.IsDeleted,
                BranchId = task.BranchId,
                CreatorUserId = task.CreatorUserId,
                StakeholderId = task.StakeholderId,
                TaskType = task.TaskType,
                CategoryId = task.TaskCategoryId,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,
                VisibilityLevel = task.VisibilityLevel,
                LastUpdateDate = task.LastUpdateDate,
                TaskTypeInput = task.TaskTypeInput,
                CreationMode = task.CreationMode,
     
            };
        }

        #endregion

        #region Team Helper Methods

        /// <summary>
        /// دریافت کاربران از تیم‌های انتخاب شده
        /// </summary>
        public async Task<List<string>> GetUsersFromTeamsAsync(List<int> teamIds)
        {
            if (teamIds == null || !teamIds.Any())
                return new List<string>();

            var teamUserIds = new List<string>();

            foreach (var teamId in teamIds)
            {
                // دریافت اعضای تیم
                var teamMembers = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.UserId)
                    .ToList();

                teamUserIds.AddRange(teamMembers);

                // اضافه کردن مدیر تیم
                var team = _context.Team_Tbl.FirstOrDefault(t => t.Id == teamId);
                if (team != null && !string.IsNullOrEmpty(team.ManagerUserId))
                {
                    teamUserIds.Add(team.ManagerUserId);
                }
            }

            return teamUserIds.Distinct().ToList();
        }

        /// <summary>
        /// دریافت شعبه کاربر
        /// </summary>
        public int GetUserBranchId(string userId)
        {
            var branchUser = _context.BranchUser_Tbl
                .FirstOrDefault(bu => bu.UserId == userId && bu.IsActive);
            return branchUser?.BranchId ?? 1; // پیش‌فرض شعبه اصلی
        }

        /// <summary>
        /// دریافت تیم‌های مرتبط با کاربر
        /// </summary>
        public async Task<List<TeamViewModel>> GetUserRelatedTeamsAsync(string userId)
        {
            var teams = new List<TeamViewModel>();

            // تیم‌هایی که کاربر مدیر آن‌هاست
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive);
            
            // تیم‌هایی که کاربر عضو آن‌هاست
            var memberTeams = _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Team)
                .Where(t => t != null && t.IsActive);

            // ترکیب و حذف تکرار
            var allTeams = managedTeams.Union(memberTeams).Distinct().ToList();

            foreach (var team in allTeams)
            {
                var manager = _context.Users.FirstOrDefault(u => u.Id == team.ManagerUserId);
                
                teams.Add(new TeamViewModel
                {
                    Id = team.Id,
                    Title = team.Title,
                    Description = team.Description,
                    BranchId = team.BranchId,
                    IsActive = team.IsActive,
                    ManagerFullName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "ندارد"
                });
            }

            return teams.OrderBy(t => t.Title).ToList();
        }

        /// <summary>
        /// دریافت کاربران مرتبط با کاربر
        /// </summary>
        public async Task<List<UserViewModelFull>> GetUserRelatedUsersAsync(string userId)
        {
            var relatedUserIds = new HashSet<string>();

            // اعضای تیم‌هایی که کاربر مدیر آن‌هاست
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive);
            foreach (var team in managedTeams)
            {
                var memberIds = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == team.Id && tm.IsActive)
                    .Select(tm => tm.UserId);
                foreach (var memberId in memberIds)
                    relatedUserIds.Add(memberId);
            }

            // همکاران در تیم‌هایی که کاربر عضو آن‌هاست
            var memberTeamIds = _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId);
            
            foreach (var teamId in memberTeamIds)
            {
                var teammateIds = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.UserId);
                foreach (var teammateId in teammateIds)
                    relatedUserIds.Add(teammateId);
            }

            // تبدیل به UserViewModelFull
            var users = new List<UserViewModelFull>();
            foreach (var relatedUserId in relatedUserIds)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == relatedUserId);
                if (user != null)
                {
                    users.Add(new UserViewModelFull
                    {
                        Id = user.Id,
                        FullNamesString = $"{user.FirstName} {user.LastName}",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        PositionName = user.PositionName,
                        IsActive = user.IsActive
                    });
                }
            }

            return users.OrderBy(u => u.FullNamesString).ToList();
        }

        #endregion

        #region Statistics and Filter Methods

        /// <summary>
        /// محاسبه آمار تسک‌ها بر اساس سطح دسترسی کاربر
        /// </summary>
        public async Task<TaskStatisticsViewModel> CalculateTaskStatisticsAsync(string userId, int dataAccessLevel, List<TaskViewModel> filteredTasks)
        {
            List<TaskViewModel> allAvailableTasks;

            switch (dataAccessLevel)
            {
                case 0: // Personal
                    var personalTasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
                    allAvailableTasks = personalTasks.Select(MapToTaskViewModel).ToList();
                    break;
                    
                case 1: // Branch
                    var userBranchId = GetUserBranchId(userId);
                    var branchTasks = GetTasksByBranch(userBranchId);
                    allAvailableTasks = branchTasks.Select(MapToTaskViewModel).ToList();
                    break;
                    
                case 2: // All
                    var systemTasks = GetTasks(includeDeleted: false);
                    allAvailableTasks = systemTasks.Select(MapToTaskViewModel).ToList();
                    break;
                    
                default:
                    allAvailableTasks = filteredTasks;
                    break;
            }

            var statistics = new TaskStatisticsViewModel
            {
                // آمار کل (بر اساس سطح دسترسی)
                TotalTasks = allAvailableTasks.Count,
                MyTasks = allAvailableTasks.Count(t => t.CreatorUserId == userId),
                AssignedToMe = allAvailableTasks.Count(t => t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)),
                
                // آمار فیلتر شده فعلی
                CompletedTasks = filteredTasks.Count(t => t.CompletionDate.HasValue),
                OverdueTasks = filteredTasks.Count(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now),
                InProgressTasks = filteredTasks.Count(t => !t.CompletionDate.HasValue && t.IsActive),
                ImportantTasks = filteredTasks.Count(t => t.TaskType == 1),
                UrgentTasks = filteredTasks.Count(t => t.TaskType == 2)
            };

            return statistics;
        }

        /// <summary>
        /// اعمال فیلترهای اضافی بر روی لیست تسک‌ها
        /// </summary>
        public async Task<List<TaskViewModel>> ApplyFiltersAsync(List<TaskViewModel> tasks, TaskFilterViewModel filters)
        {
            var filteredTasks = tasks.ToList();

            // فیلتر شعبه
            if (filters.BranchId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.BranchId == filters.BranchId.Value).ToList();
            }

            // فیلتر تیم
            if (filters.TeamId.HasValue)
            {
                var teamUserIds = await GetUsersFromTeamsAsync(new List<int> { filters.TeamId.Value });
                filteredTasks = filteredTasks.Where(t => t.AssignmentsTaskUser != null && 
                                                t.AssignmentsTaskUser.Any(a => teamUserIds.Contains(a.AssignedUserId))).ToList();
            }

            // فیلتر کاربر  
            if (!string.IsNullOrEmpty(filters.UserId))
            {
                filteredTasks = filteredTasks.Where(t => 
                    (t.AssignmentsTaskUser != null && t.AssignmentsTaskUser.Any(a => a.AssignedUserId == filters.UserId)) ||
                    t.CreatorUserId == filters.UserId).ToList();
            }

            // فیلتر اولویت
            if (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All)
            {
                filteredTasks = filteredTasks.Where(t => t.TaskType == (byte)filters.TaskPriority).ToList();
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.CategoryId == filters.CategoryId.Value).ToList();
            }

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All)
            {
                switch (filters.TaskStatus.Value)
                {
                    case TaskStatusFilter.Completed:
                        filteredTasks = filteredTasks.Where(t => t.CompletionDate.HasValue).ToList();
                        break;
                    case TaskStatusFilter.InProgress:
                        filteredTasks = filteredTasks.Where(t => !t.CompletionDate.HasValue && t.IsActive).ToList();
                        break;
                    case TaskStatusFilter.Overdue:
                        filteredTasks = filteredTasks.Where(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now).ToList();
                        break;
                }
            }

            // فیلتر طرف حساب
            if (filters.StakeholderId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.StakeholderId == filters.StakeholderId.Value).ToList();
            }

            // فیلتر جستجو در متن
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.Title.Contains(filters.SearchTerm) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(filters.SearchTerm)) ||
                    t.TaskCode.Contains(filters.SearchTerm)).ToList();
            }

            return filteredTasks;
        }

        /// <summary>
        /// بررسی وجود فیلتر فعال
        /// </summary>
        public bool HasActiveFilters(TaskFilterViewModel filters)
        {
            return filters.BranchId.HasValue ||
                   filters.TeamId.HasValue ||
                   !string.IsNullOrEmpty(filters.UserId) ||
                   filters.CategoryId.HasValue ||
                   filters.StakeholderId.HasValue ||
                   !string.IsNullOrEmpty(filters.SearchTerm) ||
                   (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All) ||
                   (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All);
        }

        #endregion

        #region AJAX and Helper Methods - متدهای حذف شده از Controller

        /// <summary>
        /// AJAX برای تغییر سریع نوع نمایش
        /// </summary>
        public async Task<TaskFilterViewModel> ChangeViewTypeAsync(TaskViewType viewType)
        {
            return new TaskFilterViewModel { ViewType = viewType };
        }

        /// <summary>
        /// فیلتر سریع بر اساس آمار (فیلتر ثانویه)
        /// </summary>
        public async Task<TaskFilterViewModel> FilterByStatusAsync(TaskStatusFilter statusFilter, TaskFilterViewModel currentFilters = null)
        {
            currentFilters ??= new TaskFilterViewModel();
            currentFilters.TaskStatus = statusFilter;
            return currentFilters;
        }

        /// <summary>
        /// فیلتر سریع بر اساس اولویت (فیلتر ثانویه)
        /// </summary>
        public async Task<TaskFilterViewModel> FilterByPriorityAsync(TaskPriorityFilter priorityFilter, TaskFilterViewModel currentFilters = null)
        {
            currentFilters ??= new TaskFilterViewModel();
            currentFilters.TaskPriority = priorityFilter;
            return currentFilters;
        }

        /// <summary>
        /// بروزرسانی لیست کاربران و تیم‌ها بر اساس شعبه انتخاب شده
        /// </summary>
        public async Task<BranchChangeDataViewModel> GetBranchChangeDataAsync(int branchId)
        {
            var result = new BranchChangeDataViewModel();
            
            try
            {
                // دریافت کاربران شعبه انتخاب شده
                result.Users = _BranchRipository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت تیم‌های شعبه انتخاب شده
                result.Teams = await GetBranchTeamsByBranchId(branchId);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                result.Stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(branchId);

                return result;
            }
            catch (Exception)
            {
                // در صورت خطا، لیست‌های خالی برگردان
                result.Users = new List<BranchUserViewModel>();
                result.Teams = new List<TeamViewModel>();
                result.Stakeholders = new List<StakeholderViewModel>();
                return result;
            }
        }

        /// <summary>
        /// متد کمکی برای دریافت تیم‌های شعبه
        /// </summary>
        private async Task<List<TeamViewModel>> GetBranchTeamsByBranchId(int branchId)
        {
            try
            {
                // دریافت تیم‌ها از طریق DbContext
                var teams = _context.Team_Tbl
                    .Where(t => t.BranchId == branchId && t.IsActive)
                    .Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        BranchId = t.BranchId,
                        IsActive = t.IsActive,
                        ManagerFullName = !string.IsNullOrEmpty(t.ManagerUserId) 
                            ? _context.Users.Where(u => u.Id == t.ManagerUserId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                            : "ندارد"
                    })
                    .OrderBy(t => t.Title)
                    .ToList();

                return teams;
            }
            catch (Exception)
            {
                return new List<TeamViewModel>();
            }
        }

        /// <summary>
        /// بروzrسانی لیست دسته‌بندی‌ها بر اساس تغییر طرف حساب
        /// </summary>
        public async Task<List<TaskCategory>> GetTaskCategoriesForStakeholderChangeAsync(int branchId, int stakeholderId)
        {
            try
            {
                // دریافت دسته‌بندی‌های تسک مربوط به شعبه و طرف حساب انتخاب شده
                return _BranchRipository.GetTaskCategoriesByBranchAndStakeholder(branchId, stakeholderId)
                    .Select(btcs => new TaskCategory
                    {
                        Id = btcs.TaskCategoryId,
                        Title = btcs.TaskCategory?.Title ?? "نامشخص",
                        IsActive = btcs.IsActive
                    })
                    .ToList();
            }
            catch (Exception)
            {
                return new List<TaskCategory>();
            }
        }

        /// <summary>
        /// متد کمکی برای بازیابی داده‌های فرم CreateTask
        /// </summary>
        public async Task<TaskViewModel> RepopulateCreateTaskModelAsync(TaskViewModel model, string userId)
        {
            try
            {
                // بازیابی لیست شعبه‌ها
                model.branchListInitial = _BranchRipository.GetBrnachListByUserId(userId);
                
                // بازیابی تنظیمات کد تسک
                if (model.TaskCodeSettings == null)
                {
                    model.TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings();
                }
                
                // اگر کد تسک خالی است، کد جدید تولید کن
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // بازیابی دسته‌بندی‌ها
                model.TaskCategoryInitial ??= GetAllCategories();

                // بازیابی کاربران (محدود به شعبه کاربر)
                var userBranchId = GetUserBranchId(userId);
                if (model.UsersInitial == null)
                {
                    var branchUsers = _BranchRipository.GetBranchUsersByBranchId(userBranchId, includeInactive: false);
                    model.UsersInitial = branchUsers.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId,
                        FullNamesString = u.UserFullName,
                        IsActive = u.IsActive
                    }).ToList();
                }

                // بازیابی تیم‌ها (محدود به شعبه کاربر)
                model.TeamsInitial ??= await GetUserRelatedTeamsAsync(userId);

                // بازیابی طرف حساب‌ها (محدود به شعبه کاربر)
                if (model.StakeholdersInitial == null)
                {
                    var stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(userBranchId);
                    model.StakeholdersInitial = stakeholders.Select(s => new StakeholderViewModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        CompanyName = s.CompanyName,
                        NationalCode = s.NationalCode,
                        IsActive = s.IsActive
                    }).ToList();
                }

                return model;
            }
            catch (Exception)
            {
                // در صورت خطا، حداقل لیست‌های خالی ایجاد کن
                model.branchListInitial ??= new List<BranchViewModel>();
                model.TaskCategoryInitial ??= new List<TaskCategory>();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= new List<TeamViewModel>();
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();
                
                model.TaskCodeSettings ??= new TaskCodeSettings
                {
                    AllowManualInput = false,
                    SystemPrefix = "TSK"
                };

                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                return model;
            }
        }
        #endregion

        #region Missing Implementation Methods

        /// <summary>
        /// دریافت رویدادهای تقویم (نسخه Async)
        /// </summary>
        public async Task<List<TaskCalendarViewModel>> GetCalendarEventsAsync(
            string userId,
            DateTime? start = null,
            DateTime? end = null,
            int? branchId = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            return GetTasksForCalendarView(userId, branchId, start, end, assignedUserIds, stakeholderId);
        }

        /// <summary>
        /// دریافت تسک‌ها برای Index با فیلترها (نسخه Async جدید)
        /// </summary>
        public async Task<TaskListForIndexViewModel> GetTasksForIndexAsync(string userId, TaskFilterViewModel filters)
        {
            try
            {
                var model = new TaskListForIndexViewModel
                {
                    UserLoginid = userId,
                    Filters = filters ?? new TaskFilterViewModel()
                };

                // پر کردن لیست‌های فیلتر
                await PopulateFilterListsAsync(model, userId);

                // دریافت تسک‌ها بر اساس فیلتر
                await LoadTasksByFilterAsync(model, userId);

                // محاسبه آمار
                model.Statistics = await CalculateTaskStatisticsAsync(userId, 0, model.Tasks);

                return model;
            }
            catch (Exception ex)
            {
                // در صورت خطا، مدل پیش‌فرض برگردان
                return new TaskListForIndexViewModel
                {
                    UserLoginid = userId,
                    Filters = filters ?? new TaskFilterViewModel(),
                    Tasks = new List<TaskViewModel>(),
                    Statistics = new TaskStatisticsViewModel(),
                    GroupedTasks = new TaskGroupedViewModel()
                };
            }
        }

        /// <summary>
        /// آماده‌سازی مدل برای ایجاد تسک جدید (نسخه Async جدید)
        /// </summary>
        public async Task<TaskViewModel> PrepareCreateTaskModelAsync(string userId)
        {
            try
            {
                var model = CreateTaskAndCollectData(userId);

                // تکمیل داده‌های اضافی به صورت async
                await PopulateCreateTaskDataAsync(model, userId);

                return model;
            }
            catch (Exception ex)
            {
                // در صورت خطا، مدل پیش‌فرض برگردان
                return new TaskViewModel
                {
                    branchListInitial = _BranchRipository.GetBrnachListByUserId(userId) ?? new List<BranchViewModel>(),
                    TaskCategoryInitial = GetAllCategories(),
                    UsersInitial = new List<UserViewModelFull>(),
                    TeamsInitial = new List<TeamViewModel>(),
                    StakeholdersInitial = new List<StakeholderViewModel>(),

                    // تولید کد تسک اتوماتیک بر اساس تنظیمات
                    TaskCode = _taskCodeGenerator.GenerateTaskCode(),
                    TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings(),

                    IsManualTaskCode = false,
                    IsActive = true
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های شعبه برای AJAX
        /// </summary>
        public async Task<BranchSelectResponseViewModel> GetBranchTriggeredDataAsync(int branchId)
        {
            try
            {
                // دریافت کاربران شعبه انتخاب شده
                var branchUsers = _BranchRipository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت تیم‌های شعبه انتخاب شده
                var branchTeams = await GetBranchTeamsByBranchId(branchId);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                var stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(branchId);

                return new BranchSelectResponseViewModel
                {
                    Users = branchUsers,
                    Teams = branchTeams,
                    Stakeholders = stakeholders
                };
            }
            catch (Exception ex)
            {
                return new BranchSelectResponseViewModel
                {
                    Users = new List<BranchUserViewModel>(),
                    Teams = new List<TeamViewModel>(),
                    Stakeholders = new List<StakeholderViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت آمار پروژه
        /// </summary>
        public async Task<ProjectStatsViewModel> GetProjectStatsAsync(int branchId, int? stakeholderId = null, int? categoryId = null)
        {
            try
            {
                var query = _context.Tasks_Tbl.Where(t => !t.IsDeleted && t.BranchId == branchId);

                var stakeholderTasksCount = stakeholderId.HasValue ? 
                    await query.CountAsync(t => t.StakeholderId == stakeholderId.Value) : 0;

                var categoryTasksCount = categoryId.HasValue ? 
                    await query.CountAsync(t => t.TaskCategoryId == categoryId.Value) : 0;

                return new ProjectStatsViewModel
                {
                    StakeholderTasksCount = stakeholderTasksCount,
                    CategoryTasksCount = categoryTasksCount
                };
            }
            catch (Exception ex)
            {
                return new ProjectStatsViewModel 
                { 
                    StakeholderTasksCount = 0, 
                    CategoryTasksCount = 0 
                };
            }
        }

        #endregion

        #region Helper Methods for New Async Implementation

        /// <summary>
        /// پر کردن لیست‌های فیلتر به صورت async
        /// </summary>
        private async Task PopulateFilterListsAsync(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                // شعبه‌های کاربر
                model.branchListInitial = _BranchRipository.GetBrnachListByUserId(userId);

                // تیم‌های کاربر
                model.TeamsInitial = await GetUserRelatedTeamsAsync(userId);

                // کاربران مرتبط
                model.UsersInitial = await GetUserRelatedUsersAsync(userId);

                // دسته‌بندی‌ها
                model.TaskCategoryInitial = GetAllCategories();

                // طرف حساب‌ها
                var stakeholders = _StakeholderRepo.GetStakeholders();
                model.StakeholdersInitial = stakeholders.Select(s => new StakeholderViewModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    CompanyName = s.CompanyName,
                    NationalCode = s.NationalCode,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                // در صورت خطا، لیست‌های خالی
                model.branchListInitial ??= new List<BranchViewModel>();
                model.TeamsInitial ??= new List<TeamViewModel>();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TaskCategoryInitial ??= new List<TaskCategory>();
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();
            }
        }

        /// <summary>
        /// بارگذاری تسک‌ها بر اساس فیلتر به صورت async
        /// </summary>
        private async Task LoadTasksByFilterAsync(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                switch (model.Filters.ViewType)
                {
                    case TaskViewType.AllTasks:
                        await LoadAllTasksAsync(model, userId);
                        break;
                        
                    case TaskViewType.MyTeamsHierarchy:
                        model.GroupedTasks = await GetHierarchicalTasksForUserAsync(userId);
                        // ترکیب تمام تسک‌ها برای نمایش
                        var allTasks = new List<TaskViewModel>();
                        allTasks.AddRange(model.GroupedTasks.MyTasks);
                        allTasks.AddRange(model.GroupedTasks.TeamMemberTasks.Values.SelectMany(tasks => tasks));
                        allTasks.AddRange(model.GroupedTasks.SubTeamTasks.Values.SelectMany(tasks => tasks));
                        model.Tasks = allTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
                        break;
                        
                    case TaskViewType.MyTasks:
                        await LoadMyTasksAsync(model, userId);
                        break;
                        
                    case TaskViewType.AssignedToMe:
                        await LoadAssignedToMeTasksAsync(model, userId);
                        break;
                        
                    default:
                        await LoadMyTasksAsync(model, userId);
                        break;
                }

                // اعمال فیلترهای اضافی
                model.Tasks = await ApplyFiltersAsync(model.Tasks, model.Filters);
            }
            catch (Exception ex)
            {
                model.Tasks = new List<TaskViewModel>();
                model.GroupedTasks = new TaskGroupedViewModel();
            }
        }

        /// <summary>
        /// بارگذاری همه تسک‌ها بر اساس سطح دسترسی
        /// </summary>
        private async Task LoadAllTasksAsync(TaskListForIndexViewModel model, string userId)
        {
            // در اینجا باید از سطح دسترسی کاربر استفاده کنید
            // فعلاً همه تسک‌های فعال را بارگذاری می‌کنیم
            var allTasks = GetTasks(includeDeleted: false);
            model.Tasks = allTasks.Select(MapToTaskViewModel).ToList();
        }

        /// <summary>
        /// بارگذاری تسک‌های شخصی
        /// </summary>
        private async Task LoadMyTasksAsync(TaskListForIndexViewModel model, string userId)
        {
            var tasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
            model.Tasks = tasks.Select(MapToTaskViewModel).ToList();
        }

        /// <summary>
        /// بارگذاری تسک‌های منتصب شده
        /// </summary>
        private async Task LoadAssignedToMeTasksAsync(TaskListForIndexViewModel model, string userId)
        {
            var tasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: false);
            model.Tasks = tasks.Select(MapToTaskViewModel).ToList();
        }

        /// <summary>
        /// تکمیل داده‌های مدل ایجاد تسک
        /// </summary>
        private async Task PopulateCreateTaskDataAsync(TaskViewModel model, string userId)
        {
            try
            {
                // بازیابی لیست شعبه‌ها
                model.branchListInitial ??= _BranchRipository.GetBrnachListByUserId(userId);

                // تنظیمات کد تسک از appsettings.json
                model.TaskCodeSettings ??= _taskCodeGenerator.GetTaskCodeSettings();

                // تولید کد تسک اتوماتیک اگر خالی باشد
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // مقداردهی پیش‌فرض لیست‌ها
                model.TaskCategoryInitial ??= GetAllCategories();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= await GetUserRelatedTeamsAsync(userId);
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();

                // اگر شعبه‌ای وجود دارد، داده‌های مربوطه را بارگذاری کن
                if (model.branchListInitial?.Any() == true)
                {
                    var firstBranchId = model.branchListInitial.First().Id;
                    var branchData = await GetBranchTriggeredDataAsync(firstBranchId);

                    model.UsersInitial = branchData.Users.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId,
                        FullNamesString = u.UserFullName,
                        IsActive = u.IsActive
                    }).ToList();

                    model.StakeholdersInitial = branchData.Stakeholders;
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا لیست‌های خالی
                InitializeEmptyCreateTaskLists(model);
            }
        }
        /// <summary>
        /// مقداردهی اولیه لیست‌های خالی برای مدل ایجاد تسک
        /// </summary>
        private void InitializeEmptyCreateTaskLists(TaskViewModel model)
        {
            model.branchListInitial ??= new List<BranchViewModel>();
            model.TaskCategoryInitial ??= new List<TaskCategory>();
            model.UsersInitial ??= new List<UserViewModelFull>();
            model.TeamsInitial ??= new List<TeamViewModel>();
            model.StakeholdersInitial ??= new List<StakeholderViewModel>();

            model.TaskCodeSettings ??= _taskCodeGenerator.GetTaskCodeSettings();


            if (string.IsNullOrEmpty(model.TaskCode))
            {
                model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
            }
        }

        #endregion

        #region Task Reminder Methods

        /// <summary>
        /// ذخیره یادآوری‌های تسک در دیتابیس - اصلاح شده
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="reminders">لیست یادآوری‌ها</param>
        public void SaveTaskReminders(int taskId, List<TaskReminderViewModel> reminders)
        {
            try
            {
                // حذف یادآوری‌های قدیمی
                var existingReminders = _context.TaskReminderSchedule_Tbl.Where(r => r.TaskId == taskId).ToList();
                _context.TaskReminderSchedule_Tbl.RemoveRange(existingReminders);

                // اضافه کردن یادآوری‌های جدید
                foreach (var reminder in reminders)
                {
                    var taskReminder = new TaskReminderSchedule
                    {
                        TaskId = taskId,
                        Title = reminder.Title,
                        Description = reminder.Description,
                        ReminderType = reminder.ReminderType,
                     
                        IntervalDays = reminder.IntervalDays,
                        DaysBeforeDeadline = reminder.DaysBeforeDeadline,
                        StartDate = reminder.StartDate,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    _context.TaskReminderSchedule_Tbl.Add(taskReminder);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ذخیره یادآوری‌های تسک: {ex.Message}", ex);
            }
        }

        #endregion





        #region Dashboard Methods Implementation

        /// <summary>
        /// دریافت داده‌های داشبورد تسک‌ها برای کاربر
        /// </summary>
        public async Task<TaskDashboardViewModel> GetTaskDashboardDataAsync(string userId)
        {
            try
            {
                // دریافت آمار کلی
                var stats = await GetUserTaskStatsAsync(userId);

                // دریافت تسک‌های فوری
                var urgentTasks = await GetUrgentTasksAsync(userId, 10);

                // دریافت فعالیت‌های اخیر
                var recentActivities = await GetRecentTaskActivitiesAsync(userId, 10);

                return new TaskDashboardViewModel
                {
                    UserStats = stats,
                    UrgentTasks = urgentTasks,
                    RecentActivities = recentActivities,
                    CompletedThisWeek = await GetCompletedTasksCountThisWeekAsync(userId),
                    InProgressThisWeek = await GetInProgressTasksCountThisWeekAsync(userId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت داده‌های داشبورد: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط کاربر
        /// </summary>
        public async Task<TasksListViewModel> GetTasksAssignedByUserAsync(string userId, TaskFilterViewModel filters)
        {
            try
            {
                var query = _context.Tasks_Tbl
                    .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskCategory)
                    .AsQueryable();

                // اعمال فیلترها
                query = ApplyFiltersToQuery(query, filters);

                var tasks = await query.OrderByDescending(t => t.CreateDate).ToListAsync();
                var taskViewModels = tasks.Select(MapToTaskViewModel).ToList();

                // محاسبه آمار
                var stats = new TasksListStatsViewModel
                {
                    TotalCount = taskViewModels.Count,
                    FilteredCount = taskViewModels.Count,
                    NeedsAttentionCount = taskViewModels.Count(t =>
                        (t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2) ||
                        (t.DueDate.HasValue && t.DueDate.Value.Date <= DateTime.Now.Date.AddDays(1))),

                    OverdueCount = taskViewModels.Count(t =>
                        t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2),
                    CompletedCount = taskViewModels.Count(t => t.Status == 2),
                    InProgressCount = taskViewModels.Count(t => t.Status == 1)
                };

                return new TasksListViewModel
                {
                    Tasks = taskViewModels,
                    Stats = stats,
                    Filters = filters,
                    TotalCount = stats.TotalCount,
                    FilteredCount = stats.FilteredCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تسک‌های واگذار شده: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تسک‌های تحت نظارت کاربر
        /// </summary>
        public async Task<TasksListViewModel> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters)
        {
            try
            {
                // دریافت تسک‌های قابل نظارت
                var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);

                var query = _context.Tasks_Tbl
                    .Where(t => visibleTaskIds.Contains(t.Id) &&
                               t.CreatorUserId != userId &&
                               !t.IsDeleted)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Creator)
                    .AsQueryable();

                // اعمال فیلترها
                query = ApplyFiltersToQuery(query, filters);

                var tasks = await query.OrderByDescending(t => t.CreateDate).ToListAsync();
                var taskViewModels = tasks.Select(MapToTaskViewModel).ToList();

                // محاسبه آمار
                var stats = new TasksListStatsViewModel
                {
                    TotalCount = taskViewModels.Count,
                    FilteredCount = taskViewModels.Count,
                    RequiresApprovalCount = taskViewModels.Count(t =>
            t.Status == 2 && !t.SupervisorApprovedDate.HasValue),

                    DelayedCount = taskViewModels.Count(t =>
                        t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2),
                    InProgressCount = taskViewModels.Count(t => t.Status == 1)
                };

                return new TasksListViewModel
                {
                    Tasks = taskViewModels,
                    Stats = stats,
                    Filters = filters,
                    TotalCount = stats.TotalCount,
                    FilteredCount = stats.FilteredCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تسک‌های نظارتی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت یادآوری‌های تسک برای کاربر
        /// </summary>
        public async Task<TaskRemindersViewModel> GetTaskRemindersAsync(string userId, TaskReminderFilterViewModel filters)
        {
            try
            {
                var query = _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId)
                    .Include(r => r.Task)
                    .AsQueryable();

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(filters?.FilterType))
                {
                    switch (filters.FilterType.ToLower())
                    {
                        case "pending":
                            query = query.Where(r => !r.IsSent && r.ScheduledDateTime <= DateTime.Now);
                            break;
                        case "sent":
                            query = query.Where(r => r.IsSent);
                            break;
                        case "overdue":
                            query = query.Where(r => !r.IsSent && r.ScheduledDateTime < DateTime.Now.AddDays(-1));
                            break;
                        case "today":
                            query = query.Where(r => r.ScheduledDateTime.Date == DateTime.Now.Date);
                            break;
                    }
                }

                var reminders = await query.OrderByDescending(r => r.ScheduledDateTime).ToListAsync();

                var reminderViewModels = reminders.Select(r => new TaskReminderItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Message = r.Message,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title,
                    TaskCode = r.Task?.TaskCode,
                    ScheduledDateTime = r.ScheduledDateTime,
                    ScheduledDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.ScheduledDateTime, "yyyy/MM/dd HH:mm"),
                    IsSent = r.IsSent,
                    IsRead = r.IsRead,
                    Priority = r.Priority,
                    EventTypeIcon = GetEventTypeIcon(r.EventType),
                    EventTypeColor = GetEventTypeColor(r.EventType)
                }).ToList();

                // محاسبه آمار
                var stats = new TaskRemindersStatsViewModel
                {
                    PendingCount = reminders.Count(r => !r.IsSent && r.ScheduledDateTime <= DateTime.Now),
                    SentCount = reminders.Count(r => r.IsSent),
                    OverdueCount = reminders.Count(r => !r.IsSent && r.ScheduledDateTime < DateTime.Now.AddDays(-1)),
                    TodayCount = reminders.Count(r => r.ScheduledDateTime.Date == DateTime.Now.Date)
                };

                return new TaskRemindersViewModel
                {
                    Reminders = reminderViewModels,
                    Stats = stats,
                    Filters = filters,
                    TotalCount = reminderViewModels.Count,
                    CurrentPage = filters?.Page ?? 1,
                    PageSize = filters?.PageSize ?? 20
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت یادآوری‌ها: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت آمار تسک‌ها برای کاربر
        /// </summary>
        public async Task<UserTaskStatsViewModel> GetUserTaskStatsAsync(string userId)
        {
            try
            {
                // تسک‌های من
                var myTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: false);

                // تسک‌های واگذار شده
                var assignedByMe = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: false, includeCreated: true);

                // تسک‌های نظارتی
                var supervisedTasks = await GetVisibleTasksForUserAsync(userId);
                supervisedTasks = supervisedTasks.Where(t => t.CreatorUserId != userId).ToList();

                var today = DateTime.Now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var weekEnd = weekStart.AddDays(6);

                return new UserTaskStatsViewModel
                {
                    MyTasksCount = myTasks.Count(t => !t.IsDeleted && t.Status != 2),
                    AssignedByMeCount = assignedByMe.Count(t => !t.IsDeleted),
                    SupervisedTasksCount = supervisedTasks.Count(t => !t.IsDeleted),
                    TodayTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date == today),
                    OverdueTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date < today && t.Status != 2),
                    ThisWeekTasksCount = myTasks.Count(t => !t.IsDeleted && t.DueDate.HasValue && t.DueDate.Value.Date >= weekStart && t.DueDate.Value.Date <= weekEnd),
                    RemindersCount = await GetActiveRemindersCountAsync(userId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در محاسبه آمار کاربر: {ex.Message}", ex);
            }
        }

        #endregion

        #region Task Summary and Activities Implementation

        /// <summary>
        /// دریافت تسک‌های فوری کاربر
        /// </summary>
        public async Task<List<TaskSummaryViewModel>> GetUrgentTasksAsync(string userId, int take = 5)
        {
            try
            {
                var userTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

                var urgentTasks = userTasks.Where(t =>
                    t.Priority == 2 || // فوری
                    t.Important || // مهم
                    (t.DueDate.HasValue && t.DueDate.Value.Date <= DateTime.Now.Date.AddDays(1)) // مهلت امروز یا فردا
                ).OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Take(take)
                .Select(t => new TaskSummaryViewModel
                {
                    Id = t.Id,
                    TaskCode = t.TaskCode,
                    Title = t.Title,
                    Priority = t.Priority,
                    Important = t.Important,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date && t.Status != 2,
                    StatusText = GetTaskStatusText(t.Status),
                    StatusBadgeClass = GetTaskStatusBadgeClass(t.Status),
                    StakeholderName = GetTaskStakeholderName(t.Id)
                    // DaysUntilDue محاسبه خودکار توسط Property انجام می‌شود
                }).ToList();

                return urgentTasks;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت تسک‌های فوری: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت آخرین فعالیت‌های تسک کاربر
        /// </summary>
        public async Task<List<RecentActivityViewModel>> GetRecentTaskActivitiesAsync(string userId, int take = 10)
        {
            try
            {
                var recentTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

                var activities = recentTasks
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.LastUpdateDate ?? t.CreateDate)
                    .Take(take)
                    .Select(t => new RecentActivityViewModel
                    {
                        Title = GetActivityTitle(t.Status),
                        Description = $"{t.Title} - {t.TaskCode}",
                        Icon = GetActivityIcon(t.Status),
                        IconClass = GetActivityIconClass(t.Status),
                        ActivityDate = t.LastUpdateDate ?? t.CreateDate,
                        TimeAgo = CalculateTimeAgo(t.LastUpdateDate ?? t.CreateDate),
                        Url = $"/AdminArea/Tasks/Details/{t.Id}"
                    }).ToList();

                return activities;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت فعالیت‌های اخیر: {ex.Message}", ex);
            }
        }

        #endregion

        #region Reminder Management Implementation

        /// <summary>
        /// علامت‌گذاری یادآوری به عنوان خوانده شده
        /// </summary>
        public async Task MarkReminderAsReadAsync(int reminderId, string userId)
        {
            try
            {
                var reminder = await _context.TaskReminderEvent_Tbl
                    .FirstOrDefaultAsync(r => r.Id == reminderId && r.RecipientUserId == userId);

                if (reminder != null)
                {
                    reminder.IsRead = true;
                    reminder.ReadDateTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در علامت‌گذاری یادآوری: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// علامت‌گذاری همه یادآوری‌ها به عنوان خوانده شده
        /// </summary>
        public async Task MarkAllRemindersAsReadAsync(string userId)
        {
            try
            {
                var reminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && !r.IsRead)
                    .ToListAsync();

                foreach (var reminder in reminders)
                {
                    reminder.IsRead = true;
                    reminder.ReadDateTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در علامت‌گذاری همه یادآوری‌ها: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// حذف یادآوری
        /// </summary>
        public async Task DeleteReminderAsync(int reminderId, string userId)
        {
            try
            {
                var reminder = await _context.TaskReminderEvent_Tbl
                    .FirstOrDefaultAsync(r => r.Id == reminderId && r.RecipientUserId == userId);

                if (reminder != null)
                {
                    _context.TaskReminderEvent_Tbl.Remove(reminder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در حذف یادآوری: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// حذف یادآوری‌های خوانده شده
        /// </summary>
        public async Task DeleteReadRemindersAsync(string userId)
        {
            try
            {
                var readReminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && r.IsRead)
                    .ToListAsync();

                _context.TaskReminderEvent_Tbl.RemoveRange(readReminders);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در حذف یادآوری‌های خوانده شده: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods

        private async Task<int> GetActiveRemindersCountAsync(string userId)
        {
            try
            {
                var today = DateTime.Now.Date;
                return await _context.TaskReminderEvent_Tbl
                    .CountAsync(r => r.RecipientUserId == userId &&
                                   r.ScheduledDateTime.Date <= today &&
                                   !r.IsSent);
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetCompletedTasksCountThisWeekAsync(string userId)
        {
            var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var myTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: false);
            return myTasks.Count(t => !t.IsDeleted &&
                                    t.CompletionDate.HasValue &&
                                    t.CompletionDate.Value.Date >= weekStart &&
                                    t.CompletionDate.Value.Date <= weekEnd);
        }

        private async Task<int> GetInProgressTasksCountThisWeekAsync(string userId)
        {
            var myTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: false);
            return myTasks.Count(t => !t.IsDeleted && t.Status == 1);
        }

        private IQueryable<Tasks> ApplyFiltersToQuery(IQueryable<Tasks> query, TaskFilterViewModel filters)
        {
            if (filters == null) return query;

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(filters.SearchTerm) ||
                                       (t.Description != null && t.Description.Contains(filters.SearchTerm)) ||
                                       t.TaskCode.Contains(filters.SearchTerm));
            }

            // سایر فیلترها...

            return query;
        }

        private string GetTaskStakeholderName(int taskId)
        {
            var stakeholder = _context.Tasks_Tbl
                .Where(t => t.Id == taskId)
                .Include(t => t.Stakeholder)
                .Select(t => t.Stakeholder)
                .FirstOrDefault();

            return stakeholder != null ? $"{stakeholder.FirstName} {stakeholder.LastName}" : "";
        }

        private string GetActivityTitle(byte status)
        {
            return status switch
            {
                0 => "تسک جدید ایجاد شد",
                1 => "تسک در حال انجام",
                2 => "تسک تکمیل شد",
                3 => "تسک تأیید شد",
                4 => "تسک رد شد",
                _ => "تسک بروزرسانی شد"
            };
        }

        private string GetActivityIcon(byte status)
        {
            return status switch
            {
                0 => "fa-plus-circle",
                1 => "fa-clock",
                2 => "fa-check-circle",
                3 => "fa-thumbs-up",
                4 => "fa-times-circle",
                _ => "fa-edit"
            };
        }

        private string GetActivityIconClass(byte status)
        {
            return status switch
            {
                0 => "primary",
                1 => "warning",
                2 => "success",
                3 => "info",
                4 => "danger",
                _ => "secondary"
            };
        }

        private string GetEventTypeIcon(byte eventType)
        {
            return eventType switch
            {
                0 => "fa-info-circle",
                1 => "fa-bell",
                2 => "fa-clock",
                3 => "fa-play",
                4 => "fa-stop",
                5 => "fa-exclamation-triangle",
                _ => "fa-bell"
            };
        }

        private string GetEventTypeColor(byte eventType)
        {
            return eventType switch
            {
                0 => "info",
                1 => "primary",
                2 => "warning",
                3 => "success",
                4 => "danger",
                5 => "warning",
                _ => "secondary"
            };
        }

        private string CalculateTimeAgo(DateTime activityDate)
        {
            var timeSpan = DateTime.Now - activityDate;

            if (timeSpan.TotalMinutes < 1)
                return "هم اکنون";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} روز پیش";

            return activityDate.ToString("yyyy/MM/dd");
        }

        #endregion
        #region Additional Missing Methods

        /// <summary>
        /// بررسی یکتایی کد تسک (نسخه Async)
        /// </summary>
        public async Task<bool> IsTaskCodeUniqueAsync(string taskCode, int? excludeTaskId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return true;

            var query = _context.Tasks_Tbl.Where(t => t.TaskCode == taskCode && !t.IsDeleted);

            if (excludeTaskId.HasValue)
                query = query.Where(t => t.Id != excludeTaskId.Value);

            return !await query.AnyAsync();
        }

        /// <summary>
        /// ذخیره عملیات‌های تسک در دیتابیس
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="operations">لیست عملیات‌ها</param>
        public void SaveTaskOperations(int taskId, List<TaskOperationViewModel> operations)
        {
            try
            {
                // حذف عملیات‌های قدیمی
                var existingOperations = _context.TaskOperation_Tbl.Where(o => o.TaskId == taskId).ToList();
                _context.TaskOperation_Tbl.RemoveRange(existingOperations);

                // اضافه کردن عملیات‌های جدید
                foreach (var operation in operations)
                {
                    var taskOperation = new TaskOperation
                    {
                        TaskId = taskId,
                        Title = operation.Title,
                        OperationOrder = operation.OperationOrder,
                        IsRequired = operation.IsRequired,
                        IsCompleted = operation.IsCompleted ,
                        EstimatedHours = operation.EstimatedHours,
                        IsStarred = operation.IsStarred ,
                        CreatedDate = DateTime.Now
                    };

                    _context.TaskOperation_Tbl.Add(taskOperation);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ذخیره عملیات‌های تسک: {ex.Message}", ex);
            }
        }

        #endregion

        /// <summary>
        /// دریافت متن وضعیت تسک
        /// </summary>
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

        /// <summary>
        /// دریافت کلاس badge برای وضعیت تسک
        /// </summary>
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
    }
}