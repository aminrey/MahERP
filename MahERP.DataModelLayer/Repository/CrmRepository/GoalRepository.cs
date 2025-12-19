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

        public async Task<(List<Goal> Goals, int TotalCount)> GetListAsync(
            GoalFilterViewModel filter, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Goal_Tbl
                    .Include(g => g.Contact)
                    .Include(g => g.Organization)
                    .Include(g => g.CurrentLeadStageStatus)
                    .Include(g => g.Creator)
                    .AsQueryable();

                // فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim().ToLower();
                    query = query.Where(g => 
                        g.Title.ToLower().Contains(searchTerm) ||
                        (g.ProductName != null && g.ProductName.ToLower().Contains(searchTerm)) ||
                        (g.Contact != null && (g.Contact.FirstName + " " + g.Contact.LastName).ToLower().Contains(searchTerm)) ||
                        (g.Organization != null && g.Organization.Name.ToLower().Contains(searchTerm))
                    );
                }

                // فیلتر وضعیت
                if (!string.IsNullOrEmpty(filter?.Status))
                {
                    if (filter.Status == "Active")
                        query = query.Where(g => g.IsActive && !g.IsConverted);
                    else if (filter.Status == "Converted")
                        query = query.Where(g => g.IsConverted);
                    else if (filter.Status == "Inactive")
                        query = query.Where(g => !g.IsActive);
                }
                else
                {
                    // پیش‌فرض: فقط اهداف فعال
                    query = query.Where(g => g.IsActive);
                }

                // فیلتر مرحله
                if (filter?.LeadStageStatusId.HasValue == true)
                {
                    query = query.Where(g => g.CurrentLeadStageStatusId == filter.LeadStageStatusId.Value);
                }

                // فیلتر تاریخ
                if (filter?.FromDate.HasValue == true)
                {
                    query = query.Where(g => g.CreatedDate >= filter.FromDate.Value);
                }
                if (filter?.ToDate.HasValue == true)
                {
                    query = query.Where(g => g.CreatedDate <= filter.ToDate.Value);
                }

                // تعداد کل
                var totalCount = await query.CountAsync();

                // صفحه‌بندی
                var goals = await query
                    .OrderByDescending(g => g.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (goals, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست اهداف");
                return (new List<Goal>(), 0);
            }
        }

        /// <summary>
        /// دریافت خلاصه اهداف یک Contact برای نمایش در Tab ها
        /// </summary>
        public async Task<List<GoalSummaryViewModel>> GetGoalSummariesForContactAsync(int contactId)
        {
            try
            {
                var goals = await _context.Goal_Tbl
                    .Include(g => g.CurrentLeadStageStatus)
                    .Include(g => g.InteractionGoals)
                    .Where(g => g.ContactId == contactId && g.IsActive)
                    .OrderByDescending(g => g.CreatedDate)
                    .Select(g => new GoalSummaryViewModel
                    {
                        Id = g.Id,
                        Title = g.Title,
                        ProductName = g.ProductName,
                        LeadStageTitle = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.Title : null,
                        LeadStageColor = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.ColorCode : "#6c757d",
                        IsConverted = g.IsConverted,
                        IsActive = g.IsActive,
                        InteractionsCount = g.InteractionGoals.Count
                    })
                    .ToListAsync();

                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت خلاصه اهداف Contact: {ContactId}", contactId);
                return new List<GoalSummaryViewModel>();
            }
        }

        /// <summary>
        /// دریافت خلاصه اهداف یک Organization برای نمایش در Tab ها
        /// </summary>
        public async Task<List<GoalSummaryViewModel>> GetGoalSummariesForOrganizationAsync(int organizationId)
        {
            try
            {
                var goals = await _context.Goal_Tbl
                    .Include(g => g.CurrentLeadStageStatus)
                    .Include(g => g.InteractionGoals)
                    .Where(g => g.OrganizationId == organizationId && g.IsActive)
                    .OrderByDescending(g => g.CreatedDate)
                    .Select(g => new GoalSummaryViewModel
                    {
                        Id = g.Id,
                        Title = g.Title,
                        ProductName = g.ProductName,
                        LeadStageTitle = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.Title : null,
                        LeadStageColor = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.ColorCode : "#6c757d",
                        IsConverted = g.IsConverted,
                        IsActive = g.IsActive,
                        InteractionsCount = g.InteractionGoals.Count
                    })
                    .ToListAsync();

                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت خلاصه اهداف Organization: {OrganizationId}", organizationId);
                return new List<GoalSummaryViewModel>();
            }
        }

        /// <summary>
        /// دریافت اهداف یک Contact با فیلتر شعبه
        /// </summary>
        public async Task<List<Goal>> GetByContactAndBranchAsync(int contactId, int? branchId = null, bool activeOnly = false)
        {
            try
            {
                var query = _context.Goal_Tbl
                    .Include(g => g.CurrentLeadStageStatus)
                    .Include(g => g.Contact)
                    .Where(g => g.ContactId == contactId);

                // فیلتر شعبه - از طریق BranchContact
                if (branchId.HasValue)
                {
                    var branchContactIds = await _context.BranchContact_Tbl
                        .Where(bc => bc.BranchId == branchId.Value && bc.ContactId == contactId)
                        .Select(bc => bc.ContactId)
                        .ToListAsync();

                    query = query.Where(g => branchContactIds.Contains(g.ContactId.Value));
                }

                if (activeOnly)
                    query = query.Where(g => g.IsActive);

                return await query
                    .OrderByDescending(g => g.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اهداف Contact: {ContactId}, Branch: {BranchId}", contactId, branchId);
                return new List<Goal>();
            }
        }

        /// <summary>
        /// دریافت اهداف یک Organization با فیلتر شعبه
        /// </summary>
        public async Task<List<Goal>> GetByOrganizationAndBranchAsync(int organizationId, int? branchId = null, bool activeOnly = false)
        {
            try
            {
                var query = _context.Goal_Tbl
                    .Include(g => g.CurrentLeadStageStatus)
                    .Include(g => g.Organization)
                    .Where(g => g.OrganizationId == organizationId);

                // فیلتر شعبه - از طریق BranchOrganization
                if (branchId.HasValue)
                {
                    var branchOrganizationIds = await _context.BranchOrganization_Tbl
                        .Where(bo => bo.BranchId == branchId.Value && bo.OrganizationId == organizationId)
                        .Select(bo => bo.OrganizationId)
                        .ToListAsync();

                    query = query.Where(g => branchOrganizationIds.Contains(g.OrganizationId.Value));
                }

                if (activeOnly)
                    query = query.Where(g => g.IsActive);

                return await query
                    .OrderByDescending(g => g.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اهداف Organization: {OrganizationId}, Branch: {BranchId}", organizationId, branchId);
                return new List<Goal>();
            }
        }

        /// <summary>
        /// دریافت 10 هدف اخیری که تغییر کرده‌اند (ایجاد شده یا وضعیت تغییر کرده)
        /// </summary>
        public async Task<List<RecentGoalItemViewModel>> GetRecentlyChangedGoalsAsync(int count = 10)
        {
            try
            {
                var goals = await _context.Goal_Tbl
                    .Include(g => g.Contact)
                    .Include(g => g.Organization)
                    .Include(g => g.CurrentLeadStageStatus)
                    .Where(g => g.IsActive)
                    .OrderByDescending(g => g.LastUpdateDate ?? g.CreatedDate)
                    .Take(count)
                    .Select(g => new RecentGoalItemViewModel
                    {
                        Id = g.Id,
                        Title = g.Title,
                        ProductName = g.ProductName,
                        TargetName = g.Contact != null ? g.Contact.FullName : (g.Organization != null ? g.Organization.Name : "نامشخص"),
                        TargetType = g.Contact != null ? "Contact" : "Organization",
                        CurrentLeadStageTitle = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.Title : null,
                        CurrentLeadStageColor = g.CurrentLeadStageStatus != null ? g.CurrentLeadStageStatus.ColorCode : "#6c757d",
                        IsConverted = g.IsConverted,
                        LastChangeDatePersian = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(g.LastUpdateDate ?? g.CreatedDate, "yyyy/MM/dd HH:mm")
                    })
                    .ToListAsync();

                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اهداف اخیر");
                return new List<RecentGoalItemViewModel>();
            }
        }
    }
}
