using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using System.Collections.Concurrent;

namespace Sany3y.API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Message> _messageRepository;

        public ChatHub(UserManager<User> userManager, IRepository<Message> messageRepository)
        {
            _userManager = userManager;
            _messageRepository = messageRepository;
        }

        public override async System.Threading.Tasks.Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name
                ?? Context.GetHttpContext()?.Request.Query["userName"].ToString();

            if (!string.IsNullOrEmpty(userName))
            {
                UserConnections[userName] = Context.ConnectionId;
                await Clients.Others.SendAsync("UserConnected", userName);
            }

            await base.OnConnectedAsync();
        }

        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name
                ?? Context.GetHttpContext()?.Request.Query["userName"].ToString();

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
            var senderUserName = Context.User?.Identity?.Name
                ?? Context.GetHttpContext()?.Request.Query["userName"].ToString();

            if (string.IsNullOrEmpty(senderUserName)) return;

            var sender = await _userManager.FindByNameAsync(senderUserName);
            if (sender == null) return;

            var receiver = await _userManager.FindByNameAsync(receiverUserName);
            if (receiver == null) return;

            var msg = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = message,
                SentAt = DateTime.Now
            };

            await _messageRepository.Add(msg);

            var timeStr = msg.SentAt.ToString("HH:mm");

            if (UserConnections.TryGetValue(receiverUserName, out var receiverConnId))
            {
                await Clients.Client(receiverConnId).SendAsync(
                    "ReceiveMessage",
                    senderUserName,
                    message,
                    timeStr
                );
            }

            await Clients.Caller.SendAsync(
                "MessageSent",
                receiverUserName,
                message,
                timeStr
            );
        }

        public async System.Threading.Tasks.Task Typing(string receiverUserName)
        {
            var senderUserName = Context.User?.Identity?.Name
                ?? Context.GetHttpContext()?.Request.Query["userName"].ToString();

            if (UserConnections.TryGetValue(receiverUserName, out var connId))
            {
                await Clients.Client(connId).SendAsync("TypingIndicator", senderUserName);
            }
        }

        public async System.Threading.Tasks.Task MarkAsSeen(string senderUserName)
        {
            if (UserConnections.TryGetValue(senderUserName, out var connId))
            {
                await Clients.Client(connId).SendAsync("MessageSeen");
            }
        }
    }
}
