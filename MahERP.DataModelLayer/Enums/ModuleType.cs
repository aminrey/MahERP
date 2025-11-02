namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// انواع ماژول‌های سیستم
    /// </summary>
    public enum ModuleType : byte
    {
        /// <summary>
        /// هسته مرکزی (Core)
        /// </summary>
        Core = 0,

        /// <summary>
        /// تسکینگ (Tasking)
        /// </summary>
        Tasking = 1,

        /// <summary>
        /// CRM
        /// </summary>
        CRM = 2
    }

    /// <summary>
    /// کلاس کمکی برای کار با ModuleType
    /// </summary>
    public static class ModuleTypeExtensions
    {
        /// <summary>
        /// دریافت نام فارسی ماژول
        /// </summary>
        public static string GetDisplayName(this ModuleType moduleType)
        {
            return moduleType switch
            {
                ModuleType.Core => "هسته مرکزی",
                ModuleType.Tasking => "تسکینگ",
                ModuleType.CRM => "CRM",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت آیکون ماژول
        /// </summary>
        public static string GetIcon(this ModuleType moduleType)
        {
            return moduleType switch
            {
                ModuleType.Core => "fa fa-home",
                ModuleType.Tasking => "fa fa-tasks",
                ModuleType.CRM => "fa fa-chart-line",
                _ => "fa fa-question"
            };
        }

        /// <summary>
        /// دریافت رنگ ماژول
        /// </summary>
        public static string GetColor(this ModuleType moduleType)
        {
            return moduleType switch
            {
                ModuleType.Core => "primary",
                ModuleType.Tasking => "success",
                ModuleType.CRM => "info",
                _ => "secondary"
            };
        }

        /// <summary>
        /// دریافت URL پایه ماژول
        /// </summary>
        public static string GetBaseUrl(this ModuleType moduleType)
        {
            return moduleType switch
            {
                ModuleType.Core => "/AppCoreArea/Dashboard/Index",
                ModuleType.Tasking => "/TaskingArea/Dashboard/Index",
                ModuleType.CRM => "/CrmArea/Dashboard/Index",
                _ => "/"
            };
        }
    }
}