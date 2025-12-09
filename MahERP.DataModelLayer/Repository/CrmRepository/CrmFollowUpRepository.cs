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
    /// Repository برای مدیریت پیگیری‌های CRM
    /// </summary>
    public class CrmFollowUpRepository : ICrmFollowUpRepository
    {
        private readonly AppDbContext _context;
        private readonly ICrmLeadRepository _leadRepository;
        private readonly ILogger<CrmFollowUpRepository> _logger;

        public CrmFollowUpRepository(
            AppDbContext context,
            ICrmLeadRepository leadRepository,
            ILogger<CrmFollowUpRepository> logger)
        {
            _context = context;
            _leadRepository = leadRepository;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<CrmFollowUp?> GetByIdAsync(int id)
        {
            return await _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Include(f => f.Interaction)
                .Include(f => f.AssignedUser)
                .Include(f => f.Task)
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<CrmFollowUp> CreateAsync(CrmFollowUp followUp)
        {
            followUp.CreatedDate = DateTime.Now;

            // تنظیم یادآوری پیش‌فرض (1 روز قبل)
            if (followUp.HasReminder && !followUp.ReminderDate.HasValue)
            {
                followUp.ReminderDate = followUp.DueDate.AddDays(-1);
            }

            _context.CrmFollowUp_Tbl.Add(followUp);
            await _context.SaveChangesAsync();

            // بروزرسانی تاریخ پیگیری بعدی در سرنخ
            await UpdateLeadNextFollowUpDateAsync(followUp.LeadId);

            _logger.LogInformation("پیگیری جدید ایجاد شد: ID {Id}, Lead: {LeadId}", followUp.Id, followUp.LeadId);

            return followUp;
        }

        public async Task<bool> UpdateAsync(CrmFollowUp followUp)
        {
            try
            {
                var existing = await _context.CrmFollowUp_Tbl.FindAsync(followUp.Id);
                if (existing == null)
                    return false;

                existing.FollowUpType = followUp.FollowUpType;
                existing.Title = followUp.Title;
                existing.Description = followUp.Description;
                existing.DueDate = followUp.DueDate;
                existing.Priority = followUp.Priority;
                existing.HasReminder = followUp.HasReminder;
                existing.ReminderDate = followUp.ReminderDate;
                existing.AssignedUserId = followUp.AssignedUserId;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = followUp.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(existing.LeadId);

                _logger.LogInformation("پیگیری بروزرسانی شد: ID {Id}", followUp.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی پیگیری: {Id}", followUp.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                var leadId = followUp.LeadId;

                _context.CrmFollowUp_Tbl.Remove(followUp);
                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(leadId);

                _logger.LogInformation("پیگیری حذف شد: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف پیگیری: {Id}", id);
                return false;
            }
        }

        // ========== List ==========

        public async Task<List<CrmFollowUp>> GetByLeadAsync(int leadId, bool pendingOnly = false)
        {
            var query = _context.CrmFollowUp_Tbl
                .Include(f => f.AssignedUser)
                .Include(f => f.Task)
                .Where(f => f.LeadId == leadId);

            if (pendingOnly)
                query = query.Where(f => f.Status == 0);

            return await query
                .OrderBy(f => f.DueDate)
                .ToListAsync();
        }

        public async Task<List<CrmFollowUp>> GetByUserAsync(string userId, CrmFollowUpListViewModel filter)
        {
            var query = _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(f => f.AssignedUserId == userId);

            if (filter.LeadId.HasValue)
                query = query.Where(f => f.LeadId == filter.LeadId.Value);

            if (filter.StatusFilter.HasValue)
                query = query.Where(f => f.Status == filter.StatusFilter.Value);

            if (filter.PriorityFilter.HasValue)
                query = query.Where(f => f.Priority == filter.PriorityFilter.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(f => f.DueDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(f => f.DueDate <= filter.ToDate.Value);

            if (filter.OnlyOverdue)
                query = query.Where(f => f.Status == 0 && f.DueDate < DateTime.Now);

            if (filter.OnlyToday)
                query = query.Where(f => f.DueDate.Date == DateTime.Today);

            return await query
                .OrderBy(f => f.DueDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<List<CrmFollowUp>> GetPendingByUserAsync(string userId, int? branchId = null)
        {
            var query = _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(f => f.AssignedUserId == userId && f.Status == 0);

            if (branchId.HasValue)
                query = query.Where(f => f.Lead.BranchId == branchId.Value);

            return await query
                .OrderBy(f => f.DueDate)
                .ToListAsync();
        }

        public async Task<List<CrmFollowUp>> GetOverdueByUserAsync(string userId, int? branchId = null)
        {
            var query = _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(f => f.AssignedUserId == userId && f.Status == 0 && f.DueDate < DateTime.Now);

            if (branchId.HasValue)
                query = query.Where(f => f.Lead.BranchId == branchId.Value);

            return await query
                .OrderBy(f => f.DueDate)
                .ToListAsync();
        }

        public async Task<List<CrmFollowUp>> GetTodayByUserAsync(string userId, int? branchId = null)
        {
            var query = _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(f => f.AssignedUserId == userId && f.DueDate.Date == DateTime.Today && f.Status == 0);

            if (branchId.HasValue)
                query = query.Where(f => f.Lead.BranchId == branchId.Value);

            return await query
                .OrderBy(f => f.DueDate)
                .ToListAsync();
        }

        // ========== Status ==========

        public async Task<bool> UpdateStatusAsync(int id, byte status, string userId)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                followUp.Status = status;
                followUp.LastUpdateDate = DateTime.Now;
                followUp.LastUpdaterUserId = userId;

                if (status == 1) // انجام شده
                {
                    followUp.CompletedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(followUp.LeadId);

                _logger.LogInformation("وضعیت پیگیری تغییر کرد: ID {Id}, Status: {Status}", id, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت پیگیری: {Id}", id);
                return false;
            }
        }

        public async Task<bool> CompleteAsync(int id, string? result, string userId)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                followUp.Status = 1; // انجام شده
                followUp.CompletedDate = DateTime.Now;
                followUp.CompletionResult = result;
                followUp.LastUpdateDate = DateTime.Now;
                followUp.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(followUp.LeadId);

                _logger.LogInformation("پیگیری تکمیل شد: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تکمیل پیگیری: {Id}", id);
                return false;
            }
        }

        public async Task<bool> CancelAsync(int id, string userId)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                followUp.Status = 2; // لغو شده
                followUp.LastUpdateDate = DateTime.Now;
                followUp.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(followUp.LeadId);

                _logger.LogInformation("پیگیری لغو شد: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لغو پیگیری: {Id}", id);
                return false;
            }
        }

        public async Task<bool> PostponeAsync(int id, DateTime newDueDate, string userId)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                followUp.Status = 3; // به تعویق افتاده
                followUp.DueDate = newDueDate;
                
                // بروزرسانی یادآوری
                if (followUp.HasReminder)
                {
                    followUp.ReminderDate = newDueDate.AddDays(-1);
                    followUp.ReminderSent = false;
                }

                followUp.LastUpdateDate = DateTime.Now;
                followUp.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await UpdateLeadNextFollowUpDateAsync(followUp.LeadId);

                _logger.LogInformation("پیگیری به تعویق افتاد: ID {Id}, تاریخ جدید: {NewDate}", id, newDueDate);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تعویق پیگیری: {Id}", id);
                return false;
            }
        }

        // ========== Task Integration ==========

        public async Task<bool> ConvertToTaskAsync(int followUpId, int taskId, string userId)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(followUpId);
                if (followUp == null)
                    return false;

                followUp.TaskId = taskId;
                followUp.LastUpdateDate = DateTime.Now;
                followUp.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("پیگیری به تسک تبدیل شد: FollowUp {FollowUpId}, Task {TaskId}",
                    followUpId, taskId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تبدیل پیگیری به تسک: {FollowUpId}", followUpId);
                return false;
            }
        }

        public async Task<CrmFollowUp?> GetByTaskIdAsync(int taskId)
        {
            return await _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                .FirstOrDefaultAsync(f => f.TaskId == taskId);
        }

        // ========== Reminder ==========

        public async Task<List<CrmFollowUp>> GetPendingRemindersAsync()
        {
            return await _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Include(f => f.AssignedUser)
                .Where(f => f.HasReminder && 
                           !f.ReminderSent && 
                           f.ReminderDate.HasValue && 
                           f.ReminderDate.Value <= DateTime.Now &&
                           f.Status == 0)
                .ToListAsync();
        }

        public async Task<bool> MarkReminderSentAsync(int id)
        {
            try
            {
                var followUp = await _context.CrmFollowUp_Tbl.FindAsync(id);
                if (followUp == null)
                    return false;

                followUp.ReminderSent = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در علامت‌گذاری یادآوری: {Id}", id);
                return false;
            }
        }

        // ========== Statistics ==========

        public async Task<CrmFollowUpDashboardViewModel> GetDashboardAsync(string userId, int? branchId = null)
        {
            var baseQuery = _context.CrmFollowUp_Tbl
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Contact)
                .Include(f => f.Lead)
                    .ThenInclude(l => l.Organization)
                .Where(f => f.AssignedUserId == userId);

            if (branchId.HasValue)
                baseQuery = baseQuery.Where(f => f.Lead.BranchId == branchId.Value);

            var pending = await baseQuery.Where(f => f.Status == 0).ToListAsync();

            var overdue = pending.Where(f => f.DueDate < DateTime.Now).ToList();
            var today = pending.Where(f => f.DueDate.Date == DateTime.Today).ToList();
            var upcoming = pending.Where(f => f.DueDate.Date > DateTime.Today && f.DueDate.Date <= DateTime.Today.AddDays(7)).ToList();

            return new CrmFollowUpDashboardViewModel
            {
                OverdueFollowUps = overdue.Take(10).Select(MapToViewModel).ToList(),
                TodayFollowUps = today.Select(MapToViewModel).ToList(),
                UpcomingFollowUps = upcoming.Take(10).Select(MapToViewModel).ToList(),
                OverdueCount = overdue.Count,
                TodayCount = today.Count,
                ThisWeekCount = upcoming.Count,
                TotalPendingCount = pending.Count
            };
        }

        public async Task<int> GetPendingCountByUserAsync(string userId)
        {
            return await _context.CrmFollowUp_Tbl
                .CountAsync(f => f.AssignedUserId == userId && f.Status == 0);
        }

        public async Task<int> GetOverdueCountByUserAsync(string userId)
        {
            return await _context.CrmFollowUp_Tbl
                .CountAsync(f => f.AssignedUserId == userId && f.Status == 0 && f.DueDate < DateTime.Now);
        }

        // ========== Private Methods ==========

        private async Task UpdateLeadNextFollowUpDateAsync(int leadId)
        {
            var nextFollowUp = await _context.CrmFollowUp_Tbl
                .Where(f => f.LeadId == leadId && f.Status == 0)
                .OrderBy(f => f.DueDate)
                .FirstOrDefaultAsync();

            await _leadRepository.UpdateNextFollowUpDateAsync(leadId, nextFollowUp?.DueDate);
        }

        private CrmFollowUpViewModel MapToViewModel(CrmFollowUp followUp)
        {
            return new CrmFollowUpViewModel
            {
                Id = followUp.Id,
                LeadId = followUp.LeadId,
                LeadDisplayName = followUp.Lead?.DisplayName,
                FollowUpType = followUp.FollowUpType,
                FollowUpTypeText = followUp.FollowUpTypeText,
                FollowUpTypeIcon = followUp.FollowUpTypeIcon,
                Title = followUp.Title,
                Description = followUp.Description,
                DueDate = followUp.DueDate,
                Priority = followUp.Priority,
                PriorityText = followUp.PriorityText,
                PriorityColor = followUp.PriorityColor,
                Status = followUp.Status,
                StatusText = followUp.StatusText,
                StatusColor = followUp.StatusColor,
                StatusIcon = followUp.StatusIcon,
                IsOverdue = followUp.IsOverdue,
                IsDueToday = followUp.IsDueToday,
                DaysUntilDue = followUp.DaysUntilDue,
                DisplayTitle = followUp.DisplayTitle
            };
        }
    }
}
