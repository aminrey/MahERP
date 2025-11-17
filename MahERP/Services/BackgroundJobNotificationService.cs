using MahERP.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MahERP.Services
{
    /// <summary>
    /// سرویس برای ارسال بروزرسانی‌های Realtime به کلاینت‌ها
    /// </summary>
    public interface IBackgroundJobNotificationService
    {
        Task NotifyJobProgress(string userId, int jobId, int progress, int processed, int success, int failed);
        Task NotifyJobCompleted(string userId, int jobId, bool isSuccess, string? errorMessage = null);
        Task NotifyJobStarted(string userId, int jobId, string title);
        Task RefreshJobsList(string userId);
    }

    public class BackgroundJobNotificationService : IBackgroundJobNotificationService
    {
        private readonly IHubContext<BackgroundJobHub> _hubContext;

        public BackgroundJobNotificationService(IHubContext<BackgroundJobHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// اطلاع‌رسانی پیشرفت Job
        /// </summary>
        public async Task NotifyJobProgress(string userId, int jobId, int progress, int processed, int success, int failed)
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("JobProgressUpdated", new
                {
                    jobId,
                    progress,
                    processed,
                    success,
                    failed,
                    timestamp = DateTime.Now
                });
        }

        /// <summary>
        /// اطلاع‌رسانی تکمیل Job
        /// </summary>
        public async Task NotifyJobCompleted(string userId, int jobId, bool isSuccess, string? errorMessage = null)
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("JobCompleted", new
                {
                    jobId,
                    isSuccess,
                    errorMessage,
                    timestamp = DateTime.Now
                });
        }

        /// <summary>
        /// اطلاع‌رسانی شروع Job
        /// </summary>
        public async Task NotifyJobStarted(string userId, int jobId, string title)
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("JobStarted", new
                {
                    jobId,
                    title,
                    timestamp = DateTime.Now
                });
        }

        /// <summary>
        /// درخواست بروزرسانی لیست Job ها
        /// </summary>
        public async Task RefreshJobsList(string userId)
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("RefreshJobsList");
        }
    }
}
