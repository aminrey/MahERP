using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Contacts;
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
                .AsNoTracking() // ⭐ اضافه شده برای جلوگیری از مشکلات tracking
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
        /// افزودن چند مخاطب به یکباره با شماره‌های خاص
        /// </summary>
        public async Task<int> AddMultipleRecipientsWithPhonesAsync(
            int templateId,
            List<(int contactId, int? phoneId)> contactData,
            List<int> organizationIds,
            string addedByUserId)
        {
            var recipients = new List<SmsTemplateRecipient>();

            // افزودن Contacts با شماره‌های خاص
            if (contactData != null && contactData.Any())
            {
                foreach (var (contactId, phoneId) in contactData)
                {
                    // بررسی تکراری نبودن (Contact + Phone ترکیبی)
                    var exists = await _context.SmsTemplateRecipient_Tbl
                        .AnyAsync(r => r.TemplateId == templateId &&
                                      r.RecipientType == 0 &&
                                      r.ContactId == contactId &&
                                      r.ContactPhoneId == phoneId);

                    if (!exists)
                    {
                        recipients.Add(new SmsTemplateRecipient
                        {
                            TemplateId = templateId,
                            RecipientType = 0,
                            ContactId = contactId,
                            ContactPhoneId = phoneId,
                            AddedByUserId = addedByUserId,
                            AddedDate = DateTime.Now
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
                            AddedByUserId = addedByUserId,
                            AddedDate = DateTime.Now
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

            // ⭐ دریافت Recipients با Include کامل
            var recipients = await _context.SmsTemplateRecipient_Tbl
                .Include(r => r.Contact)
                    .ThenInclude(c => c.Phones)
                .Include(r => r.ContactPhone) // ⭐ اضافه شده
                .Include(r => r.Organization)
                .Include(r => r.AddedBy)
                .Where(r => r.TemplateId == templateId)
                .OrderByDescending(r => r.AddedDate)
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

                TotalRecipients = recipients.Count,
                ContactRecipients = recipients.Count(r => r.RecipientType == 0),
                OrganizationRecipients = recipients.Count(r => r.RecipientType == 1),

                TotalSent = sentLogs.Count,
                SuccessfulSent = sentLogs.Count(l => l.IsSuccess),
                FailedSent = sentLogs.Count(l => !l.IsSuccess),

                Recipients = recipients.Select(r =>
                {
                    // ⭐ انتخاب شماره برای نمایش
                    string phoneNumber = null;
                    
                    if (r.RecipientType == 0) // Contact
                    {
                        if (r.ContactPhone != null)
                        {
                            // اگر شماره مشخصی انتخاب شده
                            phoneNumber = r.ContactPhone.FormattedNumber;
                        }
                        else if (r.Contact?.Phones != null && r.Contact.Phones.Any())
                        {
                            // اگر شماره مشخص نیست، شماره پیش‌فرض پیامک
                            var smsPhone = r.Contact.Phones.FirstOrDefault(p => p.IsSmsDefault);
                            if (smsPhone == null)
                            {
                                smsPhone = r.Contact.Phones.FirstOrDefault(p => p.IsDefault);
                            }
                            if (smsPhone == null)
                            {
                                smsPhone = r.Contact.Phones.OrderBy(p => p.DisplayOrder).FirstOrDefault();
                            }
                            phoneNumber = smsPhone?.FormattedNumber;
                        }
                    }
                    else if (r.RecipientType == 1) // Organization
                    {
                        phoneNumber = r.Organization?.PrimaryPhone;
                    }
                    
                    return new RecipientItemViewModel
                    {
                        Id = r.Id,
                        RecipientType = r.RecipientType,
                        RecipientTypeText = r.RecipientTypeText,
                        ContactId = r.ContactId,
                        OrganizationId = r.OrganizationId,
                        RecipientName = r.RecipientName,
                        RecipientContact = phoneNumber ?? "-",
                        AddedDate = r.AddedDate,
                        AddedByName = $"{r.AddedBy?.FirstName} {r.AddedBy?.LastName}"
                    };
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
        /// جستجوی ساده Contacts (فقط نام، بدون شماره‌ها)
        /// و انتخاب خودکار بهترین شماره برای پیامک
        /// </summary>
        public async Task<List<object>> SearchContactsSimpleAsync(string search)
        {
            var query = _context.Contact_Tbl
                .Include(c => c.Phones.Where(p => p.IsActive))
                .Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search));
            }

            var contacts = await query
                .Take(50)
                .ToListAsync();

            var results = new List<object>();

            foreach (var contact in contacts)
            {
                // انتخاب بهترین شماره با اولویت:
                // 1. IsSmsDefault = true
                // 2. IsDefault = true  
                // 3. اولین موبایل (PhoneType = 0)
                // 4. اولین شماره
                ContactPhone bestPhone = null;

                if (contact.Phones != null && contact.Phones.Any())
                {
                    // 1️⃣ اولویت اول: شماره پیش‌فرض پیامک
                    bestPhone = contact.Phones.FirstOrDefault(p => p.IsSmsDefault);

                    // 2️⃣ اولویت دوم: شماره پیش‌فرض
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones.FirstOrDefault(p => p.IsDefault);
                    }

                    // 3️⃣ اولویت سوم: اولین موبایل
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones
                            .Where(p => p.PhoneType == 0) // موبایل
                            .OrderBy(p => p.DisplayOrder)
                            .FirstOrDefault();
                    }

                    // 4️⃣ اولویت چهارم: اولین شماره
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones
                            .OrderBy(p => p.DisplayOrder)
                            .FirstOrDefault();
                    }
                }

                results.Add(new
                {
                    id = contact.Id,
                    text = $"{contact.FirstName} {contact.LastName}",
                    phoneId = bestPhone?.Id,
                    phoneNumber = bestPhone?.PhoneNumber,
                    hasPhone = bestPhone != null
                });
            }

            return results.Cast<object>().ToList();
        }

        /// <summary>
        /// دریافت شماره‌های یک Contact
        /// </summary>
        public async Task<List<object>> GetContactPhonesAsync(int contactId)
        {
            var contact = await _context.Contact_Tbl
                .Include(c => c.Phones.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == contactId && c.IsActive);

            if (contact == null || contact.Phones == null || !contact.Phones.Any())
            {
                return new List<object>();
            }

            var phones = contact.Phones
                .OrderByDescending(p => p.IsSmsDefault) // ⭐ شماره پیش‌فرض پیامک اول
                .ThenByDescending(p => p.IsDefault)
                .ThenBy(p => p.DisplayOrder)
                .Select(p => new
                {
                    id = p.Id,
                    phoneNumber = p.PhoneNumber,
                    formattedNumber = p.FormattedNumber,
                    phoneType = p.PhoneType,
                    phoneTypeText = p.PhoneTypeText,
                    isDefault = p.IsDefault,
                    isSmsDefault = p.IsSmsDefault, // ⭐ اضافه شده
                    isVerified = p.IsVerified
                })
                .Cast<object>()
                .ToList();

            return phones;
        }

        /// <summary>
        /// جستجوی Contacts با شماره تلفن - نمایش تمام شماره‌های هر Contact
        /// </summary>
        public async Task<List<object>> SearchContactsAsync(string search)
        {
            var query = _context.Contact_Tbl
                .Include(c => c.Phones.Where(p => p.IsActive))
                .Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search));
            }

            var contacts = await query
                .Take(50)
                .ToListAsync();

            var results = new List<object>();

            foreach (var contact in contacts)
            {
                // اگر شماره‌ای دارد، هر شماره رو جداگانه نمایش بده
                if (contact.Phones != null && contact.Phones.Any())
                {
                    foreach (var phone in contact.Phones.OrderByDescending(p => p.IsDefault).ThenBy(p => p.DisplayOrder))
                    {
                        results.Add(new
                        {
                            id = $"c{contact.Id}_p{phone.Id}", // ترکیبی از ContactId و PhoneId
                            text = $"{contact.FirstName} {contact.LastName} - {phone.FormattedNumber} ({phone.PhoneTypeText})" + 
                                   (phone.IsDefault ? " [پیش‌فرض]" : ""),
                            contactId = contact.Id,
                            phoneId = phone.Id,
                            phoneNumber = phone.PhoneNumber
                        });
                    }
                }
                else
                {
                    // اگر شماره‌ای نداره، فقط Contact رو نمایش بده
                    results.Add(new
                    {
                        id = $"c{contact.Id}_p0",
                        text = $"{contact.FirstName} {contact.LastName} (بدون شماره)",
                        contactId = contact.Id,
                        phoneId = (int?)null,
                        phoneNumber = (string)null
                    });
                }
            }

            return results.Cast<object>().ToList();
        }

        /// <summary>
        /// دریافت افراد یک Organization برای استفاده در SMS
        /// </summary>
        public async Task<List<object>> GetOrganizationContactsAsync(int organizationId)
        {
            var orgContacts = await _context.OrganizationContact_Tbl
                .Include(oc => oc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsActive))
                .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                .ToListAsync();

            if (!orgContacts.Any())
            {
                return new List<object>();
            }

            var results = new List<object>();

            foreach (var oc in orgContacts.Where(oc => oc.Contact != null))
            {
                var contact = oc.Contact;

                // ⭐ انتخاب بهترین شماره (مشابه SearchContactsSimpleAsync)
                ContactPhone bestPhone = null;

                if (contact.Phones != null && contact.Phones.Any())
                {
                    // 1️⃣ شماره پیش‌فرض پیامک
                    bestPhone = contact.Phones.FirstOrDefault(p => p.IsSmsDefault);

                    // 2️⃣ شماره پیش‌فرض
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones.FirstOrDefault(p => p.IsDefault);
                    }

                    // 3️⃣ اولین موبایل
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones
                            .Where(p => p.PhoneType == 0)
                            .OrderBy(p => p.DisplayOrder)
                            .FirstOrDefault();
                    }

                    // 4️⃣ اولین شماره
                    if (bestPhone == null)
                    {
                        bestPhone = contact.Phones
                            .OrderBy(p => p.DisplayOrder)
                            .FirstOrDefault();
                    }
                }

                results.Add(new
                {
                    id = contact.Id,
                    text = $"{contact.FirstName} {contact.LastName}",
                    phoneId = bestPhone?.Id,
                    phoneNumber = bestPhone?.PhoneNumber,
                    hasPhone = bestPhone != null
                });
            }

            return results.Cast<object>().ToList();
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
                .ToListAsync();

            // ⭐ تبدیل به anonymous object در حافظه
            var results = orgs
                .Select(o => new
                {
                    id = o.Id,
                    text = !string.IsNullOrEmpty(o.PrimaryPhone)
                        ? $"{o.Name} - {o.PrimaryPhone}"
                        : o.Name
                })
                .Cast<object>()
                .ToList();

            return results;
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