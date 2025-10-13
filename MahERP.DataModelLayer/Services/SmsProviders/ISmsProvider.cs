using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.SmsProviders
{
    /// <summary>
    /// قرارداد استاندارد برای تمام خدمات‌دهندگان پیامک
    /// </summary>
    public interface ISmsProvider
    {
        /// <summary>
        /// کد یکتای خدمات‌دهنده
        /// </summary>
        string ProviderCode { get; }

        /// <summary>
        /// نام نمایشی خدمات‌دهنده
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// ارسال پیامک به یک شماره
        /// </summary>
        Task<SmsResult> SendSingleAsync(string phoneNumber, string message);

        /// <summary>
        /// ارسال پیامک به چند شماره (پیام یکسان)
        /// </summary>
        Task<SmsResult> SendBulkAsync(string[] phoneNumbers, string message);

        /// <summary>
        /// ارسال پیامک به چند شماره (پیام‌های متفاوت)
        /// </summary>
        Task<List<SmsResult>> SendMultipleAsync(List<SmsMessage> messages);

        /// <summary>
        /// دریافت اعتبار باقیمانده
        /// </summary>
        Task<long> GetCreditAsync();

        /// <summary>
        /// بررسی وضعیت پیامک ارسال شده
        /// </summary>
        Task<SmsStatusResult> GetMessageStatusAsync(long messageId);

        /// <summary>
        /// تست اتصال به سرویس
        /// </summary>
        Task<bool> TestConnectionAsync();
    }

    /// <summary>
    /// مدل پیامک برای ارسال
    /// </summary>
    public class SmsMessage
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string? RecipientName { get; set; }
    }

    /// <summary>
    /// نتیجه ارسال پیامک
    /// </summary>
    public class SmsResult
    {
        public bool IsSuccess { get; set; }
        public long? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime SendTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// نتیجه بررسی وضعیت پیامک
    /// </summary>
    public class SmsStatusResult
    {
        public long MessageId { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveryTime { get; set; }
    }
}