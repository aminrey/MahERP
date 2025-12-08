using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Organizations; // ⭐ NEW
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
        /// اطمینان از وجود ماژول‌های اعلان
        /// </summary>
        private async Task EnsureNotificationModulesAsync()
        {
            foreach (var module in StaticNotificationSeedData.NotificationModules)
            {
                var exists = await _context.NotificationModuleConfig_Tbl
                    .AnyAsync(m => m.Id == module.Id);

                if (!exists)
                {
                    _context.NotificationModuleConfig_Tbl.Add(module);
                    _logger.LogInformation($"➕ Added NotificationModule: {module.ModuleNameFa} (ID: {module.Id})");
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// اطمینان از وجود انواع اعلان‌ها
        /// </summary>
        private async Task EnsureNotificationTypesAsync()
        {
            foreach (var notificationType in StaticNotificationSeedData.NotificationTypes)
            {
                var exists = await _context.NotificationTypeConfig_Tbl
                    .AnyAsync(nt => nt.Id == notificationType.Id);

                if (!exists)
                {
                    _context.NotificationTypeConfig_Tbl.Add(notificationType);
                    _logger.LogInformation($"➕ Added NotificationType: {notificationType.TypeNameFa} (ID: {notificationType.Id})");
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
        /// ⭐⭐⭐ اطمینان از وجود تنظیمات پیش‌فرض سراسری سیستم
        /// </summary>
        public async Task EnsureTaskSettingsSeedDataAsync()
        {
            try
            {
                // بررسی وجود تنظیمات سراسری (TaskId = 0 یعنی Global)
                var globalSettings = await _context.TaskSettings_Tbl
                    .FirstOrDefaultAsync(ts => ts.TaskId == 0);

                if (globalSettings == null)
                {
                    // ایجاد تنظیمات سراسری
                    var defaultSettings = StaticTaskSettingsSeedData.GetGlobalDefaultSettings();
                    defaultSettings.TaskId = 0; // Global indicator
                    defaultSettings.InheritedFrom = 0; // Global
                    defaultSettings.IsInherited = false;
                    defaultSettings.CreatedByUserId = "system";
                    defaultSettings.CreatedDate = DateTime.Now;

                    _context.TaskSettings_Tbl.Add(defaultSettings);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Global Task Settings successfully created.");
                }
                else
                {
                    _logger.LogInformation("ℹ️ Global Task Settings already exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ensuring task settings seed data");
                throw;
            }
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
                foreach (var position in StaticPositionSeedData.CommonPositions)
                {
                    var exists = await _context.OrganizationPosition_Tbl
                        .AnyAsync(p => p.Id == position.Id);

                    if (!exists)
                    {
                        _context.OrganizationPosition_Tbl.Add(position);
                        _logger.LogInformation($"➕ Added OrganizationPosition: {position.Title} (ID: {position.Id})");
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
    }
}
