using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;
using System;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Net.Http;

namespace Pawnshop.Services.HardCollection.Command
{
    public class SendAddressCommandHandler : IRequestHandler<SendAddressCommand, bool>
    {
        private readonly IHttpSender _httpSender;
        private readonly IMediator _mediator;
        private const string _sendUrl = "hard/sendAddresses";

        public SendAddressCommandHandler(IHttpSender httpSender, IMediator mediator)
        {
            _httpSender = httpSender;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SendAddressCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _mediator.Send(new IsClientInHardCollectionQuery() { ClientId = request.ClientId }))
                    return false;

                var jsonModel = JsonConvert.SerializeObject(request.clientAddresses, Formatting.None);
                var response = await _httpSender.SendRequestAsync(jsonModel, _sendUrl);

                response.EnsureSuccessStatusCode();

                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Отправка адресов по clientId={request.ClientId}")
                };
                await _mediator.Publish(historyNotification);

                return true;
            }
            catch (HttpRequestException ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                return false;
            }
            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                return false;
            }
        }
    }
}
