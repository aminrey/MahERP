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
    /// Repository برای هدف (Goal)
    /// </summary>
    public class GoalRepository : IGoalRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GoalRepository> _logger;

        public GoalRepository(AppDbContext context, ILogger<GoalRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Goal?> GetByIdAsync(int id, bool includeInteractions = false)
        {
            var query = _context.Goal_Tbl
                .Include(g => g.Contact)
                .Include(g => g.Organization)
                .Include(g => g.CurrentLeadStageStatus)
                .AsQueryable();

            if (includeInteractions)
            {
                query = query.Include(g => g.InteractionGoals)
                    .ThenInclude(ig => ig.Interaction);
            }

            return await query.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Goal>> GetByContactAsync(int contactId, bool activeOnly = true)
        {
            var query = _context.Goal_Tbl
                .Include(g => g.CurrentLeadStageStatus)
                .Where(g => g.ContactId == contactId);

            if (activeOnly)
                query = query.Where(g => g.IsActive);

            return await query
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Goal>> GetByOrganizationAsync(int organizationId, bool activeOnly = true)
        {
            var query = _context.Goal_Tbl
                .Include(g => g.CurrentLeadStageStatus)
                .Where(g => g.OrganizationId == organizationId);

            if (activeOnly)
                query = query.Where(g => g.IsActive);

            return await query
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<Goal> CreateAsync(Goal goal)
        {
            try
            {
                if (!goal.ContactId.HasValue && !goal.OrganizationId.HasValue)
                    throw new ArgumentException("هدف باید به یک فرد یا سازمان متصل باشد");

                goal.CreatedDate = DateTime.Now;
                goal.IsActive = true;
                goal.IsConverted = false;

                _context.Goal_Tbl.Add(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("هدف جدید ایجاد شد: {Title}, ID: {Id}", goal.Title, goal.Id);

                return goal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد هدف");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Goal goal)
        {
            try
            {
                var existing = await _context.Goal_Tbl.FindAsync(goal.Id);
                if (existing == null)
                    return false;

                existing.Title = goal.Title;
                existing.Description = goal.Description;
                existing.ProductName = goal.ProductName;
                existing.EstimatedValue = goal.EstimatedValue;
                existing.IsActive = goal.IsActive;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = goal.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("هدف بروزرسانی شد: {Id}", goal.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی هدف: {Id}", goal.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var goal = await _context.Goal_Tbl.FindAsync(id);
                if (goal == null)
                    return false;

                // Soft delete
                goal.IsActive = false;
                goal.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("هدف غیرفعال شد: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف هدف: {Id}", id);
                return false;
            }
        }

        public async Task<bool> MarkAsConvertedAsync(int goalId, decimal? actualValue = null)
        {
            try
            {
                var goal = await _context.Goal_Tbl.FindAsync(goalId);
                if (goal == null)
                    return false;

                // دریافت وضعیت خرید
                var purchaseStage = await _context.LeadStageStatus_Tbl
                    .FirstOrDefaultAsync(s => s.StageType == LeadStageType.Purchase);

                goal.IsConverted = true;
                goal.ConversionDate = DateTime.Now;
                goal.ActualValue = actualValue;
                goal.CurrentLeadStageStatusId = purchaseStage?.Id;
                goal.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("هدف به خرید تبدیل شد: {Id}", goalId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تبدیل هدف: {Id}", goalId);
                return false;
            }
        }

        public async Task<bool> UpdateCurrentStageAsync(int goalId, int leadStageStatusId)
        {
            try
            {
                var goal = await _context.Goal_Tbl.FindAsync(goalId);
                if (goal == null)
                    return false;

                goal.CurrentLeadStageStatusId = leadStageStatusId;
                goal.LastUpdateDate = DateTime.Now;

                // بررسی اگر به مرحله خرید رسیده
                var stage = await _context.LeadStageStatus_Tbl.FindAsync(leadStageStatusId);
                if (stage?.StageType == LeadStageType.Purchase)
                {
                    goal.IsConverted = true;
                    goal.ConversionDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی وضعیت هدف: {Id}", goalId);
                return false;
            }
        }
    }
}
