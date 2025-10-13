using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.ViewModels;

namespace MahERP.DataModelLayer.Repository
{
    public interface ISmsTemplateRepository
    {
        // ========== مدیریت قالب‌ها ==========
        
        Task<List<SmsTemplate>> GetAllTemplatesAsync();
        Task<SmsTemplate> GetTemplateByIdAsync(int id);
        Task<int> CreateTemplateAsync(SmsTemplate template);
        Task UpdateTemplateAsync(SmsTemplate template);
        Task DeleteTemplateAsync(int id);

        // ========== مدیریت مخاطبین قالب ==========
        
        Task<int> AddRecipientAsync(
            int templateId,
            byte recipientType,
            int? contactId,
            int? organizationId,
            string addedByUserId);

        Task<int> AddMultipleRecipientsAsync(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds,
            string addedByUserId);

        Task RemoveRecipientAsync(int recipientId);
        
        Task<List<SmsTemplateRecipient>> GetTemplateRecipientsAsync(int templateId);

        // ========== دریافت جزئیات کامل ==========
        
        Task<SmsTemplateDetailViewModel> GetTemplateDetailAsync(int templateId);

        // اضافه کنید:
        
        Task<List<object>> SearchContactsAsync(string search);
        Task<List<object>> SearchOrganizationsAsync(string search);
        Task IncrementUsageCountAsync(int templateId);
    }
}