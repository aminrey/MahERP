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

        // ====================  ارسال به گروه‌ها ====================

        /// <summary>
        /// ارسال پیامک به یک گروه کامل (System Level)
        /// </summary>
        public async Task<SmsBulkResult> SendToContactGroupAsync(
            int groupId,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var result = new SmsBulkResult
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
                var logs = await SendToMultipleContactsAsync(contactIds, message, senderUserId, providerId);

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"پیامک به {result.SuccessCount} نفر از {result.TotalSent} نفر گروه '{group.Title}' ارسال شد";

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
        /// ارسال پیامک به گروه شعبه (Branch Level)
        /// </summary>
        public async Task<SmsBulkResult> SendToBranchContactGroupAsync(
            int branchGroupId,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var result = new SmsBulkResult
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
                var logs = await SendToMultipleContactsAsync(contactIds, message, senderUserId, providerId);

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"پیامک به {result.SuccessCount} نفر از {result.TotalSent} نفر گروه شعبه '{group.Title}' ارسال شد";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"خطا در ارسال: {ex.Message}";
                return result;
            }
        }

        // ==================== ⭐ NEW: ارسال به گروه‌های سازمان ====================

        /// <summary>
        /// ارسال پیامک به چند سازمان
        /// </summary>
        public async Task<List<SmsLog>> SendToMultipleOrganizationsAsync(
            List<int> organizationIds,
            string message,
            string senderUserId,
            int? providerId = null,
            bool sendToOrganizationPhone = true)
        {
            var logs = new List<SmsLog>();

            foreach (var organizationId in organizationIds)
            {
                try
                {
                    if (sendToOrganizationPhone)
                    {
                        // ارسال به شماره اصلی سازمان
                        var log = await SendToOrganizationAsync(organizationId, message, senderUserId, providerId);
                        logs.Add(log);
                    }

                    // تاخیر کوتاه
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطا در ارسال به Organization {organizationId}: {ex.Message}");
                }
            }

            return logs;
        }

        /// <summary>
        /// ارسال پیامک به یک گروه سازمان (System Level)
        /// </summary>
        /// <param name="groupId">شناسه گروه سازمان</param>
        /// <param name="message">متن پیام</param>
        /// <param name="senderUserId">کاربر ارسال‌کننده</param>
        /// <param name="sendMode">حالت ارسال: 0=فقط شماره سازمان، 1=فقط افراد مرتبط، 2=هر دو</param>
        /// <param name="providerId">شناسه Provider</param>
        public async Task<SmsBulkResult> SendToOrganizationGroupAsync(
            int groupId,
            string message,
            string senderUserId,
            byte sendMode = 0,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                GroupId = groupId,
                GroupType = "OrganizationGroup"
            };

            try
            {
                // دریافت گروه سازمان
                var group = _context.OrganizationGroup_Tbl
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.Organization)
                    .FirstOrDefault(g => g.Id == groupId);

                if (group == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه سازمان یافت نشد";
                    return result;
                }

                result.GroupTitle = group.Title;

                // دریافت Organization IDs
                var organizations = group.Members
                    .Where(m => m.IsActive && m.Organization != null)
                    .Select(m => m.Organization)
                    .ToList();

                if (!organizations.Any())
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه سازمان خالی است";
                    return result;
                }

                var logs = new List<SmsLog>();

                foreach (var org in organizations)
                {
                    try
                    {
                        // sendMode = 0: فقط شماره سازمان
                        if (sendMode == 0 || sendMode == 2)
                        {
                            if (!string.IsNullOrEmpty(org.PrimaryPhone))
                            {
                                var log = await SendToOrganizationAsync(org.Id, message, senderUserId, providerId);
                                logs.Add(log);
                            }
                        }

                        // sendMode = 1: فقط افراد مرتبط
                        // sendMode = 2: هر دو
                        if (sendMode == 1 || sendMode == 2)
                        {
                            var contactLogs = await SendToOrganizationContactsAsync(org.Id, message, senderUserId, providerId);
                            logs.AddRange(contactLogs);
                        }

                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ارسال به سازمان {org.Name}: {ex.Message}");
                    }
                }

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"پیامک به {result.SuccessCount} مورد از {result.TotalSent} مورد گروه سازمان '{group.Title}' ارسال شد";

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
        /// ارسال پیامک به گروه سازمان شعبه (Branch Level)
        /// </summary>
        public async Task<SmsBulkResult> SendToBranchOrganizationGroupAsync(
            int branchGroupId,
            string message,
            string senderUserId,
            byte sendMode = 0,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                GroupId = branchGroupId,
                GroupType = "BranchOrganizationGroup"
            };

            try
            {
                // دریافت گروه سازمان شعبه
                var group = _context.BranchOrganizationGroup_Tbl
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.BranchOrganization)
                            .ThenInclude(bo => bo.Organization)
                    .FirstOrDefault(g => g.Id == branchGroupId);

                if (group == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه سازمان شعبه یافت نشد";
                    return result;
                }

                result.GroupTitle = group.Title;

                // دریافت Organizations
                var organizations = group.Members
                    .Where(m => m.IsActive && m.BranchOrganization?.Organization != null)
                    .Select(m => m.BranchOrganization.Organization)
                    .ToList();

                if (!organizations.Any())
                {
                    result.Success = false;
                    result.ErrorMessage = "گروه سازمان شعبه خالی است";
                    return result;
                }

                var logs = new List<SmsLog>();

                foreach (var org in organizations)
                {
                    try
                    {
                        // sendMode = 0: فقط شماره سازمان
                        if (sendMode == 0 || sendMode == 2)
                        {
                            if (!string.IsNullOrEmpty(org.PrimaryPhone))
                            {
                                var log = await SendToOrganizationAsync(org.Id, message, senderUserId, providerId);
                                logs.Add(log);
                            }
                        }

                        // sendMode = 1 یا 2: افراد مرتبط
                        if (sendMode == 1 || sendMode == 2)
                        {
                            var contactLogs = await SendToOrganizationContactsAsync(org.Id, message, senderUserId, providerId);
                            logs.AddRange(contactLogs);
                        }

                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ارسال به سازمان {org.Name}: {ex.Message}");
                    }
                }

                result.Logs = logs;
                result.TotalSent = logs.Count;
                result.SuccessCount = logs.Count(l => l.IsSuccess);
                result.FailedCount = logs.Count - result.SuccessCount;
                result.Success = true;
                result.Message = $"پیامک به {result.SuccessCount} مورد از {result.TotalSent} مورد گروه سازمان شعبه '{group.Title}' ارسال شد";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"خطا در ارسال: {ex.Message}";
                return result;
            }
        }

        // ==================== ⭐ NEW: ارسال به چند گروه ====================

        /// <summary>
        /// ارسال پیامک به چند گروه افراد
        /// </summary>
        public async Task<SmsBulkResult> SendToMultipleContactGroupsAsync(
            List<int> groupIds,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                Success = true,
                TotalGroups = groupIds.Count,
                GroupType = "MultipleContactGroups"
            };

            foreach (var groupId in groupIds)
            {
                try
                {
                    var groupResult = await SendToContactGroupAsync(groupId, message, senderUserId, providerId);
                    result.SubResults.Add(groupResult);

                    if (groupResult.Success)
                    {
                        result.SuccessfulGroups++;
                        result.TotalSent += groupResult.TotalSent;
                        result.SuccessCount += groupResult.SuccessCount;
                        result.FailedCount += groupResult.FailedCount;
                        result.Logs.AddRange(groupResult.Logs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطا در ارسال به گروه {groupId}: {ex.Message}");
                }
            }

            result.Message = $"پیامک به {result.SuccessCount} نفر از {result.TotalSent} نفر در {result.SuccessfulGroups} گروه از {result.TotalGroups} گروه ارسال شد";
            return result;
        }

        /// <summary>
        /// ارسال پیامک به چند گروه سازمان
        /// </summary>
        public async Task<SmsBulkResult> SendToMultipleOrganizationGroupsAsync(
            List<int> groupIds,
            string message,
            string senderUserId,
            byte sendMode = 0,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                Success = true,
                TotalGroups = groupIds.Count,
                GroupType = "MultipleOrganizationGroups"
            };

            foreach (var groupId in groupIds)
            {
                try
                {
                    var groupResult = await SendToOrganizationGroupAsync(groupId, message, senderUserId, sendMode, providerId);
                    result.SubResults.Add(groupResult);

                    if (groupResult.Success)
                    {
                        result.SuccessfulGroups++;
                        result.TotalSent += groupResult.TotalSent;
                        result.SuccessCount += groupResult.SuccessCount;
                        result.FailedCount += groupResult.FailedCount;
                        result.Logs.AddRange(groupResult.Logs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطا در ارسال به گروه سازمان {groupId}: {ex.Message}");
                }
            }

            result.Message = $"پیامک به {result.SuccessCount} مورد از {result.TotalSent} مورد در {result.SuccessfulGroups} گروه از {result.TotalGroups} گروه سازمان ارسال شد";
            return result;
        }

        /// <summary>
        /// ارسال پیامک به چند گروه شعبه افراد
        /// </summary>
        public async Task<SmsBulkResult> SendToMultipleBranchContactGroupsAsync(
            List<int> branchGroupIds,
            string message,
            string senderUserId,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                Success = true,
                TotalGroups = branchGroupIds.Count,
                GroupType = "MultipleBranchContactGroups"
            };

            foreach (var groupId in branchGroupIds)
            {
                try
                {
                    var groupResult = await SendToBranchContactGroupAsync(groupId, message, senderUserId, providerId);
                    result.SubResults.Add(groupResult);

                    if (groupResult.Success)
                    {
                        result.SuccessfulGroups++;
                        result.TotalSent += groupResult.TotalSent;
                        result.SuccessCount += groupResult.SuccessCount;
                        result.FailedCount += groupResult.FailedCount;
                        result.Logs.AddRange(groupResult.Logs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطا در ارسال به گروه شعبه {groupId}: {ex.Message}");
                }
            }

            result.Message = $"پیامک به {result.SuccessCount} نفر از {result.TotalSent} نفر در {result.SuccessfulGroups} گروه از {result.TotalGroups} گروه شعبه ارسال شد";
            return result;
        }

        /// <summary>
        /// ارسال پیامک به چند گروه شعبه سازمان
        /// </summary>
        public async Task<SmsBulkResult> SendToMultipleBranchOrganizationGroupsAsync(
            List<int> branchGroupIds,
            string message,
            string senderUserId,
            byte sendMode = 0,
            int? providerId = null)
        {
            var result = new SmsBulkResult
            {
                Success = true,
                TotalGroups = branchGroupIds.Count,
                GroupType = "MultipleBranchOrganizationGroups"
            };

            foreach (var groupId in branchGroupIds)
            {
                try
                {
                    var groupResult = await SendToBranchOrganizationGroupAsync(groupId, message, senderUserId, sendMode, providerId);
                    result.SubResults.Add(groupResult);

                    if (groupResult.Success)
                    {
                        result.SuccessfulGroups++;
                        result.TotalSent += groupResult.TotalSent;
                        result.SuccessCount += groupResult.SuccessCount;
                        result.FailedCount += groupResult.FailedCount;
                        result.Logs.AddRange(groupResult.Logs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطا در ارسال به گروه سازمان شعبه {groupId}: {ex.Message}");
                }
            }

            result.Message = $"پیامک به {result.SuccessCount} مورد از {result.TotalSent} مورد در {result.SuccessfulGroups} گروه از {result.TotalGroups} گروه سازمان شعبه ارسال شد";
            return result;
        }
    }

    // ========== مدل آمار ==========

}