using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Core.Exceptions;
using System.Linq;

namespace Pawnshop.Services.HardCollection.Query
{
    public class CheckInHardCollectionContractsQueryHandler : IRequestHandler<CheckInHardCollectionContractsQuery, bool>
    {
        private readonly HCContractStatusRepository _hardCollectionRepository;

        public CheckInHardCollectionContractsQueryHandler(HCContractStatusRepository hardCollectionRepository)
        {
            _hardCollectionRepository = hardCollectionRepository;
        }

        public async Task<bool> Handle(CheckInHardCollectionContractsQuery request, CancellationToken cancellationToken = default)
        {
            if (await _hardCollectionRepository.GetByContractIdAsync(request.ContractId) is null)
                throw new PawnshopApplicationException($"Договор с Id={request.ContractId} не найден в списке договоров Хард Коллекшн");

            var hcContract = _hardCollectionRepository.GetByClientId(request.ClientId);
            if (!hcContract.Any())
                throw new PawnshopApplicationException($"Клиент с Id={request.ClientId} не имеет договора в списке Хард Коллекшн");

            if (!hcContract.Any(x => x.ContractId == request.ContractId))
                throw new PawnshopApplicationException($"Клиент с Id={request.ClientId} и Договор с Id={request.ContractId} не найден в списке договоров Хард Коллекшн");

            return true;
        }
    }
}
