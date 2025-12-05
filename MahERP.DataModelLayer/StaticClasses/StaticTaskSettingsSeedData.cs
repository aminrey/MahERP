using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// ⭐⭐⭐ داده‌های پایه (Seed Data) برای سیستم تنظیمات تسک
    /// این کلاس توسط SystemSeedDataBackgroundService استفاده می‌شود
    /// </summary>
    public static class StaticTaskSettingsSeedData
    {
        /// <summary>
        /// ⭐⭐⭐ تنظیمات پیش‌فرض سراسری سیستم
        /// این تنظیمات زمانی استفاده می‌شود که هیچ تنظیمات شعبه/دسته‌بندی وجود نداشته باشد
        /// </summary>
        public static TaskSettings GetGlobalDefaultSettings()
        {
            return new TaskSettings
            {
                // مقادیر پیش‌فرض - دسترسی متعادل
                CanCommentRoles = "a,b,c,d,e", // همه می‌توانند کامنت بگذارند
                CanAddMembersRoles = "a,b,c",  // Admin, Manager, Supervisor
                CanRemoveMembersRoles = "a,b", // فقط Admin و Manager
                CanEditAfterCompletionRoles = "a,b", // فقط Admin و Manager
                CreatorCanEditDelete = false,   // سازنده نمی‌تواند حذف/ویرایش کند
                
                IsInherited = false,
                InheritedFrom = 0, // Global
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// ⭐⭐⭐ Preset های آماده برای استفاده سریع
        /// </summary>
        public static class Presets
        {
            /// <summary>
            /// الگوی باز - همه دسترسی دارند
            /// </summary>
            public static TaskSettings Open => new TaskSettings
            {
                CanCommentRoles = "a,b,c,d,e",
                CanAddMembersRoles = "a,b,c,d",
                CanRemoveMembersRoles = "a,b,c",
                CanEditAfterCompletionRoles = "a,b,c",
                CreatorCanEditDelete = true,
                IsInherited = false,
                CreatedDate = DateTime.Now
            };

            /// <summary>
            /// الگوی کنترل‌شده - دسترسی متعادل
            /// </summary>
            public static TaskSettings Controlled => new TaskSettings
            {
                CanCommentRoles = "a,b,c,d,e",
                CanAddMembersRoles = "a,b,c",
                CanRemoveMembersRoles = "a,b",
                CanEditAfterCompletionRoles = "a,b",
                CreatorCanEditDelete = false,
                IsInherited = false,
                CreatedDate = DateTime.Now
            };

            /// <summary>
            /// الگوی محدود - فقط مدیران
            /// </summary>
            public static TaskSettings Strict => new TaskSettings
            {
                CanCommentRoles = "a,b,c",
                CanAddMembersRoles = "a,b",
                CanRemoveMembersRoles = "a",
                CanEditAfterCompletionRoles = "a",
                CreatorCanEditDelete = false,
                IsInherited = false,
                CreatedDate = DateTime.Now
            };

            /// <summary>
            /// الگوی پروژه‌ای - همکاری تیمی
            /// </summary>
            public static TaskSettings Project => new TaskSettings
            {
                CanCommentRoles = "a,b,c,d",
                CanAddMembersRoles = "a,b,c",
                CanRemoveMembersRoles = "a,b",
                CanEditAfterCompletionRoles = "a,b,c",
                CreatorCanEditDelete = true,
                IsInherited = false,
                CreatedDate = DateTime.Now
            };

            /// <summary>
            /// الگوی خدماتی - مشتری‌محور
            /// </summary>
            public static TaskSettings Service => new TaskSettings
            {
                CanCommentRoles = "a,b,c,d,e",
                CanAddMembersRoles = "a,b,c",
                CanRemoveMembersRoles = "a,b",
                CanEditAfterCompletionRoles = "a,b",
                CreatorCanEditDelete = false,
                IsInherited = false,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// ⭐⭐⭐ نام‌های نقش‌ها برای نمایش
        /// </summary>
        public static class RoleNames
        {
            public static readonly Dictionary<string, string> Persian = new()
            {
                { "a", "مدیر سیستم" },
                { "b", "مدیر" },
                { "c", "سرپرست" },
                { "d", "کارمند" },
                { "e", "کاربر عادی" }
            };

            public static readonly Dictionary<string, string> English = new()
            {
                { "a", "Admin" },
                { "b", "Manager" },
                { "c", "Supervisor" },
                { "d", "Employee" },
                { "e", "User" }
            };

            /// <summary>
            /// دریافت نام فارسی نقش
            /// </summary>
            public static string GetPersianName(string roleCode)
            {
                return Persian.TryGetValue(roleCode, out var name) ? name : "نامشخص";
            }

            /// <summary>
            /// تبدیل رشته نقش‌ها به لیست نام‌های فارسی
            /// </summary>
            public static List<string> GetPersianNames(string roles)
            {
                if (string.IsNullOrEmpty(roles))
                    return new List<string>();

                return roles.Split(',')
                    .Select(r => r.Trim())
                    .Where(Persian.ContainsKey)
                    .Select(r => Persian[r])
                    .ToList();
            }

            /// <summary>
            /// تبدیل لیست نقش‌ها به رشته کاما-جدا
            /// </summary>
            public static string JoinRoles(params string[] roles)
            {
                return string.Join(",", roles.Distinct().OrderBy(r => r));
            }
        }

        /// <summary>
        /// ⭐⭐⭐ توضیحات تنظیمات برای UI
        /// </summary>
        public static class Descriptions
        {
            public const string CanComment = "تعیین می‌کند چه نقش‌هایی می‌توانند در تسک کامنت بگذارند";
            public const string CanAddMembers = "تعیین می‌کند چه نقش‌هایی می‌توانند عضو جدید به تسک اضافه کنند";
            public const string CanRemoveMembers = "تعیین می‌کند چه نقش‌هایی می‌توانند عضو از تسک حذف کنند";
            public const string CanEditAfterCompletion = "تعیین می‌کند چه نقش‌هایی می‌توانند پس از تکمیل تسک، آن را ویرایش کنند";
            public const string CreatorCanEditDelete = "آیا سازنده تسک می‌تواند آن را ویرایش یا حذف کند";
        }

        /// <summary>
        /// ⭐⭐⭐ آیکون‌ها برای UI
        /// </summary>
        public static class Icons
        {
            public const string Comment = "fa-comment";
            public const string AddMember = "fa-user-plus";
            public const string RemoveMember = "fa-user-minus";
            public const string Edit = "fa-edit";
            public const string Delete = "fa-trash";
        }

        /// <summary>
        /// ⭐⭐⭐ رنگ‌ها برای UI
        /// </summary>
        public static class Colors
        {
            public const string Comment = "primary";
            public const string AddMember = "success";
            public const string RemoveMember = "danger";
            public const string Edit = "warning";
            public const string Delete = "danger";
        }
    }
}
