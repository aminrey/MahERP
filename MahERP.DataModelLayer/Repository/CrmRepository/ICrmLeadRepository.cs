using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface برای مدیریت سرنخ‌های CRM
    /// </summary>
    public interface ICrmLeadRepository
    {
        // ========== CRUD ==========
        
        /// <summary>
        /// دریافت سرنخ با شناسه
        /// </summary>
        Task<CrmLead?> GetByIdAsync(int id, bool includeDetails = false);
        
        /// <summary>
        /// ایجاد سرنخ جدید
        /// </summary>
        Task<CrmLead> CreateAsync(CrmLead lead);
        
        /// <summary>
        /// بروزرسانی سرنخ
        /// </summary>
        Task<bool> UpdateAsync(CrmLead lead);
        
        /// <summary>
        /// حذف سرنخ (Soft Delete)
        /// </summary>
        Task<bool> DeleteAsync(int id);
        
        // ========== List & Search ==========
        
        /// <summary>
        /// دریافت لیست سرنخ‌ها با فیلتر
        /// </summary>
        Task<(List<CrmLead> Leads, int TotalCount)> GetListAsync(CrmLeadFilterViewModel filter, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// دریافت سرنخ‌های یک شعبه
        /// </summary>
        Task<List<CrmLead>> GetByBranchAsync(int branchId, bool includeInactive = false);
        
        /// <summary>
        /// دریافت سرنخ‌های یک کاربر
        /// </summary>
        Task<List<CrmLead>> GetByUserAsync(string userId, int? branchId = null);
        
        /// <summary>
        /// جستجوی سرنخ‌ها
        /// </summary>
        Task<List<CrmLead>> SearchAsync(string searchText, int? branchId = null, int maxResults = 20);
        
        // ========== Status & Assignment ==========
        
        /// <summary>
        /// تغییر وضعیت سرنخ
        /// </summary>
        Task<bool> ChangeStatusAsync(int leadId, int newStatusId, string userId);
        
        /// <summary>
        /// تخصیص سرنخ به کاربر
        /// </summary>
        Task<bool> AssignToUserAsync(int leadId, string assignedUserId, string byUserId);
        
        // ========== Contact/Organization Integration ==========
        
        /// <summary>
        /// دریافت سرنخ بر اساس Contact
        /// </summary>
        Task<CrmLead?> GetByContactAsync(int contactId, int branchId);
        
        /// <summary>
        /// دریافت سرنخ بر اساس Organization
        /// </summary>
        Task<CrmLead?> GetByOrganizationAsync(int organizationId, int branchId);
        
        /// <summary>
        /// ایجاد سرنخ از Contact
        /// </summary>
        Task<CrmLead> CreateFromContactAsync(int contactId, int branchId, string assignedUserId, string creatorUserId);
        
        /// <summary>
        /// ایجاد سرنخ از Organization
        /// </summary>
        Task<CrmLead> CreateFromOrganizationAsync(int organizationId, int branchId, string assignedUserId, string creatorUserId);
        
        // ========== ⭐⭐⭐ Quick Create (برای فرم سریع) ==========
        
        /// <summary>
        /// ایجاد Contact سریع و برگرداندن شناسه
        /// </summary>
        Task<int?> CreateQuickContactAndGetIdAsync(string? firstName, string? lastName, string? mobile, string? email, string creatorUserId);
        
        /// <summary>
        /// ایجاد Organization سریع و برگرداندن شناسه
        /// </summary>
        Task<int?> CreateQuickOrganizationAndGetIdAsync(string? name, string? phone, string creatorUserId);
        
        // ========== Statistics ==========
        
        /// <summary>
        /// دریافت آمار سرنخ‌ها
        /// </summary>
        Task<CrmLeadStatisticsViewModel> GetStatisticsAsync(int? branchId = null, string? userId = null);
        
        /// <summary>
        /// دریافت سرنخ‌های نیازمند پیگیری
        /// </summary>
        Task<List<CrmLead>> GetNeedingFollowUpAsync(string? userId = null, int? branchId = null, int maxResults = 50);
        
        // ========== Utilities ==========
        
        /// <summary>
        /// بروزرسانی تاریخ آخرین تماس
        /// </summary>
        Task UpdateLastContactDateAsync(int leadId, DateTime? date = null);
        
        /// <summary>
        /// بروزرسانی تاریخ پیگیری بعدی
        /// </summary>
        Task UpdateNextFollowUpDateAsync(int leadId, DateTime? nextDate = null);
        
        /// <summary>
        /// افزایش امتیاز سرنخ
        /// </summary>
        Task<bool> UpdateScoreAsync(int leadId, int newScore, string userId);
    }
}
