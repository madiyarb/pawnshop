using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Pawnshop.Web.Hubs
{
    public sealed class TasOnlineUsersHub : Hub
    {

        /// <summary>
        /// OnConnectedAsync
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, $"Users").Wait();
            var httpContext = Context.GetHttpContext();
            string userId = httpContext.Request.Query["userId"];

            if (!string.IsNullOrEmpty(userId))
            {
                Groups.AddToGroupAsync(Context.ConnectionId, $"User{userId}");
            }

            return base.OnConnectedAsync();
        }
    }
}
