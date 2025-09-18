

using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای فیلترهای تقویم تسک‌ها
    /// مورد استفاده در صفحه TaskCalendar برای فیلتر کردن تسک‌ها
    /// </summary>
    public class TaskCalendarFilterViewModel
    {
        /// <summary>
        /// شناسه شعبه انتخاب شده برای فیلتر
        /// </summary>
        public int? BranchIdSelected { get; set; }

        /// <summary>
        /// لیست شناسه کاربران انتخاب شده برای فیلتر
        /// </summary>
        public List<string>? AssignedUserIds { get; set; }

        /// <summary>
        /// شناسه طرف حساب انتخاب شده برای فیلتر
        /// </summary>
        public int? StakeholderId { get; set; }

        /// <summary>
        /// لیست شعبه‌های قابل انتخاب برای کاربر جاری
        /// هر جا initial هست یک لیست جهت انتخاب هستد
        /// initial یعنی همه لیست و لود میکنه و برای نمایش تمام کاربرها هست
        /// </summary>
        public List<BranchViewModel>? BranchListInitial { get; set; }

        /// <summary>
        /// لیست کاربران شعبه انتخاب شده (پویا - بر اساس cascade شعبه)
        /// </summary>
        public List<BranchUserViewModel>? UsersInitial { get; set; }

        /// <summary>
        /// لیست طرف حساب‌های شعبه انتخاب شده (پویا - بر اساس cascade شعبه)
        /// </summary>
        public List<StakeholderViewModel>? StakeholdersInitial { get; set; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public TaskCalendarFilterViewModel()
        {
            AssignedUserIds = new List<string>();
            BranchListInitial = new List<BranchViewModel>();
            UsersInitial = new List<BranchUserViewModel>();
            StakeholdersInitial = new List<StakeholderViewModel>();
        }
    }
}