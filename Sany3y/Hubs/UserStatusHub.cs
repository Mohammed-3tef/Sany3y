using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Sany3y.Hubs
{
    public class UserStatusHub : Hub
    {
        // حفظ المستخدمين الأونلاين في الذاكرة مؤقتاً
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                OnlineUsers[userName] = Context.ConnectionId;
                await Clients.All.SendAsync("UserStatusChanged", userName, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && OnlineUsers.TryRemove(userName, out _))
            {
                await Clients.All.SendAsync("UserStatusChanged", userName, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public static bool IsUserOnline(string userName)
            => OnlineUsers.ContainsKey(userName);
    }
}
