using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Pawnshop.Services.HardCollection.Query
{
    public class IsContractInHardCollectionQueryHandler : IRequestHandler<IsContractInHardCollectionQuery, bool>
    {
        private readonly HCContractStatusRepository _repository;

        public IsContractInHardCollectionQueryHandler(HCContractStatusRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(IsContractInHardCollectionQuery query, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByContractIdAsync(query.ContractId);
            if(result is null)
                return false;

            return true;
        }
    }
}
