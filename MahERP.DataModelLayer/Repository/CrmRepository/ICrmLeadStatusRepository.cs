using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface برای مدیریت وضعیت‌های سرنخ CRM
    /// </summary>
    public interface ICrmLeadStatusRepository
    {
        // ========== CRUD ==========
        
        /// <summary>
        /// دریافت تمام وضعیت‌ها
        /// </summary>
        Task<List<CrmLeadStatus>> GetAllAsync(bool includeInactive = false);
        
        /// <summary>
        /// دریافت وضعیت با شناسه
        /// </summary>
        Task<CrmLeadStatus?> GetByIdAsync(int id);
        
        /// <summary>
        /// ایجاد وضعیت جدید
        /// </summary>
        Task<CrmLeadStatus> CreateAsync(CrmLeadStatus status);
        
        /// <summary>
        /// بروزرسانی وضعیت
        /// </summary>
        Task<bool> UpdateAsync(CrmLeadStatus status);
        
        /// <summary>
        /// حذف وضعیت (Soft Delete)
        /// </summary>
        Task<bool> DeleteAsync(int id);
        
        // ========== Queries ==========
        
        /// <summary>
        /// دریافت وضعیت پیش‌فرض
        /// </summary>
        Task<CrmLeadStatus?> GetDefaultStatusAsync();
        
        /// <summary>
        /// دریافت وضعیت‌های نهایی
        /// </summary>
        Task<List<CrmLeadStatus>> GetFinalStatusesAsync();
        
        /// <summary>
        /// بررسی استفاده از وضعیت
        /// </summary>
        Task<bool> IsStatusInUseAsync(int statusId);
        
        /// <summary>
        /// دریافت تعداد سرنخ‌ها در هر وضعیت
        /// </summary>
        Task<Dictionary<int, int>> GetLeadsCountByStatusAsync(int? branchId = null);
        
        // ========== Utilities ==========
        
        /// <summary>
        /// تنظیم وضعیت پیش‌فرض
        /// </summary>
        Task<bool> SetAsDefaultAsync(int statusId, string userId);
        
        /// <summary>
        /// بررسی یکتا بودن عنوان
        /// </summary>
        Task<bool> IsTitleUniqueAsync(string title, int? excludeId = null);
    }
}
