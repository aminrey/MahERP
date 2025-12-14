using MahERP.DataModelLayer.Entities.TaskManagement;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.TaskViewModels
{
    /// <summary>
    /// ⭐⭐⭐ ViewModel اصلی تنظیمات تسک
    /// </summary>
    public class TaskSettingsViewModel
    {
        /// <summary>
        /// شناسه تسک
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// عنوان تسک
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// کد تسک
        /// </summary>
        public string TaskCode { get; set; }

        #region نقش کاربر فعلی

        /// <summary>
        /// نقش فعلی کاربر در تسک
        /// </summary>
        public TaskRole CurrentUserRole { get; set; }

        /// <summary>
        /// متن نقش فعلی کاربر
        /// </summary>
        public string CurrentUserRoleText { get; set; }

        /// <summary>
        /// آیا کاربر می‌تواند تنظیمات را ویرایش کند؟
        /// </summary>
        public bool CanEdit { get; set; }

        #endregion

        #region تنظیمات 6 گانه

        /// <summary>
        /// ⭐⭐⭐ تنظیم 0: چه کسانی می‌توانند تنظیمات را تغییر دهند
        /// </summary>
        public SettingItemViewModel EditSettingsSetting { get; set; }

        /// <summary>
        /// تنظیم 1: چه کسانی می‌توانند کامنت بزنند
        /// </summary>
        public SettingItemViewModel CommentSetting { get; set; }

        /// <summary>
        /// تنظیم 2: چه کسانی می‌توانند عضو اضافه کنند
        /// </summary>
        public SettingItemViewModel AddMembersSetting { get; set; }

        /// <summary>
        /// تنظیم 3: چه کسانی می‌توانند عضو حذف کنند
        /// </summary>
        public SettingItemViewModel RemoveMembersSetting { get; set; }

        /// <summary>
        /// تنظیم 4: چه کسانی می‌توانند پس از اتمام ویرایش کنند
        /// </summary>
        public SettingItemViewModel EditAfterCompletionSetting { get; set; }

        /// <summary>
        /// تنظیم 5: آیا سازنده می‌تواند حذف/ویرایش کند (فقط مدیر می‌بیند)
        /// </summary>
        public CreatorEditDeleteSettingViewModel CreatorEditDeleteSetting { get; set; }

        #endregion

        #region وراثت

        /// <summary>
        /// آیا از تنظیمات پیش‌فرض استفاده می‌کند؟
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// متن منبع وراثت
        /// </summary>
        public string InheritedFromText { get; set; }

        #endregion
    }

    /// <summary>
    /// ⭐⭐⭐ یک تنظیم خاص (با checkbox های نقش‌ها)
    /// </summary>
    public class SettingItemViewModel
    {
        /// <summary>
        /// شناسه تنظیم (1-5)
        /// </summary>
        public int SettingId { get; set; }

        /// <summary>
        /// عنوان تنظیم
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تنظیم
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// لیست نقش‌های قابل انتخاب
        /// </summary>
        public List<RoleCheckboxItem> AvailableRoles { get; set; } = new();

        /// <summary>
        /// نقش‌های انتخاب شده (برای ارسال به سرور)
        /// </summary>
        public List<string> SelectedRoles { get; set; } = new();

        /// <summary>
        /// آیا فقط خواندنی است؟
        /// </summary>
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ یک checkbox برای نقش
    /// </summary>
    public class RoleCheckboxItem
    {
        /// <summary>
        /// کد نقش (a, b, c, d, e)
        /// </summary>
        public string RoleCode { get; set; }

        /// <summary>
        /// متن نقش (مدیر، سازنده، ...)
        /// </summary>
        public string RoleText { get; set; }

        /// <summary>
        /// سطح اعتبار (1-5)
        /// </summary>
        public int AuthorityLevel { get; set; }

        /// <summary>
        /// آیا انتخاب شده؟
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// آیا غیرفعال است؟ (نمی‌تواند تغییر کند)
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// دلیل غیرفعال بودن
        /// </summary>
        public string? DisabledReason { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ تنظیم 5 - مجوز حذف/ویرایش برای سازنده (فقط مدیر می‌بیند)
    /// </summary>
    public class CreatorEditDeleteSettingViewModel
    {
        public int SettingId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// آیا فقط خواندنی است؟
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// دلیل غیرفعال بودن
        /// </summary>
        public string? DisabledReason { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ ViewModel برای ذخیره تنظیمات (از فرم ارسال می‌شود)
    /// </summary>
    public class SaveTaskSettingsViewModel
    {
        [Required]
        public int TaskId { get; set; }

        /// <summary>
        /// ⭐⭐⭐ نقش‌های مجاز برای تغییر تنظیمات (comma-separated: "a,b")
        /// </summary>
        public string CanEditSettingsRoles { get; set; }

        /// <summary>
        /// نقش‌های مجاز برای کامنت (comma-separated: "a,b,c")
        /// </summary>
        public string CanCommentRoles { get; set; }

        /// <summary>
        /// نقش‌های مجاز برای افزودن عضو
        /// </summary>
        public string CanAddMembersRoles { get; set; }

        /// <summary>
        /// نقش‌های مجاز برای حذف عضو
        /// </summary>
        public string CanRemoveMembersRoles { get; set; }

        /// <summary>
        /// نقش‌های مجاز برای ویرایش پس از اتمام
        /// </summary>
        public string CanEditAfterCompletionRoles { get; set; }

        /// <summary>
        /// آیا سازنده می‌تواند حذف/ویرایش کند
        /// </summary>
        public bool CreatorCanEditDelete { get; set; }

        /// <summary>
        /// آیا از وراثت استفاده شود (true = بازگشت به پیش‌فرض)
        /// </summary>
        public bool UseInheritedSettings { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ آمار استفاده از تنظیمات
    /// </summary>
    public class TaskSettingsStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل تسک‌ها
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// تعداد تسک‌های با تنظیمات سفارشی
        /// </summary>
        public int TasksWithCustomSettings { get; set; }

        /// <summary>
        /// تعداد تسک‌های با تنظیمات ارثی
        /// </summary>
        public int TasksWithInheritedSettings { get; set; }

        /// <summary>
        /// درصد تسک‌های با تنظیمات سفارشی
        /// </summary>
        public double CustomSettingsPercentage { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ ViewModel برای تنظیمات پیش‌فرض شعبه
    /// </summary>
    public class BranchDefaultSettingsViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }

        public SettingItemViewModel CommentSetting { get; set; }
        public SettingItemViewModel AddMembersSetting { get; set; }
        public SettingItemViewModel RemoveMembersSetting { get; set; }
        public SettingItemViewModel EditAfterCompletionSetting { get; set; }
        public CreatorEditDeleteSettingViewModel CreatorEditDeleteSetting { get; set; }

        /// <summary>
        /// آیا شعبه تنظیمات پیش‌فرض دارد؟
        /// </summary>
        public bool HasSettings { get; set; }

        /// <summary>
        /// آمار استفاده
        /// </summary>
        public TaskSettingsStatisticsViewModel? Statistics { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ ViewModel برای تنظیمات پیش‌فرض دسته‌بندی
    /// </summary>
    public class CategoryDefaultSettingsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public SettingItemViewModel CommentSetting { get; set; }
        public SettingItemViewModel AddMembersSetting { get; set; }
        public SettingItemViewModel RemoveMembersSetting { get; set; }
        public SettingItemViewModel EditAfterCompletionSetting { get; set; }
        public CreatorEditDeleteSettingViewModel CreatorEditDeleteSetting { get; set; }

        /// <summary>
        /// آیا دسته‌بندی تنظیمات پیش‌فرض دارد؟
        /// </summary>
        public bool HasSettings { get; set; }

        /// <summary>
        /// آمار استفاده
        /// </summary>
        public TaskSettingsStatisticsViewModel? Statistics { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ ViewModel برای تاریخچه تغییرات
    /// </summary>
    public class SettingsChangeLogViewModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public byte SettingType { get; set; }
        public string SettingName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ChangedByUserName { get; set; }
        public string ChangedByUserProfileImage { get; set; }
        public DateTime ChangeDate { get; set; }
        public string ChangeDatePersian { get; set; }
        public string ChangeReason { get; set; }
    }
}
