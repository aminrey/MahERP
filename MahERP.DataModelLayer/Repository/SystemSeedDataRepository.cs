using MahERP.DataModelLayer.Entities.Notifications;
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
    }
}
