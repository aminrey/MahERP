using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;

namespace MahERP.DataModelLayer.Repository
{
    public interface ISmsQueueRepository
    {
        Task<int> EnqueueAsync(SmsQueue smsQueue);
        Task<int> EnqueueBulkAsync(List<SmsQueue> smsQueues);
        Task<List<SmsQueue>> GetPendingItemsAsync(int batchSize = 10);
        Task<List<SmsQueue>> GetRetryableItemsAsync(int batchSize = 5);
        Task MarkAsProcessingAsync(int id);
        Task MarkAsSuccessAsync(int id, int smsLogId);
        Task MarkAsFailedAsync(int id, string errorMessage);
        Task CancelAsync(int id);
        Task<int> CleanupOldItemsAsync(int daysOld = 30);
        Task<QueueStatistics> GetStatisticsAsync();
    }

    
}