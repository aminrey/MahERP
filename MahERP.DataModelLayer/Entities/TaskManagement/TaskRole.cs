namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// نقش‌های کاربر در تسک (با فاصله 100 برای قابلیت توسعه)
    /// ⭐⭐⭐ هر چه عدد کمتر = سطح دسترسی بالاتر
    /// </summary>
    public enum TaskRole
    {
        /// <summary>
        /// ⭐⭐⭐ مدیر بالاسری (Hierarchy Manager) - جدید!
        /// مدیر تیم‌های سطح بالاتر که به تسک‌های تیم‌های زیرمجموعه دسترسی دارد
        /// مثال: امین (مدیر تیم مدیریت) به تسک‌های تیم بازاریابی دسترسی دارد
        /// </summary>
        HierarchyManager = 50,

        /// <summary>
        /// مدیر مستقیم (Direct Manager)
        /// مدیر تیمی که تسک در آن ایجاد شده
        /// </summary>
        Manager = 100,

        /// <summary>
        /// سازنده تسک (Creator)
        /// کسی که تسک را ایجاد کرده
        /// </summary>
        Creator = 200,

        /// <summary>
        /// عضو (Member)
        /// کاربرانی که به تسک اختصاص داده شده‌اند
        /// </summary>
        Member = 300,

        /// <summary>
        /// ناظر (Supervisor)
        /// کاربرانی که به عنوان ناظر به تسک اضافه شده‌اند
        /// </summary>
        Supervisor = 400,

        /// <summary>
        /// رونوشت (Carbon Copy)
        /// کاربرانی که به عنوان رونوشت اضافه شده‌اند (فقط مشاهده)
        /// </summary>
        CarbonCopy = 500
    }

    /// <summary>
    /// Extension Methods برای TaskRole
    /// </summary>
    public static class TaskRoleExtensions
    {
        /// <summary>
        /// دریافت متن فارسی نقش
        /// </summary>
        public static string GetText(this TaskRole role) => role switch
        {
            TaskRole.HierarchyManager => "مدیر بالاسری",
            TaskRole.Manager => "مدیر تیم",
            TaskRole.Creator => "سازنده",
            TaskRole.Member => "عضو",
            TaskRole.Supervisor => "ناظر",
            TaskRole.CarbonCopy => "رونوشت",
            _ => "نامشخص"
        };

        /// <summary>
        /// دریافت رنگ Badge
        /// </summary>
        public static string GetBadgeClass(this TaskRole role) => role switch
        {
            TaskRole.HierarchyManager => "bg-purple",
            TaskRole.Manager => "bg-primary",
            TaskRole.Creator => "bg-success",
            TaskRole.Member => "bg-info",
            TaskRole.Supervisor => "bg-warning",
            TaskRole.CarbonCopy => "bg-secondary",
            _ => "bg-dark"
        };

        /// <summary>
        /// دریافت آیکون
        /// </summary>
        public static string GetIcon(this TaskRole role) => role switch
        {
            TaskRole.HierarchyManager => "fa-crown",
            TaskRole.Manager => "fa-user-tie",
            TaskRole.Creator => "fa-user-plus",
            TaskRole.Member => "fa-user",
            TaskRole.Supervisor => "fa-eye",
            TaskRole.CarbonCopy => "fa-copy",
            _ => "fa-question"
        };

        /// <summary>
        /// بررسی اینکه آیا نقش فعلی می‌تواند نقش target را مدیریت کند
        /// </summary>
        public static bool CanManage(this TaskRole userRole, TaskRole targetRole)
        {
            // هر نقش فقط می‌تواند نقش‌های پایین‌تر از خود را مدیریت کند
            return (int)userRole <= (int)targetRole;
        }

        /// <summary>
        /// آیا این نقش می‌تواند تنظیمات را ویرایش کند؟
        /// </summary>
        public static bool CanEditSettings(this TaskRole role)
        {
            return role == TaskRole.HierarchyManager || 
                   role == TaskRole.Manager;
        }

        /// <summary>
        /// آیا این نقش می‌تواند عضو اضافه کند؟
        /// </summary>
        public static bool CanAddMembers(this TaskRole role)
        {
            return role == TaskRole.HierarchyManager || 
                   role == TaskRole.Manager || 
                   role == TaskRole.Creator;
        }

        /// <summary>
        /// آیا این نقش می‌تواند عضو حذف کند؟
        /// </summary>
        public static bool CanRemoveMembers(this TaskRole role)
        {
            return role == TaskRole.HierarchyManager || 
                   role == TaskRole.Manager || 
                   role == TaskRole.Creator;
        }

        /// <summary>
        /// سطح دسترسی (Priority) - برای مقایسه
        /// </summary>
        public static int GetAuthorityLevel(this TaskRole role)
        {
            return (int)role;
        }
    }
}
