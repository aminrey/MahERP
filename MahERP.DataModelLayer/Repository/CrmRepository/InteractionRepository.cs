using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
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
    /// Repository برای تعامل (Interaction)
    /// </summary>
    public class InteractionRepository : IInteractionRepository
    {
        private readonly AppDbContext _context;
        private readonly IGoalRepository _goalRepository;
        private readonly ILogger<InteractionRepository> _logger;

        public InteractionRepository(
            AppDbContext context,
            IGoalRepository goalRepository,
            ILogger<InteractionRepository> logger)
        {
            _context = context;
            _goalRepository = goalRepository;
            _logger = logger;
        }

        public async Task<Interaction?> GetByIdAsync(int id, bool includeGoals = false)
        {
            var query = _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.Creator)
                .AsQueryable();

            if (includeGoals)
            {
                query = query.Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal);
            }

            return await query.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Interaction>> GetByContactAsync(int contactId, bool includeGoals = false)
        {
            var query = _context.Interaction_Tbl
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Where(i => i.ContactId == contactId && i.IsActive);

            if (includeGoals)
            {
                query = query.Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal);
            }

            return await query
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();
        }

        public async Task<(List<Interaction> Interactions, int TotalCount)> GetListAsync(
            InteractionFilterViewModel filter, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal)
                .AsQueryable();

            // Filters
            if (!filter.IncludeInactive)
                query = query.Where(i => i.IsActive);

            if (filter.ContactId.HasValue)
                query = query.Where(i => i.ContactId == filter.ContactId.Value);

            if (filter.InteractionTypeId.HasValue)
                query = query.Where(i => i.InteractionTypeId == filter.InteractionTypeId.Value);

            if (filter.PostPurchaseStageId.HasValue)
                query = query.Where(i => i.PostPurchaseStageId == filter.PostPurchaseStageId.Value);

            if (filter.GoalId.HasValue)
                query = query.Where(i => i.InteractionGoals.Any(ig => ig.GoalId == filter.GoalId.Value));

            if (filter.FromDate.HasValue)
                query = query.Where(i => i.InteractionDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.InteractionDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                var search = filter.SearchText.ToLower();
                query = query.Where(i =>
                    (i.Subject != null && i.Subject.ToLower().Contains(search)) ||
                    i.Description.ToLower().Contains(search) ||
                    (i.Result != null && i.Result.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var interactions = await query
                .OrderByDescending(i => i.InteractionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (interactions, totalCount);
        }

        public async Task<Interaction> CreateAsync(Interaction interaction, List<int>? goalIds = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                interaction.CreatedDate = DateTime.Now;
                interaction.IsActive = true;

                _context.Interaction_Tbl.Add(interaction);
                await _context.SaveChangesAsync();

                // افزودن اهداف (M:N)
                if (goalIds != null && goalIds.Any())
                {
                    foreach (var goalId in goalIds)
                    {
                        var interactionGoal = new InteractionGoal
                        {
                            InteractionId = interaction.Id,
                            GoalId = goalId
                        };
                        _context.InteractionGoal_Tbl.Add(interactionGoal);

                        // بروزرسانی وضعیت هدف بر اساس نوع تعامل
                        var interactionType = await _context.InteractionType_Tbl
                            .FirstOrDefaultAsync(t => t.Id == interaction.InteractionTypeId);
                        
                        if (interactionType != null)
                        {
                            await _goalRepository.UpdateCurrentStageAsync(goalId, interactionType.LeadStageStatusId);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // بروزرسانی ContactType اگر به مرحله خرید رسیده
                await UpdateContactTypeIfPurchaseAsync(interaction);

                await transaction.CommitAsync();

                _logger.LogInformation("تعامل جدید ایجاد شد: ContactId={ContactId}, ID={Id}", 
                    interaction.ContactId, interaction.Id);

                return interaction;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "خطا در ایجاد تعامل");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Interaction interaction, List<int>? goalIds = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.Interaction_Tbl
                    .Include(i => i.InteractionGoals)
                    .FirstOrDefaultAsync(i => i.Id == interaction.Id);

                if (existing == null)
                    return false;

                existing.InteractionTypeId = interaction.InteractionTypeId;
                existing.PostPurchaseStageId = interaction.PostPurchaseStageId;
                existing.Subject = interaction.Subject;
                existing.Description = interaction.Description;
                existing.InteractionDate = interaction.InteractionDate;
                existing.DurationMinutes = interaction.DurationMinutes;
                existing.Result = interaction.Result;
                existing.NextAction = interaction.NextAction;
                existing.NextActionDate = interaction.NextActionDate;
                existing.HasReferral = interaction.HasReferral;
                existing.IsReferred = interaction.IsReferred;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = interaction.LastUpdaterUserId;

                // بروزرسانی اهداف
                if (goalIds != null)
                {
                    // حذف اهداف قبلی
                    _context.InteractionGoal_Tbl.RemoveRange(existing.InteractionGoals);

                    // افزودن اهداف جدید
                    foreach (var goalId in goalIds)
                    {
                        _context.InteractionGoal_Tbl.Add(new InteractionGoal
                        {
                            InteractionId = interaction.Id,
                            GoalId = goalId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // بروزرسانی ContactType اگر به مرحله خرید رسیده
                await UpdateContactTypeIfPurchaseAsync(existing);

                await transaction.CommitAsync();

                _logger.LogInformation("تعامل بروزرسانی شد: {Id}", interaction.Id);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "خطا در بروزرسانی تعامل: {Id}", interaction.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var interaction = await _context.Interaction_Tbl.FindAsync(id);
                if (interaction == null)
                    return false;

                // Soft delete
                interaction.IsActive = false;
                interaction.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("تعامل غیرفعال شد: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف تعامل: {Id}", id);
                return false;
            }
        }

        public async Task<Interaction?> GetLastInteractionForContactAsync(int contactId)
        {
            return await _context.Interaction_Tbl
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Where(i => i.ContactId == contactId && i.IsActive)
                .OrderByDescending(i => i.InteractionDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetInteractionCountByContactAsync(int contactId)
        {
            return await _context.Interaction_Tbl
                .CountAsync(i => i.ContactId == contactId && i.IsActive);
        }

        /// <summary>
        /// بروزرسانی نوع Contact به Customer اگر تعامل به مرحله خرید رسیده
        /// </summary>
        private async Task UpdateContactTypeIfPurchaseAsync(Interaction interaction)
        {
            var interactionType = await _context.InteractionType_Tbl
                .Include(t => t.LeadStageStatus)
                .FirstOrDefaultAsync(t => t.Id == interaction.InteractionTypeId);

            if (interactionType?.LeadStageStatus?.StageType == LeadStageType.Purchase)
            {
                var contact = await _context.Contact_Tbl.FindAsync(interaction.ContactId);
                if (contact != null && contact.ContactType == ContactType.Lead)
                {
                    contact.ContactType = ContactType.Customer;
                    contact.LastUpdateDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Contact به Customer تبدیل شد: {ContactId}", interaction.ContactId);
                }
            }
        }
    }
}
