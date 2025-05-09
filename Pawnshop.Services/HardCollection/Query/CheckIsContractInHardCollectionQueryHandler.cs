using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Services.HardCollection.Query
{
    public class CheckIsContractInHardCollectionQueryHandler : IRequestHandler<CheckIsContractInHardCollectionQuery, bool>
    {
        private readonly HCContractStatusRepository _hardCollectionRepository;

        public CheckIsContractInHardCollectionQueryHandler(HCContractStatusRepository hardCollectionRepository)
        {
            _hardCollectionRepository = hardCollectionRepository;
        }

        public async Task<bool> Handle(CheckIsContractInHardCollectionQuery request, CancellationToken cancellationToken = default)
        {
            if(await _hardCollectionRepository.GetByContractIdAsync(request.ContractId) is null)
                throw new PawnshopApplicationException($"Договор с Id={request.ContractId} не найден в списке договоров Хард Коллекшн");

            return true;
        }
    }
}
