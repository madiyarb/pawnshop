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
    public class SendAdditionalContactCommandHandler : IRequestHandler<SendAdditionalContactCommand, bool>
    {
        private readonly IHttpSender _httpSenderMobApp;
        private readonly IMediator _mediator;
        private const string _sendUrl = "hard/sendAdditionalContacts";
        public SendAdditionalContactCommandHandler(IHttpSender httpSenderMobApp, IMediator mediator)
        {
            _httpSenderMobApp = httpSenderMobApp;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SendAdditionalContactCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _mediator.Send(new IsClientInHardCollectionQuery() { ClientId = request.ClientId }))
                    return false;

                var jsonModel = JsonConvert.SerializeObject(request.ContactList, Formatting.None);
                var response = await _httpSenderMobApp.SendRequestAsync(jsonModel, _sendUrl);

                response.EnsureSuccessStatusCode();

                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Отправка дополнительных контактов по clientId={ request.ClientId }")
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
