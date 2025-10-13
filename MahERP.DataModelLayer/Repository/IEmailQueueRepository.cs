using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;

namespace MahERP.DataModelLayer.Repository
{
    public interface IEmailQueueRepository
    {
        Task<int> EnqueueAsync(EmailQueue emailQueue);
        Task<int> EnqueueBulkAsync(List<EmailQueue> emailQueues);
        Task<List<EmailQueue>> GetPendingItemsAsync(int batchSize = 5);
        Task MarkAsProcessingAsync(int id);
        Task MarkAsSuccessAsync(int id);
        Task MarkAsFailedAsync(int id, string errorMessage);
        Task<EmailQueueStatistics> GetStatisticsAsync();
    }
    public class EmailQueueStatistics
    {
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Completed { get; set; }
        public int Failed { get; set; }
        public int Cancelled { get; set; }
        public int Total => Pending + Processing + Completed + Failed + Cancelled;
    }
}