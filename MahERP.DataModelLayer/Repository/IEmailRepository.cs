using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;
using System;

namespace MahERP.DataModelLayer.Repository
{
    public interface IEmailRepository
    {
        // ========== ارسال به Contact ==========
        
        Task<EmailLog> SendToContactAsync(
            int contactId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null);

        Task<List<EmailLog>> SendToMultipleContactsAsync(
            List<int> contactIds,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null);

        // ========== ارسال به Organization ==========
        
        Task<EmailLog> SendToOrganizationAsync(
            int organizationId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null);

        Task<List<EmailLog>> SendToOrganizationContactsAsync(
            int organizationId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null);

        // ========== قالب‌ها ==========
        
        Task<List<EmailTemplate>> GetActiveTemplatesAsync();

        Task<EmailLog> SendWithTemplateToContactAsync(
            int contactId,
            int templateId,
            Dictionary<string, string> parameters,
            string senderUserId,
            List<string> attachmentPaths = null);

        // ========== آمار ==========
        
        Task<EmailStatistics> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }

    public class EmailStatistics
    {
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SentToContacts { get; set; }
        public int SentToOrganizations { get; set; }
        public int SentToUsers { get; set; }
        public double SuccessRate => TotalSent > 0 ? (SuccessCount * 100.0 / TotalSent) : 0;
    }
}