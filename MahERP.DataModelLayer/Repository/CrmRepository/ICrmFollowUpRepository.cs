using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface برای مدیریت پیگیری‌های CRM
    /// </summary>
    public interface ICrmFollowUpRepository
    {
        // ========== CRUD ==========
        
        Task<CrmFollowUp?> GetByIdAsync(int id);
        Task<CrmFollowUp> CreateAsync(CrmFollowUp followUp);
        Task<bool> UpdateAsync(CrmFollowUp followUp);
        Task<bool> DeleteAsync(int id);
        
        // ========== List ==========
        
        Task<List<CrmFollowUp>> GetByLeadAsync(int leadId, bool pendingOnly = false);
        Task<List<CrmFollowUp>> GetByUserAsync(string userId, CrmFollowUpListViewModel filter);
        Task<List<CrmFollowUp>> GetPendingByUserAsync(string userId, int? branchId = null);
        Task<List<CrmFollowUp>> GetOverdueByUserAsync(string userId, int? branchId = null);
        Task<List<CrmFollowUp>> GetTodayByUserAsync(string userId, int? branchId = null);
        
        // ========== Status ==========
        
        Task<bool> UpdateStatusAsync(int id, byte status, string userId);
        Task<bool> CompleteAsync(int id, string? result, string userId);
        Task<bool> CancelAsync(int id, string userId);
        Task<bool> PostponeAsync(int id, DateTime newDueDate, string userId);
        
        // ========== Task Integration ==========
        
        Task<bool> ConvertToTaskAsync(int followUpId, int taskId, string userId);
        Task<CrmFollowUp?> GetByTaskIdAsync(int taskId);
        
        // ========== Reminder ==========
        
        Task<List<CrmFollowUp>> GetPendingRemindersAsync();
        Task<bool> MarkReminderSentAsync(int id);
        
        // ========== Statistics ==========
        
        Task<CrmFollowUpDashboardViewModel> GetDashboardAsync(string userId, int? branchId = null);
        Task<int> GetPendingCountByUserAsync(string userId);
        Task<int> GetOverdueCountByUserAsync(string userId);
    }
}
