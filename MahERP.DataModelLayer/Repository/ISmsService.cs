using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;

namespace MahERP.DataModelLayer.Repository
{
    public interface ISmsService
    {
        // ========== ارسال پیامک به افراد ==========
        
        Task<SmsLog> SendToContactAsync(
            int contactId,
            string message,
            string senderUserId,
            int? providerId = null);

        Task<List<SmsLog>> SendToMultipleContactsAsync(
            List<int> contactIds,
            string message,
            string senderUserId,
            int? providerId = null);

        // ========== ارسال پیامک به سازمان‌ها ==========
        
        Task<SmsLog> SendToOrganizationAsync(
            int organizationId,
            string message,
            string senderUserId,
            int? providerId = null);

        Task<List<SmsLog>> SendToOrganizationContactsAsync(
            int organizationId,
            string message,
            string senderUserId,
            int? providerId = null);

        // ========== ارسال با قالب ==========
        
        Task<SmsLog> SendWithTemplateToContactAsync(
            int contactId,
            int templateId,
            Dictionary<string, string> parameters,
            string senderUserId,
            int? providerId = null);

        // ========== جستجو و فیلتر ==========
        
        Task<List<SmsLog>> GetLogsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            byte? recipientType = null,
            bool? isSuccess = null,
            int? providerId = null,
            int pageNumber = 1,
            int pageSize = 20);

        Task<SmsStatistics> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // ========== بروزرسانی وضعیت ==========
        
        Task<bool> UpdateDeliveryStatusAsync(int logId);
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
   
}