using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Services.HardCollection.Query
{
    public class CheckIsClientInHardCollectionQueryHandler : IRequestHandler<CheckIsClientInHardCollectionQuery, bool>
    {
        private readonly HCContractStatusRepository _hardCollectionRepository;
        public CheckIsClientInHardCollectionQueryHandler(HCContractStatusRepository hardCollectionRepository) 
        {
            _hardCollectionRepository = hardCollectionRepository;
        }

        public async Task<bool> Handle(CheckIsClientInHardCollectionQuery request, CancellationToken cancellationToken = default)
        {
            var model = await _hardCollectionRepository.GetByClientIdAsync(request.ClientId);
            if(!model.Any())
                throw new PawnshopApplicationException($"Клиент с Id={request.ClientId} не имеет договора в списке Хард Коллекшн");
            
            return true;
        }
    }
}
