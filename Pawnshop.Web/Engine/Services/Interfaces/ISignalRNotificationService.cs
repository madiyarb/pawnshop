using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface ISignalRNotificationService
    {
        public Task NotifyAllUsers<T>(T message, CancellationToken cancellationToken, string method = null);
        public Task NotifyUser<T>(T message, int userId, CancellationToken cancellationToken, string method = null);
    }
}
