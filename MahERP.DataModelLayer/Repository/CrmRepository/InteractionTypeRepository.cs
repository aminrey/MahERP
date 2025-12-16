using MahERP.DataModelLayer.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای نوع تعامل (CRUD)
    /// </summary>
    public class InteractionTypeRepository : IInteractionTypeRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InteractionTypeRepository> _logger;

        public InteractionTypeRepository(AppDbContext context, ILogger<InteractionTypeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<InteractionType>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.InteractionType_Tbl
                .Include(t => t.LeadStageStatus)
                .AsQueryable();

            if (activeOnly)
                query = query.Where(t => t.IsActive);

            return await query
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<InteractionType?> GetByIdAsync(int id)
        {
            return await _context.InteractionType_Tbl
                .Include(t => t.LeadStageStatus)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<InteractionType> CreateAsync(InteractionType interactionType)
        {
            try
            {
                interactionType.CreatedDate = DateTime.Now;
                
                _context.InteractionType_Tbl.Add(interactionType);
                await _context.SaveChangesAsync();

                _logger.LogInformation("نوع تعامل جدید ایجاد شد: {Title}", interactionType.Title);

                return interactionType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد نوع تعامل");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(InteractionType interactionType)
        {
            try
            {
                var existing = await _context.InteractionType_Tbl.FindAsync(interactionType.Id);
                if (existing == null)
                    return false;

                existing.Title = interactionType.Title;
                existing.Description = interactionType.Description;
                existing.LeadStageStatusId = interactionType.LeadStageStatusId;
                existing.DisplayOrder = interactionType.DisplayOrder;
                existing.ColorCode = interactionType.ColorCode;
                existing.Icon = interactionType.Icon;
                existing.IsActive = interactionType.IsActive;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = interactionType.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("نوع تعامل بروزرسانی شد: {Id}", interactionType.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی نوع تعامل: {Id}", interactionType.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var interactionType = await _context.InteractionType_Tbl.FindAsync(id);
                if (interactionType == null)
                    return false;

                // Soft delete
                interactionType.IsActive = false;
                interactionType.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("نوع تعامل غیرفعال شد: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف نوع تعامل: {Id}", id);
                return false;
            }
        }

        public async Task<List<InteractionType>> GetByLeadStageStatusAsync(int leadStageStatusId)
        {
            return await _context.InteractionType_Tbl
                .Where(t => t.LeadStageStatusId == leadStageStatusId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }
    }
}
