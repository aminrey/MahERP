using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Repository
{
    public interface ITaskRepository
    {
        List<Tasks> GetTasks(bool includeDeleted = false, int? categoryId = null, string assignedUserId = null);
        Tasks GetTaskById(int id, bool includeOperations = false, bool includeAssignments = false, bool includeAttachments = false, bool includeComments = false);
        List<Tasks> GetTasksByStakeholder(int stakeholderId, bool includeDeleted = false);
        List<Tasks> GetTasksByUser(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false);
        List<Tasks> GetTasksByBranch(int branchId, bool includeDeleted = false); // جدید
        bool IsUserRelatedToTask(string userId, int taskId); // جدید
        bool IsTaskInBranch(int taskId, int branchId); // جدید
        List<TaskOperation> GetTaskOperations(int taskId, bool includeCompleted = true);
        TaskOperation GetTaskOperationById(int id);
        List<TaskCategory> GetAllCategories(bool activeOnly = true);
        TaskCategory GetCategoryById(int id);
        List<Tasks> SearchTasks(string searchTerm, int? categoryId = null, string assignedUserId = null, bool? isCompleted = null);
        bool IsTaskCodeUnique(string taskCode, int? excludeId = null);
        List<TaskAssignment> GetTaskAssignments(int taskId);
        TaskAssignment GetTaskAssignmentById(int id);
        TaskAssignment GetTaskAssignmentByUserAndTask(string userId, int taskId);
    }
}