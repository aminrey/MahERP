using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت عملیات‌های تسک (Task Operations)
    /// شامل: Create, Update, Complete Operations
    /// </summary>
    public partial class TaskRepository 
    {
        #region Task Operations CRUD

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

        #endregion

        #region Operation Completion

        /// <summary>
        /// تکمیل خودکار عملیات باقیمانده هنگام تکمیل تسک
        /// </summary>
        private async Task CompleteRemainingOperationsAsync(Tasks task, string userId, DateTime completionDate)
        {
            if (task.TaskOperations == null || !task.TaskOperations.Any())
                return;

            var remainingOperations = task.TaskOperations
                .Where(o => !o.IsCompleted && !o.IsDeleted)
                .ToList();

            if (!remainingOperations.Any())
                return;

            foreach (var operation in remainingOperations)
            {
                operation.IsCompleted = true;
                operation.CompletionDate = completionDate;
                operation.CompletedByUserId = userId;
                operation.CompletionNote = "تکمیل خودکار هنگام اتمام تسک";
                _context.TaskOperation_Tbl.Update(operation);
            }

            Console.WriteLine($"✅ {remainingOperations.Count} عملیات باقیمانده تکمیل شد");
        }

        #endregion
    }
}
