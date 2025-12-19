using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای وضعیت لید (جدول استاتیک)
    /// </summary>
    public class LeadStageStatusRepository : ILeadStageStatusRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeadStageStatusRepository> _logger;

        public LeadStageStatusRepository(AppDbContext context, ILogger<LeadStageStatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LeadStageStatus>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.LeadStageStatus_Tbl.AsQueryable();

            if (activeOnly)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<LeadStageStatus?> GetByIdAsync(int id)
        {
            return await _context.LeadStageStatus_Tbl
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<LeadStageStatus?> GetByStageTypeAsync(LeadStageType stageType)
        {
            return await _context.LeadStageStatus_Tbl
                .FirstOrDefaultAsync(s => s.StageType == stageType && s.IsActive);
        }

        public async Task<LeadStageStatus?> GetPurchaseStageAsync()
        {
            return await GetByStageTypeAsync(LeadStageType.Purchase);
        }

        public async Task<LeadStageStatus> CreateAsync(LeadStageStatus stage)
        {
            try
            {
                stage.IsActive = true;
                
                // تعیین DisplayOrder
                var maxOrder = await _context.LeadStageStatus_Tbl.MaxAsync(s => (int?)s.DisplayOrder) ?? 0;
                if (stage.DisplayOrder <= 0)
                    stage.DisplayOrder = maxOrder + 1;

                _context.LeadStageStatus_Tbl.Add(stage);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("LeadStageStatus created: {Title}", stage.Title);
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LeadStageStatus: {Title}", stage.Title);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(LeadStageStatus stage)
        {
            try
            {
                var existing = await _context.LeadStageStatus_Tbl.FindAsync(stage.Id);
                if (existing == null)
                    return false;

                existing.Title = stage.Title;
                existing.Description = stage.Description;
                existing.ColorCode = stage.ColorCode;
                existing.DisplayOrder = stage.DisplayOrder;
                existing.IsActive = stage.IsActive;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("LeadStageStatus updated: {Id}", stage.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LeadStageStatus: {Id}", stage.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var stage = await _context.LeadStageStatus_Tbl.FindAsync(id);
                if (stage == null)
                    return false;

                // Soft delete
                stage.IsActive = false;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("LeadStageStatus deleted (soft): {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LeadStageStatus: {Id}", id);
                return false;
            }
        }
    }

    /// <summary>
    /// Repository برای وضعیت بعد از خرید (جدول استاتیک)
    /// </summary>
    public class PostPurchaseStageRepository : IPostPurchaseStageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostPurchaseStageRepository> _logger;

        public PostPurchaseStageRepository(AppDbContext context, ILogger<PostPurchaseStageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PostPurchaseStage>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.PostPurchaseStage_Tbl.AsQueryable();

            if (activeOnly)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PostPurchaseStage?> GetByIdAsync(int id)
        {
            return await _context.PostPurchaseStage_Tbl
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<PostPurchaseStage?> GetByStageTypeAsync(PostPurchaseStageType stageType)
        {
            return await _context.PostPurchaseStage_Tbl
                .FirstOrDefaultAsync(s => s.StageType == stageType && s.IsActive);
        }

        public async Task<PostPurchaseStage?> GetReferralStageAsync()
        {
            return await GetByStageTypeAsync(PostPurchaseStageType.Referral);
        }

        public async Task<PostPurchaseStage> CreateAsync(PostPurchaseStage stage)
        {
            try
            {
                stage.IsActive = true;
                
                // تعیین DisplayOrder
                var maxOrder = await _context.PostPurchaseStage_Tbl.MaxAsync(s => (int?)s.DisplayOrder) ?? 0;
                if (stage.DisplayOrder <= 0)
                    stage.DisplayOrder = maxOrder + 1;

                _context.PostPurchaseStage_Tbl.Add(stage);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("PostPurchaseStage created: {Title}", stage.Title);
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PostPurchaseStage: {Title}", stage.Title);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(PostPurchaseStage stage)
        {
            try
            {
                var existing = await _context.PostPurchaseStage_Tbl.FindAsync(stage.Id);
                if (existing == null)
                    return false;

                existing.Title = stage.Title;
                existing.Description = stage.Description;
                existing.ColorCode = stage.ColorCode;
                existing.DisplayOrder = stage.DisplayOrder;
                existing.IsActive = stage.IsActive;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("PostPurchaseStage updated: {Id}", stage.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PostPurchaseStage: {Id}", stage.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var stage = await _context.PostPurchaseStage_Tbl.FindAsync(id);
                if (stage == null)
                    return false;

                // Soft delete
                stage.IsActive = false;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("PostPurchaseStage deleted (soft): {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PostPurchaseStage: {Id}", id);
                return false;
            }
        }
    }
}
