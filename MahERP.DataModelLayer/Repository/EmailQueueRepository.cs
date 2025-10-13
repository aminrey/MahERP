using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class EmailQueueRepository : IEmailQueueRepository
    {
        private readonly AppDbContext _context;

        public EmailQueueRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// افزودن ایمیل به صف
        /// </summary>
        public async Task<int> EnqueueAsync(EmailQueue emailQueue)
        {
            _context.EmailQueue_Tbl.Add(emailQueue);
            await _context.SaveChangesAsync();
            return emailQueue.Id;
        }

        /// <summary>
        /// افزودن چند ایمیل به صف
        /// </summary>
        public async Task<int> EnqueueBulkAsync(List<EmailQueue> emailQueues)
        {
            _context.EmailQueue_Tbl.AddRange(emailQueues);
            await _context.SaveChangesAsync();
            return emailQueues.Count;
        }

        /// <summary>
        /// دریافت ایمیل‌های آماده پردازش
        /// </summary>
        public async Task<List<EmailQueue>> GetPendingItemsAsync(int batchSize = 5)
        {
            var now = DateTime.Now;

            return await _context.EmailQueue_Tbl
                .Where(q => q.Status == 0 &&
                           (!q.ScheduledDate.HasValue || q.ScheduledDate.Value <= now))
                .OrderByDescending(q => q.Priority)
                .ThenBy(q => q.CreatedDate)
                .Take(batchSize)
                .ToListAsync();
        }

        /// <summary>
        /// بروزرسانی وضعیت به "در حال پردازش"
        /// </summary>
        public async Task MarkAsProcessingAsync(int id)
        {
            var item = await _context.EmailQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 1;
                item.LastAttemptDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان موفق
        /// </summary>
        public async Task MarkAsSuccessAsync(int id)
        {
            var item = await _context.EmailQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 2;
                item.ProcessedDate = DateTime.Now;
                item.ErrorMessage = null;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان خطا
        /// </summary>
        public async Task MarkAsFailedAsync(int id, string errorMessage)
        {
            var item = await _context.EmailQueue_Tbl.FindAsync(id);
            if (item != null)
            {
                item.Status = 3;
                item.RetryCount++;
                item.ErrorMessage = errorMessage;
                item.LastAttemptDate = DateTime.Now;

                if (item.RetryCount >= item.MaxRetryCount)
                {
                    item.Status = 4; // لغو شده
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// دریافت آمار صف
        /// </summary>
        public async Task<EmailQueueStatistics> GetStatisticsAsync()
        {
            return new EmailQueueStatistics
            {
                Pending = await _context.EmailQueue_Tbl.CountAsync(q => q.Status == 0),
                Processing = await _context.EmailQueue_Tbl.CountAsync(q => q.Status == 1),
                Completed = await _context.EmailQueue_Tbl.CountAsync(q => q.Status == 2),
                Failed = await _context.EmailQueue_Tbl.CountAsync(q => q.Status == 3),
                Cancelled = await _context.EmailQueue_Tbl.CountAsync(q => q.Status == 4)
            };
        }
    }

   
}