using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MahERP.WebApp.Services.BackgroundServices
{
    /// <summary>
    /// ⭐⭐⭐ Background Service برای بررسی و اعمال خودکار تنظیمات تسک
    /// </summary>
    public class TaskSettingsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskSettingsBackgroundService> _logger;
        private Timer? _timer;

        public TaskSettingsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TaskSettingsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⭐ TaskSettingsBackgroundService started");

            // اجرای اولیه
            await CheckAndApplyDefaultSettingsAsync();

            // تنظیم Timer برای اجرای دوره‌ای (هر 6 ساعت)
            _timer = new Timer(
                async _ => await CheckAndApplyDefaultSettingsAsync(),
                null,
                TimeSpan.FromHours(6),
                TimeSpan.FromHours(6)
            );

            await Task.CompletedTask;
        }

        /// <summary>
        /// بررسی و اعمال تنظیمات پیش‌فرض برای تسک‌های جدید
        /// </summary>
        private async Task CheckAndApplyDefaultSettingsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var settingsRepo = scope.ServiceProvider.GetRequiredService<TaskRepository>();

                _logger.LogInformation("🔍 Checking tasks without settings...");

                // دریافت تسک‌هایی که تنظیمات ندارند
                var tasksWithoutSettings = await context.Tasks_Tbl
                    .Where(t => !t.IsDeleted && 
                                !context.TaskSettings_Tbl.Any(s => s.TaskId == t.Id))
                    .Select(t => new { t.Id, t.BranchId, t.TaskCategoryId, t.CreatorUserId })
                    .ToListAsync();

                if (!tasksWithoutSettings.Any())
                {
                    _logger.LogInformation("✅ All tasks have settings");
                    return;
                }

                _logger.LogInformation($"📋 Found {tasksWithoutSettings.Count} tasks without settings");

                int processedCount = 0;

                foreach (var task in tasksWithoutSettings)
                {
                    try
                    {
                        // بررسی تنظیمات دسته‌بندی
                        TaskSettings? settings = null;

                        if (task.TaskCategoryId.HasValue)
                        {
                            var categorySettings = await settingsRepo.GetCategoryDefaultSettingsAsync(task.TaskCategoryId.Value);
                            if (categorySettings != null)
                            {
                                settings = categorySettings.ToTaskSettings(task.Id, "system");
                                _logger.LogDebug($"📌 Applying category settings to task {task.Id}");
                            }
                        }

                        // بررسی تنظیمات شعبه
                        if (settings == null && task.BranchId.HasValue)
                        {
                            var branchSettings = await settingsRepo.GetBranchDefaultSettingsAsync(task.BranchId.Value);
                            if (branchSettings != null)
                            {
                                settings = branchSettings.ToTaskSettings(task.Id, "system");
                                _logger.LogDebug($"📌 Applying branch settings to task {task.Id}");
                            }
                        }

                        // اعمال تنظیمات پیش‌فرض سیستم
                        if (settings == null)
                        {
                            settings = settingsRepo.GetGlobalDefaultSettings();
                            settings.TaskId = task.Id;
                            settings.CreatedByUserId = task.CreatorUserId;
                            _logger.LogDebug($"📌 Applying global settings to task {task.Id}");
                        }

                        // ذخیره
                        await settingsRepo.SaveTaskSettingsAsync(settings, "system");
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Error processing task {task.Id}");
                    }
                }

                _logger.LogInformation($"✅ Successfully processed {processedCount} tasks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in TaskSettingsBackgroundService");
            }
        }

        /// <summary>
        /// بررسی تنظیمات منقضی شده (Orphaned Settings)
        /// </summary>
        private async Task CleanupOrphanedSettingsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                _logger.LogInformation("🧹 Cleaning up orphaned settings...");

                // حذف تنظیماتی که تسک مربوطه حذف شده
                var orphanedSettings = await context.TaskSettings_Tbl
                    .Where(s => !context.Tasks_Tbl.Any(t => t.Id == s.TaskId) ||
                                context.Tasks_Tbl.Any(t => t.Id == s.TaskId && t.IsDeleted))
                    .ToListAsync();

                if (orphanedSettings.Any())
                {
                    context.TaskSettings_Tbl.RemoveRange(orphanedSettings);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Cleaned up {orphanedSettings.Count} orphaned settings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cleaning up orphaned settings");
            }
        }

        /// <summary>
        /// تولید گزارش آماری
        /// </summary>
        private async Task GenerateStatisticsReportAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var settingsRepo = scope.ServiceProvider.GetRequiredService<TaskRepository>();

                var stats = await settingsRepo.GetSettingsStatisticsAsync();

                _logger.LogInformation("📊 Task Settings Statistics:");
                _logger.LogInformation($"   Total Tasks: {stats.TotalTasks}");
                _logger.LogInformation($"   Custom Settings: {stats.TasksWithCustomSettings} ({stats.CustomSettingsPercentage:F1}%)");
                _logger.LogInformation($"   Inherited Settings: {stats.TasksWithInheritedSettings}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating statistics");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⏹️ TaskSettingsBackgroundService stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
