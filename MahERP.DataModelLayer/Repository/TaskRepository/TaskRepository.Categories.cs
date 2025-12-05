using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت دسته‌بندی‌های تسک (Task Categories)
    /// </summary>
    public partial class TaskRepository 
    {
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

        /// <summary>
        /// دریافت دسته‌بندی بر اساس شناسه (Async)
        /// </summary>
        public async Task<TaskCategory> GetCategoryByIdAsync(int id)
        {
            return await _context.TaskCategory_Tbl.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// بروزرسانی لیست دسته‌بندی‌ها بر اساس تغییر طرف حساب
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
        #endregion
    }
}
