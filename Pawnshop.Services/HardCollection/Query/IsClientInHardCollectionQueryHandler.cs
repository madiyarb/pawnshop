using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Pawnshop.Services.HardCollection.Query
{
    public class IsClientInHardCollectionQueryHandler : IRequestHandler<IsClientInHardCollectionQuery, bool>
    {
        private readonly HCContractStatusRepository _hardCollectionRepository;

        public IsClientInHardCollectionQueryHandler(HCContractStatusRepository hardCollectionRepository)
        {
            _hardCollectionRepository = hardCollectionRepository;
        }

        public async Task<bool> Handle(IsClientInHardCollectionQuery request, CancellationToken cancellationToken = default)
        {
            var result = await _hardCollectionRepository.GetByClientIdAsync(request.ClientId);
            if (result.ToList().Any())
                return true;
            else 
                return false;
        }
    }
}
