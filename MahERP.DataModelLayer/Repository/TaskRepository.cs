using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MahERP.DataModelLayer.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        private readonly IBranchRepository _BranchRipository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStakeholderRepository _StakeholderRepo;
        private readonly IUserManagerRepository _userManagerRepository;

        public TaskRepository(AppDbContext context, IBranchRepository branchRipository, IUnitOfWork unitOfWork, IUserManagerRepository userManagerRepository, IStakeholderRepository stakeholderRepo)
        {
            _context = context;
            _BranchRipository = branchRipository;
            _unitOfWork = unitOfWork;
            _userManagerRepository = userManagerRepository;
            _StakeholderRepo = stakeholderRepo;
        }

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

            List< TaskViewModel> tasks = (from ts in _context.TaskAssignment_Tbl 
                                                  join t in _context.Tasks_Tbl on ts.TaskId equals t.Id
                                                  join cate in _context.TaskCategory_Tbl on t.TaskCategoryId equals cate .Id
                                               where ts.AssignedUserId == userId && !t.IsDeleted
                                                    select new TaskViewModel
                                                    {
                                                        Id = t.Id,
                                                        Title = t.Title,
                                                        Description = t.Description,
                                                        TaskCode = t.TaskCode,
                                                        CreateDate = t.CreateDate,
                                                        DueDate = t.DueDate,
                                                        CompletionDate = t.CompletionDate,
                                                        ManagerApprovedDate = t.ManagerApprovedDate,
                                                        SupervisorApprovedDate = t.SupervisorApprovedDate,
                                                        IsActive = t.IsActive,
                                                        IsDeleted = t.IsDeleted,
                                                        BranchId = t.BranchId,
                                                        CategoryId = cate.Id,
                                                        CategoryTitle = cate.Title,
                                                        CreatorUserId = t.CreatorUserId,
                                                        StakeholderId = t.StakeholderId

                                                    }


                                                ).ToList();

            taskForIndexViewModel.Tasks = tasks;

            return taskForIndexViewModel;
        }
        public bool IsTaskCodeUnique(string taskCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return true;

            var query = _context.Tasks_Tbl.Where(t => t.TaskCode == taskCode);

            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);

            return !query.Any();
        }
        public TaskViewModel CreateTaskAndCollectData(string UserId)
        {
            ///پیدا کردن شماره تسک بعدی
            var NewTaskId = _unitOfWork.TaskUW.GetNextPrimaryKey();
            var Tasks = new TaskViewModel();
            Tasks.branchListInitial = _BranchRipository.GetBrnachListByUserId(UserId);
            Tasks.TaskCategoryInitial = GetAllCategories();

            //int BranchFirst = Tasks.Branchs.FirstOrDefault().Id;
            //Tasks.StakeholderId = 0;
            //int CustomersFirst = Tasks.Customers.First().Id;
            Tasks.UsersInitial = _userManagerRepository.GetUserListBybranchId(0);
            //Tasks.DutyList = _DutyCustorepo.GetListDutyByCustomer_ByBranch(CustomersFirst, BranchFirst);
            //Tasks.ContractList = _Cusrepo.GetContractList(CustomersFirst);
            //Tasks.NextTaskId = NewTaskId;

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

        public List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.StakeholderId == stakeholderId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
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

        // متدهای جدید مورد نیاز
        public List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.BranchId == branchId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
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
        /// <summary>
        /// دریافت تسک‌های شعبه برای نمایش در تقویم بر اساس فیلترهای مختلف
        /// </summary>
        /// <param name="userId">شناسه کاربر جهت فیلتر تسک‌های مرتبط</param>
        /// <param name="branchId">شناسه شعبه جهت فیلتر تسک‌های شعبه (اختیاری)</param>
        /// <param name="startDate">تاریخ شروع محدوده نمایش (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان محدوده نمایش (اختیاری)</param>
        /// <param name="assignedUserIds">لیست شناسه کاربران منتصب (اختیاری)</param>
        /// <param name="stakeholderId">شناسه طرف حساب (اختیاری)</param>
        /// <returns>لیست تسک‌های دارای تاریخ مهلت برای نمایش در تقویم</returns>
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
                // کوئری پایه تسک‌ها
                var query = _context.Tasks_Tbl
                    .Where(t => !t.IsDeleted &&
                               t.IsActive &&
                               t.DueDate.HasValue) // فقط تسک‌های دارای تاریخ مهلت
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Stakeholder)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
                    .AsQueryable();

                // فیلتر بر اساس محدوده زمانی
                if (startDate.HasValue)
                {
                    query = query.Where(t => t.DueDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.DueDate <= endDate.Value);
                }

                // فیلتر بر اساس شعبه انتخاب شده
                if (branchId.HasValue)
                {
                    // تسک‌های مرتبط با شعبه مشخص
                    query = query.Where(t =>
                        // تسک‌هایی که کاربران آن شعبه در آن‌ها اختصاص دارند
                        t.TaskAssignments.Any(ta =>
                            _context.BranchUser_Tbl.Any(bu =>
                                bu.UserId == ta.AssignedUserId &&
                                bu.BranchId == branchId.Value &&
                                bu.IsActive)) ||
                        // یا تسک‌هایی که طرف حساب آن‌ها متعلق به همان شعبه است
                        (t.StakeholderId.HasValue &&
                         _context.StakeholderBranch_Tbl.Any(sb =>
                            sb.StakeholderId == t.StakeholderId &&
                            sb.BranchId == branchId.Value))
                    );
                }
                else
                {
                    // اگر شعبه انتخاب نشده، فقط تسک‌هایی که کاربر جاری در آن‌ها دخیل است
                    query = query.Where(t =>
                        t.CreatorUserId == userId ||
                        t.TaskAssignments.Any(ta => ta.AssignedUserId == userId));
                }

                // فیلتر بر اساس کاربران منتصب
                if (assignedUserIds != null && assignedUserIds.Any())
                {
                    query = query.Where(t =>
                        t.TaskAssignments.Any(ta => assignedUserIds.Contains(ta.AssignedUserId)));
                }

                // فیلتر بر اساس طرف حساب
                if (stakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == stakeholderId.Value);
                }

                // اجرای کوئری و تبدیل به ViewModel
                var tasks = query.Select(t => new TaskCalendarViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TaskCode = t.TaskCode,
                    DueDate = t.DueDate,
                    IsCompleted = t.CompletionDate.HasValue,
                    IsOverdue = !t.CompletionDate.HasValue && t.DueDate < DateTime.Now,

                    // اطلاعات طرف حساب
                    StakeholderId = t.StakeholderId,
                    StakeholderName = t.Stakeholder != null ?
                        $"{t.Stakeholder.FirstName} {t.Stakeholder.LastName}" : "ندارد",

                    // اطلاعات دسته‌بندی
                    CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : "ندارد",

                    // اطلاعات شعبه (از طریق StakeholderBranch یا BranchUser)
                    BranchName = _context.StakeholderBranch_Tbl
                        .Where(sb => sb.StakeholderId == t.StakeholderId)
                        .Join(_context.Branch_Tbl, sb => sb.BranchId, b => b.Id, (sb, b) => b.Name)
                        .FirstOrDefault() ??
                        _context.BranchUser_Tbl
                            .Where(bu => t.TaskAssignments.Any(ta => ta.AssignedUserId == bu.UserId) && bu.IsActive)
                            .Join(_context.Branch_Tbl, bu => bu.BranchId, b => b.Id, (bu, b) => b.Name)
                            .FirstOrDefault() ?? "ندارد",

                    // تعیین رنگ بر اساس وضعیت
                    CalendarColor = t.CompletionDate.HasValue ? "#28a745" : // سبز - تکمیل شده
                                   (!t.CompletionDate.HasValue && t.DueDate < DateTime.Now) ? "#dc3545" : // قرمز - عقب افتاده
                                   "#007bff", // آبی - در حال انجام

                    // متن وضعیت
                    StatusText = t.CompletionDate.HasValue ? "تکمیل شده" :
                                (!t.CompletionDate.HasValue && t.DueDate < DateTime.Now) ? "عقب افتاده" :
                                "در حال انجام",

                    // تاریخ ایجاد
                    CreateDate = t.CreateDate,

                    // شناسه سازنده
                    CreatorUserId = t.CreatorUserId
                })
                .OrderBy(t => t.DueDate)
                .ToList();

                return tasks;
            }
            catch (Exception ex)
            {
                // در صورت بروز خطا، لیست خالی برگردانده می‌شود
                // لاگ خطا باید در اینجا ثبت شود
                return new List<TaskCalendarViewModel>();
            }
        }
    }
}