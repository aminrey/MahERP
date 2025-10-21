using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;
using System;

namespace MahERP.DataModelLayer.Repository
{
    public interface IEmailRepository
    {
        // ========== ارسال به Contact ==========

        /// <summary>
        /// ارسال ایمیل به یک شخص
        /// </summary>
        Task<EmailLog> SendToContactAsync(int contactId, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        /// <summary>
        /// ارسال ایمیل به چند شخص
        /// </summary>
        Task<List<EmailLog>> SendToMultipleContactsAsync(List<int> contactIds, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        // ========== ارسال به Organization ==========

        /// <summary>
        /// ارسال ایمیل به سازمان
        /// </summary>
        Task<EmailLog> SendToOrganizationAsync(int organizationId, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        /// <summary>
        /// ارسال به تمام افراد مرتبط با سازمان
        /// </summary>
        Task<List<EmailLog>> SendToOrganizationContactsAsync(int organizationId, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        // ========== ⭐ NEW: ارسال به گروه‌ها ==========

        /// <summary>
        /// ارسال ایمیل به یک گروه کامل (System Level)
        /// </summary>
        Task<EmailBulkResult> SendToContactGroupAsync(int groupId, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        /// <summary>
        /// ارسال ایمیل به گروه شعبه (Branch Level)
        /// </summary>
        Task<EmailBulkResult> SendToBranchContactGroupAsync(int branchGroupId, string subject, string body, string senderUserId, bool isHtml = true, List<string> attachmentPaths = null);

        // ========== قالب‌ها ==========

        /// <summary>
        /// دریافت تمام قالب‌های فعال
        /// </summary>
        Task<List<EmailTemplate>> GetActiveTemplatesAsync();

        /// <summary>
        /// ارسال با قالب به Contact
        /// </summary>
        Task<EmailLog> SendWithTemplateToContactAsync(int contactId, int templateId, Dictionary<string, string> parameters, string senderUserId, List<string> attachmentPaths = null);

        // ========== آمار ==========

        /// <summary>
        /// دریافت آمار ارسال ایمیل
        /// </summary>
        Task<EmailStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    // ========== ⭐ NEW: کلاس نتیجه ارسال دسته‌جمعی ==========

    /// <summary>
    /// نتیجه ارسال دسته‌جمعی Email
    /// </summary>
    public class EmailBulkResult
    {
        public bool Success { get; set; }
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<EmailLog> Logs { get; set; } = new();
        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        // اطلاعات گروه
        public string GroupTitle { get; set; }
        public int GroupId { get; set; }
        public string GroupType { get; set; } // "System" یا "Branch"
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