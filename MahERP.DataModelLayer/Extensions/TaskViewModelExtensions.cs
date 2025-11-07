using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Extensions
{
    /// <summary>
    /// ⭐⭐⭐ Extension Methods برای محاسبه Context کاربر در TaskViewModel
    /// </summary>
    public static class TaskViewModelExtensions
    {
        /// <summary>
        /// محاسبه و تنظیم Context کاربر فعلی برای TaskViewModel
        /// </summary>
        public static TaskViewModel SetUserContext(
            this TaskViewModel viewModel, 
            string currentUserId, 
            bool isAdmin = false, 
            bool isManager = false, 
            bool isSupervisor = false)
        {
            if (viewModel == null || string.IsNullOrEmpty(currentUserId))
                return viewModel;

            viewModel.CurrentUserId = currentUserId;
            
            // ⭐ آیا کاربر به این تسک اختصاص داده شده؟
            viewModel.IsAssignedToCurrentUser = viewModel.AssignmentsTaskUser?
                .Any(a => a.AssignedUserId == currentUserId) ?? false;

            // ⭐ آیا کاربر سازنده است؟
            viewModel.IsCreator = viewModel.CreatorUserId == currentUserId;

            // ⭐ آیا کاربر تسک را تکمیل کرده؟
            viewModel.IsCompletedByMe = viewModel.AssignmentsTaskUser?
                .Any(a => a.CompletionDate.HasValue && a.AssignedUserId == currentUserId) ?? false;

            // ⭐ آیا این تسک فوکوس کاربر است؟
            var isFocused = viewModel.AssignmentsTaskUser?
                .Any(a => a.IsFocused && a.AssignedUserId == currentUserId) ?? false;
            
            // فقط اگر قبلاً مقدار نداشته، تنظیم کن
            if (!viewModel.IsFocused.HasValue)
                viewModel.IsFocused = isFocused;

            // ⭐ نقش‌های کاربر
            viewModel.IsManager = isManager;
            viewModel.IsSupervisor = isSupervisor;

            return viewModel;
        }

        /// <summary>
        /// ⭐ محاسبه Context برای لیست TaskViewModel ها
        /// </summary>
        public static IEnumerable<TaskViewModel> SetUserContext(
            this IEnumerable<TaskViewModel> viewModels, 
            string currentUserId, 
            bool isAdmin = false, 
            bool isManager = false, 
            bool isSupervisor = false)
        {
            foreach (var vm in viewModels)
            {
                vm.SetUserContext(currentUserId, isAdmin, isManager, isSupervisor);
            }
            return viewModels;
        }
    }
}