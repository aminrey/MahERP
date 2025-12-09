using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// سرویس Background برای همگام‌سازی Permission ها از فایل JSON با دیتابیس
    /// 
    /// ⭐ ویژگی‌ها:
    /// - اجرای اولیه هنگام شروع برنامه (بعد از 10 ثانیه)
    /// - اجرای دوره‌ای هر 1 ساعت
    /// - فقط Permission های جدید اضافه می‌شوند (بدون حذف یا ویرایش موجودین)
    /// - پشتیبانی از ساختار سلسله‌مراتبی (Parent-Child)
    /// </summary>
    public class PermissionSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionSyncBackgroundService> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        
        // ⏱️ فاصله زمانی بین اجراها (1 ساعت)
        private readonly TimeSpan _syncInterval = TimeSpan.FromHours(1);
        
        // ⏳ تأخیر اولیه قبل از اولین اجرا (10 ثانیه)
        private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(10);

        public PermissionSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PermissionSyncBackgroundService> logger,
            IHostEnvironment hostEnvironment)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hostEnvironment = hostEnvironment;

            _logger.LogInformation("🔐 PermissionSyncBackgroundService CONSTRUCTED");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 PermissionSyncBackgroundService ExecuteAsync STARTED");

            try
            {
                // ⏳ تأخیر اولیه برای اطمینان از راه‌اندازی کامل سیستم
                _logger.LogInformation("⏳ Waiting {Delay} seconds before first permission sync...", _initialDelay.TotalSeconds);
                await Task.Delay(_initialDelay, stoppingToken);

                // 🔄 حلقه اصلی - اجرای دوره‌ای
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await SyncPermissionsAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error during permission sync: {Message}", ex.Message);
                    }

                    // ⏱️ صبر تا اجرای بعدی
                    _logger.LogInformation("⏱️ Next permission sync in {Hours} hour(s)...", _syncInterval.TotalHours);
                    await Task.Delay(_syncInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ PermissionSyncBackgroundService was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Fatal error in PermissionSyncBackgroundService: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// همگام‌سازی Permission ها از فایل JSON با دیتابیس
        /// </summary>
        private async Task SyncPermissionsAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Starting permission synchronization...");
            var startTime = DateTime.Now;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1️⃣ خواندن فایل JSON
            var jsonPath = Path.Combine(_hostEnvironment.ContentRootPath, "Data", "SeedData", "Permissions.json");
            
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("⚠️ Permissions.json not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath, stoppingToken);
            var permissionsFromJson = ParsePermissionsFromJson(jsonContent);

            if (permissionsFromJson == null || !permissionsFromJson.Any())
            {
                _logger.LogWarning("⚠️ No permissions found in JSON file");
                return;
            }

            _logger.LogInformation("📄 Found {Count} permissions in JSON file", permissionsFromJson.Count);

            // 2️⃣ دریافت Permission های موجود در دیتابیس
            var existingPermissions = await context.Permission_Tbl
                .ToDictionaryAsync(p => p.Code, p => p, stoppingToken);

            _logger.LogInformation("📊 Found {Count} existing permissions in database", existingPermissions.Count);

            // 3️⃣ پردازش Permission ها (ابتدا Root ها، سپس بچه‌ها)
            int newCount = 0;
            int skippedCount = 0;
            var processedCodes = new Dictionary<string, int>(); // Code -> Id

            // 🔹 گام 1: پردازش Permission های بدون والد (Root)
            var rootPermissions = permissionsFromJson.Where(p => string.IsNullOrEmpty(p.ParentCode)).ToList();
            foreach (var dto in rootPermissions)
            {
                if (existingPermissions.TryGetValue(dto.Code, out var existing))
                {
                    processedCodes[dto.Code] = existing.Id;
                    skippedCount++;
                }
                else
                {
                    var newPermission = CreatePermissionEntity(dto, null);
                    context.Permission_Tbl.Add(newPermission);
                    await context.SaveChangesAsync(stoppingToken);
                    
                    processedCodes[dto.Code] = newPermission.Id;
                    newCount++;
                    
                    _logger.LogInformation("➕ Added ROOT permission: {Code} - {NameFa}", dto.Code, dto.NameFa);
                }
            }

            // 🔹 گام 2: پردازش Permission های دارای والد (بازگشتی)
            var childPermissions = permissionsFromJson.Where(p => !string.IsNullOrEmpty(p.ParentCode)).ToList();
            int maxIterations = 10; // جلوگیری از حلقه بی‌نهایت
            int iteration = 0;

            while (childPermissions.Any() && iteration < maxIterations)
            {
                iteration++;
                var processedInThisRound = new List<PermissionJsonDto>();

                foreach (var dto in childPermissions)
                {
                    // آیا والد پردازش شده؟
                    int? parentId = null;
                    
                    if (processedCodes.TryGetValue(dto.ParentCode, out var foundParentId))
                    {
                        parentId = foundParentId;
                    }
                    else if (existingPermissions.TryGetValue(dto.ParentCode, out var existingParent))
                    {
                        parentId = existingParent.Id;
                        processedCodes[dto.ParentCode] = existingParent.Id;
                    }

                    if (parentId.HasValue)
                    {
                        if (existingPermissions.TryGetValue(dto.Code, out var existing))
                        {
                            processedCodes[dto.Code] = existing.Id;
                            skippedCount++;
                        }
                        else
                        {
                            var newPermission = CreatePermissionEntity(dto, parentId);
                            context.Permission_Tbl.Add(newPermission);
                            await context.SaveChangesAsync(stoppingToken);
                            
                            processedCodes[dto.Code] = newPermission.Id;
                            newCount++;
                            
                            _logger.LogInformation("➕ Added CHILD permission: {Code} - {NameFa} (Parent: {ParentCode})", 
                                dto.Code, dto.NameFa, dto.ParentCode);
                        }
                        
                        processedInThisRound.Add(dto);
                    }
                }

                // حذف پردازش‌شده‌ها از لیست
                childPermissions = childPermissions.Except(processedInThisRound).ToList();

                if (!processedInThisRound.Any())
                {
                    _logger.LogWarning("⚠️ Could not process {Count} permissions - parent not found", childPermissions.Count);
                    foreach (var orphan in childPermissions)
                    {
                        _logger.LogWarning("   - {Code} (ParentCode: {ParentCode})", orphan.Code, orphan.ParentCode);
                    }
                    break;
                }
            }

            var duration = DateTime.Now - startTime;
            _logger.LogInformation(
                "✅ Permission sync completed in {Duration:F2}s - New: {New}, Skipped: {Skipped}, Total in JSON: {Total}",
                duration.TotalSeconds, newCount, skippedCount, permissionsFromJson.Count);
        }

        /// <summary>
        /// ایجاد Entity از DTO
        /// </summary>
        private Permission CreatePermissionEntity(PermissionJsonDto dto, int? parentId)
        {
            return new Permission
            {
                Code = dto.Code,
                NameEn = dto.NameEn,
                NameFa = dto.NameFa,
                Description = dto.Description,
                Icon = dto.Icon ?? "fa fa-key",
                Color = dto.Color ?? "#007bff",
                ParentId = parentId,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                IsSystemPermission = dto.IsSystemPermission,
                CreateDate = DateTime.Now
            };
        }

        /// <summary>
        /// پارس کردن فایل JSON
        /// </summary>
        private List<PermissionJsonDto> ParsePermissionsFromJson(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonDoc = JsonDocument.Parse(jsonContent);
                var permissionsArray = jsonDoc.RootElement.GetProperty("Permissions");
                
                var permissions = new List<PermissionJsonDto>();
                
                foreach (var item in permissionsArray.EnumerateArray())
                {
                    permissions.Add(new PermissionJsonDto
                    {
                        Code = item.GetProperty("Code").GetString() ?? "",
                        NameEn = item.GetProperty("NameEn").GetString() ?? "",
                        NameFa = item.GetProperty("NameFa").GetString() ?? "",
                        Description = item.TryGetProperty("Description", out var desc) ? desc.GetString() : null,
                        Icon = item.TryGetProperty("Icon", out var icon) ? icon.GetString() : "fa fa-key",
                        Color = item.TryGetProperty("Color", out var color) ? color.GetString() : "#007bff",
                        ParentCode = item.TryGetProperty("ParentCode", out var parentCode) && parentCode.ValueKind != JsonValueKind.Null
                            ? parentCode.GetString()
                            : null,
                        DisplayOrder = item.TryGetProperty("DisplayOrder", out var order) ? order.GetInt32() : 100,
                        IsSystemPermission = item.TryGetProperty("IsSystemPermission", out var isSys) && isSys.GetBoolean()
                    });
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error parsing Permissions.json: {Message}", ex.Message);
                return new List<PermissionJsonDto>();
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("▶️ PermissionSyncBackgroundService StartAsync called");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏹️ PermissionSyncBackgroundService StopAsync called");
            return base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// DTO برای خواندن Permission از JSON
    /// </summary>
    internal class PermissionJsonDto
    {
        public string Code { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string NameFa { get; set; } = "";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string? ParentCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystemPermission { get; set; }
    }
}
