namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// نتیجه فیلتر آماده تسک‌ها
    /// </summary>
    public class TaskFilterResultViewModel
    {
        /// <summary>
        /// نام فیلتر
        /// </summary>
        public string FilterName { get; set; }

        /// <summary>
        /// تعداد کل تسک‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تسک‌ها گروه‌بندی شده
        /// Key اول: نام تیم
        /// Key دوم: نام فرد
        /// Value: لیست تسک‌ها
        /// </summary>
        public Dictionary<string, Dictionary<string, List<TaskViewModel>>> GroupedTasks { get; set; } 
            = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>();
    }

    /// <summary>
    /// تعداد تسک‌ها برای هر فیلتر آماده
    /// </summary>
    public class TaskFilterCountsViewModel
    {
        /// <summary>
        /// تعداد همه تسک‌های قابل مشاهده
        /// </summary>
        public int AllVisibleCount { get; set; }

        /// <summary>
        /// تعداد تسک‌های منتصب شده به من
        /// </summary>
        public int MyAssignedCount { get; set; }

        /// <summary>
        /// تعداد تسک‌های واگذار شده به دیگران
        /// </summary>
        public int AssignedByMeCount { get; set; }

        /// <summary>
        /// تعداد تسک‌های تیم‌های من
        /// </summary>
        public int MyTeamsCount { get; set; }

        /// <summary>
        /// تعداد تسک‌های نظارتی
        /// </summary>
        public int SupervisedCount { get; set; }
    }

    /// <summary>
    /// نوع فیلتر آماده
    /// </summary>
    public enum QuickFilterType
    {
        AllVisible = 0,
        MyAssigned = 1,
        AssignedByMe = 2,
        MyTeams = 3,
        Supervised = 4
    }
}