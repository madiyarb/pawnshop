using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Collection;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Collection.http;
using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Microsoft.Azure.Storage.Shared.Protocol;
using Pawnshop.Core;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;

namespace Pawnshop.Web.Controllers.Api
{
    public class CollectionController : Controller
    {
        private readonly CollectionStatusRepository _collectionRepository;
        private readonly ICollectionHttpService<CollectionHistory> _httpCollectionHistoryService;
        private readonly ICollectionService _collectionService;

        public CollectionController(CollectionStatusRepository collectionRepository,
            ICollectionHttpService<CollectionHistory> httpCollectionHistoryService,
            ICollectionService collectionService)
        {
            _collectionRepository = collectionRepository;
            _httpCollectionHistoryService = httpCollectionHistoryService;
            _collectionService = collectionService;
        }

        [HttpGet("api/collection/getCollection")]
        public async Task<IActionResult> GetContractCollection(int contractId)
        {
            var result = _collectionService.GetCollection(contractId);

            return Ok(result);
        }
        
        [HttpPost("api/collection/cancelChangedStatus")]
        public async Task<IActionResult> CancelChangedStatus([FromBody]int historyId)
        {
            if(historyId <= 0)
                throw new PawnshopApplicationException("historyId <= 0");

            var history = await _httpCollectionHistoryService.Get(historyId.ToString());
            if (history == null)
                throw new PawnshopApplicationException($"Не удалось найти историю с ИД={historyId}");

            var result = await _httpCollectionHistoryService.Delete(historyId.ToString());
            if (result <= 0)
                throw new PawnshopApplicationException($"Не удалось удалить запись с ИД={historyId}");

            var statusCollection = _collectionRepository.GetByContractId(history.ContractId).IsActive ? _collectionRepository.GetByContractId(history.ContractId) : null;
            if(statusCollection == null)
                throw new PawnshopApplicationException($"Не удалось найти статус контракта с ИД={history.ContractId}");
            
            var status = new CollectionContractStatus()
            {
                Id = statusCollection.Id,
                ContractId = statusCollection.ContractId,
                FincoreStatusId = _collectionService.GetFincoreStatus(history.StatusBefore.statusCode),
                IsActive = history.StatusBeforeId == 0 ? false : true, 
                CollectionStatusCode = history.StatusBefore.statusCode,
                StartDelayDate = statusCollection.StartDelayDate
            };

            _collectionRepository.Update(status);

            return Ok();
        }

        [HttpPost("api/collection/changeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] CollectionModel collection, [FromServices] IMediator mediator)
        {
            if(collection == null)
                throw new PawnshopApplicationException("Проверьте данные");

            var isSended = _collectionService.ChangeCollectionStatus(collection);
            if(isSended && (collection.CollectionStatusAfter.statusCode == Core.Constants.HARDCOLLECTION_STATUS ||
                collection.CollectionStatusAfter.statusCode == Core.Constants.LEGALHARDCOLLECTION_STATUS))
            {
                if (mediator.Send(new IsContractInHardCollectionQuery() { ContractId = collection.ContractId }).Result)
                {
                    mediator.Send(new SendContractOnlyCommand() { ContractId = collection.ContractId }).Wait();
                }
                else
                {
                    mediator.Send(new SendContractDataCommand() { ContractId = collection.ContractId, IsJobWorking = true }).Wait();
                }
            }

            return Ok(isSended);
        }
    }
}
