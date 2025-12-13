using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository مدیریت مراحل Pipeline
    /// </summary>
    public class CrmPipelineStageRepository : ICrmPipelineStageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CrmPipelineStageRepository> _logger;

        public CrmPipelineStageRepository(AppDbContext context, ILogger<CrmPipelineStageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CrmPipelineStage?> GetByIdAsync(int id)
        {
            return await _context.CrmPipelineStage_Tbl
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<CrmPipelineStage> CreateAsync(CrmPipelineStage stage)
        {
            // تنظیم ترتیب
            var maxOrder = await _context.CrmPipelineStage_Tbl
                .Where(s => s.BranchId == stage.BranchId)
                .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;
            stage.DisplayOrder = maxOrder + 1;

            stage.CreatedDate = DateTime.Now;
            _context.CrmPipelineStage_Tbl.Add(stage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("مرحله Pipeline ایجاد شد: {Name} در شعبه {BranchId}", stage.Name, stage.BranchId);
            return stage;
        }

        public async Task<bool> UpdateAsync(CrmPipelineStage stage)
        {
            try
            {
                var existing = await _context.CrmPipelineStage_Tbl.FindAsync(stage.Id);
                if (existing == null) return false;

                existing.Name = stage.Name;
                existing.Description = stage.Description;
                existing.ColorCode = stage.ColorCode;
                existing.Icon = stage.Icon;
                existing.WinProbability = stage.WinProbability;
                existing.IsWonStage = stage.IsWonStage;
                existing.IsLostStage = stage.IsLostStage;
                existing.IsDefault = stage.IsDefault;
                existing.IsActive = stage.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی مرحله Pipeline: {Id}", stage.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var stage = await _context.CrmPipelineStage_Tbl.FindAsync(id);
                if (stage == null) return false;

                // بررسی فرصت‌های مرتبط
                var hasOpportunities = await _context.CrmOpportunity_Tbl
                    .AnyAsync(o => o.StageId == id && o.IsActive);

                if (hasOpportunities)
                {
                    throw new InvalidOperationException("این مرحله دارای فرصت‌های فعال است و قابل حذف نیست");
                }

                stage.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("مرحله Pipeline غیرفعال شد: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف مرحله Pipeline: {Id}", id);
                throw;
            }
        }

        public async Task<List<CrmPipelineStage>> GetByBranchAsync(int branchId, bool includeInactive = false)
        {
            var query = _context.CrmPipelineStage_Tbl
                .Where(s => s.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CrmPipelineStage?> GetDefaultStageAsync(int branchId)
        {
            return await _context.CrmPipelineStage_Tbl
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.IsDefault && s.IsActive);
        }

        public async Task<CrmPipelineStage?> GetWonStageAsync(int branchId)
        {
            return await _context.CrmPipelineStage_Tbl
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.IsWonStage && s.IsActive);
        }

        public async Task<CrmPipelineStage?> GetLostStageAsync(int branchId)
        {
            return await _context.CrmPipelineStage_Tbl
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.IsLostStage && s.IsActive);
        }

        public async Task<bool> ReorderAsync(int branchId, List<int> stageIds)
        {
            try
            {
                var stages = await _context.CrmPipelineStage_Tbl
                    .Where(s => s.BranchId == branchId && stageIds.Contains(s.Id))
                    .ToListAsync();

                for (int i = 0; i < stageIds.Count; i++)
                {
                    var stage = stages.FirstOrDefault(s => s.Id == stageIds[i]);
                    if (stage != null)
                        stage.DisplayOrder = i + 1;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ترتیب‌بندی مراحل Pipeline");
                return false;
            }
        }

        /// <summary>
        /// ایجاد مراحل پیش‌فرض برای شعبه جدید
        /// </summary>
        public async Task EnsureDefaultStagesAsync(int branchId, string creatorUserId)
        {
            var hasStages = await _context.CrmPipelineStage_Tbl
                .AnyAsync(s => s.BranchId == branchId);

            if (hasStages) return;

            var defaultStages = new List<CrmPipelineStage>
            {
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "تماس اولیه",
                    Description = "اولین تماس با مشتری",
                    DisplayOrder = 1,
                    ColorCode = "#9e9e9e",
                    Icon = "fa-phone",
                    WinProbability = 10,
                    IsDefault = true,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "نیازسنجی",
                    Description = "شناسایی نیازهای مشتری",
                    DisplayOrder = 2,
                    ColorCode = "#2196f3",
                    Icon = "fa-search",
                    WinProbability = 20,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "ارائه پیشنهاد",
                    Description = "ارسال پیشنهاد قیمت",
                    DisplayOrder = 3,
                    ColorCode = "#ff9800",
                    Icon = "fa-file-invoice",
                    WinProbability = 40,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "مذاکره",
                    Description = "در حال مذاکره",
                    DisplayOrder = 4,
                    ColorCode = "#9c27b0",
                    Icon = "fa-handshake",
                    WinProbability = 60,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "بررسی نهایی",
                    Description = "در انتظار تصمیم نهایی",
                    DisplayOrder = 5,
                    ColorCode = "#00bcd4",
                    Icon = "fa-clock",
                    WinProbability = 80,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "برنده شده ✅",
                    Description = "فرصت موفق",
                    DisplayOrder = 6,
                    ColorCode = "#4caf50",
                    Icon = "fa-check-circle",
                    WinProbability = 100,
                    IsWonStage = true,
                    CreatorUserId = creatorUserId
                },
                new CrmPipelineStage
                {
                    BranchId = branchId,
                    Name = "از دست رفته ❌",
                    Description = "فرصت از دست رفته",
                    DisplayOrder = 7,
                    ColorCode = "#f44336",
                    Icon = "fa-times-circle",
                    WinProbability = 0,
                    IsLostStage = true,
                    CreatorUserId = creatorUserId
                }
            };

            _context.CrmPipelineStage_Tbl.AddRange(defaultStages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("مراحل پیش‌فرض Pipeline برای شعبه {BranchId} ایجاد شد", branchId);
        }
    }
}
