using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Email;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly AppDbContext _context;

        public EmailTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========== مدیریت قالب‌ها ==========

        /// <summary>
        /// دریافت تمام قالب‌ها
        /// </summary>
        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            return await _context.EmailTemplate_Tbl
                .Include(t => t.Creator)
                .Include(t => t.Recipients)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت قالب بر اساس ID
        /// </summary>
        public async Task<EmailTemplate> GetTemplateByIdAsync(int id)
        {
            return await _context.EmailTemplate_Tbl.FindAsync(id);
        }

        /// <summary>
        /// دریافت قالب با جزئیات کامل
        /// </summary>
        public async Task<EmailTemplate> GetTemplateWithDetailsAsync(int id)
        {
            return await _context.EmailTemplate_Tbl
                .Include(t => t.Creator)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Contact)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Organization)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// ایجاد قالب جدید
        /// </summary>
        public async Task<int> CreateTemplateAsync(EmailTemplate template)
        {
            _context.EmailTemplate_Tbl.Add(template);
            await _context.SaveChangesAsync();
            return template.Id;
        }

        /// <summary>
        /// بروزرسانی قالب
        /// </summary>
        public async Task UpdateTemplateAsync(EmailTemplate template)
        {
            _context.EmailTemplate_Tbl.Update(template);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// بروزرسانی فیلدهای خاص قالب
        /// </summary>
        public async Task UpdateTemplateFieldsAsync(
            int id,
            string title,
            string subjectTemplate,
            string bodyHtml,
            string bodyPlainText,
            string description,
            byte category,
            bool isActive,
            string lastUpdaterUserId)
        {
            var template = await _context.EmailTemplate_Tbl.FindAsync(id);
            if (template == null)
                throw new ArgumentException("قالب یافت نشد");

            template.Title = title;
            template.SubjectTemplate = subjectTemplate;
            template.BodyHtml = bodyHtml;
            template.BodyPlainText = bodyPlainText;
            template.Description = description;
            template.Category = category;
            template.IsActive = isActive;
            template.LastUpdateDate = DateTime.Now;
            template.LastUpdaterUserId = lastUpdaterUserId;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// حذف قالب
        /// </summary>
        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _context.EmailTemplate_Tbl
                .Include(t => t.Recipients)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template != null)
            {
                _context.EmailTemplateRecipient_Tbl.RemoveRange(template.Recipients);
                _context.EmailTemplate_Tbl.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        // ========== مدیریت مخاطبین ==========

        /// <summary>
        /// افزودن مخاطبین به قالب
        /// </summary>
        public async Task<int> AddRecipientsAsync(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds,
            string addedByUserId)
        {
            int count = 0;

            // افزودن Contacts
            if (contactIds != null && contactIds.Any())
            {
                foreach (var contactId in contactIds)
                {
                    var exists = await _context.EmailTemplateRecipient_Tbl
                        .AnyAsync(r => r.TemplateId == templateId &&
                                      r.RecipientType == 0 &&
                                      r.ContactId == contactId);

                    if (!exists)
                    {
                        _context.EmailTemplateRecipient_Tbl.Add(new EmailTemplateRecipient
                        {
                            TemplateId = templateId,
                            RecipientType = 0,
                            ContactId = contactId,
                            AddedByUserId = addedByUserId,
                            AddedDate = DateTime.Now
                        });
                        count++;
                    }
                }
            }

            // افزودن Organizations
            if (organizationIds != null && organizationIds.Any())
            {
                foreach (var orgId in organizationIds)
                {
                    var exists = await _context.EmailTemplateRecipient_Tbl
                        .AnyAsync(r => r.TemplateId == templateId &&
                                      r.RecipientType == 1 &&
                                      r.OrganizationId == orgId);

                    if (!exists)
                    {
                        _context.EmailTemplateRecipient_Tbl.Add(new EmailTemplateRecipient
                        {
                            TemplateId = templateId,
                            RecipientType = 1,
                            OrganizationId = orgId,
                            AddedByUserId = addedByUserId,
                            AddedDate = DateTime.Now
                        });
                        count++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// حذف مخاطب از قالب
        /// </summary>
        public async Task RemoveRecipientAsync(int recipientId)
        {
            var recipient = await _context.EmailTemplateRecipient_Tbl.FindAsync(recipientId);
            if (recipient != null)
            {
                _context.EmailTemplateRecipient_Tbl.Remove(recipient);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// دریافت مخاطبین یک قالب
        /// </summary>
        public async Task<List<EmailTemplateRecipient>> GetTemplateRecipientsAsync(int templateId)
        {
            return await _context.EmailTemplateRecipient_Tbl
                .Include(r => r.Contact)
                .Include(r => r.Organization)
                .Include(r => r.AddedBy)
                .Where(r => r.TemplateId == templateId)
                .OrderByDescending(r => r.AddedDate)
                .ToListAsync();
        }

        // ========== جستجو ==========

        /// <summary>
        /// جستجوی Contacts با ایمیل فعال
        /// </summary>
        public async Task<List<object>> SearchContactsAsync(string search)
        {
            var query = _context.Contact_Tbl
                .Where(c => c.IsActive && !string.IsNullOrEmpty(c.PrimaryEmail));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search) ||
                    c.PrimaryEmail.Contains(search));
            }

            var contacts = await query
                .Take(50)
                .Select(c => new
                {
                    id = c.Id,
                    text = $"{c.FirstName} {c.LastName} ({c.PrimaryEmail})"
                })
                .ToListAsync();

            return contacts.Cast<object>().ToList();
        }

        /// <summary>
        /// جستجوی Organizations با ایمیل فعال
        /// </summary>
        public async Task<List<object>> SearchOrganizationsAsync(string search)
        {
            var query = _context.Organization_Tbl
                .Where(o => o.IsActive && !string.IsNullOrEmpty(o.Email));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Name.Contains(search) ||
                    o.Email.Contains(search));
            }

            var orgs = await query
                .Take(50)
                .Select(o => new
                {
                    id = o.Id,
                    text = $"{o.Name} ({o.Email})"
                })
                .ToListAsync();

            return orgs.Cast<object>().ToList();
        }

        // ========== قالب‌های فعال ==========

        /// <summary>
        /// دریافت قالب‌های فعال
        /// </summary>
        public async Task<List<EmailTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.EmailTemplate_Tbl
                .Where(t => t.IsActive)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت قالب‌های بر اساس دسته‌بندی
        /// </summary>
        public async Task<List<EmailTemplate>> GetTemplatesByCategoryAsync(byte category)
        {
            return await _context.EmailTemplate_Tbl
                .Where(t => t.Category == category && t.IsActive)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>
        /// بروزرسانی تعداد استفاده قالب
        /// </summary>
        public async Task IncrementUsageCountAsync(int templateId)
        {
            var template = await _context.EmailTemplate_Tbl.FindAsync(templateId);
            if (template != null)
            {
                template.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// بررسی وجود قالب
        /// </summary>
        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await _context.EmailTemplate_Tbl.AnyAsync(t => t.Id == id);
        }
    }
}