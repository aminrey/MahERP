using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.ModuleAccessViewModels
{
    /// <summary>
    /// ViewModel گزارش دسترسی به ماژول‌ها
    /// </summary>
    public class ModuleAccessReportViewModel
    {
        /// <summary>
        /// نوع ماژول (0=Core, 1=Tasking, 2=CRM)
        /// </summary>
        public byte ModuleType { get; set; }

        /// <summary>
        /// نام ماژول
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// آیکون ماژول
        /// </summary>
        public string ModuleIcon { get; set; }

        /// <summary>
        /// رنگ ماژول
        /// </summary>
        public string ModuleColor { get; set; }

        /// <summary>
        /// کاربران دارای دسترسی مستقیم
        /// </summary>
        public List<string> DirectUsers { get; set; } = new();

        /// <summary>
        /// تیم‌های دارای دسترسی
        /// </summary>
        public List<string> Teams { get; set; } = new();

        /// <summary>
        /// شعب دارای دسترسی
        /// </summary>
        public List<string> Branches { get; set; } = new();

        /// <summary>
        /// جمع کل دسترسی‌ها
        /// </summary>
        public int TotalCount => DirectUsers.Count + Teams.Count + Branches.Count;
    }

    /// <summary>
    /// ViewModel کامل گزارش تمام ماژول‌ها
    /// </summary>
    public class AllModulesReportViewModel
    {
        public List<ModuleAccessReportViewModel> Reports { get; set; } = new();
    }
}