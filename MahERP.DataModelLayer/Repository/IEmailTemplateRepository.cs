using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;

namespace MahERP.DataModelLayer.Repository
{
    public interface IEmailTemplateRepository
    {
        // ========== ?????? ??????? ==========
        
        Task<List<EmailTemplate>> GetAllTemplatesAsync();
        Task<EmailTemplate> GetTemplateByIdAsync(int id);
        Task<EmailTemplate> GetTemplateWithDetailsAsync(int id);
        Task<int> CreateTemplateAsync(EmailTemplate template);
        Task UpdateTemplateAsync(EmailTemplate template);
        
        Task UpdateTemplateFieldsAsync(
            int id,
            string title,
            string subjectTemplate,
            string bodyHtml,
            string bodyPlainText,
            string description,
            byte category,
            bool isActive,
            string lastUpdaterUserId);
        
        Task DeleteTemplateAsync(int id);

        // ========== ?????? ??????? ==========
        
        Task<int> AddRecipientsAsync(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds,
            string addedByUserId);

        Task RemoveRecipientAsync(int recipientId);
        Task<List<EmailTemplateRecipient>> GetTemplateRecipientsAsync(int templateId);

        // ========== ????? ==========
        
        Task<List<object>> SearchContactsAsync(string search);
        Task<List<object>> SearchOrganizationsAsync(string search);

        // ========== ???????? ???? ==========
        
        Task<List<EmailTemplate>> GetActiveTemplatesAsync();
        Task<List<EmailTemplate>> GetTemplatesByCategoryAsync(byte category);
        Task IncrementUsageCountAsync(int templateId);
        Task<bool> TemplateExistsAsync(int id);
    }
}