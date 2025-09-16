using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// مخزن لاگ فعالیت‌های کاربران
    /// این کلاس شامل تمام عملیات مربوط به مدیریت لاگ‌های سیستم است
    /// </summary>
    public class UserActivityLogRepository : IUserActivityLogRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="context">کنتکست پایگاه داده</param>
        public UserActivityLogRepository(AppDbContext context)
        {
            _context = context;
        }

        #region عملیات پایه - Basic Operations

        /// <summary>
        /// ثبت لاگ جدید فعالیت کاربر
        /// </summary>
        /// <param name="log">اطلاعات لاگ برای ثبت</param>
        /// <returns>شناسه لاگ ثبت شده</returns>
        public async Task<long> CreateLogAsync(UserActivityLog log)
        {
            try
            {
                // تنظیم زمان ثبت
                log.ActivityDateTime = DateTime.Now;

                // افزودن به کنتکست
                _context.UserActivityLog_Tbl.Add(log);
                await _context.SaveChangesAsync();

                return log.Id;
            }
            catch (Exception)
            {
                // در صورت خطا در ثبت لاگ، خطا را خورده و شناسه 0 برمی‌گرداند
                // تا مشکلی در عملکرد اصلی سیستم ایجاد نشود
                return 0;
            }
        }

        /// <summary>
        /// دریافت لاگ بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه لاگ</param>
        /// <returns>اطلاعات کامل لاگ</returns>
        public async Task<UserActivityLog> GetLogByIdAsync(long id)
        {
            return await _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Include(l => l.Branch)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        /// <summary>
        /// دریافت لیست کامل لاگ‌ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لیست لاگ‌ها</returns>
        public async Task<LogListViewModel> GetLogsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var query = _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Include(l => l.Branch)
                .Where(l => !l.IsArchived);

            var totalRecords = await query.CountAsync();
            
            var logs = await query
                .OrderByDescending(l => l.ActivityDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LogViewModel
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User.UserName,
                    UserFullName = $"{l.User.FirstName} {l.User.LastName}",
                    ActivityType = l.ActivityType,
                    ActivityTypeText = GetActivityTypeText(l.ActivityType),
                    ModuleName = l.ModuleName,
                    ActionName = l.ActionName,
                    RecordId = l.RecordId,
                    EntityType = l.EntityType,
                    RecordTitle = l.RecordTitle,
                    Description = l.Description,
                    ResultStatus = l.ResultStatus,
                    ResultStatusText = GetResultStatusText(l.ResultStatus),
                    ErrorMessage = l.ErrorMessage,
                    ActivityDateTime = l.ActivityDateTime,
                    ActivityDateTimePersian = ConvertToPersianDate(l.ActivityDateTime),
                    IpAddress = l.IpAddress,
                    RequestUrl = l.RequestUrl,
                    HttpMethod = l.HttpMethod,
                    ProcessingTimeMs = l.ProcessingTimeMs,
                    BranchId = l.BranchId,
                    BranchName = l.Branch != null ? l.Branch.Name : null,
                    IsSensitive = l.IsSensitive,
                    ImportanceLevel = l.ImportanceLevel,
                    ImportanceLevelText = GetImportanceLevelText(l.ImportanceLevel),
                    DeviceType = l.DeviceType,
                    DeviceTypeText = GetDeviceTypeText(l.DeviceType),
                    DeviceInfo = l.DeviceInfo,
                    Location = l.Location,
                    CorrelationId = l.CorrelationId,
                    IsArchived = l.IsArchived
                })
                .ToListAsync();

            return new LogListViewModel
            {
                Logs = logs,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region جستجو و فیلتر - Search & Filter

        /// <summary>
        /// جستجوی پیشرفته در لاگ‌ها
        /// </summary>
        /// <param name="searchModel">مدل جستجو شامل فیلترها</param>
        /// <returns>نتایج جستجو</returns>
        public async Task<LogListViewModel> SearchLogsAsync(LogSearchViewModel searchModel)
        {
            var query = _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Include(l => l.Branch)
                .Where(l => !l.IsArchived);

            // اعمال فیلترها
            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
                query = query.Where(l => l.UserId == searchModel.UserId);

            if (!string.IsNullOrWhiteSpace(searchModel.ModuleName))
                query = query.Where(l => l.ModuleName == searchModel.ModuleName);

            if (!string.IsNullOrWhiteSpace(searchModel.ActionName))
                query = query.Where(l => l.ActionName == searchModel.ActionName);

            if (searchModel.ActivityType.HasValue)
                query = query.Where(l => l.ActivityType == searchModel.ActivityType.Value);

            if (searchModel.ResultStatus.HasValue)
                query = query.Where(l => l.ResultStatus == searchModel.ResultStatus.Value);

            if (searchModel.FromDate.HasValue)
                query = query.Where(l => l.ActivityDateTime >= searchModel.FromDate.Value);

            if (searchModel.ToDate.HasValue)
                query = query.Where(l => l.ActivityDateTime <= searchModel.ToDate.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(searchModel.IpAddress))
                query = query.Where(l => l.IpAddress == searchModel.IpAddress);

            if (searchModel.BranchId.HasValue)
                query = query.Where(l => l.BranchId == searchModel.BranchId.Value);

            if (searchModel.OnlySensitive.HasValue && searchModel.OnlySensitive.Value)
                query = query.Where(l => l.IsSensitive);

            if (searchModel.ImportanceLevel.HasValue)
                query = query.Where(l => l.ImportanceLevel == searchModel.ImportanceLevel.Value);

            if (searchModel.OnlyErrors.HasValue && searchModel.OnlyErrors.Value)
                query = query.Where(l => l.ResultStatus == 2); // 2 = خطا

            if (!string.IsNullOrWhiteSpace(searchModel.SearchText))
                query = query.Where(l => l.Description.Contains(searchModel.SearchText) ||
                                       l.RecordTitle.Contains(searchModel.SearchText));

            if (!string.IsNullOrWhiteSpace(searchModel.RecordId))
                query = query.Where(l => l.RecordId == searchModel.RecordId);

            if (!string.IsNullOrWhiteSpace(searchModel.EntityType))
                query = query.Where(l => l.EntityType == searchModel.EntityType);

            // مرتب‌سازی
            query = ApplySorting(query, searchModel.SortBy, searchModel.SortDirection);

            var totalRecords = await query.CountAsync();

            var logs = await query
                .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(l => new LogViewModel
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User.UserName,
                    UserFullName = $"{l.User.FirstName} {l.User.LastName}",
                    ActivityType = l.ActivityType,
                    ActivityTypeText = GetActivityTypeText(l.ActivityType),
                    ModuleName = l.ModuleName,
                    ActionName = l.ActionName,
                    RecordId = l.RecordId,
                    EntityType = l.EntityType,
                    RecordTitle = l.RecordTitle,
                    Description = l.Description,
                    ResultStatus = l.ResultStatus,
                    ResultStatusText = GetResultStatusText(l.ResultStatus),
                    ErrorMessage = l.ErrorMessage,
                    ActivityDateTime = l.ActivityDateTime,
                    ActivityDateTimePersian = ConvertToPersianDate(l.ActivityDateTime),
                    IpAddress = l.IpAddress,
                    RequestUrl = l.RequestUrl,
                    HttpMethod = l.HttpMethod,
                    ProcessingTimeMs = l.ProcessingTimeMs,
                    BranchId = l.BranchId,
                    BranchName = l.Branch != null ? l.Branch.Name : null,
                    IsSensitive = l.IsSensitive,
                    ImportanceLevel = l.ImportanceLevel,
                    ImportanceLevelText = GetImportanceLevelText(l.ImportanceLevel),
                    DeviceType = l.DeviceType,
                    DeviceTypeText = GetDeviceTypeText(l.DeviceType),
                    DeviceInfo = l.DeviceInfo,
                    Location = l.Location,
                    CorrelationId = l.CorrelationId,
                    IsArchived = l.IsArchived
                })
                .ToListAsync();

            return new LogListViewModel
            {
                Logs = logs,
                CurrentPage = searchModel.PageNumber,
                PageSize = searchModel.PageSize,
                TotalRecords = totalRecords
            };
        }

        /// <summary>
        /// دریافت لاگ‌های یک کاربر خاص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های کاربر</returns>
        public async Task<LogListViewModel> GetUserLogsAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        /// <summary>
        /// دریافت لاگ‌های یک ماژول خاص
        /// </summary>
        /// <param name="moduleName">نام ماژول</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های ماژول</returns>
        public async Task<LogListViewModel> GetModuleLogsAsync(string moduleName, int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                ModuleName = moduleName,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        /// <summary>
        /// دریافت لاگ‌های حساس سیستم
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های حساس</returns>
        public async Task<LogListViewModel> GetSensitiveLogsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                OnlySensitive = true,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        /// <summary>
        /// دریافت لاگ‌های بازه زمانی خاص
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های بازه زمانی</returns>
        public async Task<LogListViewModel> GetLogsByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        #endregion

        #region آمار و گزارش - Statistics & Reports

        /// <summary>
        /// دریافت آمار کلی فعالیت‌های سیستم
        /// </summary>
        /// <returns>آمار کلی</returns>
        public async Task<LogStatisticsViewModel> GetLogStatisticsAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var lastWeek = today.AddDays(-7);
            var lastMonth = today.AddMonths(-1);
            var last24Hours = now.AddHours(-24);

            var stats = new LogStatisticsViewModel
            {
                TotalLogs = await _context.UserActivityLog_Tbl.CountAsync(),
                TodayLogs = await _context.UserActivityLog_Tbl.CountAsync(l => l.ActivityDateTime >= today),
                LastWeekLogs = await _context.UserActivityLog_Tbl.CountAsync(l => l.ActivityDateTime >= lastWeek),
                LastMonthLogs = await _context.UserActivityLog_Tbl.CountAsync(l => l.ActivityDateTime >= lastMonth),
                ErrorsLast24Hours = await _context.UserActivityLog_Tbl.CountAsync(l => l.ActivityDateTime >= last24Hours && l.ResultStatus == 2),
                SensitiveActivitiesLastWeek = await _context.UserActivityLog_Tbl.CountAsync(l => l.ActivityDateTime >= lastWeek && l.IsSensitive),
                ActiveUsersToday = await _context.UserActivityLog_Tbl.Where(l => l.ActivityDateTime >= today).Select(l => l.UserId).Distinct().CountAsync(),
                UniqueIpsToday = await _context.UserActivityLog_Tbl.Where(l => l.ActivityDateTime >= today).Select(l => l.IpAddress).Distinct().CountAsync(),
                ArchivedLogs = await _context.UserActivityLog_Tbl.CountAsync(l => l.IsArchived)
            };

            // محاسبه میانگین زمان پردازش
            var avgProcessingTime = await _context.UserActivityLog_Tbl
                .Where(l => l.ProcessingTimeMs.HasValue && l.ActivityDateTime >= lastWeek)
                .AverageAsync(l => (double?)l.ProcessingTimeMs.Value);

            stats.AverageProcessingTime = avgProcessingTime ?? 0;

            return stats;
        }

        /// <summary>
        /// دریافت آمار فعالیت کاربران در بازه زمانی
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار کاربران</returns>
        public async Task<List<UserActivityStatViewModel>> GetUserActivityStatsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Where(l => l.ActivityDateTime >= fromDate && l.ActivityDateTime <= toDate.AddDays(1))
                .GroupBy(l => new { l.UserId, l.User.UserName, l.User.FirstName, l.User.LastName })
                .Select(g => new UserActivityStatViewModel
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                    TotalActivities = g.Count(),
                    LoginCount = g.Count(l => l.ActivityType == 6), // 6 = ورود به سیستم
                    LastActivity = g.Max(l => l.ActivityDateTime),
                    ErrorCount = g.Count(l => l.ResultStatus == 2)
                })
                .OrderByDescending(u => u.TotalActivities)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت آمار فعالیت ماژول‌ها
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار ماژول‌ها</returns>
        public async Task<List<ModuleActivityStatViewModel>> GetModuleActivityStatsAsync(DateTime fromDate, DateTime toDate)
        {
            var totalActivities = await _context.UserActivityLog_Tbl
                .CountAsync(l => l.ActivityDateTime >= fromDate && l.ActivityDateTime <= toDate.AddDays(1));

            return await _context.UserActivityLog_Tbl
                .Where(l => l.ActivityDateTime >= fromDate && l.ActivityDateTime <= toDate.AddDays(1))
                .GroupBy(l => l.ModuleName)
                .Select(g => new ModuleActivityStatViewModel
                {
                    ModuleName = g.Key,
                    TotalActivities = g.Count(),
                    UniqueUsers = g.Select(l => l.UserId).Distinct().Count(),
                    ErrorCount = g.Count(l => l.ResultStatus == 2),
                    AverageProcessingTime = g.Where(l => l.ProcessingTimeMs.HasValue).Average(l => (double?)l.ProcessingTimeMs.Value) ?? 0,
                    UsagePercentage = totalActivities > 0 ? (decimal)g.Count() / totalActivities * 100 : 0
                })
                .OrderByDescending(m => m.TotalActivities)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پرفعالیت‌ترین کاربران
        /// </summary>
        /// <param name="topCount">تعداد کاربران برتر</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>لیست کاربران پرفعالیت</returns>
        public async Task<List<TopUserActivityViewModel>> GetTopActiveUsersAsync(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Include(l => l.Branch)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.ActivityDateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.ActivityDateTime <= toDate.Value.AddDays(1));

            var result = await query
                .GroupBy(l => new { l.UserId, l.User.UserName, l.User.FirstName, l.User.LastName })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                    ActivityCount = g.Count(),
                    LastActivity = g.Max(l => l.ActivityDateTime),
                    BranchName = g.Select(l => l.Branch.Name).FirstOrDefault()
                })
                .OrderByDescending(u => u.ActivityCount)
                .Take(topCount)
                .ToListAsync();

            return result.Select((u, index) => new TopUserActivityViewModel
            {
                Rank = index + 1,
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = u.FullName,
                ActivityCount = u.ActivityCount,
                LastActivity = u.LastActivity,
                BranchName = u.BranchName
            }).ToList();
        }

        #endregion

        #region مدیریت خطا - Error Management

        /// <summary>
        /// دریافت لاگ‌های خطا
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لاگ‌های خطا</returns>
        public async Task<LogListViewModel> GetErrorLogsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                OnlyErrors = true,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        /// <summary>
        /// دریافت تعداد خطاهای رخ داده در بازه زمانی
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تعداد خطاها</returns>
        public async Task<int> GetErrorCountByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.UserActivityLog_Tbl
                .CountAsync(l => l.ActivityDateTime >= fromDate && 
                               l.ActivityDateTime <= toDate.AddDays(1) && 
                               l.ResultStatus == 2);
        }

        #endregion

        #region آرشیو و نگهداری - Archive & Maintenance

        /// <summary>
        /// آرشیو کردن لاگ‌های قدیمی
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای آرشیو</param>
        /// <returns>تعداد لاگ‌های آرشیو شده</returns>
        public async Task<int> ArchiveOldLogsAsync(DateTime beforeDate)
        {
            var logsToArchive = await _context.UserActivityLog_Tbl
                .Where(l => l.ActivityDateTime < beforeDate && !l.IsArchived)
                .ToListAsync();

            foreach (var log in logsToArchive)
            {
                log.IsArchived = true;
                log.ArchivedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return logsToArchive.Count;
        }

        /// <summary>
        /// حذف لاگ‌های آرشیو شده
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا برای حذف</param>
        /// <returns>تعداد لاگ‌های حذف شده</returns>
        public async Task<int> DeleteArchivedLogsAsync(DateTime beforeDate)
        {
            var logsToDelete = await _context.UserActivityLog_Tbl
                .Where(l => l.IsArchived && l.ArchivedDate.HasValue && l.ArchivedDate.Value < beforeDate)
                .ToListAsync();

            _context.UserActivityLog_Tbl.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();

            return logsToDelete.Count;
        }

        /// <summary>
        /// دریافت حجم اشغال شده توسط لاگ‌ها
        /// </summary>
        /// <returns>حجم به مگابایت</returns>
        public async Task<decimal> GetLogsDatabaseSizeAsync()
        {
            // این محاسبه تقریبی است و بر اساس تعداد رکوردها
            var totalLogs = await _context.UserActivityLog_Tbl.CountAsync();
            
            // فرض می‌کنیم هر لاگ حدود 2KB فضا اشغال می‌کند
            return (decimal)(totalLogs * 2) / 1024; // تبدیل به مگابایت
        }

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
        public async Task<bool> ExistsSimilarLogAsync(string userId, string moduleName, string actionName, string recordId, int timeThreshold = 5)
        {
            var thresholdTime = DateTime.Now.AddSeconds(-timeThreshold);

            return await _context.UserActivityLog_Tbl
                .AnyAsync(l => l.UserId == userId &&
                              l.ModuleName == moduleName &&
                              l.ActionName == actionName &&
                              l.RecordId == recordId &&
                              l.ActivityDateTime >= thresholdTime);
        }

        /// <summary>
        /// دریافت آخرین فعالیت کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>آخرین لاگ کاربر</returns>
        public async Task<UserActivityLog> GetLastUserActivityAsync(string userId)
        {
            return await _context.UserActivityLog_Tbl
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.ActivityDateTime)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// دریافت لیست IP های منحصر به فرد
        /// </summary>
        /// <returns>لیست IP ها</returns>
        public async Task<List<string>> GetUniqueIpAddressesAsync()
        {
            return await _context.UserActivityLog_Tbl
                .Select(l => l.IpAddress)
                .Distinct()
                .Where(ip => !string.IsNullOrEmpty(ip))
                .OrderBy(ip => ip)
                .ToListAsync();
        }

        #endregion

        #region امنیت و نظارت - Security & Monitoring

        /// <summary>
        /// دریافت فعالیت‌های مشکوک
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>فعالیت‌های مشکوک</returns>
        public async Task<LogListViewModel> GetSuspiciousActivitiesAsync(int pageNumber = 1, int pageSize = 50)
        {
            var searchModel = new LogSearchViewModel
            {
                OnlySensitive = true,
                ImportanceLevel = 2, // بحرانی
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchLogsAsync(searchModel);
        }

        /// <summary>
        /// دریافت تلاش‌های ناموفق ورود
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>تلاش‌های ناموفق</returns>
        public async Task<List<FailedLoginAttemptViewModel>> GetFailedLoginAttemptsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.UserActivityLog_Tbl
                .Where(l => l.ActivityDateTime >= fromDate && 
                           l.ActivityDateTime <= toDate.AddDays(1) &&
                           l.ActivityType == 6 && // ورود به سیستم
                           l.ResultStatus == 1) // ناموفق
                .GroupBy(l => new { l.IpAddress, l.UserId })
                .Select(g => new FailedLoginAttemptViewModel
                {
                    UserName = g.Key.UserId,
                    IpAddress = g.Key.IpAddress,
                    AttemptTime = g.Max(l => l.ActivityDateTime),
                    UserAgent = g.Select(l => l.UserAgent).FirstOrDefault(),
                    FailureReason = g.Select(l => l.ErrorMessage).FirstOrDefault(),
                    AttemptsFromIp = g.Count()
                })
                .OrderByDescending(f => f.AttemptTime)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت فعالیت‌های خارج از ساعت کاری
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>فعالیت‌های خارج از ساعت</returns>
        public async Task<LogListViewModel> GetAfterHoursActivitiesAsync(DateTime fromDate, DateTime toDate)
        {
            // فعالیت‌هایی که خارج از ساعت 8 تا 18 انجام شده‌اند
            var query = _context.UserActivityLog_Tbl
                .Include(l => l.User)
                .Include(l => l.Branch)
                .Where(l => l.ActivityDateTime >= fromDate && 
                           l.ActivityDateTime <= toDate.AddDays(1) &&
                           (l.ActivityDateTime.Hour < 8 || l.ActivityDateTime.Hour > 18));

            var totalRecords = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.ActivityDateTime)
                .Take(1000) // محدود کردن به 1000 رکورد
                .Select(l => new LogViewModel
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User.UserName,
                    UserFullName = $"{l.User.FirstName} {l.User.LastName}",
                    ActivityType = l.ActivityType,
                    ActivityTypeText = GetActivityTypeText(l.ActivityType),
                    ModuleName = l.ModuleName,
                    ActionName = l.ActionName,
                    Description = l.Description,
                    ActivityDateTime = l.ActivityDateTime,
                    ActivityDateTimePersian = ConvertToPersianDate(l.ActivityDateTime),
                    IpAddress = l.IpAddress,
                    BranchName = l.Branch != null ? l.Branch.Name : null
                })
                .ToListAsync();

            return new LogListViewModel
            {
                Logs = logs,
                CurrentPage = 1,
                PageSize = 1000,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region متدهای کمکی - Helper Methods

        /// <summary>
        /// اعمال مرتب‌سازی بر روی کوئری
        /// </summary>
        /// <param name="query">کوئری اصلی</param>
        /// <param name="sortBy">فیلد مرتب‌سازی</param>
        /// <param name="sortDirection">جهت مرتب‌سازی</param>
        /// <returns>کوئری مرتب شده</returns>
        private IQueryable<UserActivityLog> ApplySorting(IQueryable<UserActivityLog> query, string sortBy, string sortDirection)
        {
            switch (sortBy?.ToLower())
            {
                case "username":
                    query = sortDirection?.ToUpper() == "ASC" 
                        ? query.OrderBy(l => l.User.UserName)
                        : query.OrderByDescending(l => l.User.UserName);
                    break;
                case "modulename":
                    query = sortDirection?.ToUpper() == "ASC"
                        ? query.OrderBy(l => l.ModuleName)
                        : query.OrderByDescending(l => l.ModuleName);
                    break;
                case "activitytype":
                    query = sortDirection?.ToUpper() == "ASC"
                        ? query.OrderBy(l => l.ActivityType)
                        : query.OrderByDescending(l => l.ActivityType);
                    break;
                case "resultstatus":
                    query = sortDirection?.ToUpper() == "ASC"
                        ? query.OrderBy(l => l.ResultStatus)
                        : query.OrderByDescending(l => l.ResultStatus);
                    break;
                default: // ActivityDateTime
                    query = sortDirection?.ToUpper() == "ASC"
                        ? query.OrderBy(l => l.ActivityDateTime)
                        : query.OrderByDescending(l => l.ActivityDateTime);
                    break;
            }

            return query;
        }

        /// <summary>
        /// تبدیل نوع فعالیت به متن فارسی
        /// </summary>
        /// <param name="activityType">نوع فعالیت</param>
        /// <returns>متن فارسی</returns>
        private static string GetActivityTypeText(byte activityType)
        {
            return activityType switch
            {
                0 => "مشاهده",
                1 => "ایجاد",
                2 => "ویرایش",
                3 => "حذف",
                4 => "تایید",
                5 => "رد",
                6 => "ورود به سیستم",
                7 => "خروج از سیستم",
                8 => "دانلود فایل",
                9 => "آپلود فایل",
                10 => "جستجو",
                11 => "چاپ",
                12 => "ارسال ایمیل",
                13 => "ارسال پیامک",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// تبدیل وضعیت نتیجه به متن فارسی
        /// </summary>
        /// <param name="resultStatus">وضعیت نتیجه</param>
        /// <returns>متن فارسی</returns>
        private static string GetResultStatusText(byte resultStatus)
        {
            return resultStatus switch
            {
                0 => "موفق",
                1 => "ناموفق",
                2 => "خطا",
                3 => "دسترسی رد شده",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// تبدیل سطح اهمیت به متن فارسی
        /// </summary>
        /// <param name="importanceLevel">سطح اهمیت</param>
        /// <returns>متن فارسی</returns>
        private static string GetImportanceLevelText(byte importanceLevel)
        {
            return importanceLevel switch
            {
                0 => "عادی",
                1 => "مهم",
                2 => "بحرانی",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// تبدیل نوع دستگاه به متن فارسی
        /// </summary>
        /// <param name="deviceType">نوع دستگاه</param>
        /// <returns>متن فارسی</returns>
        private static string GetDeviceTypeText(byte? deviceType)
        {
            return deviceType switch
            {
                0 => "کامپیوتر",
                1 => "موبایل",
                2 => "تبلت",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به شمسی
        /// </summary>
        /// <param name="dateTime">تاریخ میلادی</param>
        /// <returns>تاریخ شمسی</returns>
        private static string ConvertToPersianDate(DateTime dateTime)
        {
            // این متد باید با استفاده از کتابخانه تبدیل تاریخ پیاده‌سازی شود
            // فعلاً به صورت ساده برمی‌گرداند
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        #endregion
    }
}