using Microsoft.AspNetCore.SignalR;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using System.Threading.Tasks; // Ensure this is here

namespace Sany3y.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatHub(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async System.Threading.Tasks.Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId)) return;

            // 1. Send to Receiver (Real-time)
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);

            // 2. Save to DB via API (Persistence)
            var msg = new Message
            {
                SenderId = long.Parse(senderId),
                ReceiverId = long.Parse(receiverId),
                Content = message,
                SentAt = DateTime.Now
            };

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7178/"); // API URL
            await client.PostAsJsonAsync("api/Message/Create", msg);
        }
    }
}
