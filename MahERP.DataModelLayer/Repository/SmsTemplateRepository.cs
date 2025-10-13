using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class SmsTemplateRepository : ISmsTemplateRepository
    {
        private readonly AppDbContext _context;

        public SmsTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========== مدیریت قالب‌ها ==========

        public async Task<List<SmsTemplate>> GetAllTemplatesAsync()
        {
            return await _context.SmsTemplate_Tbl
                .Include(t => t.Creator)
                .Include(t => t.Recipients)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<SmsTemplate> GetTemplateByIdAsync(int id)
        {
            return await _context.SmsTemplate_Tbl
                .Include(t => t.Creator)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Contact)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Organization)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int> CreateTemplateAsync(SmsTemplate template)
        {
            _context.SmsTemplate_Tbl.Add(template);
            await _context.SaveChangesAsync();
            return template.Id;
        }

        public async Task UpdateTemplateAsync(SmsTemplate template)
        {
            _context.SmsTemplate_Tbl.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _context.SmsTemplate_Tbl.FindAsync(id);
            if (template != null)
            {
                _context.SmsTemplate_Tbl.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        // ========== مدیریت مخاطبین قالب ==========

        /// <summary>
        /// افزودن مخاطب به قالب
        /// </summary>
        public async Task<int> AddRecipientAsync(
            int templateId,
            byte recipientType,
            int? contactId,
            int? organizationId,
            string addedByUserId)
        {
            var recipient = new SmsTemplateRecipient
            {
                TemplateId = templateId,
                RecipientType = recipientType,
                ContactId = contactId,
                OrganizationId = organizationId,
                AddedDate = DateTime.Now,
                AddedByUserId = addedByUserId
            };

            _context.SmsTemplateRecipient_Tbl.Add(recipient);
            await _context.SaveChangesAsync();

            return recipient.Id;
        }

        /// <summary>
        /// افزودن چند مخاطب به یکباره
        /// </summary>
        public async Task<int> AddMultipleRecipientsAsync(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds,
            string addedByUserId)
        {
            var recipients = new List<SmsTemplateRecipient>();

            // افزودن Contacts
            if (contactIds != null && contactIds.Any())
            {
                foreach (var contactId in contactIds)
                {
                    // بررسی تکراری نبودن
                    var exists = await _context.SmsTemplateRecipient_Tbl
                        .AnyAsync(r => r.TemplateId == templateId &&
                                      r.RecipientType == 0 &&
                                      r.ContactId == contactId);

                    if (!exists)
                    {
                        recipients.Add(new SmsTemplateRecipient
                        {
                            TemplateId = templateId,
                            RecipientType = 0,
                            ContactId = contactId,
                            AddedByUserId = addedByUserId
                        });
                    }
                }
            }

            // افزودن Organizations
            if (organizationIds != null && organizationIds.Any())
            {
                foreach (var orgId in organizationIds)
                {
                    var exists = await _context.SmsTemplateRecipient_Tbl
                        .AnyAsync(r => r.TemplateId == templateId &&
                                      r.RecipientType == 1 &&
                                      r.OrganizationId == orgId);

                    if (!exists)
                    {
                        recipients.Add(new SmsTemplateRecipient
                        {
                            TemplateId = templateId,
                            RecipientType = 1,
                            OrganizationId = orgId,
                            AddedByUserId = addedByUserId
                        });
                    }
                }
            }

            if (recipients.Any())
            {
                _context.SmsTemplateRecipient_Tbl.AddRange(recipients);
                await _context.SaveChangesAsync();
            }

            return recipients.Count;
        }

        /// <summary>
        /// حذف مخاطب از قالب
        /// </summary>
        public async Task RemoveRecipientAsync(int recipientId)
        {
            var recipient = await _context.SmsTemplateRecipient_Tbl.FindAsync(recipientId);
            if (recipient != null)
            {
                _context.SmsTemplateRecipient_Tbl.Remove(recipient);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// دریافت مخاطبین یک قالب
        /// </summary>
        public async Task<List<SmsTemplateRecipient>> GetTemplateRecipientsAsync(int templateId)
        {
            return await _context.SmsTemplateRecipient_Tbl
                .Include(r => r.Contact)
                    .ThenInclude(c => c.Phones)
                .Include(r => r.Organization)
                .Include(r => r.AddedBy)
                .Where(r => r.TemplateId == templateId)
                .OrderByDescending(r => r.AddedDate)
                .ToListAsync();
        }

        // ========== دریافت جزئیات کامل ==========

        public async Task<SmsTemplateDetailViewModel> GetTemplateDetailAsync(int templateId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null)
                return null;

            // دریافت آمار ارسال‌ها
            var sentLogs = await _context.SmsLog_Tbl
                .Where(l => l.TemplateId == templateId)
                .ToListAsync();

            // دریافت لاگ‌های اخیر
            var recentLogs = await _context.SmsLog_Tbl
                .Include(l => l.Contact)
                .Include(l => l.Organization)
                .Include(l => l.Provider)
                .Where(l => l.TemplateId == templateId)
                .OrderByDescending(l => l.SendDate)
                .Take(20)
                .ToListAsync();

            var viewModel = new SmsTemplateDetailViewModel
            {
                Id = template.Id,
                Title = template.Title,
                MessageTemplate = template.MessageTemplate,
                Description = template.Description,
                TemplateType = template.TemplateType,
                TemplateTypeText = template.TemplateTypeText,
                IsActive = template.IsActive,
                UsageCount = template.UsageCount,
                CreatedDate = template.CreatedDate,
                CreatorName = $"{template.Creator?.FirstName} {template.Creator?.LastName}",

                TotalRecipients = template.Recipients.Count,
                ContactRecipients = template.Recipients.Count(r => r.RecipientType == 0),
                OrganizationRecipients = template.Recipients.Count(r => r.RecipientType == 1),

                TotalSent = sentLogs.Count,
                SuccessfulSent = sentLogs.Count(l => l.IsSuccess),
                FailedSent = sentLogs.Count(l => !l.IsSuccess),

                Recipients = template.Recipients.Select(r => new RecipientItemViewModel
                {
                    Id = r.Id,
                    RecipientType = r.RecipientType,
                    RecipientTypeText = r.RecipientTypeText,
                    ContactId = r.ContactId,
                    OrganizationId = r.OrganizationId,
                    RecipientName = r.RecipientName,
                    RecipientContact = r.RecipientContact,
                    AddedDate = r.AddedDate,
                    AddedByName = $"{r.AddedBy?.FirstName} {r.AddedBy?.LastName}"
                }).ToList(),

                RecentLogs = recentLogs.Select(l => new SmsLogItemViewModel
                {
                    Id = l.Id,
                    PhoneNumber = l.PhoneNumber,
                    RecipientName = l.Contact?.FullName ?? l.Organization?.DisplayName ?? "",
                    IsSuccess = l.IsSuccess,
                    ErrorMessage = l.ErrorMessage,
                    DeliveryStatus = l.DeliveryStatus,
                    DeliveryStatusText = l.DeliveryStatusText,
                    IsDelivered = l.IsDelivered,
                    SendDate = l.SendDate,
                    DeliveryDate = l.DeliveryDate,
                    ProviderName = l.Provider?.ProviderName ?? ""
                }).ToList()
            };

            return viewModel;
        }

        /// <summary>
        /// جستجوی Contacts با شماره تلفن
        /// </summary>
        public async Task<List<object>> SearchContactsAsync(string search)
        {
            var query = _context.Contact_Tbl
                .Include(c => c.Phones)
                .Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search));
            }

            var contacts = await query
                .Take(50)
                .Select(c => new
                {
                    id = c.Id,
                    text = $"{c.FirstName} {c.LastName} - {c.DefaultPhone.PhoneNumber}"
                })
                .ToListAsync();

            return contacts.Cast<object>().ToList();
        }

        /// <summary>
        /// جستجوی Organizations با شماره تلفن
        /// </summary>
        public async Task<List<object>> SearchOrganizationsAsync(string search)
        {
            var query = _context.Organization_Tbl
                .Where(o => o.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.Name.Contains(search));
            }

            var orgs = await query
                .Take(50)
                .Select(o => new
                {
                    id = o.Id,
                    text = $"{o.Name} - {o.PrimaryPhone}"
                })
                .ToListAsync();

            return orgs.Cast<object>().ToList();
        }

        /// <summary>
        /// بروزرسانی تعداد استفاده
        /// </summary>
        public async Task IncrementUsageCountAsync(int templateId)
        {
            var template = await _context.SmsTemplate_Tbl.FindAsync(templateId);
            if (template != null)
            {
                template.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}