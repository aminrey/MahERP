using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahERP.DataModelLayer.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<EmailRepository> _logger;

        public EmailRepository(
            AppDbContext context,
            EmailService emailService,
            ILogger<EmailRepository> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // ========== ارسال به Contact ==========

        /// <summary>
        /// ارسال ایمیل به یک شخص
        /// </summary>
        public async Task<EmailLog> SendToContactAsync(
            int contactId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var contact = await _context.Contact_Tbl.FindAsync(contactId);
            if (contact == null)
                throw new ArgumentException("شخص یافت نشد");

            if (string.IsNullOrEmpty(contact.PrimaryEmail))
                throw new InvalidOperationException("ایمیل شخص یافت نشد");

            // ارسال ایمیل
            var result = await _emailService.SendEmailAsync(
                contact.PrimaryEmail,
                subject,
                body,
                contact.FullName,
                isHtml,
                attachmentPaths
            );

            // ثبت لاگ
            var log = new EmailLog
            {
                ToEmail = contact.PrimaryEmail,
                ToName = contact.FullName,
                Subject = subject,
                Body = body,
                IsHtml = isHtml,
                RecipientType = 0, // شخص
                ContactId = contactId,
                IsSuccess = result.Success,
                ErrorMessage = result.ErrorMessage,
                SendDate = DateTime.Now,
                AttachmentCount = result.AttachmentCount,
                AttachmentTotalSizeKB = result.AttachmentTotalSizeKB,
                SenderUserId = senderUserId
            };

            _context.EmailLog_Tbl.Add(log);
            await _context.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// ارسال ایمیل به چند شخص
        /// </summary>
        public async Task<List<EmailLog>> SendToMultipleContactsAsync(
            List<int> contactIds,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var logs = new List<EmailLog>();

            foreach (var contactId in contactIds)
            {
                try
                {
                    var log = await SendToContactAsync(
                        contactId,
                        subject,
                        body,
                        senderUserId,
                        isHtml,
                        attachmentPaths
                    );
                    logs.Add(log);

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"خطا در ارسال به Contact {contactId}");
                }
            }

            return logs;
        }

        // ========== ارسال به Organization ==========

        /// <summary>
        /// ارسال ایمیل به سازمان
        /// </summary>
        public async Task<EmailLog> SendToOrganizationAsync(
            int organizationId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var organization = await _context.Organization_Tbl.FindAsync(organizationId);
            if (organization == null)
                throw new ArgumentException("سازمان یافت نشد");

            if (string.IsNullOrEmpty(organization.Email))
                throw new InvalidOperationException("ایمیل سازمان یافت نشد");

            var result = await _emailService.SendEmailAsync(
                organization.Email,
                subject,
                body,
                organization.Name,
                isHtml,
                attachmentPaths
            );

            var log = new EmailLog
            {
                ToEmail = organization.Email,
                ToName = organization.Name,
                Subject = subject,
                Body = body,
                IsHtml = isHtml,
                RecipientType = 1, // سازمان
                OrganizationId = organizationId,
                IsSuccess = result.Success,
                ErrorMessage = result.ErrorMessage,
                SendDate = DateTime.Now,
                AttachmentCount = result.AttachmentCount,
                AttachmentTotalSizeKB = result.AttachmentTotalSizeKB,
                SenderUserId = senderUserId
            };

            _context.EmailLog_Tbl.Add(log);
            await _context.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// ارسال به تمام افراد مرتبط با سازمان
        /// </summary>
        public async Task<List<EmailLog>> SendToOrganizationContactsAsync(
            int organizationId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var contactIds = await _context.OrganizationContact_Tbl
                .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                .Select(oc => oc.ContactId)
                .ToListAsync();

            return await SendToMultipleContactsAsync(
                contactIds,
                subject,
                body,
                senderUserId,
                isHtml,
                attachmentPaths
            );
        }

        // ========== قالب‌ها ==========

        /// <summary>
        /// دریافت تمام قالب‌های فعال
        /// </summary>
        public async Task<List<EmailTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.EmailTemplate_Tbl
                .Where(t => t.IsActive)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>
        /// ارسال با قالب به Contact
        /// </summary>
        public async Task<EmailLog> SendWithTemplateToContactAsync(
            int contactId,
            int templateId,
            Dictionary<string, string> parameters,
            string senderUserId,
            List<string> attachmentPaths = null)
        {
            var contact = await _context.Contact_Tbl.FindAsync(contactId);
            if (contact == null)
                throw new ArgumentException("شخص یافت نشد");

            var result = await _emailService.SendWithTemplateAsync(
                templateId,
                contact.PrimaryEmail,
                contact.FullName,
                parameters,
                attachmentPaths
            );

            var template = await _context.EmailTemplate_Tbl.FindAsync(templateId);

            var log = new EmailLog
            {
                ToEmail = contact.PrimaryEmail,
                ToName = contact.FullName,
                Subject = template.SubjectTemplate,
                Body = template.BodyHtml,
                IsHtml = true,
                RecipientType = 0,
                ContactId = contactId,
                IsSuccess = result.Success,
                ErrorMessage = result.ErrorMessage,
                SendDate = DateTime.Now,
                AttachmentCount = result.AttachmentCount,
                AttachmentTotalSizeKB = result.AttachmentTotalSizeKB,
                SenderUserId = senderUserId
            };

            _context.EmailLog_Tbl.Add(log);
            await _context.SaveChangesAsync();

            return log;
        }

        // ========== آمار ==========

        /// <summary>
        /// دریافت آمار ارسال ایمیل
        /// </summary>
        public async Task<EmailStatistics> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            IQueryable<EmailLog> query = _context.EmailLog_Tbl;

            if (fromDate.HasValue)
                query = query.Where(l => l.SendDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.SendDate <= toDate.Value);

            return new EmailStatistics
            {
                TotalSent = await query.CountAsync(),
                SuccessCount = await query.CountAsync(l => l.IsSuccess),
                FailedCount = await query.CountAsync(l => !l.IsSuccess),
                SentToContacts = await query.CountAsync(l => l.RecipientType == 0),
                SentToOrganizations = await query.CountAsync(l => l.RecipientType == 1),
                SentToUsers = await query.CountAsync(l => l.RecipientType == 2)
            };
        }

        // ==================== ⭐ NEW: ارسال به گروه‌ها ====================

        /// <summary>
        /// ارسال ایمیل به یک گروه کامل (System Level)
        /// </summary>
        public async Task<EmailBulkResult> SendToContactGroupAsync(
            int groupId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var result = new EmailBulkResult
            {
                GroupId = groupId,
                GroupType = "System"
            };

            try
            {
                // دریافت گروه
                var group = _context.ContactGroup_Tbl
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.Contact)
                    .FirstOrDefault(g => g.Id == groupId);

                if (group == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه یافت نشد";
                    return result;
                }

                result.GroupTitle = group.Title;

                // دریافت Contact IDs
                var contactIds = group.Members
                    .Where(m => m.IsActive && m.Contact != null)
                    .Select(m => m.ContactId)
                    .ToList();

                if (!contactIds.Any())
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه خالی است";
                    return result;
                }

                // ارسال دسته‌جمعی
                var logs = await SendToMultipleContactsAsync(
                    contactIds,
                    subject,
                    body,
                    senderUserId,
                    isHtml,
                    attachmentPaths
                );

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"ایمیل به {result.SuccessCount} نفر از {result.TotalSent} نفر گروه '{group.Title}' ارسال شد";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"خطا در ارسال: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// ارسال ایمیل به گروه شعبه (Branch Level)
        /// </summary>
        public async Task<EmailBulkResult> SendToBranchContactGroupAsync(
            int branchGroupId,
            string subject,
            string body,
            string senderUserId,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var result = new EmailBulkResult
            {
                GroupId = branchGroupId,
                GroupType = "Branch"
            };

            try
            {
                // دریافت گروه شعبه
                var group = _context.BranchContactGroup_Tbl
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.BranchContact)
                            .ThenInclude(bc => bc.Contact)
                    .FirstOrDefault(g => g.Id == branchGroupId);

                if (group == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه شعبه یافت نشد";
                    return result;
                }

                result.GroupTitle = group.Title;

                // دریافت Contact IDs
                var contactIds = group.Members
                    .Where(m => m.IsActive && m.BranchContact?.Contact != null)
                    .Select(m => m.BranchContact.ContactId)
                    .ToList();

                if (!contactIds.Any())
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه شعبه خالی است";
                    return result;
                }

                // ارسال دسته‌جمعی
                var logs = await SendToMultipleContactsAsync(
                    contactIds,
                    subject,
                    body,
                    senderUserId,
                    isHtml,
                    attachmentPaths
                );

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"ایمیل به {result.SuccessCount} نفر از {result.TotalSent} نفر گروه شعبه '{group.Title}' ارسال شد";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"خطا در ارسال: {ex.Message}";
                return result;
            }
        }
    }

}