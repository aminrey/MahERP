

using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای فیلترهای تقویم تسک‌ها
    /// </summary>
    public class TaskCalendarFilterViewModel
    {
        /// <summary>
        /// شناسه شعبه انتخاب شده برای فیلتر
        /// </summary>
        public int? BranchIdSelected { get; set; }

        /// <summary>
        /// لیست کاربران انتخاب شده برای فیلتر
        /// </summary>
        public List<string>? AssignedUserIds { get; set; }

        /// <summary>
        /// شناسه طرف حساب انتخاب شده برای فیلتر
        /// </summary>
        public int? StakeholderId { get; set; }

        /// <summary>
        /// لیست شعبه‌های قابل انتخاب برای کاربر جاری
        /// </summary>
        public List<BranchViewModel>? BranchListInitial { get; set; }

        /// <summary>
        /// لیست کاربران شعبه انتخاب شده (پویا)
        /// </summary>
        public List<BranchUserViewModel>? UsersInitial { get; set; }

        /// <summary>
        /// لیست طرف حساب‌های شعبه انتخاب شده (پویا)
        /// </summary>
        public List<StakeholderViewModel>? StakeholdersInitial { get; set; }
    }
}