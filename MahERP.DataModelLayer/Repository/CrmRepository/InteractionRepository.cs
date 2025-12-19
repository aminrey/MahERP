using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Extensions;
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
        /// دریافت تعاملات یک هدف خاص با صفحه‌بندی
        /// </summary>
        public async Task<(List<Interaction> Interactions, int TotalCount)> GetByGoalAsync(
            int goalId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.Creator)
                .Where(i => i.IsActive && i.InteractionGoals.Any(ig => ig.GoalId == goalId));

            // Apply filters
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var interactions = await query
                .OrderByDescending(i => i.InteractionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (interactions, totalCount);
        }

        /// <summary>
        /// دریافت تعاملات یک Contact با صفحه‌بندی
        /// </summary>
        public async Task<(List<Interaction> Interactions, int TotalCount)> GetByContactPagedAsync(
            int contactId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.Creator)
                .Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal)
                .Where(i => i.IsActive && i.ContactId == contactId);

            // Apply filters
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var interactions = await query
                .OrderByDescending(i => i.InteractionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (interactions, totalCount);
        }

        /// <summary>
        /// دریافت تعاملات یک Organization با صفحه‌بندی
        /// </summary>
        public async Task<(List<Interaction> Interactions, int TotalCount)> GetByOrganizationPagedAsync(
            int organizationId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10)
        {
            // دریافت تمام Contact های مرتبط با سازمان
            var contactIds = await _context.OrganizationContact_Tbl
                .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                .Select(oc => oc.ContactId)
                .Distinct()
                .ToListAsync();

            // همچنین اعضای چارت سازمانی
            var deptMemberContactIds = await _context.DepartmentMember_Tbl
                .Include(dm => dm.Department)
                .Where(dm => dm.Department.OrganizationId == organizationId && dm.IsActive)
                .Select(dm => dm.ContactId)
                .Distinct()
                .ToListAsync();

            var allContactIds = contactIds.Union(deptMemberContactIds).Distinct().ToList();

            var query = _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(t => t.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.Creator)
                .Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal)
                .Where(i => i.IsActive && allContactIds.Contains(i.ContactId));

            // Apply filters
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var interactions = await query
                .OrderByDescending(i => i.InteractionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (interactions, totalCount);
        }

        /// <summary>
        /// دریافت 10 تعامل اخیر کل سیستم
        /// </summary>
        public async Task<List<RecentInteractionItemViewModel>> GetRecentInteractionsAsync(int count = 10)
        {
            var interactions = await _context.Interaction_Tbl
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.InteractionDate)
                .Take(count)
                .Select(i => new RecentInteractionItemViewModel
                {
                    Id = i.Id,
                    Subject = i.Subject ?? "بدون موضوع",
                    ContactName = i.Contact != null ? i.Contact.FirstName + " " + i.Contact.LastName : "نامشخص",
                    InteractionTypeName = i.InteractionType != null ? i.InteractionType.Title : "نامشخص",
                    InteractionTypeColor = i.InteractionType != null ? i.InteractionType.ColorCode : "#6c757d",
                    InteractionTypeIcon = i.InteractionType != null ? i.InteractionType.Icon : "fa-comment",
                    DatePersian = "", // Will be set after query
                    TimeAgo = "" // Will be calculated after query
                })
                .ToListAsync();

            // محاسبه زمان‌های شمسی و TimeAgo
            var now = DateTime.Now;
            foreach (var item in interactions)
            {
                var interaction = await _context.Interaction_Tbl
                    .Where(i => i.Id == item.Id)
                    .Select(i => i.InteractionDate)
                    .FirstOrDefaultAsync();

                item.DatePersian = ConvertDateTime.ConvertMiladiToShamsi(interaction, "yyyy/MM/dd");
                item.TimeAgo = GetTimeAgo(interaction, now);
            }

            return interactions;
        }

        /// <summary>
        /// Apply filters to query
        /// </summary>
        private IQueryable<Interaction> ApplyFilters(IQueryable<Interaction> query, InteractionFilterViewModel? filter)
        {
            if (filter == null) return query;

            if (filter.InteractionTypeId.HasValue)
                query = query.Where(i => i.InteractionTypeId == filter.InteractionTypeId.Value);

            if (filter.PostPurchaseStageId.HasValue)
                query = query.Where(i => i.PostPurchaseStageId == filter.PostPurchaseStageId.Value);

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

            return query;
        }

        /// <summary>
        /// محاسبه "زمان پیش" به فارسی
        /// </summary>
        private string GetTimeAgo(DateTime date, DateTime now)
        {
            var diff = now - date;

            if (diff.TotalMinutes < 1)
                return "همین الان";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} دقیقه پیش";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} ساعت پیش";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} روز پیش";
            if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)} هفته پیش";
            if (diff.TotalDays < 365)
                return $"{(int)(diff.TotalDays / 30)} ماه پیش";

            return $"{(int)(diff.TotalDays / 365)} سال پیش";
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
