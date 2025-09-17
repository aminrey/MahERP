using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// رابط repository برای مدیریت انتصاب دسته‌بندی‌های تسک به شعبه
    /// </summary>
    public interface IBranchTaskCategoryRepository
    {
        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه مشخص
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="activeOnly">فقط موارد فعال</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه</returns>
        List<BranchTaskCategory> GetTaskCategoriesByBranchId(int branchId, bool activeOnly = true);

        /// <summary>
        /// دریافت اطلاعات کامل انتصاب دسته‌بندی به شعبه
        /// </summary>
        /// <param name="id">شناسه انتصاب</param>
        /// <returns>اطلاعات انتصاب</returns>
        BranchTaskCategory GetBranchTaskCategoryById(int id);

        /// <summary>
        /// دریافت ViewModel برای افزودن دسته‌بندی به شعبه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>ViewModel شامل اطلاعات لازم</returns>
        BranchTaskCategoryViewModel GetAddTaskCategoryToBranchViewModel(int branchId);

        /// <summary>
        /// بررسی اینکه آیا دسته‌بندی قبلاً به شعبه اضافه شده یا نه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="taskCategoryId">شناسه دسته‌بندی</param>
        /// <returns>true اگر قبلاً اضافه شده باشد</returns>
        bool IsTaskCategoryAssignedToBranch(int branchId, int taskCategoryId);

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های در دسترس برای شعبه (که قبلاً اضافه نشده‌اند)
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>لیست دسته‌بندی‌های در دسترس</returns>
        List<TaskCategory> GetAvailableTaskCategoriesForBranch(int branchId);

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه مشخص برای استفاده در فرم‌های دیگر
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه</returns>
        List<TaskCategory> GetTaskCategoriesForBranch(int branchId);
    }
}