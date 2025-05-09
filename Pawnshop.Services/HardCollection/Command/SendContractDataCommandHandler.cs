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
    public class SendContractDataCommandHandler : IRequestHandler<SendContractDataCommand, bool>
    {
        private readonly IHttpSender _httpSender;
        private readonly IMediator _mediator;
        private const string _sendJobUrl = "hard/sendPortfel";
        private const string _sendUserUrl = "hard/updateContract";

        public SendContractDataCommandHandler(IHttpSender httpSender, IMediator mediator)
        {
            _httpSender = httpSender;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SendContractDataCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                if(!request.IsJobWorking)
                {
                    if (!await _mediator.Send(new IsContractInHardCollectionQuery() { ContractId = request.ContractId }))
                        return false;
                }

                if (request.ContractData == null)
                    request.ContractData = await _mediator.Send(new GetContractDataQuery() { ContractId = request.ContractId, IsJobWorking = request.IsJobWorking });

                string url;
                if (request.IsJobWorking)
                    url = _sendJobUrl;
                else 
                    url = _sendUserUrl;

                var jsonModel = JsonConvert.SerializeObject(request.ContractData, Formatting.None);
                var response = await _httpSender.SendRequestAsync(jsonModel, url);

                response.EnsureSuccessStatusCode();

                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(@$"Отправлен полные данные контракта в МП по ContractId={request.ContractId}
{response}", null, Data.Models.Audit.EventStatus.Success, !request.IsJobWorking, request.IsJobWorking)
                };
                await _mediator.Publish(historyNotification);

                return true;
            }
            catch (HttpRequestException ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed, !request.IsJobWorking, request.IsJobWorking)
                };
                await _mediator.Publish(notification);

                return false;
            }
            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed, !request.IsJobWorking, request.IsJobWorking)
                };
                await _mediator.Publish(notification);

                return false;
            }
        }
    }
}
