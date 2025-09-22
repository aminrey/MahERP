using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس کمکی برای ثبت آسان لاگ‌های فعالیت کاربران
    /// این سرویس عملیات لاگینگ را ساده‌تر می‌کند
    /// </summary>
    public class ActivityLoggerService
    {
        private readonly IUserActivityLogRepository _logRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// سازنده سرویس
        /// </summary>
        /// <param name="logRepository">مخزن لاگ فعالیت‌ها</param>
        /// <param name="httpContextAccessor">دسترسی به HttpContext</param>
        public ActivityLoggerService(
            IUserActivityLogRepository logRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _logRepository = logRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// ثبت لاگ ساده فعالیت
        /// </summary>
        /// <param name="activityType">نوع فعالیت</param>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="actionName">نام عمل</param>
        /// <param name="description">شرح فعالیت</param>
        /// <param name="recordId">شناسه رکورد (اختیاری)</param>
        /// <param name="entityType">نوع انتیتی (اختیاری)</param>
        /// <param name="recordTitle">عنوان رکورد (اختیاری)</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> LogActivityAsync(
            ActivityTypeEnum activityType,
            string moduleName,
            string actionName,
            string description,
            string recordId = null,
            string entityType = null,
            string recordTitle = null)
        {
            try
            {
                var log = new UserActivityLog
                {
                    UserId = GetCurrentUserId(),
                    ActivityType = (byte)activityType,
                    ModuleName = moduleName,
                    ActionName = actionName,
                    RecordId = recordId,
                    EntityType = entityType,
                    RecordTitle = recordTitle,
                    Description = description,
                    ResultStatus = 0, // موفق
                    ActivityDateTime = DateTime.Now,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    RequestUrl = GetRequestUrl(),
                    HttpMethod = GetHttpMethod(),
                    SessionId = GetSessionId(),
                    BranchId = GetCurrentBranchId(),
                    IsSensitive = IsSensitiveActivity(activityType, moduleName),
                    ImportanceLevel = GetImportanceLevel(activityType),
                    DeviceType = GetDeviceType(),
                    CorrelationId = GetOrCreateCorrelationId()
                };

                return await _logRepository.CreateLogAsync(log);
            }
            catch
            {
                // در صورت خطا در ثبت لاگ، خطا را خورده تا عملکرد اصلی سیستم مختل نشود
                return 0;
            }
        }

        /// <summary>
        /// ثبت لاگ با اطلاعات تغییرات
        /// </summary>
        /// <param name="activityType">نوع فعالیت</param>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="actionName">نام عمل</param>
        /// <param name="description">شرح فعالیت</param>
        /// <param name="oldValues">مقادیر قبلی</param>
        /// <param name="newValues">مقادیر جدید</param>
        /// <param name="recordId">شناسه رکورد</param>
        /// <param name="entityType">نوع انتیتی</param>
        /// <param name="recordTitle">عنوان رکورد</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> LogChangeAsync<T>(
            ActivityTypeEnum activityType,
            string moduleName,
            string actionName,
            string description,
            T oldValues,
            T newValues,
            string recordId = null,
            string entityType = null,
            string recordTitle = null)
        {
            try
            {
                var log = new UserActivityLog
                {
                    UserId = GetCurrentUserId(),
                    ActivityType = (byte)activityType,
                    ModuleName = moduleName,
                    ActionName = actionName,
                    RecordId = recordId,
                    EntityType = entityType,
                    RecordTitle = recordTitle,
                    Description = description,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) : null,
                    ResultStatus = 0,
                    ActivityDateTime = DateTime.Now,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    RequestUrl = GetRequestUrl(),
                    HttpMethod = GetHttpMethod(),
                    SessionId = GetSessionId(),
                    BranchId = GetCurrentBranchId(),
                    IsSensitive = IsSensitiveActivity(activityType, moduleName) || IsSensitiveChange(oldValues, newValues),
                    ImportanceLevel = GetImportanceLevel(activityType),
                    DeviceType = GetDeviceType(),
                    CorrelationId = GetOrCreateCorrelationId()
                };

                return await _logRepository.CreateLogAsync(log);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ثبت لاگ خطا
        /// </summary>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="actionName">نام عمل</param>
        /// <param name="description">شرح خطا</param>
        /// <param name="exception">جزئیات خطا</param>
        /// <param name="recordId">شناسه رکورد (اختیاری)</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> LogErrorAsync(
            string moduleName,
            string actionName,
            string description,
            Exception exception = null,
            string recordId = null)
        {
            try
            {
                var log = new UserActivityLog
                {
                    UserId = GetCurrentUserId(),
                    ActivityType = (byte)ActivityTypeEnum.Error,
                    ModuleName = moduleName,
                    ActionName = actionName,
                    RecordId = recordId,
                    Description = description,
                    ErrorMessage = exception?.Message,
                    RequestParameters = exception != null ? JsonSerializer.Serialize(new
                    {
                        ExceptionType = exception.GetType().Name,
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    }) : null,
                    ResultStatus = 2, // خطا
                    ActivityDateTime = DateTime.Now,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    RequestUrl = GetRequestUrl(),
                    HttpMethod = GetHttpMethod(),
                    SessionId = GetSessionId(),
                    BranchId = GetCurrentBranchId(),
                    IsSensitive = true, // خطاها همیشه حساس هستند
                    ImportanceLevel = 2, // بحرانی
                    DeviceType = GetDeviceType(),
                    CorrelationId = GetOrCreateCorrelationId()
                };

                return await _logRepository.CreateLogAsync(log);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ثبت لاگ ورود کاربر
        /// </summary>
        /// <param name="isSuccessful">آیا ورود موفق بوده؟</param>
        /// <param name="username">نام کاربری</param>
        /// <param name="failureReason">دلیل شکست (در صورت ناموفق بودن)</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> LogLoginAsync(bool isSuccessful, string username, string failureReason = null)
        {
            try
            {
                var log = new UserActivityLog
                {
                    UserId = isSuccessful ? GetCurrentUserId() : username,
                    ActivityType = (byte)ActivityTypeEnum.Login,
                    ModuleName = "Authentication",
                    ActionName = "Login",
                    Description = isSuccessful ? "ورود موفق به سیستم" : $"تلاش ناموفق ورود: {failureReason}",
                    ErrorMessage = isSuccessful ? null : failureReason,
                    ResultStatus = (byte)(isSuccessful ? 0 : 1), // موفق یا ناموفق
                    ActivityDateTime = DateTime.Now,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    RequestUrl = GetRequestUrl(),
                    HttpMethod = GetHttpMethod(),
                    SessionId = GetSessionId(),
                    BranchId = isSuccessful ? GetCurrentBranchId() : null,
                    IsSensitive = !isSuccessful, // ورودهای ناموفق حساس هستند
                    ImportanceLevel = (byte)(isSuccessful ? 0 : 1), // عادی یا مهم
                    DeviceType = GetDeviceType(),
                    CorrelationId = GetOrCreateCorrelationId()
                };

                return await _logRepository.CreateLogAsync(log);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ثبت لاگ خروج کاربر
        /// </summary>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> LogLogoutAsync()
        {
            return await LogActivityAsync(
                ActivityTypeEnum.Logout,
                "Authentication",
                "Logout",
                "خروج از سیستم"
            );
        }

        #region متدهای کمکی - Helper Methods

        /// <summary>
        /// دریافت شناسه کاربر فعلی
        /// </summary>
        /// <returns>شناسه کاربر</returns>
        private string GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return "Anonymous";
        }

        /// <summary>
        /// دریافت شناسه شعبه فعلی کاربر
        /// </summary>
        /// <returns>شناسه شعبه</returns>
        private int? GetCurrentBranchId()
        {
            var context = _httpContextAccessor.HttpContext;
            var branchClaim = context?.User?.FindFirst("BranchId")?.Value;
            if (int.TryParse(branchClaim, out int branchId))
            {
                return branchId;
            }
            return null;
        }

        /// <summary>
        /// دریافت آدرس IP کلاینت
        /// </summary>
        /// <returns>آدرس IP</returns>
        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress ?? "Unknown";
        }

        /// <summary>
        /// دریافت User Agent
        /// </summary>
        /// <returns>User Agent</returns>
        private string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request?.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }

        /// <summary>
        /// دریافت URL درخواست
        /// </summary>
        /// <returns>URL درخواست</returns>
        private string GetRequestUrl()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request == null) return "Unknown";

            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        }

        /// <summary>
        /// دریافت نوع درخواست HTTP
        /// </summary>
        /// <returns>نوع درخواست</returns>
        private string GetHttpMethod()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request?.Method ?? "Unknown";
        }

        /// <summary>
        /// دریافت شناسه جلسه
        /// </summary>
        /// <returns>شناسه جلسه</returns>
        private string GetSessionId()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Session?.Id;
        }

        /// <summary>
        /// تشخیص نوع دستگاه بر اساس User Agent
        /// </summary>
        /// <returns>نوع دستگاه</returns>
        private byte? GetDeviceType()
        {
            var userAgent = GetUserAgent()?.ToLower();
            if (string.IsNullOrEmpty(userAgent)) return null;

            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
                return 1; // موبایل

            if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return 2; // تبلت

            return 0; // کامپیوتر
        }

        /// <summary>
        /// تعیین آیا فعالیت حساس است یا نه
        /// </summary>
        /// <param name="activityType">نوع فعالیت</param>
        /// <param name="moduleName">نام ماژول</param>
        /// <returns>true اگر حساس باشد</returns>
        private static bool IsSensitiveActivity(ActivityTypeEnum activityType, string moduleName)
        {
            // فعالیت‌های حساس
            var sensitiveActivities = new[]
            {
                ActivityTypeEnum.Delete,
                ActivityTypeEnum.Approve,
                ActivityTypeEnum.Reject
            };

            // ماژول‌های حساس
            var sensitiveModules = new[]
            {
                "Users", "UserManager", "RolePattern", "UserPermission",
                "Branch", "Contract", "Financial"
            };

            return sensitiveActivities.Contains(activityType) ||
                   sensitiveModules.Any(m => moduleName.Contains(m, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// تعیین آیا تغییرات حساس است یا نه
        /// </summary>
        /// <param name="oldValues">مقادیر قبلی</param>
        /// <param name="newValues">مقادیر جدید</param>
        /// <returns>true اگر تغییرات حساس باشد</returns>
        private static bool IsSensitiveChange<T>(T oldValues, T newValues)
        {
            // فیلدهای حساس که تغییر آنها باید لاگ حساسی ثبت شود
            var sensitiveFields = new[]
            {
                "password", "email", "phone", "salary", "role", "permission",
                "isactive", "isdeleted", "status"
            };

            try
            {
                var oldJson = JsonSerializer.Serialize(oldValues).ToLower();
                var newJson = JsonSerializer.Serialize(newValues).ToLower();

                return sensitiveFields.Any(field => 
                    oldJson.Contains(field) || newJson.Contains(field));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تعیین سطح اهمیت بر اساس نوع فعالیت
        /// </summary>
        /// <param name="activityType">نوع فعالیت</param>
        /// <returns>سطح اهمیت</returns>
        private static byte GetImportanceLevel(ActivityTypeEnum activityType)
        {
            return activityType switch
            {
                ActivityTypeEnum.Delete => 2,      // بحرانی
                ActivityTypeEnum.Approve => 1,     // مهم
                ActivityTypeEnum.Reject => 1,      // مهم
                ActivityTypeEnum.Create => 1,      // مهم
                ActivityTypeEnum.Edit => 1,        // مهم
                ActivityTypeEnum.Error => 2,       // بحرانی
                _ => 0                              // عادی
            };
        }

        /// <summary>
        /// دریافت یا ایجاد شناسه همبستگی
        /// </summary>
        /// <returns>شناسه همبستگی</returns>
        private Guid GetOrCreateCorrelationId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.ContainsKey("CorrelationId") == true)
            {
                return (Guid)context.Items["CorrelationId"];
            }

            var correlationId = Guid.NewGuid();
            if (context != null)
            {
                context.Items["CorrelationId"] = correlationId;
            }

            return correlationId;
        }

        #endregion
    }

    /// <summary>
    /// انواع فعالیت برای استفاده آسان‌تر
    /// </summary>
    public enum ActivityTypeEnum : byte
    {
        /// <summary>
        /// مشاهده
        /// </summary>
        View = 0,

        /// <summary>
        /// ایجاد
        /// </summary>
        Create = 1,

        /// <summary>
        /// ویرایش
        /// </summary>
        Edit = 2,

        /// <summary>
        /// به‌روزرسانی
        /// </summary>
        Update = 2, 

        /// <summary>
        /// حذف
        /// </summary>
        Delete = 3,

        /// <summary>
        /// تایید
        /// </summary>
        Approve = 4,

        /// <summary>
        /// رد
        /// </summary>
        Reject = 5,

        /// <summary>
        /// ورود به سیستم
        /// </summary>
        Login = 6,

        /// <summary>
        /// خروج از سیستم
        /// </summary>
        Logout = 7,

        /// <summary>
        /// دانلود فایل
        /// </summary>
        Download = 8,

        /// <summary>
        /// آپلود فایل
        /// </summary>
        Upload = 9,

        /// <summary>
        /// جستجو
        /// </summary>
        Search = 10,

        /// <summary>
        /// چاپ
        /// </summary>
        Print = 11,

        /// <summary>
        /// ارسال ایمیل
        /// </summary>
        Email = 12,

        /// <summary>
        /// ارسال پیامک
        /// </summary>
        SMS = 13,

        /// <summary>
        /// خطا
        /// </summary>
        Error = 99
    }
}