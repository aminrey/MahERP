using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
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
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;
        // ⭐ اضافه کردن این dependency ها به constructor
        private readonly ITaskGroupingRepository _groupingRepository;
        private readonly ITaskFilteringRepository _filteringRepository;

        public TaskRepository(
            AppDbContext context,
            IBranchRepository branchRipository,
            IUnitOfWork unitOfWork,
            IUserManagerRepository userManagerRepository,
            IStakeholderRepository stakeholderRepo,
            TaskCodeGenerator taskCodeGenerator,
            ITaskVisibilityRepository taskVisibilityRepository,
            ITaskHistoryRepository taskHistoryRepository,
            IMapper mapper,
            ITaskGroupingRepository groupingRepository,  // ⭐⭐⭐ جدید
            ITaskFilteringRepository filteringRepository) // ⭐⭐⭐ جدید
        {
            _context = context;
            _BranchRipository = branchRipository;
            _unitOfWork = unitOfWork;
            _userManagerRepository = userManagerRepository;
            _StakeholderRepo = stakeholderRepo;
            _taskCodeGenerator = taskCodeGenerator;
            _taskVisibilityRepository = taskVisibilityRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
            _groupingRepository = groupingRepository;      // ⭐⭐⭐ جدید
            _filteringRepository = filteringRepository;    // ⭐⭐⭐ جدید
        }
        #region Core CRUD Operations


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

            // ⭐⭐⭐ NEW: مقداردهی اولیه لیست‌های Contact/Organization
            Tasks.ContactsInitial = new List<ContactViewModel>();
            Tasks.OrganizationsInitial = new List<OrganizationViewModel>();
            Tasks.ContactOrganizations = new List<OrganizationViewModel>();

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

        
public Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, bool includeAttachments = false, bool includeComments = false, bool includeStakeHolders = false,bool includeTaskWorkLog = false)
        {
            var query = _context.Tasks_Tbl.AsQueryable();

            if (includeTaskWorkLog)
            {
                query = query.Include(w => w.TaskWorkLogs);
            }

            if (includeOperations)
                query = query.Include(t => t.TaskOperations.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.WorkLogs);

            if (includeAssignments)
                query = query.Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser)
                    .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignerUser);

            if (includeAttachments)
                query = query.Include(t => t.TaskAttachments);

            if (includeComments)
            {
                // ⭐⭐⭐ اصلاح: اضافه کردن ThenInclude برای Attachments
                query = query.Include(t => t.TaskComments)
                    .ThenInclude(c => c.Creator)
                    .Include(t => t.TaskComments) // ⭐ دوباره Include برای ThenInclude بعدی
                    .ThenInclude(c => c.Attachments) // ⭐⭐⭐ فایل‌های پیوست
                    .ThenInclude(a => a.Uploader); // ⭐ اطلاعات آپلودکننده
            }

            if (includeStakeHolders)
            {
                query = query.Include(t => t.Contact)
                    .ThenInclude(c => c.Phones);
                query = query.Include(t => t.Organization)
                    .ThenInclude(o => o.Departments);
            }

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

        /// <summary>
        /// دریافت تسک‌های چند شعبه (نسخه چندگانه)
        /// </summary>
        public List<Tasks> GetTasksByBranches(List<int> branchIds, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl
                .Where(t => branchIds.Contains(t.BranchId ?? 0))
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        /// <summary>
        /// دریافت تسک‌های شعبه (نسخه قدیمی - حفظ برای سازگاری)
        /// </summary>
        public List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false)
        {
            return GetTasksByBranches(new List<int> { branchId }, includeDeleted);
        }

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
        public async Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false, bool includeSupervisedTasks = false)
        {
            var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            // ⭐ اضافه کردن تسک‌های نظارتی اگر درخواست شده باشد
            var supervisedTaskIds = new List<int>();
            if (includeSupervisedTasks)
            {
                supervisedTaskIds = await GetSupervisedTaskIdsAsync(userId);
            }

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) || // تسک‌های قابل مشاهده
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id))); // ⭐ تسک‌های نظارتی
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) || // تسک‌های قابل مشاهده
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id))); // ⭐ تسک‌های نظارتی
            }
            else if (includeCreated)
            {
                query = query.Where(t =>
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id))); // ⭐ تسک‌های نظارتی
            }
            else if (includeSupervisedTasks)
            {
                // ⭐ فقط تسک‌های نظارتی
                query = query.Where(t => supervisedTaskIds.Contains(t.Id));
            }

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        /// <summary>
        /// دریافت شناسه تسک‌هایی که کاربر ناظر آن‌هاست - متد کمکی
        /// </summary>
        private async Task<List<int>> GetSupervisedTaskIdsAsync(string userId)
        {
            try
            {
                var supervisedTaskIds = new HashSet<int>();

                // 1. نظارت بر اساس سمت (CanViewSubordinateTasks = true)
                var supervisoryPositions = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.Position != null &&
                                tm.Position.CanViewSubordinateTasks)
                    .ToListAsync();

                foreach (var supervisoryPosition in supervisoryPositions)
                {
                    // دریافت اعضای با سمت پایین‌تر در همان تیم
                    var subordinateMembers = await _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == supervisoryPosition.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.Position != null &&
                                   tm.Position.PowerLevel > supervisoryPosition.Position.PowerLevel)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    // دریافت تسک‌های منتصب شده به افراد تحت نظارت
                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => subordinateMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    // دریافت تسک‌های ایجاد شده توسط افراد تحت نظارت
                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => subordinateMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    // اضافه کردن به مجموعه
                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                // 2. نظارت بر اساس MembershipType = 1 (ناظر هم سطح و زیر دستان)
                var supervisoryMemberships = await _context.TeamMember_Tbl
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.MembershipType == 1) // ناظر
                    .ToListAsync();

                foreach (var supervisoryMembership in supervisoryMemberships)
                {
                    // دریافت اعضای عادی تیم
                    var ordinaryMembers = await _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == supervisoryMembership.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.MembershipType == 0) // عضو عادی
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    // دریافت تسک‌های منتصب شده به اعضای عادی
                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => ordinaryMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    // دریافت تسک‌های ایجاد شده توسط اعضای عادی
                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => ordinaryMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    // اضافه کردن به مجموعه
                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                return supervisedTaskIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSupervisedTaskIdsAsync: {ex.Message}");
                return new List<int>();
            }
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
        /// تبدیل Task Entity به TaskViewModel - اصلاح شده برای حذف Stakeholder
        /// </summary>
        private TaskViewModel MapToTaskViewModel(Tasks task)
        {
            // دریافت انتساب‌های تسک
            var assignments = _context.TaskAssignment_Tbl
                .Include(ta => ta.AssignedUser)
                .Where(ta => ta.TaskId == task.Id)
                .ToList();

            // دریافت اطلاعات دسته‌بندی
            var category = _context.TaskCategory_Tbl.FirstOrDefault(c => c.Id == task.TaskCategoryId);

            // ⭐⭐⭐ دریافت اطلاعات Contact
            Contact contact = null;
            if (task.ContactId.HasValue)
            {
                contact = _context.Contact_Tbl.FirstOrDefault(c => c.Id == task.ContactId.Value);
            }

            // ⭐⭐⭐ دریافت اطلاعات Organization
            Organization organization = null;
            if (task.OrganizationId.HasValue)
            {
                organization = _context.Organization_Tbl.FirstOrDefault(o => o.Id == task.OrganizationId.Value);
            }

            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                ManagerApprovedDate = task.ManagerApprovedDate,
                SupervisorApprovedDate = task.SupervisorApprovedDate,
                IsActive = task.IsActive,
                IsDeleted = task.IsDeleted,
                BranchId = task.BranchId,
                CreatorUserId = task.CreatorUserId,

                // ⭐⭐⭐ OLD - Stakeholder (Deprecated - حذف شده)
                StakeholderId = task.StakeholderId,
                StakeholderName = null, // ⚠️ دیگر Stakeholder وجود ندارد

                // ⭐⭐⭐ NEW - Contact & Organization
                SelectedContactId = task.ContactId,
                ContactFullName = contact != null
                    ? $"{contact.FirstName} {contact.LastName}"
                    : null,

                SelectedOrganizationId = task.OrganizationId,
                OrganizationName = organization?.DisplayName,

                // بقیه فیلدها...
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

                // Assignments
                AssignmentsTaskUser = assignments
                    .Where(a => !string.IsNullOrEmpty(a.AssignedUserId))
                    .Select(a => new TaskAssignmentViewModel
                    {
                        Id = a.Id,
                        TaskId = a.TaskId,
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null
                            ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                            : "نامشخص",
                        AssignerUserId = a.AssignerUserId,
                        AssignDate = a.AssignmentDate,
                        CompletionDate = a.CompletionDate,
                        Description = a.Description,
                        IsFocused = a.IsFocused,
                        FocusedDate = a.FocusedDate
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
            var branchIds = GetUserBranchIds(userId);
            return branchIds.FirstOrDefault(); // اولین شعبه
        }
        /// <summary>
        /// دریافت همه شعبه‌های کاربر (نسخه چندگانه)
        /// </summary>
        public List<int> GetUserBranchIds(string userId)
        {
            var branchIds = _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Select(bu => bu.BranchId)
                .Distinct()
                .ToList();

            // اگر کاربر در هیچ شعبه‌ای نیست، شعبه پیش‌فرض
            return branchIds.Any() ? branchIds : new List<int> { 1 };
        }

        #endregion

        #region Statistics and Filter Methods
        /// <summary>
        /// محاسبه آمار ثابت - مستقل از ViewType - اصلاح نهایی
        /// </summary>
        private async Task<TaskStatisticsViewModel> CalculateTaskStatisticsAsync(string userId, List<TaskViewModel> filteredTasks)
        {
            try
            {
                // ⭐⭐⭐ مرحله 1: دریافت همه تسک‌هایی که کاربر مجوز دیدن آن‌ها را دارد
                // شامل: تسک‌های شخصی + قابل مشاهده + نظارتی
                var allAccessibleTasks = await GetTasksByUserWithPermissionsAsync(
                    userId,
                    includeAssigned: true,
                    includeCreated: true,
                    includeDeleted: false,
                    includeSupervisedTasks: true); // ⭐ شامل تسک‌های نظارتی

                // ⭐ اضافه کردن تسک‌های قابل مشاهده از طریق Visibility
                var visibleTaskIds = await _taskVisibilityRepository.GetVisibleTaskIdsAsync(userId);
                var visibleTasks = await _context.Tasks_Tbl
                    .Where(t => visibleTaskIds.Contains(t.Id) && !t.IsDeleted)
                    .ToListAsync();

                // ⭐ ترکیب همه تسک‌ها و حذف تکرار
                var combinedTasks = allAccessibleTasks
                    .Concat(visibleTasks)
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                var allAccessibleTaskViewModels = combinedTasks.Select(MapToTaskViewModel).ToList();

                // حذف تکرار از تسک‌های قابل دسترس
                var uniqueAccessibleTasks = allAccessibleTaskViewModels
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // ⭐ مرحله 2: دریافت تسک‌های شخصی کاربر (برای آمار AssignedToMe و AssignedByMe)
                var myTasks = await GetTasksByUserWithPermissionsAsync(
                    userId,
                    includeAssigned: true,
                    includeCreated: true,
                    includeDeleted: false,
                    includeSupervisedTasks: false); // فقط تسک‌های شخصی

                var myTaskViewModels = myTasks.Select(MapToTaskViewModel).ToList();

                // حذف تکرار از تسک‌های شخصی
                var uniqueMyTasks = myTaskViewModels
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // حذف تکرار از تسک‌های فیلتر شده
                var uniqueFilteredTasks = filteredTasks
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                // ⭐⭐⭐ محاسبه آمار
                var statistics = new TaskStatisticsViewModel
                {
                    // ⭐⭐⭐ آمار ثابت (از همه تسک‌های قابل دسترس)
                    TotalTasks = uniqueAccessibleTasks.Count, // ⭐ این عدد ثابت می‌ماند و شامل همه تسک‌های قابل دسترس است

                    // ⭐⭐⭐ آمار شخصی (از تسک‌های شخصی کاربر)
                    AssignedToMe = uniqueMyTasks.Count(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId) &&
                        t.CreatorUserId != userId),

                    AssignedByMe = uniqueMyTasks.Count(t =>
                        t.CreatorUserId == userId),

                    // ⭐⭐⭐ آمار متغیر (از تسک‌های فیلتر شده - بسته به ViewType)
                    CompletedTasks = uniqueFilteredTasks.Count(t =>
                        t.CompletionDate.HasValue),

                    OverdueTasks = uniqueFilteredTasks.Count(t =>
                        !t.CompletionDate.HasValue &&
                        t.DueDate.HasValue &&
                        t.DueDate < DateTime.Now),

                    InProgressTasks = uniqueFilteredTasks.Count(t =>
                        !t.CompletionDate.HasValue &&
                        t.IsActive),

                    ImportantTasks = uniqueFilteredTasks.Count(t =>
                        t.Important ||
                        t.Priority == 1),

                    UrgentTasks = uniqueFilteredTasks.Count(t =>
                        t.Priority == 2),

                    TeamTasks = 0,
                    SubTeamTasks = 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در CalculateTaskStatisticsAsync: {ex.Message}");

                return new TaskStatisticsViewModel
                {
                    TotalTasks = 0,
                    AssignedToMe = 0,
                    AssignedByMe = 0,
                    CompletedTasks = 0,
                    OverdueTasks = 0,
                    InProgressTasks = 0,
                    ImportantTasks = 0,
                    UrgentTasks = 0,
                    TeamTasks = 0,
                    SubTeamTasks = 0
                };
            }
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
                            // تسک‌هایی که کل بازه را می‌پوشاند
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

                // فیلتر کاربران انتساب
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
   .Include(t => t.Contact)           
    .Include(t => t.Organization)
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
                        t.CreatorUserId,
                        t.StakeholderId,
                        t.TaskCategoryId,
                        t.Status,
                        t.Priority,
                        t.Important,
                        t.BranchId,
                     t.TaskAssignments, // شامل انتساب‌ها
                                        // اطلاعات طرف حساب
                                        // ⭐⭐⭐ اطلاعات Contact
                        ContactFirstName = t.Contact != null ? t.Contact.FirstName : null,
                        ContactLastName = t.Contact != null ? t.Contact.LastName : null,
                        // ⭐⭐⭐ اطلاعات Organization
                        OrganizationName = t.Organization != null ? t.Organization.DisplayName : null,

                        // اطلاعات دسته‌بندی
                        CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : null
                    })
                    .ToListAsync();

                // مرحله سوم: تولید رویدادهای تقویم
                var calendarEvents = new List<TaskCalendarViewModel>();

                foreach (var task in rawTasks)
                {
                    // ⭐ تعیین نام (Contact یا Organization)
                    string displayName = "ندارد";

                    if (!string.IsNullOrEmpty(task.ContactFirstName) || !string.IsNullOrEmpty(task.ContactLastName))
                    {
                        displayName = $"{task.ContactFirstName} {task.ContactLastName}".Trim();
                    }
                    else if (!string.IsNullOrEmpty(task.OrganizationName))
                    {
                        displayName = task.OrganizationName;
                    }

                    // تعیین رنگ و وضعیت
                    bool isCompleted =  task.TaskAssignments.Any(t=> t.CompletionDate.HasValue && t.AssignedUserId == userId);
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

                        StakeholderName = displayName,
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
        /// آماده‌سازی مدل برای ایجاد تسک جدید (نسخه Async جدید) - بروزرسانی شده
        /// </summary>
        public async Task<TaskViewModel> PrepareCreateTaskModelAsync(string userId)
        {
            var model = new TaskViewModel
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                TaskCode = _taskCodeGenerator.GenerateTaskCode(),
                TaskCodeSettings = new TaskCodeSettings
                {
                    AllowManualInput = true,
                    SystemPrefix = "TSK"
                }
            };

            // دریافت شعبه‌های کاربر
            var userBranches = _BranchRipository.GetBrnachListByUserId(userId);
            model.branchListInitial = userBranches;

            // ⭐⭐⭐ اگر فقط یک شعبه باشد، خودکار پر کن
            if (userBranches?.Count() == 1)
            {
                var singleBranch = userBranches.First();
                model.BranchIdSelected = singleBranch.Id;

                // بارگذاری کاربران شعبه با "خودم" در صدر
                var branchUsers = await GetBranchUsersWithCurrentUserFirstAsync(singleBranch.Id, userId);
                model.UsersInitial = branchUsers;

                // بارگذاری تیم‌ها
                model.TeamsInitial = await GetBranchTeamsWithManagersAsync(singleBranch.Id);

                // بارگذاری Contacts و Organizations
                model.ContactsInitial = await GetBranchContactsAsync(singleBranch.Id);
                model.OrganizationsInitial = await GetBranchOrganizationsAsync(singleBranch.Id);
            }

            return model;
        }

        /// <summary>
        /// ⭐⭐⭐ متد جدید: دریافت کاربران با "خودم" در صدر
        /// </summary>
        private async Task<List<UserViewModelFull>> GetBranchUsersWithCurrentUserFirstAsync(int branchId, string currentUserId)
        {
            var allUsers = _context.BranchUser_Tbl.Where(
                bu => bu.BranchId == branchId && bu.IsActive
            ).Include(bu => bu.User)
            .Where(bu => bu.User != null && bu.User.IsActive)
            .Select(bu => new UserViewModelFull
            {
                Id = bu.UserId,
                FirstName = bu.User.FirstName,
                LastName = bu.User.LastName,
                UserName = bu.User.UserName,
                Email = bu.User.Email,
                // ⭐ افزودن فیلد برای تصویر پروفایل
                ProfileImagePath = bu.User.ProfileImagePath ?? "/images/default-avatar.png"
            })
            .ToList();

            // ⭐⭐⭐ جدا کردن کاربر جاری
            var currentUser = allUsers.FirstOrDefault(u => u.Id == currentUserId);
            var otherUsers = allUsers.Where(u => u.Id != currentUserId).OrderBy(u => u.FirstName).ToList();

            // ⭐⭐⭐ ساخت لیست نهایی با "خودم" در صدر
            var result = new List<UserViewModelFull>();

            if (currentUser != null)
            {
                // ⭐ تغییر نمایش به "خودم"
                currentUser.FullNamesString = $"خودم ({currentUser.FirstName} {currentUser.LastName})";
                result.Add(currentUser);
            }

            result.AddRange(otherUsers);
            return result;
        }
        /// <summary>
        /// دریافت داده‌های شعبه برای AJAX - بروزرسانی شده
        /// </summary>
        public async Task<BranchSelectResponseViewModel> GetBranchTriggeredDataAsync(int branchId)
        {
            try
            {
                var response = new BranchSelectResponseViewModel
                {
                    // دریافت کاربران شعبه
                    Users = _BranchRipository.GetBranchUsersByBranchId(branchId, includeInactive: false),

                    // دریافت تیم‌های شعبه
                    Teams = await GetBranchTeamsByBranchId(branchId),

                    // ⭐⭐⭐ OLD - حفظ برای backward compatibility
                    Stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(branchId),

                    // ⭐⭐⭐ NEW - سیستم جدید
                    Contacts = await GetBranchContactsAsync(branchId),
                    Organizations = await GetBranchOrganizationsAsync(branchId)
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchTriggeredDataAsync: {ex.Message}");

                return new BranchSelectResponseViewModel
                {
                    Users = new List<BranchUserViewModel>(),
                    Teams = new List<TeamViewModel>(),
                    Stakeholders = new List<StakeholderViewModel>(),
                    Contacts = new List<ContactViewModel>(),
                    Organizations = new List<OrganizationViewModel>()
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
        /// تکمیل داده‌های مدل ایجاد تسک - بروزرسانی شده
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

                // ⭐⭐⭐ OLD - نگهداری برای backward compatibility
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();

                // ⭐⭐⭐ NEW - مقداردهی لیست‌های جدید
                model.ContactsInitial ??= new List<ContactViewModel>();
                model.OrganizationsInitial ??= new List<OrganizationViewModel>();
                model.ContactOrganizations ??= new List<OrganizationViewModel>();

                // اگر شعبه‌ای وجود دارد، داده‌های مربوطه را بارگذاری کن
                if (model.branchListInitial?.Any() == true)
                {
                    var firstBranchId = model.branchListInitial.First().Id;

                    // بارگذاری کاربران
                    var branchData = await GetBranchTriggeredDataAsync(firstBranchId);
                    model.UsersInitial = branchData.Users.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId,
                        FullNamesString = u.UserFullName,
                        IsActive = u.IsActive
                    }).ToList();

                    // ⭐⭐⭐ OLD - بارگذاری Stakeholders
                    model.StakeholdersInitial = branchData.Stakeholders;

                    // ⭐⭐⭐ NEW - بارگذاری Contacts & Organizations
                    model.ContactsInitial = await GetBranchContactsAsync(firstBranchId);
                    model.OrganizationsInitial = await GetBranchOrganizationsAsync(firstBranchId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PopulateCreateTaskDataAsync: {ex.Message}");

                // در صورت خطا لیست‌های خالی
                InitializeEmptyCreateTaskLists(model);
            }
        }
        /// <summary>
        /// مقداردهی اولیه لیست‌های خالی برای مدل ایجاد تسک - بروزرسانی شده
        /// </summary>
        private void InitializeEmptyCreateTaskLists(TaskViewModel model)
        {
            model.branchListInitial ??= new List<BranchViewModel>();
            model.TaskCategoryInitial ??= new List<TaskCategory>();
            model.UsersInitial ??= new List<UserViewModelFull>();
            model.TeamsInitial ??= new List<TeamViewModel>();

            // ⭐⭐⭐ OLD
            model.StakeholdersInitial ??= new List<StakeholderViewModel>();

            // ⭐⭐⭐ NEW
            model.ContactsInitial ??= new List<ContactViewModel>();
            model.OrganizationsInitial ??= new List<OrganizationViewModel>();
            model.ContactOrganizations ??= new List<OrganizationViewModel>();

            model.TaskCodeSettings ??= new TaskCodeSettings
            {
                AllowManualInput = false,
                SystemPrefix = "TSK"
            };

            if (string.IsNullOrEmpty(model.TaskCode))
            {
                model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
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
    .Include(t => t.Contact)       
    .Include(t => t.Organization)  
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
        /// دریافت یادآوری‌های تسک برای کاربر - اصلاح شده برای حل مشکل LINQ Translation
        /// </summary>
        public async Task<TaskRemindersViewModel> GetTaskRemindersAsync(string userId, TaskReminderFilterViewModel filters)
        {
            try
            {
                var now = DateTime.Now;
                var today = now.Date;

                var query = _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId)
                    .Include(r => r.Task)
                    .AsQueryable();

                // ⭐ اصلاح شده: اعمال فیلترها به صورت مستقیم بدون Expression پیچیده
                if (filters != null)
                {
                    // فیلتر قدیمی (برای سازگاری)
                    if (!string.IsNullOrEmpty(filters.FilterType) && filters.FilterType != "all")
                    {
                        switch (filters.FilterType.ToLower())
                        {
                            case "pending":
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime <= now);
                                break;
                            case "sent":
                                query = query.Where(r => r.IsSent);
                                break;
                            case "overdue":
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime < now);
                                break;
                            case "today":
                                query = query.Where(r => r.ScheduledDateTime.Date == today);
                                break;
                            case "upcoming":
                                var maxDate = now.AddDays(filters.DaysAhead ?? 1);
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= maxDate);
                                break;
                            case "unread":
                                query = query.Where(r => r.IsSent && !r.IsRead);
                                break;
                        }
                    }
                    else
                    {
                        // ⭐ فیلترهای جدید - حل شده برای مشکل LINQ
                        bool hasFilter = false;
                        IQueryable<TaskReminderEvent> filteredQuery = null;

                        // یادآوری‌های عقب افتاده
                        if (filters.IncludeOverdueReminders == true)
                        {
                            var overdueQuery = _context.TaskReminderEvent_Tbl
                                .Where(r => r.RecipientUserId == userId && !r.IsSent && r.ScheduledDateTime < now)
                                .Include(r => r.Task);

                            filteredQuery = hasFilter ? filteredQuery.Union(overdueQuery) : overdueQuery;
                            hasFilter = true;
                        }

                        // یادآوری‌های آینده نزدیک
                        if (filters.IncludeUpcomingReminders == true)
                        {
                            var maxDate = now.AddDays(filters.DaysAhead ?? 1);
                            var upcomingQuery = _context.TaskReminderEvent_Tbl
                                .Where(r => r.RecipientUserId == userId && !r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= maxDate)
                                .Include(r => r.Task);

                            filteredQuery = hasFilter ? filteredQuery.Union(upcomingQuery) : upcomingQuery;
                            hasFilter = true;
                        }

                        // یادآوری‌های ارسال شده ولی خوانده نشده
                        if (filters.IncludeUnreadSent == true)
                        {
                            var unreadQuery = _context.TaskReminderEvent_Tbl
                                .Where(r => r.RecipientUserId == userId && r.IsSent && !r.IsRead)
                                .Include(r => r.Task);

                            filteredQuery = hasFilter ? filteredQuery.Union(unreadQuery) : unreadQuery;
                            hasFilter = true;
                        }

                        // یادآوری‌های امروز
                        if (filters.IncludeTodayReminders == true)
                        {
                            var todayQuery = _context.TaskReminderEvent_Tbl
                                .Where(r => r.RecipientUserId == userId && r.ScheduledDateTime.Date == today)
                                .Include(r => r.Task);

                            filteredQuery = hasFilter ? filteredQuery.Union(todayQuery) : todayQuery;
                            hasFilter = true;
                        }

                        // اگر فیلتری اعمال شده، کوئری اصلی را جایگزین کن
                        if (hasFilter)
                        {
                            query = filteredQuery;
                        }
                    }

                    // فیلترهای زمانی
                    if (filters.FromDate.HasValue)
                    {
                        query = query.Where(r => r.ScheduledDateTime >= filters.FromDate.Value);
                    }

                    if (filters.ToDate.HasValue)
                    {
                        query = query.Where(r => r.ScheduledDateTime <= filters.ToDate.Value);
                    }

                    // فیلترهای وضعیت
                    if (filters.IsSent.HasValue)
                    {
                        query = query.Where(r => r.IsSent == filters.IsSent.Value);
                    }

                    if (filters.IsRead.HasValue)
                    {
                        query = query.Where(r => r.IsRead == filters.IsRead.Value);
                    }

                    if (filters.Priority.HasValue)
                    {
                        query = query.Where(r => r.Priority == filters.Priority.Value);
                    }

                    // فیلترهای تسک
                    if (filters.TaskId.HasValue)
                    {
                        query = query.Where(r => r.TaskId == filters.TaskId.Value);
                    }

                    if (filters.TaskIds?.Any() == true)
                    {
                        query = query.Where(r => filters.TaskIds.Contains(r.TaskId));
                    }

                    if (!string.IsNullOrEmpty(filters.TaskTitle))
                    {
                        query = query.Where(r => r.Task != null && r.Task.Title.Contains(filters.TaskTitle));
                    }
                }

                // مرتب‌سازی
                query = ApplyReminderSorting(query, filters);

                // صفحه‌بندی یا محدودیت داشبورد
                if (filters?.ForDashboard == true && filters.DashboardLimit.HasValue)
                {
                    query = query.Take(filters.DashboardLimit.Value);
                }
                else
                {
                    var skip = ((filters?.Page ?? 1) - 1) * (filters?.PageSize ?? 20);
                    query = query.Skip(skip).Take(filters?.PageSize ?? 20);
                }

                var reminders = await query.ToListAsync();

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

                // محاسبه آمار (روی کل داده‌ها)
                var statsQuery = _context.TaskReminderEvent_Tbl.Where(r => r.RecipientUserId == userId);
                var stats = new TaskRemindersStatsViewModel
                {
                    PendingCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime <= now),
                    SentCount = await statsQuery.CountAsync(r => r.IsSent),
                    OverdueCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime < now),
                    TodayCount = await statsQuery.CountAsync(r => r.ScheduledDateTime.Date == today),
                    UnreadCount = await statsQuery.CountAsync(r => r.IsSent && !r.IsRead),
                    UpcomingCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= now.AddDays(1))
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
        /// دریافت یادآوری‌های داشبورد - نسخه ساده و بهینه
        /// </summary>
        public async Task<List<TaskReminderItemViewModel>> GetDashboardRemindersAsync(string userId, int maxResults = 10, int daysAhead = 1)
        {
            try
            {
                var now = DateTime.Now;
                var maxDate = now.AddDays(daysAhead);

                // دریافت یادآوری‌های مهم برای داشبورد
                var reminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId &&
                               (
                                   // عقب افتاده
                                   (!r.IsSent && r.ScheduledDateTime < now) ||
                                   // آینده نزدیک
                                   (!r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= maxDate) ||
                                   // ارسال شده ولی خوانده نشده
                                   (r.IsSent && !r.IsRead)
                               ))
                    .Include(r => r.Task)
                    .OrderBy(r => r.ScheduledDateTime)
                    .Take(maxResults)
                    .ToListAsync();

                return reminders.Select(r => new TaskReminderItemViewModel
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
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت یادآوری‌های داشبورد: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// اعمال مرتب‌سازی بر روی کوئری یادآوری‌ها
        /// </summary>
        private IQueryable<TaskReminderEvent> ApplyReminderSorting(IQueryable<TaskReminderEvent> query, TaskReminderFilterViewModel filters)
        {
            if (filters == null) return query.OrderByDescending(r => r.ScheduledDateTime);

            var sortBy = filters.SortBy?.ToLower() ?? "scheduleddatetime";
            var isDescending = filters.SortDirection?.ToLower() == "desc";

            return sortBy switch
            {
                "scheduleddatetime" => isDescending ? query.OrderByDescending(r => r.ScheduledDateTime) : query.OrderBy(r => r.ScheduledDateTime),
                "title" => isDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "priority" => isDescending ? query.OrderByDescending(r => r.Priority) : query.OrderBy(r => r.Priority),
                "issent" => isDescending ? query.OrderByDescending(r => r.IsSent) : query.OrderBy(r => r.IsSent),
                "isread" => isDescending ? query.OrderByDescending(r => r.IsRead) : query.OrderBy(r => r.IsRead),
                "tasktitle" => isDescending ? query.OrderByDescending(r => r.Task.Title) : query.OrderBy(r => r.Task.Title),
                _ => isDescending ? query.OrderByDescending(r => r.ScheduledDateTime) : query.OrderBy(r => r.ScheduledDateTime)
            };
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
        /// دریافت انتساب تسک برای تنظیم تاریخ‌های شخصی
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
                throw new Exception($"خطا در دریافت انتساب تسک برای تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت انتساب تسک بر اساس شناسه انتساب برای تنظیم تاریخ‌های شخصی
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
                throw new Exception($"خطا در دریافت انتساب تسک بر اساس شناسه: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// بروزرسانی تاریخ‌های شخصی انتساب تسک
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
                throw new Exception($"خطا در دریافت انتساب‌های تسک با تاریخ‌های شخصی: {ex.Message}", ex);
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
        /// اضافه کردن تسک به "روز من" - اصلاح شده برای استفاده از TaskAssignment
        /// </summary>
        public async Task<bool> AddTaskToMyDayAsync(int taskId, string userId, DateTime plannedDate, string? planNote = null)
        {
            try
            {
                // ⭐ دریافت TaskAssignment مربوطه
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null)
                {
                    Console.WriteLine($"❌ Assignment not found for Task {taskId} and User {userId}");
                    return false; // کاربر به این تسک اختصاص داده نشده
                }

                // ⭐ بررسی وجود رکورد قبلی در همین تاریخ
                var existingRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == plannedDate.Date &&
                        !tmd.IsRemoved);

                if (existingRecord != null)
                {
                    // ⭐ بروزرسانی یادداشت
                    existingRecord.PlanNote = planNote;
                    existingRecord.UpdatedDate = DateTime.Now;
                    _context.TaskMyDay_Tbl.Update(existingRecord);
                }
                else
                {
                    // ⭐ ایجاد رکورد جدید
                    var newRecord = new TaskMyDay
                    {
                        TaskAssignmentId = assignment.Id,
                        PlannedDate = plannedDate.Date,
                        PlanNote = planNote,
                        CreatedDate = DateTime.Now,
                        IsRemoved = false
                    };

                    await _context.TaskMyDay_Tbl.AddAsync(newRecord);
                }

                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                    taskId,
                    userId,
                    assignment.Task?.Title ?? "نامشخص",
                    assignment.Task?.TaskCode ?? string.Empty,
                    plannedDate);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddTaskToMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک - اصلاح شده
        /// </summary>
        public async Task<bool> LogTaskWorkAsync(int taskId, string userId, string? workNote = null, int? workDurationMinutes = null)
        {
            try
            {
                var today = DateTime.Now.Date;

                // ⭐ دریافت TaskAssignment
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null)
                {
                    Console.WriteLine($"❌ Assignment not found for Task {taskId} and User {userId}");
                    return false;
                }

                // ⭐ پیدا کردن یا ایجاد رکورد "روز من"
                var myDayRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == today &&
                        !tmd.IsRemoved);

                if (myDayRecord == null)
                {
                    // اگر در "روز من" نیست، ایجاد کن
                    myDayRecord = new TaskMyDay
                    {
                        TaskAssignmentId = assignment.Id,
                        PlannedDate = today,
                        CreatedDate = DateTime.Now,
                        IsRemoved = false
                    };
                    await _context.TaskMyDay_Tbl.AddAsync(myDayRecord);
                }

                // ⭐ بروزرسانی اطلاعات کار
                myDayRecord.WorkStartDate = DateTime.Now;
                myDayRecord.WorkNote = workNote;
                myDayRecord.WorkDurationMinutes = workDurationMinutes;
                myDayRecord.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogTaskWorkAsync: {ex.Message}");
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

            // ⭐ کوئری اصلاح شده
            var myDayTasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
  .Include(tmd => tmd.TaskAssignment.Task.Contact)       
    .Include(tmd => tmd.TaskAssignment.Task.Organization)   
                                                            
    .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    (tmd.PlannedDate.Date == yesterday ||
                     tmd.PlannedDate.Date == today ||
                     tmd.PlannedDate.Date == tomorrow))
                .OrderBy(tmd => tmd.PlannedDate)
                .ThenBy(tmd => tmd.CreatedDate)
                .ToListAsync();

            var result = new MyDayTasksViewModel
            {
                SelectedDate = targetDate,
                SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(targetDate, "yyyy/MM/dd"),
                PlannedTasks = new List<MyDayTaskItemViewModel>(),
                WorkedTasks = new List<MyDayTaskItemViewModel>(),
                TasksByDate = new Dictionary<string, List<MyDayTaskItemViewModel>>()
            };

            foreach (var myDayTask in myDayTasks)
            {
                var task = myDayTask.TaskAssignment.Task;
                var isWorkedOn = !string.IsNullOrEmpty(myDayTask.WorkNote) || myDayTask.WorkStartDate.HasValue;
                // ⭐ تعیین نام (Contact یا Organization)
                string displayName = "ندارد";
                if (task.Contact != null)
                {
                    displayName = $"{task.Contact.FirstName} {task.Contact.LastName}";
                }
                else if (task.Organization != null)
                {
                    displayName = task.Organization.DisplayName;
                }
                var taskItem = new MyDayTaskItemViewModel
                {
                    TaskId = task.Id,
                    TaskAssignmentId = myDayTask.TaskAssignmentId,
                    TaskCode = task.TaskCode,
                    TaskTitle = task.Title,
                    TaskDescription = task.Description,
                    CategoryTitle = task.TaskCategory?.Title,
                    StakeholderName = displayName,
                    TaskPriority = task.Priority,
                    IsImportant = task.Important,
                    IsFocused = myDayTask.TaskAssignment.IsFocused,
                    PlanNote = myDayTask.PlanNote,
                    WorkNote = myDayTask.WorkNote,
                    WorkDurationMinutes = myDayTask.WorkDurationMinutes,
                    IsWorkedOn = isWorkedOn,
                    WorkStartDate = myDayTask.WorkStartDate,
                    CreatedDate = myDayTask.CreatedDate,
                    TaskStatus = myDayTask.TaskAssignment.Status,
                    ProgressPercentage = CalculateTaskProgress(task),
                    PlannedDate = myDayTask.PlannedDate,
                    PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(myDayTask.PlannedDate, "yyyy/MM/dd"),
                };

                // گروه‌بندی بر اساس تاریخ
                var dateKey = taskItem.PlannedDatePersian;
                if (!result.TasksByDate.ContainsKey(dateKey))
                {
                    result.TasksByDate[dateKey] = new List<MyDayTaskItemViewModel>();
                }
                result.TasksByDate[dateKey].Add(taskItem);

                // همچنان نگهداری لیست‌های قدیمی برای سازگاری
                if (isWorkedOn)
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
        /// بررسی اینکه آیا تسک در "روز من" وجود دارد - اصلاح شده
        /// </summary>
        public async Task<bool> IsTaskInMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            // ⭐ کوئری اصلاح شده
            var assignment = await _context.TaskAssignment_Tbl
                .FirstOrDefaultAsync(a =>
                    a.TaskId == taskId &&
                    a.AssignedUserId == userId);

            if (assignment == null) return false;

            return await _context.TaskMyDay_Tbl
                .AnyAsync(tmd =>
                    tmd.TaskAssignmentId == assignment.Id &&
                    tmd.PlannedDate.Date == checkDate &&
                    !tmd.IsRemoved);
        }

        /// <summary>
        /// حذف تسک از "روز من" - اصلاح شده
        /// </summary>
        public async Task<bool> RemoveTaskFromMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            try
            {
                var checkDate = targetDate?.Date ?? DateTime.Now.Date;

                // ⭐ کوئری اصلاح شده
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null) return false;

                var record = await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment.Task)
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == checkDate &&
                        !tmd.IsRemoved);

                if (record != null)
                {
                    record.IsRemoved = true;
                    record.RemovedDate = DateTime.Now;
                    record.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // ⭐ ثبت در تاریخچه
                    await _taskHistoryRepository.LogTaskRemovedFromMyDayAsync(
                        taskId,
                        userId,
                        record.TaskAssignment.Task?.Title ?? "نامشخص",
                        record.TaskAssignment.Task?.TaskCode ?? string.Empty);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveTaskFromMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای کاربر - اصلاح شده
        /// </summary>
        public async Task<int> GetMyDayTasksCountAsync(string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            return await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .CountAsync(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    tmd.PlannedDate.Date == checkDate &&
                    !tmd.IsRemoved);
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

     
        #endregion

        #region Comprehensive User Tasks

        /// <summary>
        /// دریافت همه انواع تسک‌های کاربر به تفکیک نوع - نسخه انتخابی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="includeCreatedTasks">شامل تسک‌های ایجاد شده توسط کاربر</param>
        /// <param name="includeAssignedTasks">شامل تسک‌های منتصب شده به کاربر</param>
        /// <param name="includeSupervisedTasks">شامل تسک‌های تحت نظارت کاربر</param>
        /// <param name="includeDeletedTasks">شامل تسک‌های حذف شده</param>
        /// <returns>ViewModel جامع حاوی انواع تسک‌های انتخاب شده</returns>
        public async Task<UserTasksComprehensiveViewModel> GetUserTasksComprehensiveAsync(
            string userId,
            bool includeCreatedTasks = true,
            bool includeAssignedTasks = true,
            bool includeSupervisedTasks = false,
            bool includeDeletedTasks = false)
        {
            try
            {
                var result = new UserTasksComprehensiveViewModel();

                // 1. دریافت تسک‌های ایجاد شده توسط کاربر (اختیاری)
                if (includeCreatedTasks)
                {
                    var createdTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: false, includeCreated: true, includeDeleted: includeDeletedTasks);
                    result.CreatedTasks = createdTasks.Select(MapToTaskViewModel).ToList();
                }

                // 2. دریافت تسک‌های منتصب شده به کاربر (اختیاری)
                if (includeAssignedTasks)
                {
                    var assignedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: true, includeCreated: false, includeDeleted: includeDeletedTasks);
                    var filteredAssignedTasks = assignedTasks.Where(t => t.CreatorUserId != userId).ToList();
                    result.AssignedTasks = filteredAssignedTasks.Select(MapToTaskViewModel).ToList();
                }

                // 3. دریافت تسک‌های تحت نظارت (اختیاری)
                if (includeSupervisedTasks)
                {
                    var supervisedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: false, includeCreated: false, includeDeleted: includeDeletedTasks, includeSupervisedTasks: true);
                    result.SupervisedTasks = supervisedTasks.Select(MapToTaskViewModel).ToList();
                }

                // 4. دریافت تسک‌های حذف شده (اختیاری)
                if (includeDeletedTasks)
                {
                    var deletedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: true, includeCreated: true, includeDeleted: true, includeSupervisedTasks: includeSupervisedTasks);
                    result.DeletedTasks = deletedTasks.Where(t => t.IsDeleted).Select(MapToTaskViewModel).ToList();
                }

                // 5. محاسبه آمار
                result.Stats = CalculateUserTasksStats(result);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserTasksComprehensiveAsync: {ex.Message}");
                return new UserTasksComprehensiveViewModel();
            }
        }

        /// <summary>
        /// محاسبه آمار تفصیلی تسک‌های کاربر
        /// </summary>
        private UserTasksStatsViewModel CalculateUserTasksStats(UserTasksComprehensiveViewModel data)
        {
            var today = DateTime.Now.Date;
            var allActiveTasks = data.CreatedTasks
                .Concat(data.AssignedTasks)
                .Concat(data.SupervisedTasks)
                .ToList();

            return new UserTasksStatsViewModel
            {
                CreatedTasksCount = data.CreatedTasks.Count,
                AssignedTasksCount = data.AssignedTasks.Count,
                SupervisedTasksCount = data.SupervisedTasks.Count,
                DeletedTasksCount = data.DeletedTasks.Count,
                CompletedTasksCount = allActiveTasks.Count(t => t.CompletionDate.HasValue),
                OverdueTasksCount = allActiveTasks.Count(t => 
                    !t.CompletionDate.HasValue && 
                    t.DueDate.HasValue && 
                    t.DueDate.Value.Date < today),
                TodayTasksCount = allActiveTasks.Count(t => 
                    t.DueDate.HasValue && 
                    t.DueDate.Value.Date == today)
            };
        }

        #endregion

        #region Task Creation and Validation Helper Methods Implementation

        /// <summary>
        /// اعتبارسنجی مدل تسک قبل از ایجاد یا ویرایش
        /// </summary>
        public async Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateTaskModelAsync(TaskViewModel model, string userId)
        {
            var errors = new Dictionary<string, string>();
            var isValid = true;

            try
            {
                // بررسی شعبه
                if (model.BranchIdSelected <= 0)
                {
                    errors.Add("BranchIdSelected", "انتخاب شعبه الزامی است");
                    isValid = false;
                }
                else
                {
                    var userBranches = _BranchRipository.GetBrnachListByUserId(userId);
                    if (!userBranches.Any(b => b.Id == model.BranchIdSelected))
                    {
                        errors.Add("BranchIdSelected", "شما به شعبه انتخاب شده دسترسی ندارید");
                        isValid = false;
                    }
                }

                // بررسی کد دستی
                if (model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode))
                {
                    if (!_taskCodeGenerator.ValidateTaskCode(model.ManualTaskCode))
                    {
                        var settings = _taskCodeGenerator.GetTaskCodeSettings();
                        errors.Add("ManualTaskCode",
                            $"کد تسک نامعتبر است. نمی‌بایست از پیشوند '{settings.SystemPrefix}-' استفاده کنید");
                        isValid = false;
                    }
                }

                return (isValid, errors);
            }
            catch (Exception ex)
            {
                errors.Add("General", $"خطا در اعتبارسنجی: {ex.Message}");
                return (false, errors);
            }
        }
        /// <summary>
        /// ایجاد entity تسک از ViewModel - بروزرسانی شده
        /// </summary>
        public async Task<Tasks> CreateTaskEntityAsync(TaskViewModel model, string currentUserId, IMapper mapper)
        {
            try
            {
                string finalTaskCode = model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode)
                    ? model.ManualTaskCode
                    : _taskCodeGenerator.GenerateTaskCode();

                var task = mapper.Map<Tasks>(model);
                task.TaskCode = finalTaskCode;
                task.CreateDate = DateTime.Now;
                task.CreatorUserId = currentUserId;
                task.IsActive = model.IsActive;
                task.IsDeleted = false;
                task.TaskTypeInput = 1;

                // ⭐⭐⭐ مقدار پیش‌فرض تیمی
                task.VisibilityLevel = model.VisibilityLevel > 0 ? model.VisibilityLevel : (byte)2;

                task.Priority = 0;
                task.Important = false;
                task.Status = 0;
                task.CreationMode = 0;
                task.TaskCategoryId = model.TaskCategoryIdSelected;
                task.BranchId = model.BranchIdSelected;

                // ⭐⭐⭐ OLD - Stakeholder (حفظ برای backward compatibility)
                task.StakeholderId = model.StakeholderId;

                // ⭐⭐⭐ NEW - Contact & Organization
                task.ContactId = model.SelectedContactId;
                task.OrganizationId = model.SelectedOrganizationId;
                if (!task.OrganizationId.HasValue && task.ContactId.HasValue)
                {
                    // پیدا کردن اولین سازمان مرتبط با Contact
                    var contactOrganization = await _context.OrganizationContact_Tbl
                        .Where(oc => oc.ContactId == task.ContactId.Value && oc.IsActive && oc.IsPrimary)
                        .FirstOrDefaultAsync();

                    if (contactOrganization != null)
                    {
                        task.OrganizationId = contactOrganization.OrganizationId;
                    }
                    else
                    {
                        // اگر سازمان اصلی نداشت، اولین سازمان را انتخاب کن
                        var anyContactOrganization = await _context.OrganizationContact_Tbl
                            .Where(oc => oc.ContactId == task.ContactId.Value && oc.IsActive)
                            .FirstOrDefaultAsync();

                        if (anyContactOrganization != null)
                        {
                            task.OrganizationId = anyContactOrganization.OrganizationId;
                        }
                    }
                }

                // تبدیل تاریخ‌های شمسی
                if (!string.IsNullOrEmpty(model.SuggestedStartDatePersian))
                {
                    task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.SuggestedStartDatePersian);
                }

                if (!string.IsNullOrEmpty(model.StartDatePersian))
                {
                    task.StartDate = ConvertDateTime.ConvertShamsiToMiladi(model.StartDatePersian);
                }

                // ذخیره در دیتابیس
                _unitOfWork.TaskUW.Create(task);
                await _unitOfWork.SaveAsync();

                return task;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد entity تسک: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ذخیره عملیات‌ها و یادآوری‌های تسک
        /// </summary>
        public async Task SaveTaskOperationsAndRemindersAsync(int taskId, TaskViewModel model)
        {
            try
            {
                // ذخیره عملیات‌ها
                if (!string.IsNullOrEmpty(model.TaskOperationsJson))
                {
                    try
                    {
                        var operations = System.Text.Json.JsonSerializer.Deserialize<List<TaskOperationViewModel>>(model.TaskOperationsJson);
                        if (operations?.Any() == true)
                        {
                            SaveTaskOperations(taskId, operations);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ذخیره عملیات‌ها: {ex.Message}");
                    }
                }

                // ذخیره یادآوری‌ها
                if (!string.IsNullOrEmpty(model.TaskRemindersJson))
                {
                    try
                    {
                        var reminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(model.TaskRemindersJson);
                        if (reminders?.Any() == true)
                        {
                            SaveTaskReminders(taskId, reminders);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ذخیره یادآوری‌ها: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ذخیره عملیات‌ها و یادآوری‌ها: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// مدیریت انتصاب‌های تسک (نسخه ساده)
        /// </summary>
        public async Task HandleTaskAssignmentsAsync(Tasks task, TaskViewModel model, string currentUserId)
        {
            try
            {
                var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

                // کاربران از تیم‌ها
                var teamUserIds = await GetUsersFromTeamsAsync(assignedTeamIds);
                var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                // اختصاص به سایرین
                foreach (var assignedUserId in allAssignedUserIds)
                {
                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = assignedUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 0,
                        AssignmentDate = DateTime.Now,
                        Description = assignedUserIds.Contains(assignedUserId) ? "انتصاب مستقیم" : "انتصاب از طریق تیم",
                        Status = 0,
                    };
                    _unitOfWork.TaskAssignmentUW.Create(assignment);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در مدیریت انتصاب‌ها: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ذخیره فایل‌های پیوست تسک
        /// </summary>
        public async Task SaveTaskAttachmentsAsync(int taskId, List<IFormFile> files, string uploaderUserId, string webRootPath)
        {
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("⚠️ No files to save");
                return;
            }

            // ⭐ ایجاد پوشه uploads
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "tasks", taskId.ToString());

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                Console.WriteLine($"📁 Created folder: {uploadsFolder}");
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        // ⭐ نام فایل یکتا
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // ⭐ ذخیره فایل
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // ⭐ ذخیره رکورد در دیتابیس
                        var attachment = new TaskAttachment
                        {
                            TaskId = taskId,
                            FileName = file.FileName,
                            FilePath = $"/uploads/tasks/{taskId}/{fileName}",
                            FileSize = file.Length,
                            UploaderUserId = uploaderUserId,
                            UploadDate = DateTime.Now
                        };

                        _context.TaskAttachment_Tbl.Add(attachment);
                        Console.WriteLine($"✅ File saved: {file.FileName} ({file.Length} bytes)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error saving file {file.FileName}: {ex.Message}");
                        throw;
                    }
                }
            }

            _context.SaveChanges();
        }

        #endregion

        #region Task Status Helper Methods Implementation

        /// <summary>
        /// دریافت رنگ وضعیت تسک برای تقویم
        /// </summary>
        public string GetTaskStatusColor(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "#28a745"; // سبز
            if (task.IsOverdue) return "#dc3545";   // قرمز
            return "#007bff";                       // آبی
        }

        /// <summary>
        /// دریافت متن وضعیت تسک برای تقویم
        /// </summary>
        public string GetTaskStatusTextForCalendar(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "تکمیل شده";
            if (task.IsOverdue) return "عقب افتاده";
            return "در حال انجام";
        }

        #endregion



        #endregion

        // ⭐ اضافه کردن این متدها به کلاس TaskRepository

        #region Missing Methods from ITaskRepository

        /// <summary>
        /// دریافت تسک‌های طرف حساب
        /// </summary>
        public List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.StakeholderId == stakeholderId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
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

        /// <summary>
        /// مدیریت انتصاب‌های تسک با Bulk Insert - نسخه کامل
        /// </summary>
        public async Task HandleTaskAssignmentsBulkAsync(Tasks task, TaskViewModel model, string currentUserId)
        {
            try
            {
                var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

                // پاکسازی userId ها
                assignedUserIds = assignedUserIds
                    .SelectMany(id => {
                        if (string.IsNullOrWhiteSpace(id)) return Enumerable.Empty<string>();

                        var cleaned = id
                            .Trim()
                            .Trim('[', ']', '/', ' ', '\t', '\n', '\r', '"', '\'', '`')
                            .Replace("[", "").Replace("]", "")
                            .Replace("/", "").Replace("\\", "")
                            .Replace("\"", "").Replace("'", "")
                            .Trim();

                        if (cleaned.Contains(","))
                        {
                            return cleaned.Split(',')
                                .Select(s => s.Trim().Trim('"', '\'', '[', ']'))
                                .Where(s => !string.IsNullOrWhiteSpace(s));
                        }

                        return new[] { cleaned };
                    })
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                // Parse User-Team Map
                Dictionary<string, int> userTeamMap = new Dictionary<string, int>();
                if (!string.IsNullOrEmpty(model.UserTeamAssignmentsJson))
                {
                    try
                    {
                        var rawMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(
                            model.UserTeamAssignmentsJson) ?? new Dictionary<string, int>();

                        foreach (var kvp in rawMap)
                        {
                            var cleanedKey = kvp.Key
                                .Trim('[', ']', '/', ' ', '"', '\'')
                                .Replace("[", "").Replace("]", "").Replace("/", "")
                                .Trim();

                            if (!string.IsNullOrWhiteSpace(cleanedKey))
                            {
                                userTeamMap[cleanedKey] = kvp.Value;
                            }
                        }
                    }
                    catch
                    {
                        // Silent fail - userTeamMap remains empty
                    }
                }

                // دریافت کاربران از تیم‌ها
                var teamUserIds = await GetUsersFromTeamsAsync(assignedTeamIds);
                teamUserIds = teamUserIds
                    .Select(id => id?.Trim('[', ']', '/', ' ', '"').Replace("[", "").Replace("]", "").Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                if (!allAssignedUserIds.Any())
                {
                    return;
                }

                // اعتبارسنجی در دیتابیس
                var existingUserIds = await _context.Users
                    .AsNoTracking()
                    .Where(u => allAssignedUserIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                var invalidUserIds = allAssignedUserIds.Except(existingUserIds).ToList();
                if (invalidUserIds.Any())
                {
                    var invalidUsersStr = string.Join(", ", invalidUserIds.Select(id => $"'{id}'"));
                    throw new InvalidOperationException($"کاربران زیر یافت نشدند: {invalidUsersStr}");
                }

                // ایجاد assignments
                var assignments = new List<TaskAssignment>();
                var assignmentDate = DateTime.Now;

                foreach (var assignedUserId in existingUserIds)
                {
                    int? assignedInTeamId = null;

                    // تعیین تیم مربوطه
                    if (userTeamMap.ContainsKey(assignedUserId))
                    {
                        var teamId = userTeamMap[assignedUserId];
                        assignedInTeamId = teamId == 0 ? null : (int?)teamId;
                    }
                    else if (teamUserIds.Contains(assignedUserId))
                    {
                        var userTeams = await GetUserTeamsInBranchAsync(assignedUserId, task.BranchId ?? 0);
                        assignedInTeamId = userTeams.FirstOrDefault()?.Id;
                    }

                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = assignedUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 0,
                        AssignmentDate = assignmentDate,
                        Description = assignedUserIds.Contains(assignedUserId)
                            ? "انتصاب مستقیم"
                            : "انتصاب از طریق تیم",
                        Status = 0,
                        AssignedInTeamId = assignedInTeamId,
                        DueDate = task.DueDate,
                        StartDate = task.StartDate,
                        IsRead = false,
                        IsFavorite = false,
                        IsMyDay = false
                    };

                    assignments.Add(assignment);
                }

                if (assignments.Any())
                {
                    _context.TaskAssignment_Tbl.AddRange(assignments);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در مدیریت انتصاب‌ها: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// دریافت تیم‌های کاربر در شعبه مشخص - با اطلاعات کامل مدیر
        /// </summary>
        public async Task<List<TeamViewModel>> GetUserTeamsByBranchAsync(string userId, int branchId)
        {
            try
            {
                Console.WriteLine($"🔍 GetUserTeamsByBranchAsync: UserId={userId}, BranchId={branchId}");

                var userTeams = await _context.TeamMember_Tbl
                    .Where(tm =>
                        tm.UserId == userId &&
                        tm.IsActive &&
                        tm.Team.BranchId == branchId &&
                        tm.Team.IsActive)
                    .Include(tm => tm.Team)
                        .ThenInclude(t => t.Manager) // ⭐⭐⭐ Include Manager
                    .Include(tm => tm.Team.TeamMembers.Where(m => m.IsActive))
                    .Select(tm => new TeamViewModel
                    {
                        Id = tm.Team.Id,
                        Title = tm.Team.Title,
                        ManagerUserId = tm.Team.ManagerUserId,
                        ManagerName = tm.Team.Manager != null
                            ? $"{tm.Team.Manager.FirstName} {tm.Team.Manager.LastName}"
                            : null,
                        MemberCount = tm.Team.TeamMembers.Count(m => m.IsActive),
                        BranchId = tm.Team.BranchId,
                        IsActive = tm.Team.IsActive
                    })
                    .Distinct()
                    .OrderBy(t => t.Title)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {userTeams.Count} teams");
                foreach (var team in userTeams)
                {
                    Console.WriteLine($"   - Team: {team.Title}, Manager: {team.ManagerName ?? "بدون مدیر"}, Members: {team.MemberCount}");
                }

                // ⭐⭐⭐ اگر هیچ تیمی نیافت، گزینه "بدون تیم" برگردان
                if (!userTeams.Any())
                {
                    Console.WriteLine("⚠️ No teams found, returning 'بدون تیم' option");
                    return new List<TeamViewModel>
            {
                new TeamViewModel
                {
                    Id = 0,
                    Title = "بدون تیم",
                    ManagerName = null,
                    MemberCount = 0,
                    BranchId = branchId,
                    IsActive = true
                }
            };
                }

                return userTeams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserTeamsByBranchAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // در صورت خطا، لیست خالی با گزینه "بدون تیم" برگردان
                return new List<TeamViewModel>
        {
            new TeamViewModel
            {
                Id = 0,
                Title = "بدون تیم (خطا)",
                ManagerName = null,
                MemberCount = 0
            }
        };
            }
        }

        /// <summary>
        /// دریافت تیم‌های یک کاربر در شعبه - متد کمکی برای HandleTaskAssignmentsBulkAsync
        /// </summary>
        private async Task<List<Team>> GetUserTeamsInBranchAsync(string userId, int branchId)
        {
            return await _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId &&
                            tm.IsActive &&
                            tm.Team.BranchId == branchId &&
                            tm.Team.IsActive)
                .Select(tm => tm.Team)
                .Distinct()
                .ToListAsync();
        }

        #endregion
        #region Task Reminders Management Implementation

        /// <summary>
        /// دریافت لیست یادآوری‌های تسک
        /// </summary>
        public async Task<List<TaskReminderViewModel>> GetTaskRemindersListAsync(int taskId)
        {
            try
            {
                var reminders = await _context.TaskReminderSchedule_Tbl
                    .Include(r => r.Creator)
                    .Where(r => r.TaskId == taskId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .Select(r => new TaskReminderViewModel
                    {
                        Id = r.Id,
                        TaskId = r.TaskId,
                        Title = r.Title,
                        Description = r.Description,
                        ReminderType = r.ReminderType,
                        IntervalDays = r.IntervalDays,
                        DaysBeforeDeadline = r.DaysBeforeDeadline,
                        StartDatePersian = r.StartDate.HasValue
                            ? ConvertDateTime.ConvertMiladiToShamsi(r.StartDate.Value, "yyyy/MM/dd")
                            : null,
                        EndDatePersian = r.EndDate.HasValue
                            ? ConvertDateTime.ConvertMiladiToShamsi(r.EndDate.Value, "yyyy/MM/dd")
                            : null,
                        NotificationTime = r.NotificationTime,
                        IsSystemDefault = r.IsSystemDefault,
                        IsActive = r.IsActive,
                        CreatedDate = r.CreatedDate,
                        CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.CreatedDate, "yyyy/MM/dd HH:mm"),
                        CreatorName = r.Creator != null ? $"{r.Creator.FirstName} {r.Creator.LastName}" : "سیستم"
                    })
                    .ToListAsync();

                return reminders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskRemindersListAsync: {ex.Message}");
                return new List<TaskReminderViewModel>();
            }
        }

        /// <summary>
        /// دریافت یادآوری بر اساس شناسه
        /// </summary>
        public async Task<TaskReminderSchedule> GetReminderByIdAsync(int reminderId)
        {
            try
            {
                return await _context.TaskReminderSchedule_Tbl
                    .Include(r => r.Task)
                    .Include(r => r.Creator)
                    .FirstOrDefaultAsync(r => r.Id == reminderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetReminderByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ایجاد یادآوری جدید
        /// </summary>
        public async Task<int> CreateReminderAsync(TaskReminderViewModel model, string userId)
        {
            try
            {
                var reminder = new TaskReminderSchedule
                {
                    TaskId = model.TaskId,
                    Title = model.Title,
                    Description = model.Description,
                    ReminderType = model.ReminderType,
                    IntervalDays = model.IntervalDays,
                    DaysBeforeDeadline = model.DaysBeforeDeadline,
                    NotificationTime = model.NotificationTime ?? new TimeSpan(9, 0, 0),
                    IsActive = model.IsActive,
                    IsSystemDefault = false,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = userId
                };

                // تبدیل تاریخ شمسی به میلادی
                if (!string.IsNullOrEmpty(model.StartDatePersian))
                {
                    reminder.StartDate = ConvertDateTime.ConvertShamsiToMiladi(model.StartDatePersian);
                }

                if (!string.IsNullOrEmpty(model.EndDatePersian))
                {
                    reminder.EndDate = ConvertDateTime.ConvertShamsiToMiladi(model.EndDatePersian);
                }

                _context.TaskReminderSchedule_Tbl.Add(reminder);
                await _context.SaveChangesAsync();

                return reminder.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CreateReminderAsync: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// غیرفعال کردن یادآوری
        /// </summary>
        public async Task<bool> DeactivateReminderAsync(int reminderId)
        {
            try
            {
                var reminder = await _context.TaskReminderSchedule_Tbl.FindAsync(reminderId);
                if (reminder == null)
                {
                    return false;
                }

                reminder.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DeactivateReminderAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال یادآوری
        /// </summary>
        public async Task<bool> ToggleReminderActiveStatusAsync(int reminderId)
        {
            try
            {
                var reminder = await _context.TaskReminderSchedule_Tbl.FindAsync(reminderId);
                if (reminder == null)
                {
                    return false;
                }

                reminder.IsActive = !reminder.IsActive;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ToggleReminderActiveStatusAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت تسک از طریق شناسه (async)
        /// </summary>
        public async Task<Tasks> GetTaskByIdAsync(int taskId)
        {
            try
            {
                return await _context.Tasks_Tbl
                    .Include(t => t.TaskCategory)
                          .Include(t => t.Contact)
            .Include(t => t.Organization)
                    .Include(t => t.Creator)
                    .Include(t=> t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskByIdAsync: {ex.Message}");
                return null;
            }
        }

        #endregion

        /// <summary>
        /// آماده‌سازی مودال تکمیل تسک
        /// </summary>
        public async Task<CompleteTaskViewModel> PrepareCompleteTaskModalAsync(int taskId, string userId)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskOperations)
                    .Include(t=> t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.IsActive);

                if (task == null)
                    return null;

                // بررسی دسترسی کاربر
                var hasAccess = await CanUserViewTaskAsync(userId, taskId);
                if (!hasAccess)
                    return null;

                // بررسی اینکه آیا کاربر به تسک اختصاص داده شده
                var isAssigned = await _context.TaskAssignment_Tbl
                    .AnyAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (!isAssigned)
                    return null;

                // شمارش عملیات تکمیل نشده
                var pendingOperationsCount = task.TaskOperations
                    ?.Count(o => !o.IsCompleted ) ?? 0;

                var model = new CompleteTaskViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    AdditionalNote = null,
                    AllOperationsCompleted = pendingOperationsCount == 0,
                    PendingOperationsCount = pendingOperationsCount,
                    IsAlreadyCompleted = task.TaskAssignments.Any(t=> t.AssignedUserId == userId && t.CompletionDate.HasValue)
                };

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PrepareCompleteTaskModalAsync: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// ثبت تکمیل تسک - با قفل خودکار، پیشرفت 100% و غیرفعال کردن یادآوری‌ها
        /// </summary>
        public async Task<(bool IsSuccess, string ErrorMessage)> CompleteTaskAsync(CompleteTaskViewModel model, string userId)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskAssignments)
                    .Include(t => t.TaskOperations) // ⭐ برای محاسبه پیشرفت
                    .FirstOrDefaultAsync(t => t.Id == model.TaskId && t.IsActive);

                if (task == null)
                    return (false, "تسک یافت نشد");

                // بررسی اینکه آیا کاربر به تسک اختصاص داده شده
                var assignment = task.TaskAssignments
                    .FirstOrDefault(a => a.AssignedUserId == userId);

                if (assignment == null)
                    return (false, "شما به این تسک اختصاص داده نشده‌اید");

                // ⭐ بررسی اینکه آیا قبلاً تکمیل شده
                if (assignment.CompletionDate.HasValue)
                {
                    // فقط بروزرسانی گزارش در TaskAssignment
                    assignment.UserReport = model.CompletionReport;
                    assignment.ReportDate = DateTime.Now;
                    task.LastUpdateDate = DateTime.Now;
                    task.Status = 2;

                    _context.TaskAssignment_Tbl.Update(assignment);
                }
                else
                {
                    // ⭐⭐⭐ تکمیل جدید - بروزرسانی Tasks
                    assignment.CompletionDate = DateTime.Now;
                    assignment.Status = 2; // تکمیل شده - منتظر تایید
                    task.LastUpdateDate = DateTime.Now;

                    // ⭐⭐⭐ تکمیل خودکار همه عملیات باقیمانده
                    if (task.TaskOperations != null && task.TaskOperations.Any())
                    {
                        foreach (var operation in task.TaskOperations.Where(o => !o.IsCompleted && !o.IsDeleted))
                        {
                            operation.IsCompleted = true;
                            operation.CompletionDate = DateTime.Now;
                            operation.CompletedByUserId = userId;
                            operation.CompletionNote = "تکمیل خودکار هنگام اتمام تسک";

                            _context.TaskOperation_Tbl.Update(operation);
                        }
                    }

                    // ⭐⭐⭐⭐⭐ غیرفعال کردن یادآوری‌های تسک
                    await DeactivateTaskRemindersAsync(model.TaskId);

                    // ⭐ بروزرسانی TaskAssignment
                    assignment.Status = 2; // تکمیل شده
                    assignment.CompletionDate = DateTime.Now;
                    assignment.UserReport = model.CompletionReport;
                    assignment.ReportDate = DateTime.Now;

                    _context.TaskAssignment_Tbl.Update(assignment);
                }

                _context.Tasks_Tbl.Update(task);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CompleteTaskAsync: {ex.Message}\n{ex.StackTrace}");
                return (false, $"خطا در ثبت تکمیل: {ex.Message}");
            }
        }

        /// <summary>
        /// غیرفعال کردن یادآوری‌های یک تسک - متد خصوصی
        /// </summary>
        private async Task DeactivateTaskRemindersAsync(int taskId)
        {
            try
            {
                // ⭐ دریافت همه یادآوری‌های فعال تسک
                var activeReminders = await _context.TaskReminderSchedule_Tbl
                    .Where(r => r.TaskId == taskId && r.IsActive)
                    .ToListAsync();

                if (!activeReminders.Any())
                {
                    Console.WriteLine($"ℹ️ No active reminders found for task {taskId}");
                    return;
                }

                // ⭐ غیرفعال کردن تمام یادآوری‌ها
                foreach (var reminder in activeReminders)
                {
                    reminder.IsActive = false;
                    _context.TaskReminderSchedule_Tbl.Update(reminder);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Deactivated {activeReminders.Count} reminders for task {taskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deactivating reminders for task {taskId}: {ex.Message}");
                // ⚠️ عدم throw کردن exception برای جلوگیری از rollback کل transaction
            }
        }
        #region Task History Methods Implementation
        /// <summary>
        /// غیرفعال کردن یادآوری‌های یک تسک - نسخه عمومی
        /// </summary>
        public async Task<bool> DeactivateAllTaskRemindersAsync(int taskId)
        {
            try
            {
                await DeactivateTaskRemindersAsync(taskId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DeactivateAllTaskRemindersAsync: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// دریافت تاریخچه کامل تسک
        /// </summary>
        public async Task<List<TaskHistoryViewModel>> GetTaskHistoryAsync(int taskId)
        {
            try
            {
                var histories = await _context.TaskHistory_Tbl
                    .Include(h => h.User)
                    .Where(h => h.TaskId == taskId)
                    .OrderByDescending(h => h.ActionDate)
                    .ToListAsync();

                return histories.Select(h => new TaskHistoryViewModel
                {
                    Id = h.Id,
                    ActionType = h.ActionType,
                    Title = h.Title,
                    Description = h.Description,
                    UserName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "سیستم",
                    ActionDate = h.ActionDate,
                    ActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(h.ActionDate, "yyyy/MM/dd HH:mm"),
                    RelatedItemId = h.RelatedItemId,
                    RelatedItemType = h.RelatedItemType,
                    IconClass = GetHistoryIcon(h.ActionType),
                    BadgeClass = GetHistoryBadgeClass(h.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting task history: {ex.Message}");
                return new List<TaskHistoryViewModel>();
            }
        }
        /// <summary>
        /// تخصیص کاربر جدید به تسک
        /// </summary>
        public async Task<bool> AssignUserToTaskAsync(
            int taskId,
            string userId,
            string assignerUserId,
            int? teamId = null,
            string description = null)
        {
            try
            {
                var assignment = new TaskAssignment
                {
                    TaskId = taskId,
                    AssignedUserId = userId,
                    AssignerUserId = assignerUserId,
                    AssignedInTeamId = teamId == 0 ? null : teamId,
                    AssignmentDate = DateTime.Now,
                    Description = description ?? "تخصیص مستقیم",
                    Status = 0,
                    AssignmentType = 0,
                    IsRead = false,
                    IsFavorite = false,
                    IsMyDay = false
                };

                _context.TaskAssignment_Tbl.Add(assignment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignUserToTaskAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// حذف Assignment
        /// </summary>
        public async Task<bool> RemoveTaskAssignmentAsync(int assignmentId)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl.FindAsync(assignmentId);

                if (assignment == null)
                    return false;

                _context.TaskAssignment_Tbl.Remove(assignment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveTaskAssignmentAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت Assignment با اطلاعات کامل
        /// </summary>
        public async Task<TaskAssignment> GetTaskAssignmentByIdAsync(int assignmentId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(a => a.Task)
                    .Include(a => a.AssignedUser)
                    .Include(a => a.AssignerUser)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskAssignmentByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// بررسی تکراری نبودن Assignment
        /// </summary>
        public async Task<TaskAssignment> GetTaskAssignmentByUserAndTaskAsync(string userId, int taskId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskAssignmentByUserAndTaskAsync: {ex.Message}");
                return null;
            }
        }

        // در انتهای کلاس اضافه کنید:

        #region Task Work Log Management

        /// <summary>
        /// ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<(bool Success, string Message, int? WorkLogId)> AddTaskWorkLogAsync(
            int taskId,
            string userId,
            string workDescription,
            int? durationMinutes = null,
            int? progressPercentage = null)
        {
            try
            {
                // بررسی دسترسی کاربر
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (assignment == null)
                {
                    return (false, "شما عضو این تسک نیستید", null);
                }

                var workLog = new TaskWorkLog
                {
                    TaskId = taskId,
                    UserId = userId,
                    WorkDescription = workDescription,
                    WorkDate = DateTime.Now,
                    DurationMinutes = durationMinutes,
                    ProgressPercentage = progressPercentage,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _context.TaskWorkLog_Tbl.Add(workLog);
                await _context.SaveChangesAsync();

                // ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskWorkLogAddedAsync(
             taskId,
             userId,
             workLog.Id,
             workDescription,
             durationMinutes
         );

                return (true, "گزارش کار با موفقیت ثبت شد", workLog.Id);
            }
            catch (Exception ex)
            {
                return (false, $"خطا در ثبت گزارش کار: {ex.Message}", null);
            }
        }

        /// <summary>
        /// دریافت لیست WorkLog های یک تسک
        /// </summary>
        public async Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId, int take = 0)
        {
            var query = _context.TaskWorkLog_Tbl
                .Include(w => w.User)
                .Where(w => w.TaskId == taskId && !w.IsDeleted)
                .OrderByDescending(w => w.WorkDate);

            if (take > 0)
            {
                query = (IOrderedQueryable<TaskWorkLog>)query.Take(take);
            }

            var workLogs = await query.ToListAsync();

            return workLogs.Select(w => new TaskWorkLogViewModel
            {
                Id = w.Id,
                TaskId = w.TaskId,
                WorkDescription = w.WorkDescription,
                WorkDate = w.WorkDate,
                WorkDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.WorkDate, "yyyy/MM/dd HH:mm"),
                DurationMinutes = w.DurationMinutes,
                ProgressPercentage = w.ProgressPercentage,
                UserId = w.UserId,
                UserName = w.User != null ? $"{w.User.FirstName} {w.User.LastName}" : "نامشخص",
                CreatedDate = w.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.CreatedDate, "yyyy/MM/dd HH:mm")
            }).ToList();
        }

        #endregion

        #region Task Focus Management

        /// <summary>
        /// تنظیم فوکوس کاربر روی یک تسک
        /// </summary>
        public async Task<(bool Success, string Message)> SetTaskFocusAsync(int taskId, string userId)
        {
            try
            {
                // بررسی عضویت کاربر در تسک
                var targetAssignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (targetAssignment == null)
                {
                    return (false, "شما عضو این تسک نیستید");
                }

                // حذف فوکوس از تسک‌های قبلی
                var previousFocused = await _context.TaskAssignment_Tbl
                    .Where(a => a.AssignedUserId == userId && a.IsFocused)
                    .ToListAsync();

                foreach (var assignment in previousFocused)
                {
                    assignment.IsFocused = false;
                    assignment.FocusedDate = null;
                }

                // تنظیم فوکوس جدید
                targetAssignment.IsFocused = true;
                targetAssignment.FocusedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "تسک با موفقیت به عنوان فوکوس انتخاب شد");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در تنظیم فوکوس: {ex.Message}");
            }
        }

        /// <summary>
        /// حذف فوکوس از یک تسک
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveTaskFocusAsync(int taskId, string userId)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (assignment == null)
                {
                    return (false, "شما عضو این تسک نیستید");
                }

                assignment.IsFocused = false;
                assignment.FocusedDate = null;

                await _context.SaveChangesAsync();

                return (true, "فوکوس از تسک حذف شد");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در حذف فوکوس: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت تسک فوکوس شده کاربر
        /// </summary>
        public async Task<int?> GetUserFocusedTaskIdAsync(string userId)
        {
            var focusedAssignment = await _context.TaskAssignment_Tbl
                .FirstOrDefaultAsync(a => a.AssignedUserId == userId && a.IsFocused);

            return focusedAssignment?.TaskId;
        }

        #endregion
        #region Task Work Log Management - بخش موجود را گسترش می‌دهیم

        /// <summary>
        /// آماده‌سازی مودال ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<TaskWorkLogViewModel?> PrepareLogTaskWorkModalAsync(int taskId, string userId)
        {
            try
            {
                // بررسی دسترسی کاربر به تسک
                var assignment = await _context.TaskAssignment_Tbl
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (assignment == null)
                {
                    return null; // کاربر عضو این تسک نیست
                }

                // ⭐ دریافت اطلاعات تسک
                var task = assignment.Task;

                // ⭐ ایجاد ViewModel
                var model = new TaskWorkLogViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task?.Title ?? "نامشخص",
                    TaskCode = task?.TaskCode ?? string.Empty
                };

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PrepareLogTaskWorkModalAsync: {ex.Message}");
                return null;
            }
        }

        #endregion
        /// <summary>
        /// دریافت آیکون برای نوع تغییر
        /// </summary>
        private string GetHistoryIcon(byte actionType)
        {
            return actionType switch
            {
                0 => "fa-plus",              // ایجاد
                1 => "fa-edit",              // ویرایش
                2 => "fa-sync",              // تغییر وضعیت
                3 => "fa-user-plus",         // اضافه کاربر
                4 => "fa-user-minus",        // حذف کاربر
                5 => "fa-plus-circle",       // افزودن عملیات
                7 => "fa-check-circle",      // تکمیل عملیات
                8 => "fa-trash",             // حذف عملیات
                9 => "fa-clipboard-check",   // ثبت گزارش کار
                10 => "fa-bell-plus",        // افزودن یادآوری
                15 => "fa-user-check",       // تایید سرپرست
                16 => "fa-award",            // تایید مدیر
                _ => "fa-circle"
            };
        }

        /// <summary>
        /// دریافت رنگ Badge برای نوع تغییر
        /// </summary>
        private string GetHistoryBadgeClass(byte actionType)
        {
            return actionType switch
            {
                0 => "bg-primary",           // ایجاد
                1 => "bg-warning",           // ویرایش
                2 => "bg-info",              // تغییر وضعیت
                5 => "bg-primary",           // افزودن عملیات
                7 => "bg-success",           // تکمیل عملیات
                8 => "bg-danger",            // حذف
                9 => "bg-info",              // گزارش کار
                10 => "bg-warning",          // یادآوری
                15 => "bg-info",             // تایید سرپرست
                16 => "bg-success",          // تایید مدیر
                _ => "bg-secondary"
            };
        }
        #endregion

        public async Task<List<ContactViewModel>> GetBranchContactsAsync(int branchId)
        {
            try
            {
                var contacts = await _context.BranchContact_Tbl
                    .Include(bc => bc.Contact)
                        .ThenInclude(c => c.Phones)
                    .Where(bc => bc.BranchId == branchId && bc.IsActive)
                    .Select(bc => new ContactViewModel
                    {
                        Id = bc.ContactId,
                        FirstName = bc.Contact.FirstName,
                        LastName = bc.Contact.LastName,
                        FullName = $"{bc.Contact.FirstName} {bc.Contact.LastName}", // ⭐ اکنون قابل نوشتن است
                        NationalCode = bc.Contact.NationalCode,
                        PrimaryPhone = bc.Contact.Phones
                    .Where(p => p.IsDefault && p.IsActive) 
                            .Select(p => p.PhoneNumber) 
                            .FirstOrDefault(),
                        RelationType = bc.RelationType,
                        IsActive = bc.Contact.IsActive
                    })
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();

                return contacts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchContactsAsync: {ex.Message}");
                return new List<ContactViewModel>();
            }
        }

        /// <summary>
        /// دریافت Organizationهای شعبه (سازمان‌های مرتبط با شعبه) - اصلاح شده
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetBranchOrganizationsAsync(int branchId)
        {
            try
            {
                // ⭐⭐⭐ اصلاح شده: استفاده از Name به جای DisplayName
                var organizations = await _context.BranchOrganization_Tbl
                    .Include(bo => bo.Organization)
                        .ThenInclude(o => o.Departments)
                    .Where(bo => bo.BranchId == branchId && bo.IsActive)
                    .Select(bo => new
                    {
                        bo.OrganizationId,
                        bo.Organization.Name,
                        bo.Organization.Brand,
                        bo.Organization.RegistrationNumber,
                        bo.Organization.EconomicCode,
                        bo.Organization.IsActive,
                        Departments = bo.Organization.Departments
                            .Where(d => d.IsActive)
                            .Select(d => new
                            {
                                d.Id,
                                Members = d.Members.Where(m => m.IsActive).Select(m => m.Id)
                            })
                    })
                    .OrderBy(o => o.Name) // ⭐ مرتب‌سازی بر اساس Name
                    .ToListAsync();

                // ⭐⭐⭐ محاسبه DisplayName و mapping در Client-Side
                return organizations.Select(o => new OrganizationViewModel
                {
                    Id = o.OrganizationId,
                    DisplayName = !string.IsNullOrEmpty(o.Brand) ? o.Brand : o.Name, // ⭐ محاسبه DisplayName
                    Name = o.Name,
                    Brand = o.Brand,
                    RegistrationNumber = o.RegistrationNumber,
                    EconomicCode = o.EconomicCode,
                    TotalDepartments = o.Departments.Count(),
                    TotalMembers = o.Departments.SelectMany(d => d.Members).Count(),
                    IsActive = o.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchOrganizationsAsync: {ex.Message}");
                return new List<OrganizationViewModel>();
            }
        }

        /// <summary>
        /// دریافت سازمان‌هایی که یک Contact در آن‌ها عضو است
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetContactOrganizationsAsync(int contactId)
        {
            try
            {
                // دریافت سازمان‌هایی که Contact از طریق OrganizationContact و DepartmentMember عضو آن‌هاست
                var organizations = await _context.OrganizationContact_Tbl
                    .Include(oc => oc.Organization)
                    .Where(oc => oc.ContactId == contactId && oc.IsActive)
                    .Select(oc => new OrganizationViewModel
                    {
                        Id = oc.OrganizationId,
                        DisplayName = oc.Organization.DisplayName,
                        RegistrationNumber = oc.Organization.RegistrationNumber,
                        IsActive = oc.Organization.IsActive
                    })
                    .Distinct()
                    .OrderBy(o => o.DisplayName)
                    .ToListAsync();

                return organizations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetContactOrganizationsAsync: {ex.Message}");
                return new List<OrganizationViewModel>();
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تیم‌های شعبه با اطلاعات کامل مدیر و اعضا
        /// </summary>
        public async Task<List<TeamViewModel>> GetBranchTeamsWithManagersAsync(int branchId)
        {
            try
            {
                Console.WriteLine($"🔍 Fetching teams for branch: {branchId}");

                var teams = await _context.Team_Tbl
                    .Where(t => t.BranchId == branchId && t.IsActive)
                    .Include(t => t.Manager)
                    .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        ManagerUserId = t.ManagerUserId,
                        ManagerName = t.Manager != null
                            ? $"{t.Manager.FirstName} {t.Manager.LastName}"
                            : "بدون مدیر",
                        MemberCount = t.TeamMembers.Count(tm => tm.IsActive)
                    })
                    .OrderBy(t => t.Title)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {teams.Count} teams");
                Console.WriteLine($"📊 Teams with managers: {teams.Count(t => t.ManagerName != "بدون مدیر")}");

                return teams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// دریافت افراد عضو یک سازمان
        /// </summary>
        public async Task<List<ContactViewModel>> GetOrganizationContactsAsync(int organizationId)
        {
            try
            {
                var contacts = await _context.OrganizationContact_Tbl
                    .Include(oc => oc.Contact)
                        .ThenInclude(c => c.Phones)
                    .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                    .Select(oc => new ContactViewModel
                    {
                        Id = oc.ContactId,
                        FirstName = oc.Contact.FirstName,
                        LastName = oc.Contact.LastName,
                        FullName = $"{oc.Contact.FirstName} {oc.Contact.LastName}",
                        NationalCode = oc.Contact.NationalCode,
                        PrimaryPhone = oc.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault(),
                        IsActive = oc.Contact.IsActive
                    })
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();

                return contacts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetOrganizationContactsAsync: {ex.Message}");
                return new List<ContactViewModel>();
            }
        }
        public async Task<List<TaskCommentViewModel>> GetTaskCommentsAsync(int taskId)
        {
            try
            {
                var comments = await _context.TaskComment_Tbl
                    .Where(c => c.TaskId == taskId && c.ParentCommentId == null) // فقط کامنت‌های اصلی
                    .Include(c => c.Creator)
                    .Include(c => c.Attachments)
                    .OrderBy(c => c.CreateDate)
                    .ToListAsync();

                return _mapper.Map<List<TaskCommentViewModel>>(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskCommentsAsync: {ex.Message}");
                return new List<TaskCommentViewModel>();
            }
        }
        /// <summary>
        /// دریافت اطلاعات فایل پیوست شده به کامنت تسک برای دانلود
        /// </summary>
        public async Task<TaskCommentAttachment?> GetCommentAttachmentByIdAsync(int attachmentId)
        {
            try
            {
                return await _context.TaskCommentAttachment_Tbl
                    .Include(a => a.Comment)
                        .ThenInclude(c => c.Task)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetCommentAttachmentByIdAsync: {ex.Message}");
                return null;
            }
        }


        public async Task<TaskListViewModel> GetTaskListAsync(
            string userId,
            TaskViewType viewType,
            TaskGroupingType grouping,
            TaskFilterViewModel filters = null)
        {
            try
            {
                Console.WriteLine($"🔍 GetTaskListAsync - User: {userId}, ViewType: {viewType}");

                var model = new TaskListViewModel
                {
                    UserLoginid = userId,
                    CurrentViewType = viewType,
                    CurrentGrouping = grouping,
                    Filters = filters ?? new TaskFilterViewModel()
                };

                // ⭐⭐⭐ استفاده از FilteringRepository
                List<Tasks> tasks = viewType switch
                {
                    TaskViewType.MyTasks => await _filteringRepository.GetMyTasksAsync(userId, filters),
                    TaskViewType.AssignedByMe => await _filteringRepository.GetAssignedByMeTasksAsync(userId, filters),
                    TaskViewType.Supervised => await _filteringRepository.GetSupervisedTasksAsync(userId, filters),
                    _ => new List<Tasks>()
                };

                // ⭐ حذف تکرار
                var uniqueTasks = tasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();

                // ⭐⭐⭐ استفاده از GroupingRepository
                model.TaskGroups = await _groupingRepository.GroupTasksAsync(uniqueTasks, grouping, userId);

                // ⭐⭐⭐ استفاده از FilteringRepository برای آمار
                model.Stats = _filteringRepository.CalculateStats(uniqueTasks, userId);

                // ⭐ پر کردن لیست‌های قدیمی (compatibility)
                model.Tasks = uniqueTasks.Select(t => MapToTaskViewModel(t)).ToList();
                model.PendingTasks = model.Tasks.Where(t => !IsTaskCompletedForUser(t.Id, userId)).ToList();
                model.CompletedTasks = model.Tasks.Where(t => IsTaskCompletedForUser(t.Id, userId)).ToList();

                await FillLegacyStatsAsync(model, userId);

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return new TaskListViewModel { UserLoginid = userId };
            }
        }
        #region Helper Methods for GetTaskListAsync

        /// <summary>
        /// بررسی تکمیل تسک توسط کاربر جاری
        /// </summary>
        private bool IsTaskCompletedForUser(int taskId, string userId)
        {
            try
            {
                return _context.TaskAssignment_Tbl
                    .Any(a => a.TaskId == taskId &&
                             a.AssignedUserId == userId &&
                             a.CompletionDate.HasValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in IsTaskCompletedForUser: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// پر کردن آمار قدیمی برای سازگاری با نسخه‌های قبلی
        /// </summary>
        private async Task FillLegacyStatsAsync(TaskListViewModel model, string userId)
        {
            try
            {
                // ⭐ محاسبه آمار برای Compatibility با کدهای قدیمی
                model.FilterCounts = new TaskFilterCountsViewModel
                {
                    AllVisibleCount = model.Tasks.Count,
                    MyAssignedCount = model.Tasks.Count(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)),
                    AssignedByMeCount = model.Tasks.Count(t => t.CreatorUserId == userId),
                    MyTeamsCount = 0, // محاسبه در صورت نیاز
                    SupervisedCount = model.Tasks.Count(t => t.CreatorUserId != userId)
                };

                // ⭐ پر کردن GroupedTasks برای نمایش سلسله مراتبی (در صورت نیاز)
                // این بخش فقط در صورتی که از نمایش قدیمی استفاده می‌کنید لازم است
                // در غیر این صورت می‌توانید خالی بگذارید
                model.GroupedTasks = new TaskGroupedViewModel
                {
                    MyTasks = model.Tasks.Where(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)).ToList(),

                    AssignedToMe = model.Tasks.Where(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId) &&
                        t.CreatorUserId != userId).ToList(),

                    TeamMemberTasks = new Dictionary<string, List<TaskViewModel>>(),
                    SubTeamTasks = new Dictionary<string, List<TaskViewModel>>(),
                    MyTasksGrouped = new MyTasksGroupedViewModel(),
                    TeamTasksGrouped = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in FillLegacyStatsAsync: {ex.Message}");
            }
        }

        #endregion
        public async Task<TaskCardViewModel> GetTaskCardViewModelAsync(int taskId, string userId)
        {
            var task = await _context.Tasks_Tbl
                .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser)
                .Include(t => t.TaskOperations)
                .Include(t => t.TaskCategory)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return null;

            // محاسبه پیشرفت
            var totalOps = task.TaskOperations.Count;
            var completedOps = task.TaskOperations.Count(o => o.IsCompleted);
            var progressPercentage = totalOps > 0 ? (completedOps * 100 / totalOps) : 0;

            // بررسی تکمیل شدن برای کاربر جاری
            var userAssignment = task.TaskAssignments.FirstOrDefault(a => a.AssignedUserId == userId);
            var isCompleted = userAssignment?.CompletionDate.HasValue ?? false;

            // تعیین نام Stakeholder (Contact یا Organization)
            string stakeholderName = "ندارد";
            if (task.Contact != null)
            {
                stakeholderName = $"{task.Contact.FirstName} {task.Contact.LastName}";
            }
            else if (task.Organization != null)
            {
                stakeholderName = task.Organization.DisplayName;
            }

            // محاسبه DaysRemaining
            int? daysRemaining = null;
            if (task.DueDate.HasValue)
            {
                daysRemaining = (task.DueDate.Value.Date - DateTime.Now.Date).Days;
            }

            // تبدیل به ViewModel
            return new TaskCardViewModel
            {
                Id = task.Id,
                CardNumber = 0, // باید از بیرون set شود
                Title = task.Title,
                ShortDescription = task.Description?.Length > 100
                    ? task.Description.Substring(0, 100) + "..."
                    : task.Description,
                TaskCode = task.TaskCode,
                Priority = task.Priority,

                // وضعیت‌ها
                IsCompleted = isCompleted,
                IsOverdue = task.DueDate.HasValue &&
                           task.DueDate.Value < DateTime.Now &&
                           !isCompleted,

                // تاریخ‌ها
                DueDate = task.DueDate,
                DueDatePersian = task.DueDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd")
                    : null,
                CreateDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.CreateDate, "yyyy/MM/dd"),

                // افراد
                CreatorName = task.Creator != null
                    ? $"{task.Creator.FirstName} {task.Creator.LastName}"
                    : "نامشخص",
                StakeholderName = stakeholderName,

                // دسته‌بندی
                CategoryTitle = task.TaskCategory?.Title ?? "بدون دسته",
                CategoryBadgeClass = GetCategoryBadgeClass(task.TaskCategoryId),

                // اولویت
                PriorityText = GetPriorityText(task.Priority),
                PriorityBadgeClass = GetPriorityBadgeClass(task.Priority),

                // وضعیت
                StatusText = GetTaskStatusText(task.Status),
                StatusBadgeClass = GetTaskStatusBadgeClass(task.Status),

                // پیشرفت
                TotalOperations = totalOps,
                CompletedOperations = completedOps,
                ProgressPercentage = progressPercentage,

                // زمان باقیمانده
                DaysRemaining = daysRemaining,

                // دسترسی‌ها
                CanEdit = task.CreatorUserId == userId,
                CanDelete = task.CreatorUserId == userId,
                CanComplete = userAssignment != null && !isCompleted
            };
        }

        private string GetCategoryBadgeClass(int? categoryId)
        {
            if (!categoryId.HasValue) return "bg-secondary";
            return "bg-info";
        }

        private string GetPriorityText(byte priority)
        {
            return priority switch
            {
                2 => "فوری",
                1 => "مهم",
                _ => "عادی"
            };
        }

        private string GetPriorityBadgeClass(byte priority)
        {
            return priority switch
            {
                2 => "bg-danger",
                1 => "bg-warning",
                _ => "bg-primary"
            };
        }
        /// <summary>
        /// دریافت لیست UserId های اختصاص داده شده به تسک
        /// </summary>
        public async Task<List<string>> GetTaskAssignedUserIdsAsync(int taskId)
        {
            return await _context.TaskAssignment_Tbl
                .Where(a => a.TaskId == taskId)
                .Select(a => a.AssignedUserId)
                .Distinct()
                .ToListAsync();
        }
        /// <summary>
        /// دریافت همه کاربران مرتبط با تسک (سازنده + اعضا + ناظران)
        /// </summary>
        public async Task<List<string>> GetTaskRelatedUserIdsAsync(int taskId)
        {
            var userIds = new HashSet<string>(); // استفاده از HashSet برای جلوگیری از تکرار

            // ⭐ 1. دریافت اطلاعات تسک
            var task = await _context.Tasks_Tbl
                .Where(t => t.Id == taskId)
                .Select(t => new { t.CreatorUserId, t.BranchId })
                .FirstOrDefaultAsync();

            if (task == null) return new List<string>();

            // ⭐ 2. سازنده تسک
            if (!string.IsNullOrEmpty(task.CreatorUserId))
            {
                userIds.Add(task.CreatorUserId);
            }

            // ⭐ 3. اعضای تسک (TaskAssignments)
            var assignedUserIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.TaskId == taskId)
                .Select(ta => ta.AssignedUserId)
                .ToListAsync();

            foreach (var userId in assignedUserIds)
            {
                userIds.Add(userId);
            }

            // ⭐⭐⭐ 4. ناظران تسک - استفاده از TaskVisibilityRepository
            var supervisors = await _taskVisibilityRepository.GetTaskSupervisorsAsync(taskId, includeCreator: false);
            foreach (var supervisorId in supervisors)
            {
                userIds.Add(supervisorId);
            }

            return userIds.ToList();
        }
    }
}