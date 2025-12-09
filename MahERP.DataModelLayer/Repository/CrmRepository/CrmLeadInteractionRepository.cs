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
    /// Repository برای مدیریت تعاملات سرنخ CRM
    /// </summary>
    public class CrmLeadInteractionRepository : ICrmLeadInteractionRepository
    {
        private readonly AppDbContext _context;
        private readonly ICrmLeadRepository _leadRepository;
        private readonly ILogger<CrmLeadInteractionRepository> _logger;

        public CrmLeadInteractionRepository(
            AppDbContext context,
            ICrmLeadRepository leadRepository,
            ILogger<CrmLeadInteractionRepository> logger)
        {
            _context = context;
            _leadRepository = leadRepository;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<CrmLeadInteraction?> GetByIdAsync(int id)
        {
            return await _context.CrmLeadInteraction_Tbl
                .Include(i => i.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(i => i.Lead)
                    .ThenInclude(l => l.Organization)
                .Include(i => i.Creator)
                .Include(i => i.RelatedTask)
                .Include(i => i.FollowUps)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<CrmLeadInteraction> CreateAsync(CrmLeadInteraction interaction)
        {
            interaction.CreatedDate = DateTime.Now;

            _context.CrmLeadInteraction_Tbl.Add(interaction);
            await _context.SaveChangesAsync();

            // بروزرسانی تاریخ آخرین تماس در سرنخ
            await _leadRepository.UpdateLastContactDateAsync(interaction.LeadId);

            _logger.LogInformation("تعامل جدید ایجاد شد: ID {Id}, Lead: {LeadId}, Type: {Type}",
                interaction.Id, interaction.LeadId, interaction.InteractionType);

            return interaction;
        }

        public async Task<bool> UpdateAsync(CrmLeadInteraction interaction)
        {
            try
            {
                var existing = await _context.CrmLeadInteraction_Tbl.FindAsync(interaction.Id);
                if (existing == null)
                    return false;

                existing.InteractionType = interaction.InteractionType;
                existing.Direction = interaction.Direction;
                existing.Subject = interaction.Subject;
                existing.Description = interaction.Description;
                existing.Result = interaction.Result;
                existing.DurationMinutes = interaction.DurationMinutes;
                existing.PhoneNumber = interaction.PhoneNumber;
                existing.EmailAddress = interaction.EmailAddress;
                existing.InteractionDate = interaction.InteractionDate;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = interaction.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("تعامل بروزرسانی شد: ID {Id}", interaction.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی تعامل: {Id}", interaction.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var interaction = await _context.CrmLeadInteraction_Tbl.FindAsync(id);
                if (interaction == null)
                    return false;

                _context.CrmLeadInteraction_Tbl.Remove(interaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("تعامل حذف شد: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف تعامل: {Id}", id);
                return false;
            }
        }

        // ========== List ==========

        public async Task<List<CrmLeadInteraction>> GetByLeadAsync(int leadId)
        {
            return await _context.CrmLeadInteraction_Tbl
                .Include(i => i.Creator)
                .Include(i => i.RelatedTask)
                .Include(i => i.FollowUps)
                .Where(i => i.LeadId == leadId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();
        }

        public async Task<List<CrmLeadInteraction>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.CrmLeadInteraction_Tbl
                .Include(i => i.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(i => i.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(i => i.CreatorUserId == userId);

            if (fromDate.HasValue)
                query = query.Where(i => i.InteractionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.InteractionDate <= toDate.Value);

            return await query
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();
        }

        public async Task<List<CrmLeadInteraction>> GetRecentAsync(int leadId, int count = 10)
        {
            return await _context.CrmLeadInteraction_Tbl
                .Include(i => i.Creator)
                .Where(i => i.LeadId == leadId)
                .OrderByDescending(i => i.InteractionDate)
                .Take(count)
                .ToListAsync();
        }

        // ========== Statistics ==========

        public async Task<int> GetCountByLeadAsync(int leadId)
        {
            return await _context.CrmLeadInteraction_Tbl
                .CountAsync(i => i.LeadId == leadId);
        }

        public async Task<int> GetTodayCountByUserAsync(string userId)
        {
            return await _context.CrmLeadInteraction_Tbl
                .CountAsync(i => i.CreatorUserId == userId && i.InteractionDate.Date == DateTime.Today);
        }

        public async Task<Dictionary<byte, int>> GetCountByTypeAsync(int leadId)
        {
            return await _context.CrmLeadInteraction_Tbl
                .Where(i => i.LeadId == leadId)
                .GroupBy(i => i.InteractionType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        // ========== Task Integration ==========

        public async Task<bool> AttachTaskAsync(int interactionId, int taskId, string userId)
        {
            try
            {
                var interaction = await _context.CrmLeadInteraction_Tbl.FindAsync(interactionId);
                if (interaction == null)
                    return false;

                interaction.RelatedTaskId = taskId;
                interaction.LastUpdateDate = DateTime.Now;
                interaction.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("تسک به تعامل متصل شد: Interaction {InteractionId}, Task {TaskId}",
                    interactionId, taskId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اتصال تسک به تعامل: {InteractionId}", interactionId);
                return false;
            }
        }

        public async Task<bool> DetachTaskAsync(int interactionId, string userId)
        {
            try
            {
                var interaction = await _context.CrmLeadInteraction_Tbl.FindAsync(interactionId);
                if (interaction == null)
                    return false;

                interaction.RelatedTaskId = null;
                interaction.LastUpdateDate = DateTime.Now;
                interaction.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("تسک از تعامل جدا شد: Interaction {InteractionId}", interactionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در جدا کردن تسک از تعامل: {InteractionId}", interactionId);
                return false;
            }
        }
    }
}
