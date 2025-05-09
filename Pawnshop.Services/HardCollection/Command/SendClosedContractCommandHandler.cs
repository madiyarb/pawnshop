using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;
using System;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Core;
using System.Net.Http;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;

namespace Pawnshop.Services.HardCollection.Command
{
    public class SendClosedContractCommandHandler : IRequestHandler<SendClosedContractCommand, bool>
    {
        private readonly IHttpSender _httpSender;
        private readonly IMediator _mediator;
        private const string _sendUrl = "hard/updateContract";
        private readonly HCContractStatusRepository _hardCollectionRepository;

        public SendClosedContractCommandHandler(HCContractStatusRepository hardCollectionRepository, IHttpSender httpSender, IMediator mediator)
        {
            _httpSender = httpSender;
            _mediator = mediator;
            _hardCollectionRepository = hardCollectionRepository;
        }

        public async Task<bool> Handle(SendClosedContractCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _mediator.Send(new IsContractInHardCollectionQuery() { ContractId = request.ContractId }))
                    return false;

                if(request.ContractDataOnly == null)
                {
                    request.ContractDataOnly = await _mediator.Send(new GetContractOnlyQuery() { ContractId = request.ContractId });
                }

                var hcId = await _hardCollectionRepository.GetByContractIdAsync(request.ContractId);
                var model = new HCContractStatus()
                {
                    Id = hcId.Id,
                    ContractId = request.ContractId,
                    StageId = (int)HardCollectionStageEnum.Finished,
                    IsActive = false
                };

                await _hardCollectionRepository.UpdateAsync(model);

                request.ContractDataOnly.Contract.ContractStatus = Constants.NOCOLLECTION_STATUS;

                var jsonModel = JsonConvert.SerializeObject(request.ContractDataOnly, Formatting.None);
                var response = await _httpSender.SendRequestAsync(jsonModel, _sendUrl);

                response.EnsureSuccessStatusCode();

                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Отправка исключения договора из Collection по clientId={request.ContractId}")
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
