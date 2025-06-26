using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
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
                    t.Description.Contains(searchTerm) ||
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
                query = query.Where(t => t.CompletionDate != null == isCompleted.Value);
            }

            return query.OrderByDescending(t => t.CreateDate).ToList();
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