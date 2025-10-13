using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.SmsProviders
{
    /// <summary>
    /// پیاده‌سازی سرویس راه آفتاب (SunWay)
    /// </summary>
    public class SunWaySmsProvider : ISmsProvider
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _senderNumber;
        private const string BASE_URL = "https://sms.sunwaysms.com/smsws/HttpService.ashx";

        public string ProviderCode => "SUNWAY";
        public string ProviderName => "راه آفتاب";

        public SunWaySmsProvider(string username, string password, string senderNumber)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _senderNumber = senderNumber ?? throw new ArgumentNullException(nameof(senderNumber));
        }

        public async Task<SmsResult> SendSingleAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                return new SmsResult
                {
                    IsSuccess = false,
                    ErrorMessage = "شماره موبایل یا متن پیام نامعتبر است",
                    PhoneNumber = phoneNumber
                };
            }

            try
            {
                long checkingMessageId = DateTime.Now.Ticks;
                long[] messageIds = await SendArrayInternal(
                    new[] { phoneNumber },
                    message,
                    false,
                    new[] { checkingMessageId }
                );

                if (messageIds != null && messageIds.Length > 0 && (messageIds[0] > 1000 || messageIds[0] == 50))
                {
                    return new SmsResult
                    {
                        IsSuccess = true,
                        MessageId = messageIds[0],
                        PhoneNumber = phoneNumber
                    };
                }

                return new SmsResult
                {
                    IsSuccess = false,
                    ErrorMessage = "خطا در ارسال پیام",
                    PhoneNumber = phoneNumber
                };
            }
            catch (Exception ex)
            {
                return new SmsResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"خطای سیستمی: {ex.Message}",
                    PhoneNumber = phoneNumber
                };
            }
        }

        public async Task<SmsResult> SendBulkAsync(string[] phoneNumbers, string message)
        {
            if (phoneNumbers == null || phoneNumbers.Length == 0)
            {
                return new SmsResult
                {
                    IsSuccess = false,
                    ErrorMessage = "لیست شماره‌ها خالی است"
                };
            }

            try
            {
                long[] checkingMessageIds = phoneNumbers.Select((_, i) => DateTime.Now.Ticks + i).ToArray();
                long[] messageIds = await SendArrayInternal(phoneNumbers, message, false, checkingMessageIds);

                bool success = messageIds != null && messageIds.Length > 0 && (messageIds[0] > 1000 || messageIds[0] == 50);

                return new SmsResult
                {
                    IsSuccess = success,
                    MessageId = success ? messageIds[0] : null,
                    ErrorMessage = success ? null : "خطا در ارسال پیام‌های گروهی"
                };
            }
            catch (Exception ex)
            {
                return new SmsResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"خطای سیستمی: {ex.Message}"
                };
            }
        }

        public async Task<List<SmsResult>> SendMultipleAsync(List<SmsMessage> messages)
        {
            var results = new List<SmsResult>();

            foreach (var msg in messages)
            {
                var result = await SendSingleAsync(msg.PhoneNumber, msg.Message);
                results.Add(result);
                
                // تاخیر کوتاه برای جلوگیری از فشار به API
                await Task.Delay(200);
            }

            return results;
        }

        public async Task<long> GetCreditAsync()
        {
            try
            {
                string requestUrl = $"{BASE_URL}?service=GetCredit&UserName={WebUtility.UrlEncode(_username)}&Password={WebUtility.UrlEncode(_password)}";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    string response = await client.GetStringAsync(requestUrl);

                    if (long.TryParse(response, out long credit))
                    {
                        return credit;
                    }
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<SmsStatusResult> GetMessageStatusAsync(long messageId)
        {
            try
            {
                string requestUrl = $"{BASE_URL}?service=GetMessageStatus&username={WebUtility.UrlEncode(_username)}&password={WebUtility.UrlEncode(_password)}&messageid={messageId}";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (int.TryParse(responseContent.Trim(), out int statusCode))
                        {
                            return new SmsStatusResult
                            {
                                MessageId = messageId,
                                StatusCode = statusCode,
                                StatusDescription = GetStatusDescription(statusCode),
                                IsDelivered = statusCode == 1 || statusCode == 2
                            };
                        }
                    }

                    return new SmsStatusResult
                    {
                        MessageId = messageId,
                        StatusCode = 6,
                        StatusDescription = "وضعیت نامشخص",
                        IsDelivered = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new SmsStatusResult
                {
                    MessageId = messageId,
                    StatusCode = 6,
                    StatusDescription = $"خطا: {ex.Message}",
                    IsDelivered = false
                };
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                long credit = await GetCreditAsync();
                return credit >= 0;
            }
            catch
            {
                return false;
            }
        }

        // ========== متدهای داخلی ==========

        private async Task<long[]> SendArrayInternal(
            string[] recipients,
            string message,
            bool isFlash,
            long[] checkingMessageIds)
        {
            try
            {
                string recipientNumbers = string.Join(",", recipients);
                string checkingIds = string.Join(",", checkingMessageIds);

                string requestUrl = $"{BASE_URL}?service=SendArray&UserName={_username}&Password={_password}" +
                                   $"&To={WebUtility.UrlEncode(recipientNumbers)}&Message={WebUtility.UrlEncode(message)}" +
                                   $"&From={WebUtility.UrlEncode(_senderNumber)}&Flash={(isFlash ? "true" : "false")}" +
                                   $"&chkMessageId={WebUtility.UrlEncode(checkingIds)}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "GET";
                request.Timeout = 30000;

                using (WebResponse response = await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string result = await reader.ReadToEndAsync();

                    if (string.IsNullOrWhiteSpace(result))
                    {
                        return new long[0];
                    }

                    return result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => {
                                    long.TryParse(s.Trim(), out long messageId);
                                    return messageId;
                                })
                                .ToArray();
                }
            }
            catch
            {
                return new long[0];
            }
        }

        private string GetStatusDescription(int statusCode)
        {
            return statusCode switch
            {
                0 => "در حال پردازش",
                1 => "ارسال شده",
                2 => "خوانده شده",
                3 => "منقضی شده",
                4 => "ناموفق در تحویل",
                5 => "رد شده",
                _ => "وضعیت نامشخص"
            };
        }
    }
}