using MahERP.DataModelLayer.Entities.Contacts;
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
    /// Repository برای مدیریت سرنخ‌های CRM
    /// </summary>
    public class CrmLeadRepository : ICrmLeadRepository
    {
        private readonly AppDbContext _context;
        private readonly ICrmLeadStatusRepository _statusRepository;
        private readonly ILogger<CrmLeadRepository> _logger;

        public CrmLeadRepository(
            AppDbContext context,
            ICrmLeadStatusRepository statusRepository,
            ILogger<CrmLeadRepository> logger)
        {
            _context = context;
            _statusRepository = statusRepository;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<CrmLead?> GetByIdAsync(int id, bool includeDetails = false)
        {
            var query = _context.CrmLead_Tbl.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(l => l.Contact)
                        .ThenInclude(c => c.Phones)
                    .Include(l => l.Organization)
                    .Include(l => l.Branch)
                    .Include(l => l.AssignedUser)
                    .Include(l => l.Status)
                    .Include(l => l.Creator)
                    .Include(l => l.Interactions.OrderByDescending(i => i.InteractionDate).Take(5))
                    .Include(l => l.FollowUps.Where(f => f.Status == 0).OrderBy(f => f.DueDate));
            }

            return await query.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<CrmLead> CreateAsync(CrmLead lead)
        {
            // اعتبارسنجی: باید ContactId یا OrganizationId داشته باشد
            if (!lead.ContactId.HasValue && !lead.OrganizationId.HasValue)
            {
                throw new ArgumentException("سرنخ باید به یک فرد یا سازمان متصل باشد");
            }

            // اگر وضعیت مشخص نشده، وضعیت پیش‌فرض را بگذار
            if (lead.StatusId == 0)
            {
                var defaultStatus = await _statusRepository.GetDefaultStatusAsync();
                if (defaultStatus != null)
                {
                    lead.StatusId = defaultStatus.Id;
                }
                else
                {
                    throw new InvalidOperationException("وضعیت پیش‌فرض سرنخ تعریف نشده است");
                }
            }

            lead.CreatedDate = DateTime.Now;

            _context.CrmLead_Tbl.Add(lead);
            await _context.SaveChangesAsync();

            _logger.LogInformation("سرنخ جدید ایجاد شد: ID {Id}, Type: {Type}", 
                lead.Id, 
                lead.ContactId.HasValue ? "Contact" : "Organization");

            return lead;
        }

        public async Task<bool> UpdateAsync(CrmLead lead)
        {
            try
            {
                var existing = await _context.CrmLead_Tbl.FindAsync(lead.Id);
                if (existing == null)
                    return false;

                existing.StatusId = lead.StatusId;
                existing.AssignedUserId = lead.AssignedUserId;
                existing.Source = lead.Source;
                existing.Score = lead.Score;
                existing.Notes = lead.Notes;
                existing.Tags = lead.Tags;
                existing.NextFollowUpDate = lead.NextFollowUpDate;
                existing.EstimatedValue = lead.EstimatedValue;
                existing.IsActive = lead.IsActive;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = lead.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("سرنخ بروزرسانی شد: ID {Id}", lead.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی سرنخ: {Id}", lead.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var lead = await _context.CrmLead_Tbl.FindAsync(id);
                if (lead == null)
                    return false;

                // Soft Delete
                lead.IsActive = false;
                lead.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("سرنخ غیرفعال شد: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف سرنخ: {Id}", id);
                return false;
            }
        }

        // ========== List & Search ==========

        public async Task<(List<CrmLead> Leads, int TotalCount)> GetListAsync(
            CrmLeadFilterViewModel filter, 
            int pageNumber = 1, 
            int pageSize = 20)
        {
            var query = _context.CrmLead_Tbl
                .Include(l => l.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .AsQueryable();

            // فیلترها
            if (!filter.IncludeInactive)
                query = query.Where(l => l.IsActive);

            if (filter.BranchId.HasValue)
                query = query.Where(l => l.BranchId == filter.BranchId.Value);

            if (!string.IsNullOrEmpty(filter.AssignedUserId))
                query = query.Where(l => l.AssignedUserId == filter.AssignedUserId);

            if (filter.StatusId.HasValue)
                query = query.Where(l => l.StatusId == filter.StatusId.Value);

            if (!string.IsNullOrEmpty(filter.Source))
                query = query.Where(l => l.Source == filter.Source);

            if (!string.IsNullOrEmpty(filter.LeadType))
            {
                if (filter.LeadType == "Contact")
                    query = query.Where(l => l.ContactId.HasValue);
                else if (filter.LeadType == "Organization")
                    query = query.Where(l => l.OrganizationId.HasValue);
            }

            if (filter.FromDate.HasValue)
                query = query.Where(l => l.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(l => l.CreatedDate <= filter.ToDate.Value);

            if (filter.NeedsFollowUp == true)
                query = query.Where(l => l.NextFollowUpDate.HasValue && l.NextFollowUpDate.Value <= DateTime.Now);

            // جستجو
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                var searchText = filter.SearchText.ToLower();
                query = query.Where(l =>
                    (l.Contact != null && (
                        l.Contact.FirstName.ToLower().Contains(searchText) ||
                        l.Contact.LastName.ToLower().Contains(searchText) ||
                        (l.Contact.PrimaryEmail != null && l.Contact.PrimaryEmail.ToLower().Contains(searchText))
                    )) ||
                    (l.Organization != null && (
                        l.Organization.Name.ToLower().Contains(searchText) ||
                        (l.Organization.Brand != null && l.Organization.Brand.ToLower().Contains(searchText))
                    )) ||
                    (l.Notes != null && l.Notes.ToLower().Contains(searchText))
                );
            }

            var totalCount = await query.CountAsync();

            var leads = await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (leads, totalCount);
        }

        public async Task<List<CrmLead>> GetByBranchAsync(int branchId, bool includeInactive = false)
        {
            var query = _context.CrmLead_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .Where(l => l.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(l => l.IsActive);

            return await query
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<CrmLead>> GetByUserAsync(string userId, int? branchId = null)
        {
            var query = _context.CrmLead_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .Where(l => l.AssignedUserId == userId && l.IsActive);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            return await query
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<CrmLead>> SearchAsync(string searchText, int? branchId = null, int maxResults = 20)
        {
            var search = searchText.ToLower();

            var query = _context.CrmLead_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .Where(l => l.IsActive &&
                    ((l.Contact != null && (
                        l.Contact.FirstName.ToLower().Contains(search) ||
                        l.Contact.LastName.ToLower().Contains(search)
                    )) ||
                    (l.Organization != null && (
                        l.Organization.Name.ToLower().Contains(search)
                    ))));

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            return await query
                .Take(maxResults)
                .ToListAsync();
        }

        // ========== Status & Assignment ==========

        public async Task<bool> ChangeStatusAsync(int leadId, int newStatusId, string userId)
        {
            try
            {
                var lead = await _context.CrmLead_Tbl.FindAsync(leadId);
                if (lead == null)
                    return false;

                var oldStatusId = lead.StatusId;
                lead.StatusId = newStatusId;
                lead.LastUpdateDate = DateTime.Now;
                lead.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("وضعیت سرنخ تغییر کرد: ID {Id}, از {OldStatus} به {NewStatus}",
                    leadId, oldStatusId, newStatusId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت سرنخ: {Id}", leadId);
                return false;
            }
        }

        public async Task<bool> AssignToUserAsync(int leadId, string assignedUserId, string byUserId)
        {
            try
            {
                var lead = await _context.CrmLead_Tbl.FindAsync(leadId);
                if (lead == null)
                    return false;

                var oldUserId = lead.AssignedUserId;
                lead.AssignedUserId = assignedUserId;
                lead.LastUpdateDate = DateTime.Now;
                lead.LastUpdaterUserId = byUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("سرنخ تخصیص داده شد: ID {Id}, از {OldUser} به {NewUser}",
                    leadId, oldUserId, assignedUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تخصیص سرنخ: {Id}", leadId);
                return false;
            }
        }

        // ========== Contact/Organization Integration ==========

        public async Task<CrmLead?> GetByContactAsync(int contactId, int branchId)
        {
            return await _context.CrmLead_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(l => l.ContactId == contactId && l.BranchId == branchId && l.IsActive);
        }

        public async Task<CrmLead?> GetByOrganizationAsync(int organizationId, int branchId)
        {
            return await _context.CrmLead_Tbl
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(l => l.OrganizationId == organizationId && l.BranchId == branchId && l.IsActive);
        }

        public async Task<CrmLead> CreateFromContactAsync(int contactId, int branchId, string assignedUserId, string creatorUserId)
        {
            // بررسی وجود سرنخ قبلی
            var existing = await GetByContactAsync(contactId, branchId);
            if (existing != null)
            {
                throw new InvalidOperationException("سرنخ برای این فرد در این شعبه قبلاً ایجاد شده است");
            }

            var lead = new CrmLead
            {
                ContactId = contactId,
                BranchId = branchId,
                AssignedUserId = assignedUserId,
                CreatorUserId = creatorUserId
            };

            return await CreateAsync(lead);
        }

        public async Task<CrmLead> CreateFromOrganizationAsync(int organizationId, int branchId, string assignedUserId, string creatorUserId)
        {
            // بررسی وجود سرنخ قبلی
            var existing = await GetByOrganizationAsync(organizationId, branchId);
            if (existing != null)
            {
                throw new InvalidOperationException("سرنخ برای این سازمان در این شعبه قبلاً ایجاد شده است");
            }

            var lead = new CrmLead
            {
                OrganizationId = organizationId,
                BranchId = branchId,
                AssignedUserId = assignedUserId,
                CreatorUserId = creatorUserId
            };

            return await CreateAsync(lead);
        }

        // ========== ⭐⭐⭐ Quick Create (برای فرم سریع) ==========
        
        public async Task<int?> CreateQuickContactAndGetIdAsync(string? firstName, string? lastName, string? mobile, string? email, string creatorUserId)
        {
            try
            {
                var contact = new Contact
                {
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    PrimaryEmail = email,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = creatorUserId,
                    IsActive = true
                };
                
                _context.Contact_Tbl.Add(contact);
                await _context.SaveChangesAsync();
                
                // اضافه کردن شماره موبایل
                if (!string.IsNullOrWhiteSpace(mobile))
                {
                    var phone = new ContactPhone
                    {
                        ContactId = contact.Id,
                        PhoneNumber = mobile,
                        PhoneType = 1, // موبایل
                        IsDefault = true,
                        IsActive = true
                    };
                    _context.ContactPhone_Tbl.Add(phone);
                    await _context.SaveChangesAsync();
                }
                
                _logger.LogInformation("Contact سریع ایجاد شد: ID {Id}, نام: {Name}", contact.Id, contact.FullName);
                
                return contact.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد Contact سریع");
                throw;
            }
        }
        
        public async Task<int?> CreateQuickOrganizationAndGetIdAsync(string? name, string? phone, string creatorUserId)
        {
            try
            {
                var organization = new Organization
                {
                    Name = name ?? "",
                    PrimaryPhone = phone,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = creatorUserId,
                    IsActive = true
                };
                
                _context.Organization_Tbl.Add(organization);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Organization سریع ایجاد شد: ID {Id}, نام: {Name}", organization.Id, organization.DisplayName);
                
                return organization.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد Organization سریع");
                throw;
            }
        }

        // ========== Statistics ==========

        public async Task<CrmLeadStatisticsViewModel> GetStatisticsAsync(int? branchId = null, string? userId = null)
        {
            var query = _context.CrmLead_Tbl.Where(l => l.IsActive);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.AssignedUserId == userId);

            var allLeads = await query
                .Include(l => l.Status)
                .ToListAsync();

            var stats = new CrmLeadStatisticsViewModel
            {
                TotalLeads = allLeads.Count,
                NewLeads = allLeads.Count(l => l.Status?.IsDefault == true),
                InProgressLeads = allLeads.Count(l => !l.Status?.IsFinal == true && !l.Status?.IsDefault == true),
                ConvertedLeads = allLeads.Count(l => l.Status?.IsFinal == true && l.Status?.IsPositive == true),
                LostLeads = allLeads.Count(l => l.Status?.IsFinal == true && !l.Status?.IsPositive == true),
                NeedsFollowUpCount = allLeads.Count(l => l.NextFollowUpDate.HasValue && l.NextFollowUpDate.Value <= DateTime.Now),
                LeadsByStatus = allLeads.GroupBy(l => l.Status?.Title ?? "نامشخص")
                    .ToDictionary(g => g.Key, g => g.Count()),
                LeadsBySource = allLeads.Where(l => !string.IsNullOrEmpty(l.Source))
                    .GroupBy(l => l.Source)
                    .ToDictionary(g => g.Key!, g => g.Count())
            };

            // نرخ تبدیل
            var finalLeads = stats.ConvertedLeads + stats.LostLeads;
            stats.ConversionRate = finalLeads > 0 
                ? Math.Round((decimal)stats.ConvertedLeads / finalLeads * 100, 1) 
                : 0;

            // تعاملات امروز
            stats.TodayInteractionsCount = await _context.CrmLeadInteraction_Tbl
                .CountAsync(i => i.InteractionDate.Date == DateTime.Today && 
                    query.Any(l => l.Id == i.LeadId));

            return stats;
        }

        public async Task<List<CrmLead>> GetNeedingFollowUpAsync(string? userId = null, int? branchId = null, int maxResults = 50)
        {
            var query = _context.CrmLead_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .Where(l => l.IsActive && 
                    l.NextFollowUpDate.HasValue && 
                    l.NextFollowUpDate.Value <= DateTime.Now);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.AssignedUserId == userId);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            return await query
                .OrderBy(l => l.NextFollowUpDate)
                .Take(maxResults)
                .ToListAsync();
        }

        // ========== Utilities ==========

        public async Task UpdateLastContactDateAsync(int leadId, DateTime? date = null)
        {
            var lead = await _context.CrmLead_Tbl.FindAsync(leadId);
            if (lead != null)
            {
                lead.LastContactDate = date ?? DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateNextFollowUpDateAsync(int leadId, DateTime? nextDate = null)
        {
            var lead = await _context.CrmLead_Tbl.FindAsync(leadId);
            if (lead != null)
            {
                if (nextDate.HasValue)
                {
                    lead.NextFollowUpDate = nextDate;
                }
                else
                {
                    // محاسبه تاریخ پیگیری بعدی از پیگیری‌های در انتظار
                    var nextFollowUp = await _context.CrmFollowUp_Tbl
                        .Where(f => f.LeadId == leadId && f.Status == 0 && f.IsActive)
                        .OrderBy(f => f.DueDate)
                        .FirstOrDefaultAsync();

                    lead.NextFollowUpDate = nextFollowUp?.DueDate;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateScoreAsync(int leadId, int newScore, string userId)
        {
            try
            {
                var lead = await _context.CrmLead_Tbl.FindAsync(leadId);
                if (lead == null)
                    return false;

                lead.Score = Math.Clamp(newScore, 0, 100);
                lead.LastUpdateDate = DateTime.Now;
                lead.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی امتیاز سرنخ: {Id}", leadId);
                return false;
            }
        }
    }
}
