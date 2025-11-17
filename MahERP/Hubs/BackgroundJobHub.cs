using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MahERP.Hubs
{
    /// <summary>
    /// SignalR Hub برای بروزرسانی Realtime Background Jobs
    /// </summary>
    [Authorize]
    public class BackgroundJobHub : Hub
    {
        /// <summary>
        /// اتصال کاربر به Hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // اضافه کردن کاربر به گروه شخصی خودش
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// قطع اتصال کاربر از Hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// درخواست وضعیت فعلی Job ها
        /// </summary>
        public async Task RequestJobsUpdate()
        {
            // کلاینت می‌تواند درخواست بروزرسانی دستی بدهد
            // در صورت نیاز می‌توان وضعیت Job ها را از دیتابیس خواند و ارسال کرد
            await Task.CompletedTask;
        }
    }
}
