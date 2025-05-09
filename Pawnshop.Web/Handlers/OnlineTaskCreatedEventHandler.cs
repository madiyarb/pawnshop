using KafkaFlow;
using Pawnshop.Data.Models.OnlineTasks.Events;
using System.Threading;
using System.Threading.Tasks;
using Pawnshop.Web.Engine.Services.Interfaces;

namespace Pawnshop.Web.Handlers
{
    public class OnlineTaskCreatedEventHandler : IMessageHandler<OnlineTaskCreated>
    {
        private readonly ISignalRNotificationService _signalRNotificationService;
        public OnlineTaskCreatedEventHandler(ISignalRNotificationService signalRNotificationService)
        {
            _signalRNotificationService = signalRNotificationService;
        }

        public async Task Handle(IMessageContext context, OnlineTaskCreated message)
        {
            await _signalRNotificationService.NotifyAllUsers(message, CancellationToken.None, "TaskCreated");
        }
    }
}
