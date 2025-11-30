using Microsoft.AspNetCore.SignalR;
using Sany3y.Infrastructure.Models;
using System.Collections.Concurrent;

namespace Sany3y.Hubs
{
    public class ChatHub : Hub
    {
        // userName → connectionId
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();

        public override async System.Threading.Tasks.Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                UserConnections[userName] = Context.ConnectionId;

                await Clients.Others.SendAsync("UserConnected", userName);
            }

            await base.OnConnectedAsync();
        }

        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                UserConnections.TryRemove(userName, out _);
                await Clients.Others.SendAsync("UserDisconnected", userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static bool IsOnline(string userName)
            => UserConnections.ContainsKey(userName);

        public async System.Threading.Tasks.Task SendPrivateMessage(string receiverUserName, string message)
        {
            var senderUserName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUserName)) return;

            using var http = new HttpClient();
            http.BaseAddress = new Uri("https://localhost:7178"); // API base URL

            // جلب sender
            var senderResponse = await http.GetAsync($"/api/User/GetByUserName/{senderUserName}");
            if (!senderResponse.IsSuccessStatusCode) return;
            var sender = await senderResponse.Content.ReadFromJsonAsync<User>();
            if (sender == null) return;

            // جلب receiver
            var receiverResponse = await http.GetAsync($"/api/User/GetByUserName/{receiverUserName}");
            if (!receiverResponse.IsSuccessStatusCode) return;
            var receiver = await receiverResponse.Content.ReadFromJsonAsync<User>();
            if (receiver == null) return;

            // إنشاء الرسالة
            var msg = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = message,
                SentAt = DateTime.Now
            };

            // **حفظ الرسالة في DB عبر API**
            var saveResponse = await http.PostAsJsonAsync("/api/Message/Create", msg);
            if (!saveResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to save message in DB");
            }

            // إرسال للمستلم إذا كان online
            if (UserConnections.TryGetValue(receiverUserName, out var receiverConnId))
            {
                await Clients.Client(receiverConnId).SendAsync(
                    "ReceiveMessage",
                    senderUserName,
                    message,
                    DateTime.Now.ToString("HH:mm")
                );
            }

            // إرسال للمرسل نفسه
            await Clients.Caller.SendAsync(
                "MessageSent",
                receiverUserName,
                message,
                DateTime.Now.ToString("HH:mm")
            );
        }

        // إشعار الكتابة
        public async System.Threading.Tasks.Task Typing(string receiverUserName)
        {
            if (UserConnections.TryGetValue(receiverUserName, out var connId))
            {
                await Clients.Client(connId).SendAsync("TypingIndicator");
            }
        }

        // إشعار مشاهدة الرسالة
        public async System.Threading.Tasks.Task MarkAsSeen(string senderUserName)
        {
            if (UserConnections.TryGetValue(senderUserName, out var connId))
            {
                await Clients.Client(connId).SendAsync("MessageSeen");
            }
        }
    }
}
