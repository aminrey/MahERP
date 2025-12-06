using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels
{
    /// <summary>
    /// ViewModel برای تنظیمات نمایش تسک مدیران در شعبه
    /// </summary>
    public class BranchTaskVisibilitySettingsViewModel
    {
        /// <summary>
        /// شناسه تنظیمات (برای ویرایش)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// نام شعبه
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// شناسه مدیر (null = تنظیمات پیش‌فرض برای همه)
        /// </summary>
        public string? ManagerUserId { get; set; }

        /// <summary>
        /// نام مدیر
        /// </summary>
        public string? ManagerFullName { get; set; }

        /// <summary>
        /// آیا نمایش همه زیرتیم‌ها فعال است؟
        /// </summary>
        public bool ShowAllSubTeamsByDefault { get; set; }

        /// <summary>
        /// لیست تیم‌های انتخابی برای نمایش
        /// </summary>
        public List<int>? SelectedTeamIds { get; set; } = new List<int>();

        /// <summary>
        /// حداکثر تعداد تسک قابل نمایش (0 = نامحدود)
        /// </summary>
        public int MaxTasksToShow { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تیم‌های موجود در شعبه
        /// </summary>
        public List<TeamItemViewModel> AvailableTeams { get; set; } = new List<TeamItemViewModel>();

        /// <summary>
        /// مدیران شعبه (برای تنظیمات شخصی)
        /// </summary>
        public List<ManagerItemViewModel> AvailableManagers { get; set; } = new List<ManagerItemViewModel>();

        /// <summary>
        /// آیا این تنظیمات شخصی است یا پیش‌فرض؟
        /// </summary>
        public bool IsPersonalSettings => !string.IsNullOrEmpty(ManagerUserId);
    }

    /// <summary>
    /// تیم برای انتخاب
    /// </summary>
    public class TeamItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ManagerName { get; set; }
        public int Level { get; set; }
        public int? ParentTeamId { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// مدیر برای انتخاب
    /// </summary>
    public class ManagerItemViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> ManagedTeams { get; set; } = new List<string>();
    }
}
