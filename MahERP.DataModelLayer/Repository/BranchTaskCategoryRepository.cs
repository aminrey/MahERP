using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// پیاده‌سازی repository برای مدیریت انتصاب دسته‌بندی‌های تسک به شعبه
    /// </summary>
    public class BranchTaskCategoryRepository : IBranchTaskCategoryRepository
    {
        private readonly AppDbContext _context;

        public BranchTaskCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه مشخص
        /// </summary>
        public List<BranchTaskCategory> GetTaskCategoriesByBranchId(int branchId, bool activeOnly = true)
        {
            var query = _context.BranchTaskCategory_Tbl
                .Include(btc => btc.TaskCategory)
                .Include(btc => btc.Branch)
                .Include(btc => btc.AssignedByUser)
                .Where(btc => btc.BranchId == branchId);

            if (activeOnly)
                query = query.Where(btc => btc.IsActive);

            return query.OrderBy(btc => btc.TaskCategory.Title).ToList();
        }

        /// <summary>
        /// دریافت اطلاعات کامل انتصاب دسته‌بندی به شعبه
        /// </summary>
        public BranchTaskCategory GetBranchTaskCategoryById(int id)
        {
            return _context.BranchTaskCategory_Tbl
                .Include(btc => btc.TaskCategory)
                .Include(btc => btc.Branch)
                .Include(btc => btc.AssignedByUser)
                .FirstOrDefault(btc => btc.Id == id);
        }

        /// <summary>
        /// دریافت ViewModel برای افزودن دسته‌بندی به شعبه
        /// </summary>
        public BranchTaskCategoryViewModel GetAddTaskCategoryToBranchViewModel(int branchId)
        {
            var branch = _context.Branch_Tbl.FirstOrDefault(b => b.Id == branchId);
            if (branch == null)
                return null;

            // دریافت دسته‌بندی‌های در دسترس (که قبلاً به این شعبه اضافه نشده‌اند)
            var availableCategories = GetAvailableTaskCategoriesForBranch(branchId);

            var viewModel = new BranchTaskCategoryViewModel
            {
                BranchId = branchId,
                BranchName = branch.Name,
                TaskCategoriesInitial = availableCategories.Select(tc => new TaskCategoryViewModel
                {
                    Id = tc.Id,
                    Title = tc.Title,
                    Description = tc.Description,
                    IsActive = tc.IsActive
                }).ToList(),
                IsActive = true
            };

            return viewModel;
        }

        /// <summary>
        /// بررسی اینکه آیا دسته‌بندی قبلاً به شعبه اضافه شده یا نه
        /// </summary>
        public bool IsTaskCategoryAssignedToBranch(int branchId, int taskCategoryId)
        {
            return _context.BranchTaskCategory_Tbl
                .Any(btc => btc.BranchId == branchId && btc.TaskCategoryId == taskCategoryId);
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های در دسترس برای شعبه (که قبلاً اضافه نشده‌اند)
        /// </summary>
        public List<TaskCategory> GetAvailableTaskCategoriesForBranch(int branchId)
        {
            // دریافت شناسه‌های دسته‌بندی‌هایی که قبلاً به این شعبه اضافه شده‌اند
            var assignedCategoryIds = _context.BranchTaskCategory_Tbl
                .Where(btc => btc.BranchId == branchId)
                .Select(btc => btc.TaskCategoryId)
                .ToList();

            // دریافت دسته‌بندی‌های فعال که هنوز اضافه نشده‌اند
            return _context.TaskCategory_Tbl
                .Where(tc => tc.IsActive && !assignedCategoryIds.Contains(tc.Id))
                .OrderBy(tc => tc.Title)
                .ToList();
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه مشخص برای استفاده در فرم‌های دیگر
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه</returns>
        public List<TaskCategory> GetTaskCategoriesForBranch(int branchId)
        {
            try
            {
                // دریافت دسته‌بندی‌های اختصاص یافته به شعبه
                var assignedCategoryIds = _context.BranchTaskCategory_Tbl
                    .Where(btc => btc.BranchId == branchId && btc.IsActive)
                    .Select(btc => btc.TaskCategoryId)
                    .ToList();

                // دریافت دسته‌بندی‌های مربوطه
                return _context.TaskCategory_Tbl
                    .Where(tc => assignedCategoryIds.Contains(tc.Id) && tc.IsActive)
                    .OrderBy(tc => tc.Title)
                    .ToList();
            }
            catch (Exception ex)
            {
                // لاگ خطا
                throw new Exception($"خطا در دریافت دسته‌بندی‌های شعبه {branchId}: {ex.Message}", ex);
            }
        }
    }
}