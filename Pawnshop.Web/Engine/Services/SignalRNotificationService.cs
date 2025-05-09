using Microsoft.AspNetCore.SignalR;
using Pawnshop.Web.Hubs;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pawnshop.Web.Engine.Services.Interfaces;

namespace Pawnshop.Web.Engine.Services
{
    public sealed class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<TasOnlineUsersHub> _userHab;
        private static JsonSerializerOptions _serializerOptions;
        public SignalRNotificationService(IHubContext<TasOnlineUsersHub> userHab)
        {
            _userHab = userHab;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task NotifyAllUsers<T>(T message, CancellationToken cancellationToken, string method = null)//TODO cancellationToken must be in end of method params 
        {
            if (string.IsNullOrEmpty(method))
            {
                method = typeof(T).Name;//if method not set use Typeof
            }
            await _userHab.Clients.Group($"Users")
                .SendAsync(method, JsonSerializer.Serialize(message, _serializerOptions), cancellationToken);
        }

        public async Task NotifyUser<T>(T message, int userId, CancellationToken cancellationToken, string method = null)
        {
            if (string.IsNullOrEmpty(method))
            {
                method = typeof(T).Name;//if method not set use Typeof
            }
            await _userHab.Clients.Group($"User{userId}")
                .SendAsync(method, JsonSerializer.Serialize(message, _serializerOptions), cancellationToken);
        }
    }
}
