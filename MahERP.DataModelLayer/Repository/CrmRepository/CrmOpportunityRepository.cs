using MahERP.CommonLayer.PublicClasses;
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
    /// Repository مدیریت فرصت‌های فروش
    /// </summary>
    public class CrmOpportunityRepository : ICrmOpportunityRepository
    {
        private readonly AppDbContext _context;
        private readonly ICrmPipelineStageRepository _stageRepo;
        private readonly ICrmLeadRepository _leadRepo;
        private readonly ILogger<CrmOpportunityRepository> _logger;

        public CrmOpportunityRepository(
            AppDbContext context,
            ICrmPipelineStageRepository stageRepo,
            ICrmLeadRepository leadRepo,
            ILogger<CrmOpportunityRepository> logger)
        {
            _context = context;
            _stageRepo = stageRepo;
            _leadRepo = leadRepo;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<CrmOpportunity?> GetByIdAsync(int id, bool includeDetails = false)
        {
            var query = _context.CrmOpportunity_Tbl.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(o => o.Contact)
                    .Include(o => o.Organization)
                    .Include(o => o.Stage)
                    .Include(o => o.Branch)
                    .Include(o => o.AssignedUser)
                    .Include(o => o.SourceLead)
                    .Include(o => o.Products.Where(p => p.IsActive))
                    .Include(o => o.Activities.OrderByDescending(a => a.ActivityDate).Take(10));
            }

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<CrmOpportunity> CreateAsync(CrmOpportunity opportunity)
        {
            // تنظیم مرحله پیش‌فرض
            if (opportunity.StageId == 0)
            {
                var defaultStage = await _stageRepo.GetDefaultStageAsync(opportunity.BranchId);
                if (defaultStage == null)
                {
                    await _stageRepo.EnsureDefaultStagesAsync(opportunity.BranchId, opportunity.CreatorUserId ?? "");
                    defaultStage = await _stageRepo.GetDefaultStageAsync(opportunity.BranchId);
                }
                opportunity.StageId = defaultStage?.Id ?? 0;
            }

            // تنظیم احتمال از مرحله
            var stage = await _stageRepo.GetByIdAsync(opportunity.StageId);
            opportunity.Probability = stage?.WinProbability ?? 0;

            // محاسبه ارزش وزنی
            if (opportunity.Value.HasValue)
            {
                opportunity.WeightedValue = opportunity.Value.Value * opportunity.Probability / 100;
            }

            opportunity.CreatedDate = DateTime.Now;

            _context.CrmOpportunity_Tbl.Add(opportunity);
            await _context.SaveChangesAsync();

            // ثبت فعالیت
            await LogActivityAsync(new CrmOpportunityActivity
            {
                OpportunityId = opportunity.Id,
                ActivityType = "Created",
                Title = "فرصت ایجاد شد",
                Description = $"فرصت «{opportunity.Title}» ایجاد شد",
                NewStageId = opportunity.StageId,
                UserId = opportunity.CreatorUserId
            });

            _logger.LogInformation("فرصت فروش ایجاد شد: {Id} - {Title}", opportunity.Id, opportunity.Title);

            return opportunity;
        }

        public async Task<bool> UpdateAsync(CrmOpportunity opportunity)
        {
            try
            {
                var existing = await _context.CrmOpportunity_Tbl.FindAsync(opportunity.Id);
                if (existing == null) return false;

                existing.Title = opportunity.Title;
                existing.Description = opportunity.Description;
                existing.Value = opportunity.Value;
                existing.Currency = opportunity.Currency;
                existing.Probability = opportunity.Probability;
                existing.ExpectedCloseDate = opportunity.ExpectedCloseDate;
                existing.Source = opportunity.Source;
                existing.Tags = opportunity.Tags;
                existing.Notes = opportunity.Notes;
                existing.NextActionType = opportunity.NextActionType;
                existing.NextActionDate = opportunity.NextActionDate;
                existing.NextActionNote = opportunity.NextActionNote;
                existing.NextActionTaskId = opportunity.NextActionTaskId;
                existing.AssignedUserId = opportunity.AssignedUserId;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = opportunity.LastUpdaterUserId;

                // محاسبه ارزش وزنی
                if (existing.Value.HasValue)
                {
                    existing.WeightedValue = existing.Value.Value * existing.Probability / 100;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی فرصت: {Id}", opportunity.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var opportunity = await _context.CrmOpportunity_Tbl.FindAsync(id);
                if (opportunity == null) return false;

                opportunity.IsActive = false;
                opportunity.LastUpdateDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف فرصت: {Id}", id);
                return false;
            }
        }

        // ========== List & Search ==========

        public async Task<(List<CrmOpportunity> Items, int TotalCount)> GetListAsync(
            CrmOpportunityFilterViewModel filter,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Include(o => o.AssignedUser)
                .AsQueryable();

            // فیلترها
            if (!filter.IncludeClosed)
            {
                query = query.Where(o => o.IsActive && !o.Stage.IsWonStage && !o.Stage.IsLostStage);
            }
            else
            {
                query = query.Where(o => o.IsActive);
            }

            if (filter.BranchId.HasValue)
                query = query.Where(o => o.BranchId == filter.BranchId.Value);

            if (filter.StageId.HasValue)
                query = query.Where(o => o.StageId == filter.StageId.Value);

            if (!string.IsNullOrEmpty(filter.AssignedUserId))
                query = query.Where(o => o.AssignedUserId == filter.AssignedUserId);

            if (!string.IsNullOrEmpty(filter.Source))
                query = query.Where(o => o.Source == filter.Source);

            if (filter.MinValue.HasValue)
                query = query.Where(o => o.Value >= filter.MinValue.Value);

            if (filter.MaxValue.HasValue)
                query = query.Where(o => o.Value <= filter.MaxValue.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedDate <= filter.ToDate.Value);

            if (filter.ExpectedCloseFrom.HasValue)
                query = query.Where(o => o.ExpectedCloseDate >= filter.ExpectedCloseFrom.Value);

            if (filter.ExpectedCloseTo.HasValue)
                query = query.Where(o => o.ExpectedCloseDate <= filter.ExpectedCloseTo.Value);

            if (filter.IsOverdue == true)
                query = query.Where(o => o.ExpectedCloseDate.HasValue && o.ExpectedCloseDate.Value < DateTime.Now);

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                var search = filter.SearchText.ToLower();
                query = query.Where(o =>
                    o.Title.ToLower().Contains(search) ||
                    (o.Contact != null && (o.Contact.FirstName.ToLower().Contains(search) || o.Contact.LastName.ToLower().Contains(search))) ||
                    (o.Organization != null && o.Organization.Name.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<CrmOpportunity>> GetByBranchAsync(int branchId, bool includeClosed = false)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Where(o => o.BranchId == branchId && o.IsActive);

            if (!includeClosed)
                query = query.Where(o => !o.Stage.IsWonStage && !o.Stage.IsLostStage);

            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<List<CrmOpportunity>> GetByUserAsync(string userId, int? branchId = null)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Where(o => o.AssignedUserId == userId && o.IsActive && !o.Stage.IsWonStage && !o.Stage.IsLostStage);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<List<CrmOpportunity>> GetByStageAsync(int stageId)
        {
            return await _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.AssignedUser)
                .Where(o => o.StageId == stageId && o.IsActive)
                .OrderBy(o => o.ExpectedCloseDate)
                .ToListAsync();
        }

        public async Task<List<CrmOpportunity>> SearchAsync(string searchText, int? branchId = null, int maxResults = 20)
        {
            var search = searchText.ToLower();

            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Where(o => o.IsActive &&
                    (o.Title.ToLower().Contains(search) ||
                    (o.Contact != null && (o.Contact.FirstName.ToLower().Contains(search) || o.Contact.LastName.ToLower().Contains(search))) ||
                    (o.Organization != null && o.Organization.Name.ToLower().Contains(search))));

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.Take(maxResults).ToListAsync();
        }

        // ========== Pipeline Board ==========

        public async Task<CrmPipelineBoardViewModel> GetPipelineBoardAsync(int branchId, CrmOpportunityFilterViewModel? filter = null)
        {
            // دریافت مراحل
            var stages = await _stageRepo.GetByBranchAsync(branchId);

            // دریافت فرصت‌های باز
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.AssignedUser)
                .Where(o => o.BranchId == branchId && o.IsActive);

            // فیلترها
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.AssignedUserId))
                    query = query.Where(o => o.AssignedUserId == filter.AssignedUserId);

                if (filter.MinValue.HasValue)
                    query = query.Where(o => o.Value >= filter.MinValue.Value);

                if (filter.MaxValue.HasValue)
                    query = query.Where(o => o.Value <= filter.MaxValue.Value);

                if (!filter.IncludeClosed)
                    query = query.Where(o => !o.Stage.IsWonStage && !o.Stage.IsLostStage);
            }
            else
            {
                query = query.Where(o => !o.Stage.IsWonStage && !o.Stage.IsLostStage);
            }

            var opportunities = await query.ToListAsync();

            // ساخت ستون‌ها
            var columns = stages.Select(stage => new CrmPipelineColumnViewModel
            {
                StageId = stage.Id,
                StageName = stage.Name,
                StageColor = stage.ColorCode,
                StageIcon = stage.Icon,
                WinProbability = stage.WinProbability,
                DisplayOrder = stage.DisplayOrder,
                IsWonStage = stage.IsWonStage,
                IsLostStage = stage.IsLostStage,
                Opportunities = opportunities
                    .Where(o => o.StageId == stage.Id)
                    .Select(o => MapToCard(o))
                    .OrderBy(c => c.ExpectedCloseDate)
                    .ToList(),
                TotalValue = opportunities
                    .Where(o => o.StageId == stage.Id)
                    .Sum(o => o.Value ?? 0)
            }).ToList();

            return new CrmPipelineBoardViewModel
            {
                BranchId = branchId,
                Columns = columns,
                Statistics = await GetStatisticsAsync(branchId, filter?.AssignedUserId),
                Filters = filter ?? new CrmOpportunityFilterViewModel()
            };
        }

        public async Task<CrmPipelineStatisticsViewModel> GetStatisticsAsync(int? branchId = null, string? userId = null)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Stage)
                .Where(o => o.IsActive);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.AssignedUserId == userId);

            var opportunities = await query.ToListAsync();

            var open = opportunities.Where(o => !o.Stage?.IsWonStage == true && !o.Stage?.IsLostStage == true).ToList();
            var won = opportunities.Where(o => o.Stage?.IsWonStage == true).ToList();
            var lost = opportunities.Where(o => o.Stage?.IsLostStage == true).ToList();

            var stats = new CrmPipelineStatisticsViewModel
            {
                TotalOpportunities = opportunities.Count,
                OpenOpportunities = open.Count,
                WonOpportunities = won.Count,
                LostOpportunities = lost.Count,
                TotalValue = open.Sum(o => o.Value ?? 0),
                TotalWeightedValue = open.Sum(o => o.WeightedValue ?? 0),
                WonValue = won.Sum(o => o.Value ?? 0),
                LostValue = lost.Sum(o => o.Value ?? 0),
                OverdueCount = open.Count(o => o.ExpectedCloseDate.HasValue && o.ExpectedCloseDate.Value < DateTime.Now),
                NeedsActionCount = open.Count(o => o.NextActionDate.HasValue && o.NextActionDate.Value.Date <= DateTime.Now.Date)
            };

            // نرخ برد
            var closedCount = won.Count + lost.Count;
            stats.WinRate = closedCount > 0 ? (decimal)won.Count / closedCount * 100 : 0;

            // میانگین اندازه معامله
            stats.AverageDealSize = won.Count > 0 ? won.Average(o => o.Value ?? 0) : 0;

            // میانگین روز تا بسته شدن
            var closedWithDates = won.Where(o => o.ActualCloseDate.HasValue).ToList();
            if (closedWithDates.Any())
            {
                stats.AverageDaysToClose = (int)closedWithDates.Average(o => (o.ActualCloseDate!.Value - o.CreatedDate).TotalDays);
            }

            // به تفکیک مرحله
            stats.ValueByStage = open.GroupBy(o => o.Stage?.Name ?? "نامشخص")
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Value ?? 0));
            stats.CountByStage = open.GroupBy(o => o.Stage?.Name ?? "نامشخص")
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        // ========== Stage Operations ==========

        public async Task<bool> MoveToStageAsync(int opportunityId, int newStageId, string userId, string? note = null)
        {
            try
            {
                var opportunity = await _context.CrmOpportunity_Tbl.FindAsync(opportunityId);
                if (opportunity == null) return false;

                var oldStageId = opportunity.StageId;
                var newStage = await _stageRepo.GetByIdAsync(newStageId);
                if (newStage == null) return false;

                opportunity.StageId = newStageId;
                opportunity.Probability = newStage.WinProbability;
                opportunity.LastUpdateDate = DateTime.Now;
                opportunity.LastUpdaterUserId = userId;

                // محاسبه مجدد ارزش وزنی
                if (opportunity.Value.HasValue)
                {
                    opportunity.WeightedValue = opportunity.Value.Value * opportunity.Probability / 100;
                }

                // اگر مرحله پایانی است
                if (newStage.IsWonStage || newStage.IsLostStage)
                {
                    opportunity.ActualCloseDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // ثبت فعالیت
                await LogActivityAsync(new CrmOpportunityActivity
                {
                    OpportunityId = opportunityId,
                    ActivityType = "StageChanged",
                    Title = $"تغییر مرحله به «{newStage.Name}»",
                    Description = note,
                    PreviousStageId = oldStageId,
                    NewStageId = newStageId,
                    UserId = userId
                });

                _logger.LogInformation("فرصت {Id} به مرحله {Stage} منتقل شد", opportunityId, newStage.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر مرحله فرصت: {Id}", opportunityId);
                return false;
            }
        }

        public async Task<bool> MarkAsWonAsync(int opportunityId, string userId, string? note = null)
        {
            var opportunity = await GetByIdAsync(opportunityId);
            if (opportunity == null) return false;

            var wonStage = await _stageRepo.GetWonStageAsync(opportunity.BranchId);
            if (wonStage == null)
            {
                _logger.LogWarning("مرحله برنده برای شعبه {BranchId} یافت نشد", opportunity.BranchId);
                return false;
            }

            return await MoveToStageAsync(opportunityId, wonStage.Id, userId, note);
        }

        public async Task<bool> MarkAsLostAsync(int opportunityId, string userId, string? lostReason = null, string? competitor = null)
        {
            var opportunity = await GetByIdAsync(opportunityId);
            if (opportunity == null) return false;

            var lostStage = await _stageRepo.GetLostStageAsync(opportunity.BranchId);
            if (lostStage == null)
            {
                _logger.LogWarning("مرحله باخت برای شعبه {BranchId} یافت نشد", opportunity.BranchId);
                return false;
            }

            opportunity.LostReason = lostReason;
            opportunity.WinningCompetitor = competitor;
            await _context.SaveChangesAsync();

            return await MoveToStageAsync(opportunityId, lostStage.Id, userId, lostReason);
        }

        public async Task<bool> ReopenAsync(int opportunityId, int stageId, string userId)
        {
            try
            {
                var opportunity = await _context.CrmOpportunity_Tbl.FindAsync(opportunityId);
                if (opportunity == null) return false;

                opportunity.ActualCloseDate = null;
                opportunity.LostReason = null;
                opportunity.WinningCompetitor = null;
                await _context.SaveChangesAsync();

                return await MoveToStageAsync(opportunityId, stageId, userId, "فرصت بازگشایی شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بازگشایی فرصت: {Id}", opportunityId);
                return false;
            }
        }

        // ========== Lead Conversion ==========

        public async Task<CrmOpportunity> CreateFromLeadAsync(int leadId, ConvertLeadToOpportunityViewModel model, string userId)
        {
            var lead = await _leadRepo.GetByIdAsync(leadId, includeDetails: true);
            if (lead == null)
                throw new InvalidOperationException("سرنخ یافت نشد");

            // ایجاد فرصت
            var opportunity = new CrmOpportunity
            {
                Title = model.Title,
                Description = model.Notes,
                BranchId = lead.BranchId,
                StageId = model.StageId ?? 0,
                SourceLeadId = leadId,
                ContactId = lead.ContactId,
                OrganizationId = lead.OrganizationId,
                AssignedUserId = lead.AssignedUserId,
                Value = model.Value ?? lead.EstimatedValue,
                Source = lead.Source,
                Tags = lead.Tags,
                NextActionType = model.NextActionType,
                NextActionNote = model.NextActionNote,
                CreatorUserId = userId
            };

            // تاریخ پیش‌بینی
            if (!string.IsNullOrEmpty(model.ExpectedCloseDatePersian))
            {
                opportunity.ExpectedCloseDate = ConvertDateTime.ConvertShamsiToMiladi(model.ExpectedCloseDatePersian);
            }

            // تاریخ اقدام بعدی
            if (!string.IsNullOrEmpty(model.NextActionDatePersian))
            {
                var date = ConvertDateTime.ConvertShamsiToMiladi(model.NextActionDatePersian);
                if (!string.IsNullOrEmpty(model.NextActionTime) && TimeSpan.TryParse(model.NextActionTime, out var time))
                {
                    date = date.Date.Add(time);
                }
                opportunity.NextActionDate = date;
            }

            var created = await CreateAsync(opportunity);

            // بستن سرنخ
            if (model.CloseLead)
            {
                lead.IsActive = false;
                lead.Notes = (lead.Notes ?? "") + $"\n\n[تبدیل به فرصت #{created.Id} در {DateTime.Now:yyyy/MM/dd}]";
                await _leadRepo.UpdateAsync(lead);
            }

            _logger.LogInformation("سرنخ {LeadId} به فرصت {OpportunityId} تبدیل شد", leadId, created.Id);

            return created;
        }

        public async Task<CrmOpportunity?> GetByLeadAsync(int leadId)
        {
            return await _context.CrmOpportunity_Tbl
                .Include(o => o.Stage)
                .FirstOrDefaultAsync(o => o.SourceLeadId == leadId && o.IsActive);
        }

        // ========== Products ==========

        public async Task<List<CrmOpportunityProduct>> GetProductsAsync(int opportunityId)
        {
            return await _context.CrmOpportunityProduct_Tbl
                .Where(p => p.OpportunityId == opportunityId && p.IsActive)
                .ToListAsync();
        }

        public async Task<CrmOpportunityProduct> AddProductAsync(CrmOpportunityProduct product)
        {
            // محاسبه مبلغ کل
            product.TotalAmount = product.Quantity * product.UnitPrice * (1 - product.DiscountPercent / 100);
            product.CreatedDate = DateTime.Now;

            _context.CrmOpportunityProduct_Tbl.Add(product);
            await _context.SaveChangesAsync();

            // بروزرسانی ارزش فرصت
            await RecalculateValueAsync(product.OpportunityId);

            return product;
        }

        public async Task<bool> UpdateProductAsync(CrmOpportunityProduct product)
        {
            try
            {
                var existing = await _context.CrmOpportunityProduct_Tbl.FindAsync(product.Id);
                if (existing == null) return false;

                existing.ProductName = product.ProductName;
                existing.Description = product.Description;
                existing.Quantity = product.Quantity;
                existing.UnitPrice = product.UnitPrice;
                existing.DiscountPercent = product.DiscountPercent;
                existing.TotalAmount = product.Quantity * product.UnitPrice * (1 - product.DiscountPercent / 100);

                await _context.SaveChangesAsync();
                await RecalculateValueAsync(existing.OpportunityId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی محصول: {Id}", product.Id);
                return false;
            }
        }

        public async Task<bool> RemoveProductAsync(int productId)
        {
            try
            {
                var product = await _context.CrmOpportunityProduct_Tbl.FindAsync(productId);
                if (product == null) return false;

                var opportunityId = product.OpportunityId;
                product.IsActive = false;

                await _context.SaveChangesAsync();
                await RecalculateValueAsync(opportunityId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف محصول: {Id}", productId);
                return false;
            }
        }

        public async Task RecalculateValueAsync(int opportunityId)
        {
            var opportunity = await _context.CrmOpportunity_Tbl.FindAsync(opportunityId);
            if (opportunity == null) return;

            var totalProducts = await _context.CrmOpportunityProduct_Tbl
                .Where(p => p.OpportunityId == opportunityId && p.IsActive)
                .SumAsync(p => p.TotalAmount);

            if (totalProducts > 0)
            {
                opportunity.Value = totalProducts;
                opportunity.WeightedValue = totalProducts * opportunity.Probability / 100;
                await _context.SaveChangesAsync();
            }
        }

        // ========== Activities ==========

        public async Task<List<CrmOpportunityActivity>> GetActivitiesAsync(int opportunityId, int maxResults = 20)
        {
            return await _context.CrmOpportunityActivity_Tbl
                .Include(a => a.User)
                .Where(a => a.OpportunityId == opportunityId)
                .OrderByDescending(a => a.ActivityDate)
                .Take(maxResults)
                .ToListAsync();
        }

        public async Task<CrmOpportunityActivity> LogActivityAsync(CrmOpportunityActivity activity)
        {
            activity.ActivityDate = DateTime.Now;
            activity.CreatedDate = DateTime.Now;

            _context.CrmOpportunityActivity_Tbl.Add(activity);
            await _context.SaveChangesAsync();

            return activity;
        }

        // ========== Utilities ==========

        public async Task UpdateProbabilityFromStageAsync(int opportunityId)
        {
            var opportunity = await _context.CrmOpportunity_Tbl
                .Include(o => o.Stage)
                .FirstOrDefaultAsync(o => o.Id == opportunityId);

            if (opportunity?.Stage != null)
            {
                opportunity.Probability = opportunity.Stage.WinProbability;
                if (opportunity.Value.HasValue)
                {
                    opportunity.WeightedValue = opportunity.Value.Value * opportunity.Probability / 100;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CrmOpportunity>> GetNeedingActionAsync(string? userId = null, int? branchId = null, int maxResults = 50)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Where(o => o.IsActive &&
                    !o.Stage.IsWonStage && !o.Stage.IsLostStage &&
                    o.NextActionDate.HasValue &&
                    o.NextActionDate.Value.Date <= DateTime.Now.Date);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.AssignedUserId == userId);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.OrderBy(o => o.NextActionDate).Take(maxResults).ToListAsync();
        }

        public async Task<List<CrmOpportunity>> GetOverdueAsync(string? userId = null, int? branchId = null)
        {
            var query = _context.CrmOpportunity_Tbl
                .Include(o => o.Contact)
                .Include(o => o.Organization)
                .Include(o => o.Stage)
                .Where(o => o.IsActive &&
                    !o.Stage.IsWonStage && !o.Stage.IsLostStage &&
                    o.ExpectedCloseDate.HasValue &&
                    o.ExpectedCloseDate.Value < DateTime.Now);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.AssignedUserId == userId);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.OrderBy(o => o.ExpectedCloseDate).ToListAsync();
        }

        // ========== Helpers ==========

        private CrmOpportunityCardViewModel MapToCard(CrmOpportunity o)
        {
            return new CrmOpportunityCardViewModel
            {
                Id = o.Id,
                Title = o.Title,
                CustomerName = o.CustomerName,
                CustomerType = o.CustomerType,
                ContactId = o.ContactId,
                OrganizationId = o.OrganizationId,
                Value = o.Value,
                ValueFormatted = o.ValueFormatted,
                Currency = o.Currency,
                Probability = o.Probability,
                WeightedValue = o.WeightedValue,
                AssignedUserId = o.AssignedUserId,
                AssignedUserName = o.AssignedUser != null ? $"{o.AssignedUser.FirstName} {o.AssignedUser.LastName}" : null,
                ExpectedCloseDate = o.ExpectedCloseDate,
                ExpectedCloseDatePersian = o.ExpectedCloseDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.ExpectedCloseDate.Value, "yyyy/MM/dd")
                    : null,
                DaysToClose = o.DaysToClose,
                NextActionType = o.NextActionType,
                NextActionDate = o.NextActionDate,
                NextActionDatePersian = o.NextActionDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.NextActionDate.Value, "MM/dd")
                    : null,
                Tags = o.TagsList,
                CreatedDate = o.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(o.CreatedDate, "yyyy/MM/dd")
            };
        }
    }
}
