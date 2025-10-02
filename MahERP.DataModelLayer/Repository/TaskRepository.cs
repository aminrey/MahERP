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

        /// <summary>
        /// دریافت تسک‌ها برای نمایش تقویم - اصلاح شده
        /// </summary>
        public List<TaskCalendarViewModel> GetTasksForCalendarView(
            string userId,
            int? branchId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            // استفاده از نسخه Async
            var result = GetCalendarEventsAsync(userId, startDate, endDate, branchId, assignedUserIds, stakeholderId);
            return result.Result; // فقط برای سازگاری با کدهای قدیمی
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
        /// تبدیل Task Entity به TaskViewModel - اصلاح شده برای گروه‌بندی صحیح assignments
        /// </summary>
        private TaskViewModel MapToTaskViewModel(Tasks task)
        {
            // دریافت انتساب‌های تسک
            var assignments = _context.TaskAssignment_Tbl
                .Include(ta => ta.AssignedUser)
                .Where(ta => ta.TaskId == task.Id)
                .ToList();

            // دریافت اطلاعات دسته‌بندی و طرف حساب
            var category = _context.TaskCategory_Tbl.FirstOrDefault(c => c.Id == task.TaskCategoryId);
            var stakeholder = _context.Stakeholder_Tbl.FirstOrDefault(s => s.Id == task.StakeholderId);

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
                CategoryTitle = category?.Title,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,
                VisibilityLevel = task.VisibilityLevel,
                LastUpdateDate = task.LastUpdateDate,
                TaskTypeInput = task.TaskTypeInput,
                CreationMode = task.CreationMode,
                StakeholderName = stakeholder != null ?
                    (!string.IsNullOrEmpty(stakeholder.CompanyName) ? stakeholder.CompanyName : $"{stakeholder.FirstName} {stakeholder.LastName}")
                    : null,

                // ⭐ اصلاح شده: ذخیره همه assignments به صورت کامل 
                AssignmentsTaskUser = assignments
                    .Where(a => !string.IsNullOrEmpty(a.AssignedUserId)) // فقط assignments معتبر
                    .Select(a => new TaskAssignmentViewModel
                    {
                        Id = a.Id,
                        TaskId = a.TaskId,
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : "نامشخص",
                        AssignerUserId = a.AssignerUserId, // ⭐ کلیدی: ذخیره AssignerUserId
                        AssignDate = a.AssignmentDate,
                        CompletionDate = a.CompletionDate,
                        Description = a.Description
                    }).ToList()
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
        /// محاسبه آمار تسک‌ها بر اساس سطح دسترسی کاربر - اصلاح شده
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

            // ⭐ حذف تسک‌های تکراری در محاسبه آمار
            var uniqueAllTasks = allAvailableTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
            var uniqueFilteredTasks = filteredTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();

            var statistics = new TaskStatisticsViewModel
            {
                // آمار کل (بر اساس سطح دسترسی)
                TotalTasks = uniqueAllTasks.Count,

      
                // تسک‌های منتصب به من - اصلاح شده
                AssignedToMe = uniqueAllTasks.Count(t =>
                    t.AssignmentsTaskUser != null &&
                    t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId) &&
                    t.CreatorUserId != userId), // فقط تسک‌هایی که خودم نساخته‌ام

                // تسک‌های واگذار شده توسط من
                AssignedByMe = uniqueAllTasks.Count(t => t.CreatorUserId == userId),

                // آمار فیلتر شده فعلی
                CompletedTasks = uniqueFilteredTasks.Count(t => t.CompletionDate.HasValue),
                OverdueTasks = uniqueFilteredTasks.Count(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now),
                InProgressTasks = uniqueFilteredTasks.Count(t => !t.CompletionDate.HasValue && t.IsActive),
                ImportantTasks = uniqueFilteredTasks.Count(t => t.Important || t.Priority == 1),
                UrgentTasks = uniqueFilteredTasks.Count(t => t.Priority == 2),

                // آمار تیمی (در صورت نیاز)
                TeamTasks = 0, // باید پیاده‌سازی شود
                SubTeamTasks = 0 // باید پیاده‌سازی شود
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
        /// دریافت رویدادهای تقویم (نسخه Async) - بازنویسی کامل برای نمایش تسک‌های چند روزه
        /// </summary>
        public async Task<List<TaskCalendarViewModel>> GetCalendarEventsAsync(
            string userId,
            DateTime? start = null, 
            DateTime? end = null,
            int? branchId = null,
            List<string> assignedUserIds = null,
            int? stakeholderId = null)
        {
            try
            {
                // مرحله اول: ساخت کوئری اصلی تسک‌ها
                var tasksQuery = _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted)
                    .AsQueryable();

                // ⭐ بهبود فیلتر تاریخ برای نمایش تسک‌های چند روزه
                if (start.HasValue || end.HasValue)
                {
                    // تسک‌هایی که در بازه زمانی مورد نظر قرار دارند
                    // شامل: تسک‌هایی که شروع یا پایان آن‌ها در این بازه است یا کل بازه را می‌پوشاند
                    if (start.HasValue && end.HasValue)
                    {
                        tasksQuery = tasksQuery.Where(t =>
                            // تسک‌هایی که CreateDate در بازه است
                            (t.CreateDate >= start.Value && t.CreateDate <= end.Value) ||
                            // تسک‌هایی که DueDate در بازه است
                            (t.DueDate.HasValue && t.DueDate.Value >= start.Value && t.DueDate.Value <= end.Value) ||
                            // تسک‌هایی که کل بازه را می‌پوشانند
                            (t.CreateDate <= start.Value && t.DueDate.HasValue && t.DueDate.Value >= end.Value)
                        );
                    }
                    else if (start.HasValue)
                    {
                        tasksQuery = tasksQuery.Where(t =>
                            t.CreateDate >= start.Value ||
                            (t.DueDate.HasValue && t.DueDate.Value >= start.Value)
                        );
                    }
                    else if (end.HasValue)
                    {
                        tasksQuery = tasksQuery.Where(t =>
                            t.CreateDate <= end.Value ||
                            (t.DueDate.HasValue && t.DueDate.Value <= end.Value)
                        );
                    }
                }

                // فیلتر شعبه
                if (branchId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t =>
                        t.BranchId == branchId.Value ||
                        _context.TaskAssignment_Tbl.Any(ta =>
                            ta.TaskId == t.Id &&
                            _context.BranchUser_Tbl.Any(bu =>
                                bu.UserId == ta.AssignedUserId &&
                                bu.BranchId == branchId.Value &&
                                bu.IsActive))
                    );
                }
                else
                {
                    // محدود کردن به تسک‌های مرتبط با کاربر
                    tasksQuery = tasksQuery.Where(t =>
                        t.CreatorUserId == userId ||
                        _context.TaskAssignment_Tbl.Any(ta => ta.TaskId == t.Id && ta.AssignedUserId == userId)
                    );
                }

                // فیلتر کاربران انتصاب
                if (assignedUserIds != null && assignedUserIds.Any())
                {
                    tasksQuery = tasksQuery.Where(t =>
                        _context.TaskAssignment_Tbl.Any(ta =>
                            ta.TaskId == t.Id && assignedUserIds.Contains(ta.AssignedUserId))
                    );
                }

                // فیلتر طرف حساب
                if (stakeholderId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t => t.StakeholderId == stakeholderId.Value);
                }

                // مرحله دوم: اجرای کوئری و دریافت داده‌های خام
                var rawTasks = await tasksQuery
                    .Include(t => t.Stakeholder)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.TaskAssignments)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.TaskCode,
                        t.CreateDate,
                        t.StartDate,
                        t.DueDate,
                        t.CompletionDate,
                        t.CreatorUserId,
                        t.StakeholderId,
                        t.TaskCategoryId,
                        t.Status,
                        t.Priority,
                        t.Important,
                        t.BranchId,
                     
                        // اطلاعات طرف حساب
                        StakeholderFirstName = t.Stakeholder != null ? t.Stakeholder.FirstName : null,
                        StakeholderLastName = t.Stakeholder != null ? t.Stakeholder.LastName : null,
                        StakeholderCompanyName = t.Stakeholder != null ? t.Stakeholder.CompanyName : null,
                        // اطلاعات دسته‌بندی
                        CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : null
                    })
                    .ToListAsync();

                // مرحله سوم: تولید رویدادهای تقویم
                var calendarEvents = new List<TaskCalendarViewModel>();

                foreach (var task in rawTasks)
                {
                    // تعیین نام طرف حساب
                    string stakeholderName = "ندارد";
                    if (!string.IsNullOrEmpty(task.StakeholderCompanyName))
                    {
                        stakeholderName = task.StakeholderCompanyName;
                    }
                    else if (!string.IsNullOrEmpty(task.StakeholderFirstName) || !string.IsNullOrEmpty(task.StakeholderLastName))
                    {
                        stakeholderName = $"{task.StakeholderFirstName} {task.StakeholderLastName}".Trim();
                    }

                    // تعیین رنگ و وضعیت
                    bool isCompleted = task.CompletionDate.HasValue;
                    bool isOverdue = !isCompleted && task.DueDate.HasValue && task.DueDate < DateTime.Now;

                    string calendarColor = isCompleted ? "#28a745" :    // سبز برای تکمیل شده
                                          isOverdue ? "#dc3545" :       // قرمز برای عقب افتاده
                                          task.Important ? "#ff6b35" :  // نارنجی برای مهم
                                          task.Priority == 2 ? "#e74c3c" : // قرمز تیره برای فوری
                                          "#007bff";                     // آبی برای عادی

                    string statusText = isCompleted ? "تکمیل شده" :
                                      isOverdue ? "عقب افتاده" :
                                      task.Important ? "مهم" :
                                      task.Priority == 2 ? "فوری" :
                                      "در حال انجام";

                    // ⭐ تعیین تاریخ شروع و پایان برای نمایش چند روزه
                    DateTime startDate = task.StartDate ?? task.CreateDate;
                    DateTime? endDate = task.DueDate ?? task.CreateDate.AddDays(1);

                    // اطمینان از اینکه تاریخ پایان بعد از تاریخ شروع است
                    if (endDate <= startDate)
                    {
                        endDate = startDate != null ? startDate.AddDays(1) : null;
                    }

                    // ایجاد رویداد تقویم
                    var calendarEvent = new TaskCalendarViewModel
                    {
                        Id = task.Id,
                        Title = task.Title ?? string.Empty,
                        Description = task.Description,
                        TaskCode = task.TaskCode,
                        CreateDate = task.CreateDate,
                        DueDate = task.DueDate,
                        StartDate = startDate,  // ⭐ تاریخ شروع
                        EndDate = endDate,      // ⭐ تاریخ پایان
                        IsCompleted = isCompleted,
                        IsOverdue = isOverdue,
                        StakeholderId = task.StakeholderId,
                        StakeholderName = stakeholderName,
                        CategoryTitle = task.CategoryTitle ?? "ندارد",
                        BranchName = "ندارد", // TODO: اضافه کردن منطق شعبه در صورت نیاز
                        CalendarColor = calendarColor,
                        StatusText = statusText,
                        CreatorUserId = task.CreatorUserId ?? string.Empty
                    };

                    calendarEvents.Add(calendarEvent);
                }

                // مرحله چهارم: اضافه کردن رویدادهای تاریخ‌های شخصی
                var taskIds = rawTasks.Select(t => t.Id).ToList();
                await AddPersonalEventsToCalendarAsync(calendarEvents, taskIds, userId);

                // مرحله پنجم: مرتب‌سازی نتایج
                return calendarEvents
                    .OrderBy(e => e.StartDate)
                    .ThenBy(e => e.Title)
                    .ToList();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"Error in GetCalendarEventsAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<TaskCalendarViewModel>();
            }
        }


        /// <summary>
        /// اضافه کردن رویدادهای تاریخ‌های شخصی به تقویم - بازنویسی شده
        /// </summary>
        private async Task AddPersonalEventsToCalendarAsync(List<TaskCalendarViewModel> calendarEvents, List<int> taskIds, string userId)
        {
            try
            {
                if (!taskIds.Any()) return;

                var personalAssignments = await _context.TaskAssignment_Tbl
                    .Include(ta => ta.AssignedUser)
                    .Include(ta => ta.Task)
                    .Where(ta => taskIds.Contains(ta.TaskId) &&
                               (ta.PersonalStartDate.HasValue || ta.PersonalEndDate.HasValue))
                    .ToListAsync();

                foreach (var assignment in personalAssignments)
                {
                    var isMyAssignment = assignment.AssignedUserId == userId;
                    var userInitials = GetUserInitials(assignment.AssignedUser?.FirstName, assignment.AssignedUser?.LastName);

                    // رویداد شروع شخصی
                    if (assignment.PersonalStartDate.HasValue)
                    {
                        var personalStartEvent = new TaskCalendarViewModel
                        {
                            Id = assignment.TaskId,
                            Title = $"[شروع {userInitials}] {assignment.Task.Title}",
                            Description = assignment.PersonalTimeNote,
                            TaskCode = assignment.Task.TaskCode,
                            CreateDate = assignment.Task.CreateDate,
                            DueDate = assignment.PersonalStartDate,
                            StartDate = assignment.PersonalStartDate.Value,
                            EndDate = assignment.PersonalStartDate.Value.AddHours(1), // رویداد یک ساعته
                            IsCompleted = false,
                            IsOverdue = false,
                            CalendarColor = isMyAssignment ? "#4CAF50" : "#81C784",
                            StatusText = isMyAssignment ? "شروع شخصی من" : "شروع شخصی همکار",
                            CreatorUserId = assignment.Task.CreatorUserId ?? string.Empty,
                            CategoryTitle = "تاریخ شخصی",
                            StakeholderName = assignment.AssignedUser != null ?
                                $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}" : "نامشخص",
                            BranchName = "ندارد"
                        };

                        calendarEvents.Add(personalStartEvent);
                    }

                    // رویداد پایان شخصی
                    if (assignment.PersonalEndDate.HasValue)
                    {
                        var personalEndEvent = new TaskCalendarViewModel
                        {
                            Id = assignment.TaskId,
                            Title = $"[پایان {userInitials}] {assignment.Task.Title}",
                            Description = assignment.PersonalTimeNote,
                            TaskCode = assignment.Task.TaskCode,
                            CreateDate = assignment.Task.CreateDate,
                            DueDate = assignment.PersonalEndDate,
                            StartDate = assignment.PersonalEndDate.Value,
                            EndDate = assignment.PersonalEndDate.Value.AddHours(1), // رویداد یک ساعته
                            IsCompleted = false,
                            IsOverdue = false,
                            CalendarColor = isMyAssignment ? "#FF9800" : "#FFB74D",
                            StatusText = isMyAssignment ? "پایان شخصی من" : "پایان شخصی همکار",
                            CreatorUserId = assignment.Task.CreatorUserId ?? string.Empty,
                            CategoryTitle = "تاریخ شخصی",
                            StakeholderName = assignment.AssignedUser != null ?
                                $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}" : "نامشخص",
                            BranchName = "ندارد"
                        };

                        calendarEvents.Add(personalEndEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding personal events: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت تسک‌ها برای Index با فیلترها (نسخه Async جدید) - اصلاح شده
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

                // تنظیم HasActiveFilters
                model.HasActiveFilters = HasActiveFilters(model.Filters);

                // دریافت تسک‌ها بر اساس فیلتر
                await LoadTasksByFilterAsync(model, userId);

                // اگر نوع نمایش MyTasks باشد، گروه‌بندی خاص انجام بده
                if (model.Filters.ViewType == TaskViewType.MyTasks && model.Tasks.Any())
                {
                    await LoadMyTasksGroupedAsync(model, userId);
                }

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
                    GroupedTasks = new TaskGroupedViewModel(),
                    HasActiveFilters = false
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
        /// بارگذاری تسک‌های شخصی (همه تسک‌های مرتبط با کاربر)
        /// </summary>
        private async Task LoadMyTasksAsync(TaskListForIndexViewModel model, string userId)
        {
            var tasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
            model.Tasks = tasks.Select(MapToTaskViewModel).ToList();
        }

        /// <summary>
        /// بارگذاری تسک‌های منتصب شده (فقط تسک‌های واگذار شده از طرف دیگران)
        /// </summary>
        private async Task LoadAssignedToMeTasksAsync(TaskListForIndexViewModel model, string userId)
        {
            // فقط تسک‌هایی که کاربر سازنده آن‌ها نیست ولی منتصب شده
            var tasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: false)
                .Where(t => t.CreatorUserId != userId) // اضافه کردن این شرط
                .ToList();
            model.Tasks = tasks.Select(MapToTaskViewModel).ToList();
        }
        /// <summary>
        /// بارگذاری تسک‌های من با گروه‌بندی خاص - منطق کاملاً اصلاح شده
        /// </summary>
        private async Task LoadMyTasksGroupedAsync(TaskListForIndexViewModel model, string userId)
        {
            try
            {
                if (model.GroupedTasks == null)
                    model.GroupedTasks = new TaskGroupedViewModel();

                if (model.GroupedTasks.MyTasksGrouped == null)
                    model.GroupedTasks.MyTasksGrouped = new MyTasksGroupedViewModel();

                // ⭐ تسک‌های دریافتی (منتصب به من)
                // شامل: تسک‌هایی که من در AssignmentsTaskUser با AssignedUserId من هستم
                var assignedToMe = model.Tasks.Where(t =>
                    t.AssignmentsTaskUser != null &&
                    t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)).ToList();

                model.GroupedTasks.MyTasksGrouped.TasksAssignedToMe = assignedToMe;

                // ⭐ تسک‌های واگذار شده (ایجاد شده توسط من)
                // شامل: تسک‌هایی که من سازنده آنها هستم
                var assignedByMe = model.Tasks.Where(t => t.CreatorUserId == userId).ToList();

                var assignedByMeGrouped = new Dictionary<AssigneeInfo, List<TaskViewModel>>();

                // برای هر تسک که ایجاد کرده‌ام
                foreach (var task in assignedByMe)
                {
                    if (task.AssignmentsTaskUser != null)
                    {
                        // ⭐ گروه‌بندی بر اساس AssignedUserId (نه AssignerUserId)
                        // فیلتر کردن self-assignment برای نمایش در "واگذار شده"
                        var assignees = task.AssignmentsTaskUser
                            .Where(a => !string.IsNullOrEmpty(a.AssignedUserId)) // فقط assignments معتبر
                            .Where(a => a.AssignedUserId != userId) // ⭐ حذف self-assignment از "واگذار شده"
                            .GroupBy(a => a.AssignedUserId) // گروه‌بندی بر اساس AssignedUserId
                            .Select(g => g.First()) // فقط اولین assignment هر کاربر
                            .ToList();

                        foreach (var assignment in assignees)
                        {
                            var assigneeInfo = new AssigneeInfo
                            {
                                Id = assignment.AssignedUserId,
                                FullName = assignment.AssignedUserName,
                                Type = "User",
                                IsTeam = false
                            };

                            // ⭐ ایجاد گروه اگر وجود ندارد
                            if (!assignedByMeGrouped.ContainsKey(assigneeInfo))
                            {
                                assignedByMeGrouped[assigneeInfo] = new List<TaskViewModel>();
                            }

                            // ⭐ اضافه کردن تسک فقط یکبار برای هر کاربر
                            if (!assignedByMeGrouped[assigneeInfo].Any(t => t.Id == task.Id))
                            {
                                assignedByMeGrouped[assigneeInfo].Add(task);
                            }
                        }
                    }
                }

                model.GroupedTasks.MyTasksGrouped.TasksAssignedByMe = assignedByMeGrouped;

                // ⭐ لاگ برای debug
                Console.WriteLine($"Debug: Total assigned to me tasks: {assignedToMe.Count}");
                Console.WriteLine($"Debug: Total created by me tasks: {assignedByMe.Count}");
                Console.WriteLine($"Debug: Unique assignee groups (excluding self): {assignedByMeGrouped.Count}");

                foreach (var group in assignedByMeGrouped)
                {
                    Console.WriteLine($"Debug: Assignee {group.Key.FullName} ({group.Key.Id}) has {group.Value.Count} tasks");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadMyTasksGroupedAsync: {ex.Message}");

                // در صورت خطا، گروه‌بندی خالی
                if (model.GroupedTasks?.MyTasksGrouped != null)
                {
                    model.GroupedTasks.MyTasksGrouped.TasksAssignedToMe = new List<TaskViewModel>();
                    model.GroupedTasks.MyTasksGrouped.TasksAssignedByMe = new Dictionary<AssigneeInfo, List<TaskViewModel>>();
                }
            }
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
                  
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت داده‌های داشبورد: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت تسک‌های واگذارشده توسط کاربر
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
                    .Include(t => t.Stakeholder)
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
                if (!string.IsNullOrEmpty(filters?.FilterType) && filters.FilterType != "all")
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
             
                }).ToList();

                // محاسبه آمار (روی کل داده‌ها، نه فقط فیلتر شده)
                var allReminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId)
                    .ToListAsync();

                var stats = new TaskRemindersStatsViewModel
                {
                    PendingCount = allReminders.Count(r => !r.IsSent && r.ScheduledDateTime <= DateTime.Now),
                    SentCount = allReminders.Count(r => r.IsSent),
                    OverdueCount = allReminders.Count(r => !r.IsSent && r.ScheduledDateTime < DateTime.Now.AddDays(-1)),
                    TodayCount = allReminders.Count(r => r.ScheduledDateTime.Date == DateTime.Now.Date)
                };

                return new TaskRemindersViewModel
                {
                    Reminders = reminderViewModels,
                    Stats = stats,
                    Filters = filters ?? new TaskReminderFilterViewModel { FilterType = "all" },
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
                var myTasks = await GetTasksByUserWithPermissionsAsync(userId, includeAssigned: true, includeCreated: true);

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

        #region Missing Helper Methods Implementation

        /// <summary>
        /// اعمال فیلترها روی IQueryable تسک‌ها
        /// </summary>
        private IQueryable<Tasks> ApplyFiltersToQuery(IQueryable<Tasks> query, TaskFilterViewModel filters)
        {
            if (filters == null) return query;

            // فیلتر شعبه
            if (filters.BranchId.HasValue)
            {
                query = query.Where(t => t.BranchId == filters.BranchId.Value);
            }

            // فیلتر دسته‌بندی
            if (filters.CategoryId.HasValue)
            {
                query = query.Where(t => t.TaskCategoryId == filters.CategoryId.Value);
            }

            // فیلتر وضعیت
            if (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All)
            {
                switch (filters.TaskStatus.Value)
                {
                    case TaskStatusFilter.Completed:
                        query = query.Where(t => t.CompletionDate.HasValue);
                        break;
                    case TaskStatusFilter.InProgress:
                        query = query.Where(t => !t.CompletionDate.HasValue && t.IsActive);
                        break;
                    case TaskStatusFilter.Overdue:
                        query = query.Where(t => !t.CompletionDate.HasValue && t.DueDate.HasValue && t.DueDate < DateTime.Now);
                        break;
                    case TaskStatusFilter.Created:
                        query = query.Where(t => t.Status == 0);
                        break;
                    case TaskStatusFilter.Approved:
                        query = query.Where(t => t.Status == 3);
                        break;
                    case TaskStatusFilter.Rejected:
                        query = query.Where(t => t.Status == 4);
                        break;
                }
            }

            // فیلتر اولویت
            if (filters.TaskPriority.HasValue && filters.TaskPriority != TaskPriorityFilter.All)
            {
                switch (filters.TaskPriority.Value)
                {
                    case TaskPriorityFilter.Normal:
                        query = query.Where(t => t.Priority == 0 && !t.Important);
                        break;
                    case TaskPriorityFilter.Important:
                        query = query.Where(t => t.Important || t.Priority == 1);
                        break;
                    case TaskPriorityFilter.Urgent:
                        query = query.Where(t => t.Priority == 2);
                        break;
                }
            }

            // فیلتر طرف حساب
            if (filters.StakeholderId.HasValue)
            {
                query = query.Where(t => t.StakeholderId == filters.StakeholderId.Value);
            }

            // فیلتر جستجو در متن
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t => 
                    t.Title.Contains(filters.SearchTerm) ||
                    (t.Description != null && t.Description.Contains(filters.SearchTerm)) ||
                    t.TaskCode.Contains(filters.SearchTerm));
            }

            return query;
        }

        /// <summary>
        /// دریافت تعداد یادآوری‌های فعال کاربر
        /// </summary>
        private async Task<int> GetActiveRemindersCountAsync(string userId)
        {
            try
            {
                return await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && 
                               !r.IsRead && 
                               r.ScheduledDateTime <= DateTime.Now)
                    .CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// دریافت نام طرف حساب تسک
        /// </summary>
        private string GetTaskStakeholderName(int taskId)
        {
            try
            {
                var stakeholderName = _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Join(_context.Stakeholder_Tbl,
                          t => t.StakeholderId,
                          s => s.Id,
                          (t, s) => new { s.FirstName, s.LastName, s.CompanyName })
                    .Select(s => !string.IsNullOrEmpty(s.CompanyName) ? s.CompanyName : $"{s.FirstName} {s.LastName}")
                    .FirstOrDefault();

                return stakeholderName ?? "ندارد";
            }
            catch (Exception)
            {
                return "ندارد";
            }
        }

        /// <summary>
        /// دریافت عنوان فعالیت بر اساس وضعیت
        /// </summary>
        private string GetActivityTitle(byte status)
        {
            return status switch
            {
                0 => "تسک ایجاد شد",
                1 => "تسک در حال انجام است",
                2 => "تسک تکمیل شد",
                3 => "تسک تایید شد",
                4 => "تسک رد شد",
                5 => "تسک در انتظار است",
                _ => "وضعیت تسک تغییر کرد"
            };
        }

        /// <summary>
        /// محاسبه زمان گذشته از تاریخ
        /// </summary>
        private string CalculateTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "اکنون";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} روز پیش";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} ماه پیش";
            
            return $"{(int)(timeSpan.TotalDays / 365)} سال پیش";
        }

        #endregion

        #region Personal Dates Management Implementation

        /// <summary>
        /// دریافت انتصاب تسک برای تنظیم تاریخ‌های شخصی
        /// </summary>
        public async Task<TaskAssignment> GetTaskAssignmentForPersonalDatesAsync(int taskId, string userId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .Include(ta => ta.AssignedUser)
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتصاب تسک برای تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت انتصاب تسک بر اساس شناسه انتصاب برای تنظیم تاریخ‌های شخصی
        /// </summary>
        public async Task<TaskAssignment> GetTaskAssignmentByIdForPersonalDatesAsync(int assignmentId, string userId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .Include(ta => ta.AssignedUser)
                    .FirstOrDefaultAsync(ta => ta.Id == assignmentId && ta.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتصاب تسک بر اساس شناسه: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// بروزرسانی تاریخ‌های شخصی انتصاب تسک
        /// </summary>
        public async Task<bool> UpdatePersonalDatesAsync(int assignmentId, string userId, DateTime? personalStartDate, DateTime? personalEndDate, string personalTimeNote)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .FirstOrDefaultAsync(ta => ta.Id == assignmentId && ta.AssignedUserId == userId);

                if (assignment == null)
                {
                    return false;
                }

                // بررسی امکان تغییر تاریخ‌ها (فقط قبل از تکمیل)
                if (assignment.Status >= 3)
                {
                    return false;
                }

                assignment.PersonalStartDate = personalStartDate;
                assignment.PersonalEndDate = personalEndDate;
                assignment.PersonalTimeNote = personalTimeNote;
                assignment.PersonalDatesUpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در بروزرسانی تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        #endregion

        #region Additional Missing Interface Methods
        /// <summary>
        /// دریافت انتصاب‌های تسک همراه با تاریخ‌های شخصی
        /// </summary>
        public async Task<List<TaskAssignment>> GetTaskAssignmentsWithPersonalDatesAsync(int taskId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.AssignedUser)
                    .Include(ta => ta.Task)
                    .Where(ta => ta.TaskId == taskId &&
                               (ta.PersonalStartDate.HasValue || ta.PersonalEndDate.HasValue))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتصاب‌های تسک با تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت حروف اول نام کاربر
        /// </summary>
        public string GetUserInitials(string firstName, string lastName)
        {
            var initials = "";
            if (!string.IsNullOrEmpty(firstName)) initials += firstName[0];
            if (!string.IsNullOrEmpty(lastName)) initials += lastName[0];
            return string.IsNullOrEmpty(initials) ? "کاربر" : initials;
        }

        #endregion

        #region Additional Missing Interface Methods

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
                        IsCompleted = operation.IsCompleted,
                        EstimatedHours = operation.EstimatedHours,
                        IsStarred = operation.IsStarred,
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

        /// <summary>
        /// دریافت متن وضعیت تسک - تبدیل به public
        /// </summary>
        public string GetTaskStatusText(byte status)
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
        /// دریافت کلاس badge برای وضعیت تسک - تبدیل به public
        /// </summary>
        public string GetTaskStatusBadgeClass(byte status)
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
        // اضافه کردن این متدها به کلاس TaskRepository

        /// <summary>
        /// اضافه کردن تسک به "روز من"
        /// </summary>
        public async Task<bool> AddTaskToMyDayAsync(int taskId, string userId, DateTime plannedDate, string? planNote = null)
        {
            try
            {
                // بررسی اینکه آیا قبلاً در این تاریخ وجود دارد
                var existingRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(x => x.TaskId == taskId &&
                                            x.UserId == userId &&
                                            x.PlannedDate.Date == plannedDate.Date &&
                                            x.IsActive);

                if (existingRecord != null)
                {
                    // اگر وجود دارد، فقط یادداشت را بروزرسانی کن
                    existingRecord.PlanNote = planNote;
                    _context.TaskMyDay_Tbl.Update(existingRecord);
                }
                else
                {
                    // ایجاد رکورد جدید
                    var newRecord = new TaskMyDay
                    {
                        TaskId = taskId,
                        UserId = userId,
                        PlannedDate = plannedDate.Date,
                        PlanNote = planNote,
                        CreatedDate = DateTime.Now,
                        IsWorkedOn = false,
                        IsActive = true
                    };

                    await _context.TaskMyDay_Tbl.AddAsync(newRecord);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<bool> LogTaskWorkAsync(int taskId, string userId, string? workNote = null, int? workDurationMinutes = null)
        {
            try
            {
                var today = DateTime.Now.Date;

                // پیدا کردن یا ایجاد رکورد "روز من"
                var myDayRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(x => x.TaskId == taskId &&
                                            x.UserId == userId &&
                                            x.PlannedDate.Date == today &&
                                            x.IsActive);

                if (myDayRecord == null)
                {
                    // اگر در "روز من" نیست، ایجاد کن
                    myDayRecord = new TaskMyDay
                    {
                        TaskId = taskId,
                        UserId = userId,
                        PlannedDate = today,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    await _context.TaskMyDay_Tbl.AddAsync(myDayRecord);
                }

                // بروزرسانی اطلاعات کار
                myDayRecord.IsWorkedOn = true;
                myDayRecord.WorkStartDate = DateTime.Now;
                myDayRecord.WorkNote = workNote;
                myDayRecord.WorkDurationMinutes = workDurationMinutes;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    
        /// <summary>
        /// دریافت تسک‌های "روز من" برای کاربر - اصلاح شده برای نمایش امروز، فردا و دیروز
        /// </summary>
        public async Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? selectedDate = null)
        {
            var targetDate = selectedDate?.Date ?? DateTime.Now.Date;
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // ⭐ فیلتر کردن برای نمایش فقط دیروز، امروز و فردا
            var myDayTasks = await _context.TaskMyDay_Tbl
                .Include(x => x.Task)
                    .ThenInclude(x => x.TaskCategory)
                .Include(x => x.Task)
                    .ThenInclude(x => x.Stakeholder)
                .Where(x => x.UserId == userId && 
                           x.IsActive &&
                           (x.PlannedDate.Date == yesterday || 
                            x.PlannedDate.Date == today || 
                            x.PlannedDate.Date == tomorrow))
                .OrderBy(x => x.PlannedDate)
                .ThenBy(x => x.CreatedDate)
                .ToListAsync();

            var result = new MyDayTasksViewModel
            {
                SelectedDate = targetDate,
                SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(targetDate, "yyyy/MM/dd"),
                PlannedTasks = new List<MyDayTaskItemViewModel>(),
                WorkedTasks = new List<MyDayTaskItemViewModel>(),
                TasksByDate = new Dictionary<string, List<MyDayTaskItemViewModel>>()
            };

            foreach (var item in myDayTasks)
            {
                var taskItem = new MyDayTaskItemViewModel
                {
                    TaskId = item.TaskId,
                    TaskCode = item.Task.TaskCode,
                    TaskTitle = item.Task.Title,
                    TaskDescription = item.Task.Description,
                    CategoryTitle = item.Task.TaskCategory?.Title,
                    StakeholderName = item.Task.Stakeholder?.CompanyName ??
                                   (string.IsNullOrEmpty(item.Task.Stakeholder?.FirstName) ? "ندارد" :
                                    $"{item.Task.Stakeholder.FirstName} {item.Task.Stakeholder.LastName}"),
                    TaskPriority = item.Task.Priority,
                    IsImportant = item.Task.Important,
                    PlanNote = item.PlanNote,
                    WorkNote = item.WorkNote,
                    WorkDurationMinutes = item.WorkDurationMinutes,
                    IsWorkedOn = item.IsWorkedOn,
                    WorkStartDate = item.WorkStartDate,
                    CreatedDate = item.CreatedDate,
                    TaskStatus = item.Task.Status,
                    ProgressPercentage = CalculateTaskProgress(item.Task),
                    PlannedDate = item.PlannedDate,
                    PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(item.PlannedDate, "yyyy/MM/dd"),
                 
                };

                // گروه‌بندی بر اساس تاریخ
                var dateKey = taskItem.PlannedDatePersian;
                if (!result.TasksByDate.ContainsKey(dateKey))
                {
                    result.TasksByDate[dateKey] = new List<MyDayTaskItemViewModel>();
                }
                result.TasksByDate[dateKey].Add(taskItem);

                // همچنان نگهداری لیست‌های قدیمی برای سازگاری
                if (item.IsWorkedOn)
                    result.WorkedTasks.Add(taskItem);
                else
                    result.PlannedTasks.Add(taskItem);
            }

            // محاسبه آمار کلی
            result.Stats = new MyDayStatsViewModel
            {
                TotalPlannedTasks = result.PlannedTasks.Count + result.WorkedTasks.Count,
                WorkedTasks = result.WorkedTasks.Count,
                CompletedTasks = result.WorkedTasks.Count(x => x.TaskStatus >= 2),
                TotalWorkTimeMinutes = result.WorkedTasks.Sum(x => x.WorkDurationMinutes ?? 0)
            };

            return result;
        }

        /// <summary>
        /// بررسی اینکه آیا تسک در "روز من" وجود دارد
        /// </summary>
        public async Task<bool> IsTaskInMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            return await _context.TaskMyDay_Tbl
                .AnyAsync(x => x.TaskId == taskId &&
                              x.UserId == userId &&
                              x.PlannedDate.Date == checkDate &&
                              x.IsActive);
        }

        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        public async Task<bool> RemoveTaskFromMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            try
            {
                var checkDate = targetDate?.Date ?? DateTime.Now.Date;

                var record = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(x => x.TaskId == taskId &&
                                            x.UserId == userId &&
                                            x.PlannedDate.Date == checkDate &&
                                            x.IsActive);

                if (record != null)
                {
                    record.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// محاسبه درصد پیشرفت تسک
        /// </summary>
        private int CalculateTaskProgress(Tasks task)
        {
            if (task.Status >= 2) return 100; // تکمیل شده یا بالاتر

            var totalOperations = task.TaskOperations?.Count ?? 0;
            if (totalOperations == 0) return task.Status * 25; // 0%, 25%, 50%, 75% بر اساس وضعیت

            var completedOperations = task.TaskOperations?.Count(x => x.IsCompleted) ?? 0;
            return (int)((double)completedOperations / totalOperations * 100);
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای کاربر
        /// </summary>
        public async Task<int> GetMyDayTasksCountAsync(string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            return await _context.TaskMyDay_Tbl
                .CountAsync(x => x.UserId == userId &&
                                x.PlannedDate.Date == checkDate &&
                                x.IsActive);
        }
        #endregion
    }
}