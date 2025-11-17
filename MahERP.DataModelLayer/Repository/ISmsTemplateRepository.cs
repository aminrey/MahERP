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

        /// <summary>
        /// افزودن چند مخاطب با شماره‌های خاص
        /// </summary>
        Task<int> AddMultipleRecipientsWithPhonesAsync(
            int templateId,
            List<(int contactId, int? phoneId)> contactData,
            List<int> organizationIds,
            string addedByUserId);

        Task RemoveRecipientAsync(int recipientId);
        
        Task<List<SmsTemplateRecipient>> GetTemplateRecipientsAsync(int templateId);

        // ========== دریافت جزئیات کامل ==========
        
        Task<SmsTemplateDetailViewModel> GetTemplateDetailAsync(int templateId);

        // اضافه کنید:
        
        // ========== جستجو ==========
        
        /// <summary>
        /// جستجوی ساده Contacts (بدون شماره‌ها)
        /// </summary>
        Task<List<object>> SearchContactsSimpleAsync(string search);
        
        /// <summary>
        /// دریافت شماره‌های یک Contact
        /// </summary>
        Task<List<object>> GetContactPhonesAsync(int contactId);
        
        /// <summary>
        /// دریافت افراد یک Organization
        /// </summary>
        Task<List<object>> GetOrganizationContactsAsync(int organizationId);
        
        Task<List<object>> SearchContactsAsync(string search);
        Task<List<object>> SearchOrganizationsAsync(string search);
        Task IncrementUsageCountAsync(int templateId);
    }
}