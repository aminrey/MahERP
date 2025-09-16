using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// رابط مخزن لاگ فعالیت‌های کاربران
    /// این رابط شامل تمام عملیات مورد نیاز برای مدیریت لاگ‌های کاربران است
    /// </summary>
    public interface IUserActivityLogRepository
    {
        #region عملیات پایه - Basic Operations

        /// <summary>
        /// ثبت لاگ جدید فعالیت کاربر
        /// </summary>
        /// <param name="log">اطلاعات لاگ برای ثبت</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        Task<long> CreateLogAsync(UserActivityLog log);

        /// <summary>
        /// دریافت لاگ بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه لاگ</param>
        /// <returns>اطلاعات کامل لاگ</returns>
        Task<UserActivityLog> GetLogByIdAsync(long id);

        /// <summary>
        /// دریافت لیست کامل لاگ‌ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لیست لاگ‌ها</returns>
        Task<LogListViewModel> GetLogsAsync(int pageNumber = 1, int pageSize = 50);

        #endregion

        #region جستجو و فیلتر - Search & Filter

        /// <summary>
        /// جستجوی پیشرفته در لاگ‌ها
        /// </summary>
        /// <param name="searchModel">مدل جستجو شامل فیلترها</param>
        /// <returns>نتایج جستجو</returns>
        Task<LogListViewModel> SearchLogsAsync(LogSearchViewModel searchModel);

        /// <summary>
        /// دریافت لاگ‌های یک کاربر خاص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های کاربر</returns>
        Task<LogListViewModel> GetUserLogsAsync(string userId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت لاگ‌های یک ماژول خاص
        /// </summary>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های ماژول</returns>
        Task<LogListViewModel> GetModuleLogsAsync(string moduleName, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت لاگ‌های حساس سیستم
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های حساس</returns>
        Task<LogListViewModel> GetSensitiveLogsAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت لاگ‌های بازه زمانی خاص
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های بازه زمانی</returns>
        Task<LogListViewModel> GetLogsByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region آمار و گزارش - Statistics & Reports

        /// <summary>
        /// دریافت آمار کلی فعالیت‌های سیستم
        /// </summary>
        /// <returns>آمار کلی</returns>
        Task<LogStatisticsViewModel> GetLogStatisticsAsync();

        /// <summary>
        /// دریافت آمار فعالیت کاربران در بازه زمانی
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار کاربران</returns>
        Task<List<UserActivityStatViewModel>> GetUserActivityStatsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت آمار فعالیت ماژول‌ها
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار ماژول‌ها</returns>
        Task<List<ModuleActivityStatViewModel>> GetModuleActivityStatsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت پرفعالیت‌ترین کاربران
        /// </summary>
        /// <param name="topCount">تعداد کاربران برتر</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>لیست کاربران پرفعالیت</returns>
        Task<List<TopUserActivityViewModel>> GetTopActiveUsersAsync(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null);

        #endregion

        #region مدیریت خطا - Error Management

        /// <summary>
        /// دریافت لاگ‌های خطا
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های خطا</returns>
        Task<LogListViewModel> GetErrorLogsAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تعداد خطاهای رخ داده در بازه زمانی
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تعداد خطاها</returns>
        Task<int> GetErrorCountByDateRangeAsync(DateTime fromDate, DateTime toDate);

        #endregion

        #region آرشیو و نگهداری - Archive & Maintenance

        /// <summary>
        /// آرشیو کردن لاگ‌های قدیمی
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای آرشیو</param>
        /// <returns>تعداد لاگ‌های آرشیو شده</returns>
        Task<int> ArchiveOldLogsAsync(DateTime beforeDate);

        /// <summary>
        /// حذف لاگ‌های آرشیو شده
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای حذف</param>
        /// <returns>تعداد لاگ‌های حذف شده</returns>
        Task<int> DeleteArchivedLogsAsync(DateTime beforeDate);

        /// <summary>
        /// دریافت حجم اشغال شده توسط لاگ‌ها
        /// </summary>
        /// <returns>حجم به مگابایت</returns>
        Task<decimal> GetLogsDatabaseSizeAsync();

        #endregion

        #region عملیات کمکی - Helper Operations

        /// <summary>
        /// بررسی وجود لاگ با مشخصات مشابه (جلوگیری از ثبت تکراری)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="actionName">نام عمل</param>
        /// <param name="recordId">شناسه رکورد</param>
        /// <param name="timeThreshold">آستانه زمانی (ثانیه)</param>
        /// <returns>true اگر لاگ مشابه وجود داشته باشد</returns>
        Task<bool> ExistsSimilarLogAsync(string userId, string moduleName, string actionName, string recordId, int timeThreshold = 5);

        /// <summary>
        /// دریافت آخرین فعالیت کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>آخرین لاگ کاربر</returns>
        Task<UserActivityLog> GetLastUserActivityAsync(string userId);

        /// <summary>
        /// دریافت لیست IP های منحصر به فرد
        /// </summary>
        /// <returns>لیست IP ها</returns>
        Task<List<string>> GetUniqueIpAddressesAsync();

        #endregion

        #region امنیت و نظارت - Security & Monitoring

        /// <summary>
        /// دریافت فعالیت‌های مشکوک
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>فعالیت‌های مشکوک</returns>
        Task<LogListViewModel> GetSuspiciousActivitiesAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تلاش‌های ناموفق ورود
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تلاش‌های ناموفق</returns>
        Task<List<FailedLoginAttemptViewModel>> GetFailedLoginAttemptsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت فعالیت‌های خارج از ساعت کاری
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>فعالیت‌های خارج از ساعت</returns>
        Task<LogListViewModel> GetAfterHoursActivitiesAsync(DateTime fromDate, DateTime toDate);

        #endregion
    }
}