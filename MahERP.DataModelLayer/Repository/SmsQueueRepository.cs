using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class SmsQueueRepository : ISmsQueueRepository
    {
        private readonly AppDbContext _context;

        public SmsQueueRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// افزودن پیامک به صف
        /// </summary>
        public async Task<int> EnqueueAsync(SmsQueue smsQueue)
        {
            _context.SmsQueue_Tbl.Add(smsQueue);
            await _context.SaveChangesAsync();
            return smsQueue.Id;
        }

        /// <summary>
        /// افزودن چند پیامک به صف
        /// </summary>
        public async Task<int> EnqueueBulkAsync(List<SmsQueue> smsQueues)
        {
            _context.SmsQueue_Tbl.AddRange(smsQueues);
            await _context.SaveChangesAsync();
            return smsQueues.Count;
        }

        /// <summary>
        /// دریافت پیامک‌های آماده پردازش
        /// </summary>
        public async Task<List<SmsQueue>> GetPendingItemsAsync(int batchSize = 10)
        {
            var now = DateTime.Now;

            return await _context.SmsQueue_Tbl
                .Where(q => q.Status == 0 && // در صف
                           (!q.ScheduledDate.HasValue || q.ScheduledDate.Value <= now))
                .OrderByDescending(q => q.Priority)
                .ThenBy(q => q.CreatedDate)
                .Take(batchSize)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پیامک‌های خطا که قابل تلاش مجدد هستند
        /// </summary>
        public async Task<List<SmsQueue>> GetRetryableItemsAsync(int batchSize = 5)
        {
            return await _context.SmsQueue_Tbl
                .Where(q => q.Status == 3 && // خطا
                           q.RetryCount < q.MaxRetryCount)
                .OrderBy(q => q.LastAttemptDate)
                .Take(batchSize)
                .ToListAsync();
        }

        /// <summary>
        /// بروزرسانی وضعیت به "در حال پردازش"
        /// </summary>
        public async Task MarkAsProcessingAsync(int id)
        {
            var item = await _context.SmsQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 1; // در حال پردازش
                item.LastAttemptDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان موفق
        /// </summary>
        public async Task MarkAsSuccessAsync(int id, int smsLogId)
        {
            var item = await _context.SmsQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 2; // ارسال شده
                item.ProcessedDate = DateTime.Now;
                item.SmsLogId = smsLogId;
                item.ErrorMessage = null;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان خطا
        /// </summary>
        public async Task MarkAsFailedAsync(int id, string errorMessage)
        {
            var item = await _context.SmsQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 3; // خطا
                item.RetryCount++;
                item.ErrorMessage = errorMessage;
                item.LastAttemptDate = DateTime.Now;

                // اگر تلاش‌ها تمام شد، وضعیت را به "لغو شده" تغییر می‌دهیم
                if (item.RetryCount >= item.MaxRetryCount)
                {
                    item.Status = 4; // لغو شده
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// لغو پیامک
        /// </summary>
        public async Task CancelAsync(int id)
        {
            var item = await _context.SmsQueue_Tbl.FindAsync(id);
            if (item != null && item.Status == 0)
            {
                item.Status = 4; // لغو شده
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// پاکسازی صف (حذف موارد قدیمی)
        /// </summary>
        public async Task<int> CleanupOldItemsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);

            var oldItems = await _context.SmsQueue_Tbl
                .Where(q => (q.Status == 2 || q.Status == 4) && // ارسال شده یا لغو شده
                           q.CreatedDate < cutoffDate)
                .ToListAsync();

            _context.SmsQueue_Tbl.RemoveRange(oldItems);
            await _context.SaveChangesAsync();

            return oldItems.Count;
        }

        /// <summary>
        /// دریافت آمار صف
        /// </summary>
        public async Task<QueueStatistics> GetStatisticsAsync()
        {
            return new QueueStatistics
            {
                Pending = await _context.SmsQueue_Tbl.CountAsync(q => q.Status == 0),
                Processing = await _context.SmsQueue_Tbl.CountAsync(q => q.Status == 1),
                Completed = await _context.SmsQueue_Tbl.CountAsync(q => q.Status == 2),
                Failed = await _context.SmsQueue_Tbl.CountAsync(q => q.Status == 3),
                Cancelled = await _context.SmsQueue_Tbl.CountAsync(q => q.Status == 4)
            };
        }
    }
   
    public class QueueStatistics
    {
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Completed { get; set; }
        public int Failed { get; set; }
        public int Cancelled { get; set; }
        public int Total => Pending + Processing + Completed + Failed + Cancelled;
    }
}