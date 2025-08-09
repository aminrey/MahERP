using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public TaskListForIndexViewModel GetTaskForIndexByUser(TaskListForIndexViewModel FilterModel )
        {
            string UserIdLogin = FilterModel.UserLoginid;

            var taskForIndexViewModel = new TaskListForIndexViewModel
            {
                branchListInitial = _BranchRipository.GetBrnachListByUserId(UserIdLogin),
                TaskCategoryInitial = GetAllCategories(),
                UsersInitial = _userManagerRepository.GetUserListBybranchId(0),
                StakeholdersInitial = _StakeholderRepo.GetStakeholdersByBranchId(0)
            };

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
    }
}