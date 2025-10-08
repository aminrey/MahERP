using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تسک‌های زیرتیم‌ها به صورت گروه‌بندی شده
    /// </summary>
    public class SubTeamTasksGroupedViewModel
    {
        /// <summary>
        /// تسک‌های گروه‌بندی شده بر اساس تیم اصلی
        /// Key = شناسه تیم اصلی (که کاربر مدیر آن است)
        /// Value = لیست زیرتیم‌ها و تسک‌های آن‌ها
        /// </summary>
        public Dictionary<int, SubTeamGroupViewModel> TeamGroups { get; set; } = new();

        /// <summary>
        /// تعداد کل تسک‌های زیرتیم‌ها
        /// </summary>
        public int TotalSubTeamTasks { get; set; }
    }

    /// <summary>
    /// ViewModel برای گروه تیم اصلی
    /// </summary>
    public class SubTeamGroupViewModel
    {
        /// <summary>
        /// شناسه تیم اصلی
        /// </summary>
        public int ParentTeamId { get; set; }

        /// <summary>
        /// نام تیم اصلی
        /// </summary>
        public string ParentTeamName { get; set; }

        /// <summary>
        /// زیرتیم‌ها و تسک‌های آن‌ها
        /// Key = شناسه زیرتیم
        /// Value = تسک‌های گروه‌بندی شده بر اساس کاربر
        /// </summary>
        public Dictionary<int, SubTeamTasksViewModel> SubTeams { get; set; } = new();

        /// <summary>
        /// تعداد کل تسک‌های این گروه
        /// </summary>
        public int TotalTasks { get; set; }
    }

    /// <summary>
    /// ViewModel برای تسک‌های یک زیرتیم
    /// </summary>
    public class SubTeamTasksViewModel
    {
        /// <summary>
        /// شناسه زیرتیم
        /// </summary>
        public int SubTeamId { get; set; }

        /// <summary>
        /// نام زیرتیم
        /// </summary>
        public string SubTeamName { get; set; }

        /// <summary>
        /// سطح عمق زیرتیم (برای نمایش سلسله مراتبی)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// تسک‌های گروه‌بندی شده بر اساس کاربر سازنده
        /// Key = UserId
        /// Value = لیست تسک‌های آن کاربر
        /// </summary>
        public Dictionary<string, UserTasksGroupViewModel> TasksByUser { get; set; } = new();

        /// <summary>
        /// تعداد کل تسک‌های این زیرتیم
        /// </summary>
        public int TotalTasks { get; set; }
    }

    /// <summary>
    /// ViewModel برای تسک‌های یک کاربر
    /// </summary>
    public class UserTasksGroupViewModel
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کامل کاربر
        /// </summary>
        public string UserFullName { get; set; }

        /// <summary>
        /// لیست تسک‌های این کاربر
        /// </summary>
        public List<TaskViewModel> Tasks { get; set; } = new();

        /// <summary>
        /// تعداد تسک‌های این کاربر
        /// </summary>
        public int TaskCount => Tasks.Count;
    }
}