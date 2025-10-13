using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;
using Microsoft.Extensions.Logging;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس ارسال ایمیل
    /// </summary>
    public class EmailService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(AppDbContext context, ILogger<EmailService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ارسال ایمیل ساده
        /// </summary>
        public async Task<EmailResult> SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            string toName = null,
            bool isHtml = true,
            List<string> attachmentPaths = null,
            string ccEmails = null,
            string bccEmails = null)
        {
            try
            {
                var settings = _context.Settings_Tbl.FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.SmtpHost))
                {
                    return new EmailResult
                    {
                        Success = false,
                        ErrorMessage = "تنظیمات SMTP یافت نشد"
                    };
                }

                using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
                {
                    EnableSsl = settings.SmtpEnableSsl,
                    Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                    Timeout = 30000 // 30 seconds
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        settings.SmtpFromEmail,
                        settings.SmtpFromName ?? settings.SmtpFromEmail
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                // گیرنده اصلی
                mailMessage.To.Add(new MailAddress(toEmail, toName ?? toEmail));

                // CC
                if (!string.IsNullOrEmpty(ccEmails))
                {
                    foreach (var cc in ccEmails.Split(',', ';'))
                    {
                        if (!string.IsNullOrWhiteSpace(cc))
                            mailMessage.CC.Add(cc.Trim());
                    }
                }

                // BCC
                if (!string.IsNullOrEmpty(bccEmails))
                {
                    foreach (var bcc in bccEmails.Split(',', ';'))
                    {
                        if (!string.IsNullOrWhiteSpace(bcc))
                            mailMessage.Bcc.Add(bcc.Trim());
                    }
                }

                // پیوست‌ها
                long totalAttachmentSize = 0;
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    foreach (var path in attachmentPaths)
                    {
                        if (File.Exists(path))
                        {
                            var fileInfo = new FileInfo(path);
                            totalAttachmentSize += fileInfo.Length;

                            // بررسی حجم
                            if (totalAttachmentSize > settings.MaxAttachmentSizeMB * 1024 * 1024)
                            {
                                return new EmailResult
                                {
                                    Success = false,
                                    ErrorMessage = $"حجم پیوست‌ها بیش از {settings.MaxAttachmentSizeMB} مگابایت است"
                                };
                            }

                            mailMessage.Attachments.Add(new Attachment(path));
                        }
                    }
                }

                // ارسال
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"✅ ایمیل با موفقیت به {toEmail} ارسال شد");

                return new EmailResult
                {
                    Success = true,
                    SentDate = DateTime.Now,
                    AttachmentCount = attachmentPaths?.Count ?? 0,
                    AttachmentTotalSizeKB = totalAttachmentSize / 1024
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ارسال ایمیل به {toEmail}");

                return new EmailResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// ارسال ایمیل به چند گیرنده
        /// </summary>
        public async Task<List<EmailResult>> SendBulkEmailAsync(
            List<EmailRecipient> recipients,
            string subject,
            string body,
            bool isHtml = true,
            List<string> attachmentPaths = null)
        {
            var results = new List<EmailResult>();

            foreach (var recipient in recipients)
            {
                var result = await SendEmailAsync(
                    recipient.Email,
                    subject,
                    body,
                    recipient.Name,
                    isHtml,
                    attachmentPaths
                );

                result.RecipientEmail = recipient.Email;
                results.Add(result);

                // تاخیر کوتاه برای جلوگیری از اسپم
                await Task.Delay(500);
            }

            return results;
        }

        /// <summary>
        /// ارسال با استفاده از قالب
        /// </summary>
        public async Task<EmailResult> SendWithTemplateAsync(
            int templateId,
            string toEmail,
            string toName,
            Dictionary<string, string> parameters,
            List<string> attachmentPaths = null)
        {
            var template = await _context.EmailTemplate_Tbl.FindAsync(templateId);
            if (template == null)
            {
                return new EmailResult
                {
                    Success = false,
                    ErrorMessage = "قالب یافت نشد"
                };
            }

            // جایگزینی پارامترها
            string subject = template.SubjectTemplate;
            string body = template.BodyHtml;

            foreach (var param in parameters)
            {
                subject = subject.Replace($"{{{param.Key}}}", param.Value);
                body = body.Replace($"{{{param.Key}}}", param.Value);
            }

            // بروزرسانی تعداد استفاده
            template.UsageCount++;
            await _context.SaveChangesAsync();

            return await SendEmailAsync(toEmail, subject, body, toName, true, attachmentPaths);
        }

        /// <summary>
        /// تست اتصال SMTP
        /// </summary>
        public async Task<bool> TestSmtpConnectionAsync()
        {
            try
            {
                var settings = _context.Settings_Tbl.FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.SmtpHost))
                    return false;

                using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
                {
                    EnableSsl = settings.SmtpEnableSsl,
                    Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                    Timeout = 10000
                };

                // ارسال ایمیل تست به خود
                var testMessage = new MailMessage
                {
                    From = new MailAddress(settings.SmtpFromEmail),
                    Subject = "Test Email",
                    Body = "اتصال SMTP با موفقیت برقرار شد",
                    IsBodyHtml = false
                };
                testMessage.To.Add(settings.SmtpFromEmail);

                await smtpClient.SendMailAsync(testMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    // ========== مدل‌های کمکی ==========

    public class EmailResult
    {
        public bool Success { get; set; }
        public string RecipientEmail { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentDate { get; set; }
        public int AttachmentCount { get; set; }
        public long AttachmentTotalSizeKB { get; set; }
    }

    public class EmailRecipient
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}