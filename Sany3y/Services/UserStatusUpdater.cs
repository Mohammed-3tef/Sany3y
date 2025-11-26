using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sany3y.Hubs;
using Sany3y.Infrastructure.Models;

namespace Sany3y.Services
{
    public static class UserStatusUpdater
    {
        public static async System.Threading.Tasks.Task UpdateUserOnlineStatus(
            User user,
            bool isOnline,
            HttpClient http,
            IHubContext<UserStatusHub> hubContext,
            ControllerBase controller)
        {
            var content = JsonContent.Create(isOnline);

            var response = await http.PutAsync($"/api/User/UpdateState/{user.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errors = await ErrorResponseHandler.SafeReadErrors(response);
                foreach (var e in errors)
                    controller.ModelState.AddModelError(string.Empty, e);
            }

            await hubContext.Clients.All.SendAsync("ReceiveUserStatus", user.Id, isOnline);
        }
    }
}