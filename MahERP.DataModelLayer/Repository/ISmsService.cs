using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;

namespace MahERP.DataModelLayer.Repository
{
    public interface ISmsService
    {
        // ========== ارسال به افراد ==========

        /// <summary>
        /// ارسال پیامک به یک شخص (Contact)
        /// </summary>
        Task<SmsLog> SendToContactAsync(int contactId, string message, string senderUserId, int? providerId = null);

        /// <summary>
        /// ارسال پیامک به چند شخص
        /// </summary>
        Task<List<SmsLog>> SendToMultipleContactsAsync(List<int> contactIds, string message, string senderUserId, int? providerId = null);

        // ========== ارسال به سازمان‌ها ==========

        /// <summary>
        /// ارسال پیامک به شماره اصلی سازمان
        /// </summary>
        Task<SmsLog> SendToOrganizationAsync(int organizationId, string message, string senderUserId, int? providerId = null);

        /// <summary>
        /// ارسال پیامک به تمام افراد مرتبط با سازمان
        /// </summary>
        Task<List<SmsLog>> SendToOrganizationContactsAsync(int organizationId, string message, string senderUserId, int? providerId = null);

        // ========== ارسال با قالب ==========

        /// <summary>
        /// ارسال پیامک با استفاده از قالب
        /// </summary>
        Task<SmsLog> SendWithTemplateToContactAsync(int contactId, int templateId, Dictionary<string, string> parameters, string senderUserId, int? providerId = null);

        // ========== ⭐ NEW: ارسال به گروه‌ها ==========

        /// <summary>
        /// ارسال پیامک به یک گروه کامل (System Level)
        /// </summary>
        Task<SmsBulkResult> SendToContactGroupAsync(int groupId, string message, string senderUserId, int? providerId = null);

        /// <summary>
        /// ارسال پیامک به گروه شعبه (Branch Level)
        /// </summary>
        Task<SmsBulkResult> SendToBranchContactGroupAsync(int branchGroupId, string message, string senderUserId, int? providerId = null);

        // ========== جستجو و فیلتر ==========

        /// <summary>
        /// دریافت لاگ‌های پیامک با فیلتر
        /// </summary>
        Task<List<SmsLog>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, byte? recipientType = null, bool? isSuccess = null, int? providerId = null, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// دریافت آمار ارسال پیامک
        /// </summary>
        Task<SmsStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // ========== بروزرسانی وضعیت ==========

        /// <summary>
        /// بروزرسانی وضعیت تحویل پیامک
        /// </summary>
        Task<bool> UpdateDeliveryStatusAsync(int logId);

        /// <summary>
        /// بروزرسانی وضعیت پیامک‌های در انتظار
        /// </summary>
        Task<int> UpdatePendingDeliveriesAsync();
    }

    public class SmsStatistics
    {
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int PendingCount { get; set; }
        public int SentToContacts { get; set; }
        public int SentToOrganizations { get; set; }
        public int SentToUsers { get; set; }

        public double SuccessRate => TotalSent > 0 ? (SuccessCount * 100.0 / TotalSent) : 0;
        public double DeliveryRate => SuccessCount > 0 ? (DeliveredCount * 100.0 / SuccessCount) : 0;
    }

    /// <summary>
    /// نتیجه ارسال دسته‌جمعی SMS
    /// </summary>
    public class SmsBulkResult
    {
        public bool Success { get; set; }
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<SmsLog> Logs { get; set; } = new();
        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        // اطلاعات گروه
        public string GroupTitle { get; set; }
        public int GroupId { get; set; }
        public string GroupType { get; set; } // "System" یا "Branch"
    }
}