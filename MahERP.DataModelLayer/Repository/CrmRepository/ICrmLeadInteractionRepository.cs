using MahERP.DataModelLayer.Entities.Crm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface برای مدیریت تعاملات سرنخ CRM
    /// </summary>
    public interface ICrmLeadInteractionRepository
    {
        // ========== CRUD ==========
        
        Task<CrmLeadInteraction?> GetByIdAsync(int id);
        Task<CrmLeadInteraction> CreateAsync(CrmLeadInteraction interaction);
        Task<bool> UpdateAsync(CrmLeadInteraction interaction);
        Task<bool> DeleteAsync(int id);
        
        // ========== List ==========
        
        Task<List<CrmLeadInteraction>> GetByLeadAsync(int leadId);
        Task<List<CrmLeadInteraction>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<CrmLeadInteraction>> GetRecentAsync(int leadId, int count = 10);
        
        // ========== Statistics ==========
        
        Task<int> GetCountByLeadAsync(int leadId);
        Task<int> GetTodayCountByUserAsync(string userId);
        Task<Dictionary<byte, int>> GetCountByTypeAsync(int leadId);
        
        // ========== Task Integration ==========
        
        Task<bool> AttachTaskAsync(int interactionId, int taskId, string userId);
        Task<bool> DetachTaskAsync(int interactionId, string userId);
    }
}
