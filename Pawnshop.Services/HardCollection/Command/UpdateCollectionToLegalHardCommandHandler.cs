using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.Collection;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Core;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.HardCollection.Command
{
    public class UpdateCollectionToLegalHardCommandHandler : IRequestHandler<UpdateCollectionToLegalHardCommand, bool>
    {
        private readonly ICollectionService _collectionService;
        private readonly IContractService _contractService;
        private readonly IMediator _mediator;
        public UpdateCollectionToLegalHardCommandHandler(ICollectionService collectionService, IContractService contractService, IMediator mediator)
        {
            _collectionService = collectionService;
            _contractService = contractService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateCollectionToLegalHardCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                var contractOnly = await _contractService.GetOnlyContractAsync(request.ContractId);
                if(contractOnly.ContractClass == Data.Models.Contracts.ContractClass.Tranche)
                {
                    var creditLineId = await _contractService.GetCreditLineId(request.ContractId);
                    var trancheIdList = _contractService.GetTranches(creditLineId);

                    foreach(var trancheId in trancheIdList)
                    {
                        await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = trancheId.Id });

                        var collectionModel = _collectionService.GetCollection(trancheId.Id);
                        collectionModel.SelectedReasonId = request.ReasonId;
                        collectionModel.Note = request.Comment;
                        _collectionService.ChangeCollectionStatus(collectionModel);

                        var notification = new HardCollectionNotification()
                        {
                            HistoryNotification = request.GetHistoryNotification(),
                            LogNotification = request.GetLogNotifications($"Отправка в {Constants.LEGALHARDCOLLECTION_STATUS} по договору ContractId={trancheId.Id}")
                        };
                        await _mediator.Publish(notification);
                    }
                }
                else
                {
                    await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                    var collectionModel = _collectionService.GetCollection(request.ContractId);
                    collectionModel.SelectedReasonId = request.ReasonId;
                    collectionModel.Note = request.Comment;
                    var isChanged = _collectionService.ChangeCollectionStatus(collectionModel);

                    var notification = new HardCollectionNotification()
                    {
                        HistoryNotification = request.GetHistoryNotification(),
                        LogNotification = request.GetLogNotifications($"Отправка в {Constants.LEGALHARDCOLLECTION_STATUS} по договору ContractId={request.ContractId}")
                    };
                    await _mediator.Publish(notification);
                }

                return true;
            }
            catch (NullReferenceException ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Договор с ContractId={request.ContractId} уже находится в статусе {Constants.LEGALHARDCOLLECTION_STATUS} ошибка:{ex.Message}", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                return false;
            }
            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Ошибка при отправке в Legal по ContractId={request.ContractId}", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
