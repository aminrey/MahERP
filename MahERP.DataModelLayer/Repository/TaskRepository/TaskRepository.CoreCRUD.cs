using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// عملیات CRUD اصلی تسک‌ها
    /// شامل: Create, Read, Update, Delete, Get, Search
    /// </summary>
    public partial class TaskRepository 
    {
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

            // مقداردهی اولیه لیست‌های Contact/Organization
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

        public Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, 
            bool includeAttachments = false, bool includeComments = false, bool includeStakeHolders = false, 
            bool includeTaskWorkLog = false)
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
            {
                query = query.Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser)
                    .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignerUser);
            }

            if (includeAttachments)
                query = query.Include(t => t.TaskAttachments);

            if (includeComments)
            {
                query = query.Include(t => t.TaskComments)
                    .ThenInclude(c => c.Creator)
                    .Include(t => t.TaskComments)
                    .ThenInclude(c => c.Attachments)
                    .ThenInclude(a => a.Uploader);
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

        public async Task<Tasks> GetTaskByIdAsync(int taskId)
        {
            try
            {
                return await _context.Tasks_Tbl
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Contact)
                    .Include(t => t.Organization)
                    .Include(t => t.Creator)
                    .Include(t => t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskByIdAsync: {ex.Message}");
                return null;
            }
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

        public List<Tasks> GetTasksByBranches(List<int> branchIds, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl
                .Where(t => branchIds.Contains(t.BranchId ?? 0))
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false)
        {
            return GetTasksByBranches(new List<int> { branchId }, includeDeleted);
        }

        public List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false)
        {
            var query = _context.Tasks_Tbl.Where(t => t.StakeholderId == stakeholderId);

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        public List<Tasks> SearchTasks(string searchTerm, int? categoryId = null, string assignedUserId = null, bool? isCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) && !categoryId.HasValue && string.IsNullOrEmpty(assignedUserId) && !isCompleted.HasValue)
                return GetTasks();

            var query = _context.Tasks_Tbl.Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)) ||
                    t.TaskCode.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.TaskCategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(assignedUserId))
            {
                query = query.Where(t => _context.TaskAssignment_Tbl
                    .Any(a => a.TaskId == t.Id && a.AssignedUserId == assignedUserId));
            }

            return query.OrderByDescending(t => t.CreateDate).ToList();
        }

        #endregion

        #region Task Validation

        public bool IsTaskCodeUnique(string taskCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return true;

            var query = _context.Tasks_Tbl.Where(t => t.TaskCode == taskCode);

            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);

            return !query.Any();
        }

        public async Task<bool> IsTaskCodeUniqueAsync(string taskCode, int? excludeTaskId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return true;

            var query = _context.Tasks_Tbl.Where(t => t.TaskCode == taskCode && !t.IsDeleted);

            if (excludeTaskId.HasValue)
                query = query.Where(t => t.Id != excludeTaskId.Value);

            return !await query.AnyAsync();
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
    }
}
