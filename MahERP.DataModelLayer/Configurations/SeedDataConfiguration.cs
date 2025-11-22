using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کلاس مدیریت Seed Data های پایه سیستم
    /// ⚠️ توجه: Seed Data های سیستم اعلان به StaticNotificationSeedData منتقل شده است
    /// </summary>
    public static class SeedDataConfiguration
    {
        /// <summary>
        /// اعمال تمام Seed Data ها
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            SeedDefaultBranches(modelBuilder);
            SeedDefaultRoles(modelBuilder);
            SeedDefaultTaskCategories(modelBuilder);
            SeedDefaultPredefinedCopyDescriptions(modelBuilder);
            SeedDefaultRolePatterns(modelBuilder);
            SeedNotificationData(modelBuilder);

            // ⚠️ SeedNotificationData حذف شد - اکنون توسط SystemSeedDataBackgroundService مدیریت می‌شود
        }

        private static void SeedDefaultBranches(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>().HasData(
                new Branch
                {
                    Id = 1,
                    Name = "شعبه رسنا",
                    Description = "شعبه برند رسنا",
                    IsActive = true,
                    IsMainBranch = true,
                    CreateDate = new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified)
                }
            );
        }

        private static void SeedDefaultRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppRoles>().HasData(
                new AppRoles
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "مدیر سیستم",
                    RoleLevel = "1",
                    ConcurrencyStamp = "8e446cc7-743a-4133-8241-0f374fcbbc0d"
                },
                new AppRoles
                {
                    Id = "2",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "مدیر",
                    RoleLevel = "2",
                    ConcurrencyStamp = "5b6877d1-6fe6-4f8c-92a4-33fdf65a391f"
                },
                new AppRoles
                {
                    Id = "3",
                    Name = "Supervisor",
                    NormalizedName = "SUPERVISOR",
                    Description = "سرپرست",
                    RoleLevel = "3",
                    ConcurrencyStamp = "8f4cee96-4bf9-4019-b589-4de5c0230e2c"
                },
                new AppRoles
                {
                    Id = "4",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    Description = "کارمند",
                    RoleLevel = "4",
                    ConcurrencyStamp = "523c9ab5-4b4c-43e2-84be-12c4b6f74eed"
                },
                new AppRoles
                {
                    Id = "5",
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "کاربر عادی",
                    RoleLevel = "5",
                    ConcurrencyStamp = "aa5d01a0-a905-44ef-9e53-9c694828dbff"
                }
            );
        }

        private static void SeedDefaultTaskCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { Id = 1, Title = "عمومی", Description = "دسته‌بندی عمومی برای تسک‌ها", DisplayOrder = 1, IsActive = true },
                new TaskCategory { Id = 2, Title = "اداری", Description = "تسک‌های مربوط به امور اداری", DisplayOrder = 2, IsActive = true },
                new TaskCategory { Id = 3, Title = "فروش", Description = "تسک‌های مربوط به فروش", DisplayOrder = 4, IsActive = true },
                new TaskCategory { Id = 4, Title = "خدمات حضوری", Description = "تسک‌های مربوط به خدمات مشتریان غیر حضوری", DisplayOrder = 5, IsActive = true },
                new TaskCategory { Id = 5, Title = "خدمات  غیر حضوری", Description = "تسک‌های مربوط به خدمات مشتریان حضوری", DisplayOrder = 5, IsActive = true },
                new TaskCategory { Id = 6, Title = "بازاریابی", Description = "تسک‌های بازاریابی و تبلیغات", DisplayOrder = 6, IsActive = true },
                new TaskCategory { Id = 7, Title = "مالی", Description = "تسک‌های مربوط به امور مالی", DisplayOrder = 7, IsActive = true },
                new TaskCategory { Id = 8, Title = "منابع انسانی", Description = "تسک‌های مربوط به HR", DisplayOrder = 8, IsActive = true },
                new TaskCategory { Id = 9, Title = "فوری", Description = "تسک‌های فوری و اضطراری", DisplayOrder = 10, IsActive = true }
            );
        }

        private static void SeedDefaultPredefinedCopyDescriptions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PredefinedCopyDescription>().HasData(
                new PredefinedCopyDescription { Id = 1, Title = "جهت اطلاع", Description = "جهت اطلاع و پیگیری", IsActive = true },
                new PredefinedCopyDescription { Id = 2, Title = "جهت اقدام", Description = "جهت انجام اقدامات لازم", IsActive = true },
                new PredefinedCopyDescription { Id = 3, Title = "جهت بررسی", Description = "جهت بررسی و اعلام نظر", IsActive = true },
                new PredefinedCopyDescription { Id = 4, Title = "جهت تایید", Description = "جهت تایید و ابلاغ", IsActive = true },
                new PredefinedCopyDescription { Id = 5, Title = "جهت نظارت", Description = "جهت نظارت و کنترل", IsActive = true },
                new PredefinedCopyDescription { Id = 6, Title = "جهت هماهنگی", Description = "جهت هماهنگی‌های لازم", IsActive = true },
                new PredefinedCopyDescription { Id = 7, Title = "جهت پیگیری", Description = "جهت پیگیری و گزارش", IsActive = true },
                new PredefinedCopyDescription { Id = 8, Title = "جهت اجرا", Description = "جهت اجرای دستورات", IsActive = true }
            );
        }

        private static void SeedDefaultRolePatterns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePattern>().HasData(
                new RolePattern
                {
                    Id = 1,
                    PatternName = "مدیریت کامل",
                    Description = "دسترسی کامل به تمام بخش‌ها",
                    AccessLevel = 1,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 2,
                    PatternName = "مدیر عملیات",
                    Description = "مدیریت عملیات و تسک‌ها",
                    AccessLevel = 2,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 3,
                    PatternName = "کارشناس فروش",
                    Description = "دسترسی به ماژول فروش و CRM",
                    AccessLevel = 4,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 4,
                    PatternName = "کاربر عادی",
                    Description = "دسترسی محدود به تسک‌های شخصی",
                    AccessLevel = 5,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                }
            );

            modelBuilder.Entity<RolePatternDetails>().HasData(
                // الگوی مدیریت کامل - دسترسی کامل به همه چیز
                new RolePatternDetails { Id = 1, RolePatternId = 1, ControllerName = "Tasks", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 2, RolePatternId = 1, ControllerName = "CRM", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 3, RolePatternId = 1, ControllerName = "Stakeholder", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 4, RolePatternId = 1, ControllerName = "Contract", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 5, RolePatternId = 1, ControllerName = "User", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 6, RolePatternId = 1, ControllerName = "RolePattern", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 7, RolePatternId = 1, ControllerName = "UserPermission", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 8, RolePatternId = 1, ControllerName = "Branch", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 9, RolePatternId = 1, ControllerName = "Team", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 10, RolePatternId = 1, ControllerName = "TaskCategory", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },

                // الگوی مدیر عملیات
                new RolePatternDetails { Id = 11, RolePatternId = 2, ControllerName = "Tasks", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 1, IsActive = true },
                new RolePatternDetails { Id = 12, RolePatternId = 2, ControllerName = "CRM", ActionName = "Index,Details,Create,Edit", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 1, IsActive = true },
                new RolePatternDetails { Id = 13, RolePatternId = 2, ControllerName = "Stakeholder", ActionName = "Index,Details", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 1, IsActive = true },

                // الگوهای کارشناس فروش و کاربر عادی
                new RolePatternDetails { Id = 14, RolePatternId = 3, ControllerName = "CRM", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },
                new RolePatternDetails { Id = 15, RolePatternId = 3, ControllerName = "Stakeholder", ActionName = "Index,Details,Create,Edit", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },
                new RolePatternDetails { Id = 16, RolePatternId = 3, ControllerName = "Tasks", ActionName = "Index,Details,MyTasks", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },
                new RolePatternDetails { Id = 17, RolePatternId = 4, ControllerName = "Tasks", ActionName = "Index,Details,MyTasks", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true }
            );
        }

        private static void SeedNotificationData(ModelBuilder modelBuilder)
        {
            // 1️⃣ ماژول تسکینگ
            modelBuilder.Entity<NotificationModuleConfig>().HasData(
                new NotificationModuleConfig
                {
                    Id = 1,
                    ModuleCode = "TASKING",
                    ModuleNameFa = "ماژول تسکینگ",
                    ModuleNameEn = "Tasking Module",
                    Description = "سیستم مدیریت تسک‌ها و پروژه‌ها",
                    ColorCode = "#2196F3",
                    IsActive = true,
                    DisplayOrder = 1
                }
            );

            // 2️⃣ انواع اعلان تسکینگ
            modelBuilder.Entity<NotificationTypeConfig>().HasData(
                // اعلان روزانه
                new NotificationTypeConfig
                {
                    Id = 1,
                    ModuleConfigId = 1,
                    TypeCode = "TASK_DAILY_DIGEST",
                    TypeNameFa = "اعلان زمانبدی شده",
                    Description = "ارسال پیام زمان بندی شده)",
                    CoreNotificationTypeGeneral = 0,
                    CoreNotificationTypeSpecific = 0,
                    IsActive = true,
                    DefaultPriority = 0,
                    SupportsEmail = true,
                    SupportsSms = false,
                    SupportsTelegram = true,
                    AllowUserCustomization = true,
                    DisplayOrder = 1,
                    RelatedEventTypes = "[13]" // ⭐ فقط DailyTaskDigest
                },

                // تخصیص تسک
                new NotificationTypeConfig
                {
                    Id = 2,
                    ModuleConfigId = 1,
                    TypeCode = "TASK_ASSIGNED",
                    TypeNameFa = "تخصیص تسک جدید",
                    Description = "اعلان هنگام تخصیص تسک جدید به کاربر",
                    CoreNotificationTypeGeneral = 9,
                    CoreNotificationTypeSpecific = 1,
                    IsActive = true,
                    DefaultPriority = 1,
                    SupportsEmail = true,
                    SupportsSms = true,
                    SupportsTelegram = true,
                    AllowUserCustomization = true,
                    DisplayOrder = 2,
                    RelatedEventTypes = "[1,12]" // ⭐ TaskAssigned و TaskReassigned
                },

                // تکمیل تسک
                new NotificationTypeConfig
                {
                    Id = 3,
                    ModuleConfigId = 1,
                    TypeCode = "TASK_COMPLETED",
                    TypeNameFa = "تکمیل تسک واگذار شده",
                    Description = "اعلان تکمیل تسک به سازنده",
                    CoreNotificationTypeGeneral = 8,
                    CoreNotificationTypeSpecific = 2,
                    IsActive = true,
                    DefaultPriority = 1,
                    SupportsEmail = true,
                    SupportsSms = false,
                    SupportsTelegram = true,
                    AllowUserCustomization = true,
                    DisplayOrder = 3,
                    RelatedEventTypes = "[2,6]" // ⭐ TaskCompleted و TaskOperationCompleted
                },

                // یادآوری
                new NotificationTypeConfig
                {
                    Id = 4,
                    ModuleConfigId = 1,
                    TypeCode = "TASK_REMINDER",
                    TypeNameFa = "یادآوری سررسید تسک",
                    Description = "یادآوری تسک‌های نزدیک به سررسید",
                    CoreNotificationTypeGeneral = 6,
                    CoreNotificationTypeSpecific = 3,
                    IsActive = true,
                    DefaultPriority = 2,
                    SupportsEmail = true,
                    SupportsSms = true,
                    SupportsTelegram = true,
                    AllowUserCustomization = true,
                    DisplayOrder = 4,
                    RelatedEventTypes = "[3]" // ⭐ فقط TaskDeadlineReminder
                },

                // تغییرات در تسک
                new NotificationTypeConfig
                {
                    Id = 5,
                    ModuleConfigId = 1,
                    TypeCode = "TASK_UPDATED",
                    TypeNameFa = "تغییرات در تسک",
                    Description = "اعلان ثبت کامنت، WorkLog یا تغییرات",
                    CoreNotificationTypeGeneral = 10,
                    CoreNotificationTypeSpecific = 4,
                    IsActive = true,
                    DefaultPriority = 0,
                    SupportsEmail = true,
                    SupportsSms = false,
                    SupportsTelegram = true,
                    AllowUserCustomization = true,
                    DisplayOrder = 5,
                    RelatedEventTypes = "[4,5,8,10,11,14]" // ⭐ TaskCommentAdded, TaskUpdated, TaskStatusChanged, CommentMentioned, TaskPriorityChanged, TaskWorkLog
                }

            );
        }
    }
}