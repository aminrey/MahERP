using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.StaticClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// مدیریت Seed Data های پایه سیستم
    /// </summary>
    public interface ISystemSeedDataRepository
    {
        Task EnsureNotificationSeedDataAsync();
        Task<bool> CheckIfModuleExistsAsync(int moduleId);
        Task<bool> CheckIfNotificationTypeExistsAsync(int typeId);
        
        // ⭐⭐⭐ Task Settings Seed Data
        Task EnsureTaskSettingsSeedDataAsync();
        Task<TaskSettings> GetGlobalDefaultSettingsAsync();
        
        // ⭐⭐⭐ Position Seed Data
        Task EnsurePositionSeedDataAsync();
        Task<bool> CheckIfPositionExistsAsync(int positionId);
        
        // ⭐⭐⭐ CRM Lead Status Seed Data
        Task EnsureCrmLeadStatusSeedDataAsync();
        Task<bool> CheckIfCrmLeadStatusExistsAsync(int statusId);
    }

    public class SystemSeedDataRepository : ISystemSeedDataRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SystemSeedDataRepository> _logger;

        public SystemSeedDataRepository(
            AppDbContext context,
            ILogger<SystemSeedDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// اطمینان از وجود Seed Data های سیستم اعلان
        /// </summary>
        public async Task EnsureNotificationSeedDataAsync()
        {
            try
            {
                // 1️⃣ ایجاد ماژول‌ها
                await EnsureNotificationModulesAsync();

                // 2️⃣ ایجاد انواع اعلان‌ها
                await EnsureNotificationTypesAsync();

                _logger.LogInformation("✅ Notification Seed Data successfully ensured.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ensuring notification seed data");
                throw;
            }
        }

        /// <summary>
        /// ⭐ اصلاح شده - اطمینان از وجود ماژول‌های اعلان
        /// </summary>
        private async Task EnsureNotificationModulesAsync()
        {
            foreach (var module in StaticNotificationSeedData.NotificationModules)
            {
                // ⭐ بررسی بر اساس ModuleCode (نه Id)
                var exists = await _context.NotificationModuleConfig_Tbl
                    .AnyAsync(m => m.ModuleCode == module.ModuleCode);

                if (!exists)
                {
                    // ⭐ ایجاد entity جدید بدون Id ثابت
                    var newModule = new NotificationModuleConfig
                    {
                        ModuleCode = module.ModuleCode,
                        ModuleNameFa = module.ModuleNameFa,
                        ModuleNameEn = module.ModuleNameEn,
                        Description = module.Description,
                        ColorCode = module.ColorCode,
                        IsActive = module.IsActive,
                        DisplayOrder = module.DisplayOrder
                    };

                    _context.NotificationModuleConfig_Tbl.Add(newModule);
                    _logger.LogInformation($"➕ Added NotificationModule: {module.ModuleNameFa} ({module.ModuleCode})");
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ⭐ اصلاح شده - اطمینان از وجود انواع اعلان‌ها
        /// </summary>
        private async Task EnsureNotificationTypesAsync()
        {
            // ⭐ اول ماژول TASKING را پیدا کن (که قبلاً ایجاد شده)
            var taskingModule = await _context.NotificationModuleConfig_Tbl
                .FirstOrDefaultAsync(m => m.ModuleCode == "TASKING");

            if (taskingModule == null)
            {
                _logger.LogWarning("⚠️ TASKING module not found, skipping notification types");
                return;
            }

            foreach (var notificationType in StaticNotificationSeedData.NotificationTypes)
            {
                // ⭐ بررسی بر اساس TypeCode (نه Id)
                var exists = await _context.NotificationTypeConfig_Tbl
                    .AnyAsync(nt => nt.TypeCode == notificationType.TypeCode);

                if (!exists)
                {
                    // ⭐ ایجاد entity جدید بدون Id ثابت
                    var newType = new NotificationTypeConfig
                    {
                        ModuleConfigId = taskingModule.Id, // ⭐ از Id واقعی ماژول استفاده کن
                        TypeCode = notificationType.TypeCode,
                        TypeNameFa = notificationType.TypeNameFa,
                        Description = notificationType.Description,
                        CoreNotificationTypeGeneral = notificationType.CoreNotificationTypeGeneral,
                        CoreNotificationTypeSpecific = notificationType.CoreNotificationTypeSpecific,
                        IsActive = notificationType.IsActive,
                        DefaultPriority = notificationType.DefaultPriority,
                        SupportsEmail = notificationType.SupportsEmail,
                        SupportsSms = notificationType.SupportsSms,
                        SupportsTelegram = notificationType.SupportsTelegram,
                        AllowUserCustomization = notificationType.AllowUserCustomization,
                        DisplayOrder = notificationType.DisplayOrder,
                        RelatedEventTypes = notificationType.RelatedEventTypes
                    };

                    _context.NotificationTypeConfig_Tbl.Add(newType);
                    _logger.LogInformation($"➕ Added NotificationType: {notificationType.TypeNameFa} ({notificationType.TypeCode})");
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// بررسی وجود ماژول
        /// </summary>
        public async Task<bool> CheckIfModuleExistsAsync(int moduleId)
        {
            return await _context.NotificationModuleConfig_Tbl
                .AnyAsync(m => m.Id == moduleId);
        }

        /// <summary>
        /// بررسی وجود نوع اعلان
        /// </summary>
        public async Task<bool> CheckIfNotificationTypeExistsAsync(int typeId)
        {
            return await _context.NotificationTypeConfig_Tbl
                .AnyAsync(nt => nt.Id == typeId);
        }

        /// <summary>
        /// ⭐⭐⭐ تنظیمات پیش‌فرض تسک از StaticClass خوانده می‌شود
        /// توجه: TaskSettings_Tbl فقط برای تنظیمات تسک‌های خاص است (نه Global)
        /// برای Global از StaticTaskSettingsSeedData.GetGlobalDefaultSettings() استفاده کنید
        /// </summary>
        public async Task EnsureTaskSettingsSeedDataAsync()
        {
            // ⭐ این متد دیگر نیازی به seed کردن ندارد
            // چون TaskSettings_Tbl دارای FK به Tasks_Tbl است
            // و برای تنظیمات Global از StaticTaskSettingsSeedData استفاده می‌شود
            _logger.LogInformation("ℹ️ Task Settings: Using StaticTaskSettingsSeedData for global defaults (no DB seeding needed)");
            await Task.CompletedTask;
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تنظیمات پیش‌فرض سراسری سیستم
        /// </summary>
        public async Task<TaskSettings> GetGlobalDefaultSettingsAsync()
        {
            // TaskId = 0 نشان‌دهنده تنظیمات سراسری است
            var globalSettings = await _context.TaskSettings_Tbl
                .FirstOrDefaultAsync(ts => ts.TaskId == 0);

            if (globalSettings == null)
            {
                // اگر تنظیمات سراسری وجود ندارد، از استاتیک کلاس استفاده می‌کنیم
                return StaticTaskSettingsSeedData.GetGlobalDefaultSettings();
            }

            return globalSettings;
        }

        /// <summary>
        /// ⭐⭐⭐ اطمینان از وجود سمت‌های استاندارد سازمانی
        /// </summary>
        public async Task EnsurePositionSeedDataAsync()
        {
            try
            {
                // ⭐ پیدا کردن اولین کاربر برای CreatorUserId
                var systemUser = await _context.Users.FirstOrDefaultAsync();
                
                if (systemUser == null)
                {
                    _logger.LogWarning("⚠️ No users found in database, skipping Position seed");
                    return;
                }

                foreach (var position in StaticPositionSeedData.CommonPositions)
                {
                    // ⭐ بررسی بر اساس Title و Category (نه Id چون IDENTITY است)
                    var exists = await _context.OrganizationPosition_Tbl
                        .AnyAsync(p => p.Title == position.Title && p.Category == position.Category);

                    if (!exists)
                    {
                        // ⭐ ایجاد entity جدید بدون Id ثابت (SQL Server خودش Id میدهد)
                        var newPosition = new OrganizationPosition
                        {
                            Title = position.Title,
                            TitleEnglish = position.TitleEnglish,
                            Category = position.Category,
                            Description = position.Description,
                            Level = position.Level,
                            DefaultPowerLevel = position.DefaultPowerLevel,
                            IsCommon = position.IsCommon,
                            RequiresDegree = position.RequiresDegree,
                            MinimumDegree = position.MinimumDegree,
                            MinimumExperienceYears = position.MinimumExperienceYears,
                            SuggestedMinSalary = position.SuggestedMinSalary,
                            SuggestedMaxSalary = position.SuggestedMaxSalary,
                            CanHireSubordinates = position.CanHireSubordinates,
                            DisplayOrder = position.DisplayOrder,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = systemUser.Id // ⭐ استفاده از کاربر واقعی
                        };

                        _context.OrganizationPosition_Tbl.Add(newPosition);
                        _logger.LogInformation($"➕ Added OrganizationPosition: {position.Title} ({position.Category})");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ OrganizationPosition Seed Data successfully ensured.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ensuring organization position seed data");
                throw;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ بررسی وجود سمت
        /// </summary>
        public async Task<bool> CheckIfPositionExistsAsync(int positionId)
        {
            return await _context.OrganizationPosition_Tbl
                .AnyAsync(p => p.Id == positionId);
        }

        /// <summary>
        /// ⭐⭐⭐ اطمینان از وجود وضعیت‌های پیش‌فرض سرنخ CRM
        /// </summary>
        public async Task EnsureCrmLeadStatusSeedDataAsync()
        {
            // TODO: این متد نیاز به بازنویسی دارد - سیستم قدیمی CrmLeadStatus با LeadStageStatus جایگزین شده
            // داده‌های جدید در CrmSeedData.cs تعریف شده و در Migration اضافه می‌شوند
            _logger.LogInformation("✅ CRM Seed Data is handled via Migration (LeadStageStatus, PostPurchaseStage)");
        }

        /// <summary>
        /// ⭐⭐⭐ بررسی وجود وضعیت سرنخ
        /// </summary>
        public async Task<bool> CheckIfCrmLeadStatusExistsAsync(int statusId)
        {
            // TODO: تغییر به LeadStageStatus
            return await _context.LeadStageStatus_Tbl
                .AnyAsync(s => s.Id == statusId);
        }
    }
}
