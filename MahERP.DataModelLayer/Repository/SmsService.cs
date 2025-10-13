using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Services.SmsProviders;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class SmsService : ISmsService
    {
        private readonly AppDbContext _context;
        private readonly SmsProviderRepository _providerRepo;

        public SmsService(AppDbContext context)
        {
            _context = context;
            _providerRepo = new SmsProviderRepository(context);
        }

        // ========== ارسال پیامک به افراد ==========

        /// <summary>
        /// ارسال پیامک به یک شخص (Contact)
        /// </summary>
        public async Task<SmsLog> SendToContactAsync(
            int contactId,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var contact = await _context.Contact_Tbl
                .Include(c => c.Phones)
                .FirstOrDefaultAsync(c => c.Id == contactId);

            if (contact == null)
                throw new ArgumentException("شخص یافت نشد");

            var defaultPhone = contact.DefaultPhone;
            if (defaultPhone == null || string.IsNullOrEmpty(defaultPhone.PhoneNumber))
                throw new InvalidOperationException("شماره تماس پیش‌فرض یافت نشد");

            // دریافت Provider
            var provider = providerId.HasValue
                ? _context.SmsProvider_Tbl.Find(providerId.Value)
                : _providerRepo.GetDefaultProvider();

            if (provider == null)
                throw new InvalidOperationException("خدمات‌دهنده پیامک یافت نشد");

            // ایجاد Instance
            var smsProvider = _providerRepo.CreateProviderInstance(provider);

            // ارسال پیامک
            var result = await smsProvider.SendSingleAsync(defaultPhone.PhoneNumber, message);

            // ثبت لاگ
            var log = new SmsLog
            {
                ProviderId = provider.Id,
                PhoneNumber = defaultPhone.PhoneNumber,
                MessageText = message,
                RecipientType = 0, // شخص
                ContactId = contactId,
                ProviderMessageId = result.MessageId,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                DeliveryStatus = result.IsSuccess ? 1 : 3,
                SendDate = DateTime.Now,
                SenderUserId = senderUserId
            };

            _context.SmsLog_Tbl.Add(log);
            await _context.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// ارسال پیامک به چند شخص
        /// </summary>
        public async Task<List<SmsLog>> SendToMultipleContactsAsync(
            List<int> contactIds,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var logs = new List<SmsLog>();

            foreach (var contactId in contactIds)
            {
                try
                {
                    var log = await SendToContactAsync(contactId, message, senderUserId, providerId);
                    logs.Add(log);

                    // تاخیر کوتاه
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    // ثبت خطا
                    Console.WriteLine($"خطا در ارسال به Contact {contactId}: {ex.Message}");
                }
            }

            return logs;
        }

        // ========== ارسال پیامک به سازمان‌ها ==========

        /// <summary>
        /// ارسال پیامک به شماره اصلی سازمان
        /// </summary>
        public async Task<SmsLog> SendToOrganizationAsync(
            int organizationId,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var organization = await _context.Organization_Tbl
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
                throw new ArgumentException("سازمان یافت نشد");

            if (string.IsNullOrEmpty(organization.PrimaryPhone))
                throw new InvalidOperationException("شماره تلفن سازمان یافت نشد");

            // دریافت Provider
            var provider = providerId.HasValue
                ? _context.SmsProvider_Tbl.Find(providerId.Value)
                : _providerRepo.GetDefaultProvider();

            if (provider == null)
                throw new InvalidOperationException("خدمات‌دهنده پیامک یافت نشد");

            var smsProvider = _providerRepo.CreateProviderInstance(provider);

            // ارسال پیامک
            var result = await smsProvider.SendSingleAsync(organization.PrimaryPhone, message);

            // ثبت لاگ
            var log = new SmsLog
            {
                ProviderId = provider.Id,
                PhoneNumber = organization.PrimaryPhone,
                MessageText = message,
                RecipientType = 1, // سازمان
                OrganizationId = organizationId,
                ProviderMessageId = result.MessageId,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                DeliveryStatus = result.IsSuccess ? 1 : 3,
                SendDate = DateTime.Now,
                SenderUserId = senderUserId
            };

            _context.SmsLog_Tbl.Add(log);
            await _context.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// ارسال پیامک به تمام افراد مرتبط با سازمان
        /// </summary>
        public async Task<List<SmsLog>> SendToOrganizationContactsAsync(
            int organizationId,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var organizationContacts = await _context.OrganizationContact_Tbl
                .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                .Select(oc => oc.ContactId)
                .ToListAsync();

            return await SendToMultipleContactsAsync(organizationContacts, message, senderUserId, providerId);
        }

        // ========== ارسال با قالب ==========

        /// <summary>
        /// ارسال پیامک با استفاده از قالب
        /// </summary>
        public async Task<SmsLog> SendWithTemplateToContactAsync(
            int contactId,
            int templateId,
            Dictionary<string, string> parameters,
            string senderUserId,
            int? providerId = null)
        {
            var template = await _context.SmsTemplate_Tbl.FindAsync(templateId);
            if (template == null)
                throw new ArgumentException("قالب یافت نشد");

            // جایگزینی پارامترها
            string message = template.MessageTemplate;
            foreach (var param in parameters)
            {
                message = message.Replace($"{{{param.Key}}}", param.Value);
            }

            // بروزرسانی تعداد استفاده
            template.UsageCount++;
            await _context.SaveChangesAsync();

            return await SendToContactAsync(contactId, message, senderUserId, providerId);
        }

        // ========== جستجو و فیلتر ==========

        /// <summary>
        /// دریافت لاگ‌های پیامک با فیلتر
        /// </summary>
        public async Task<List<SmsLog>> GetLogsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            byte? recipientType = null,
            bool? isSuccess = null,
            int? providerId = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            IQueryable<SmsLog> query = _context.SmsLog_Tbl
                .Include(l => l.Provider)
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Sender);

            if (fromDate.HasValue)
                query = query.Where(l => l.SendDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.SendDate <= toDate.Value);

            if (recipientType.HasValue)
                query = query.Where(l => l.RecipientType == recipientType.Value);

            if (isSuccess.HasValue)
                query = query.Where(l => l.IsSuccess == isSuccess.Value);

            if (providerId.HasValue)
                query = query.Where(l => l.ProviderId == providerId.Value);

            return await query
                .OrderByDescending(l => l.SendDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت آمار ارسال پیامک
        /// </summary>
        public async Task<SmsStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<SmsLog> query = _context.SmsLog_Tbl;

            if (fromDate.HasValue)
                query = query.Where(l => l.SendDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.SendDate <= toDate.Value);

            return new SmsStatistics
            {
                TotalSent = await query.CountAsync(),
                SuccessCount = await query.CountAsync(l => l.IsSuccess),
                FailedCount = await query.CountAsync(l => !l.IsSuccess),
                DeliveredCount = await query.CountAsync(l => l.IsDelivered),
                PendingCount = await query.CountAsync(l => !l.IsDelivered && l.IsSuccess),

                SentToContacts = await query.CountAsync(l => l.RecipientType == 0),
                SentToOrganizations = await query.CountAsync(l => l.RecipientType == 1),
                SentToUsers = await query.CountAsync(l => l.RecipientType == 2)
            };
        }

        // ========== بروزرسانی وضعیت ==========

        /// <summary>
        /// بروزرسانی وضعیت تحویل پیامک
        /// </summary>
        public async Task<bool> UpdateDeliveryStatusAsync(int logId)
        {
            var log = await _context.SmsLog_Tbl
                .Include(l => l.Provider)
                .FirstOrDefaultAsync(l => l.Id == logId);

            if (log == null || !log.ProviderMessageId.HasValue)
                return false;

            try
            {
                var smsProvider = _providerRepo.CreateProviderInstance(log.Provider);
                var statusResult = await smsProvider.GetMessageStatusAsync(log.ProviderMessageId.Value);

                log.DeliveryStatus = statusResult.StatusCode;
                log.IsDelivered = statusResult.IsDelivered;
                log.LastStatusCheckDate = DateTime.Now;

                if (statusResult.IsDelivered && !log.DeliveryDate.HasValue)
                {
                    log.DeliveryDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بروزرسانی وضعیت پیامک‌های در انتظار
        /// </summary>
        public async Task<int> UpdatePendingDeliveriesAsync()
        {
            var cutoffTime = DateTime.Now.AddHours(-48);

            var pendingLogs = await _context.SmsLog_Tbl
                .Where(l => l.SendDate > cutoffTime &&
                           l.ProviderMessageId.HasValue &&
                           !l.IsDelivered &&
                           l.IsSuccess &&
                           (!l.LastStatusCheckDate.HasValue ||
                            l.LastStatusCheckDate < DateTime.Now.AddHours(-1)))
                .ToListAsync();

            int updatedCount = 0;

            foreach (var log in pendingLogs)
            {
                try
                {
                    bool updated = await UpdateDeliveryStatusAsync(log.Id);
                    if (updated) updatedCount++;

                    await Task.Delay(200);
                }
                catch
                {
                    // ادامه به بقیه
                }
            }

            return updatedCount;
        }
    }

    // ========== مدل آمار ==========

}