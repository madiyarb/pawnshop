using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Postponements;

namespace Pawnshop.Services.Contracts
{
    public class PostponementService : IPostponementService
    {
        private readonly ContractPostponementRepository _contractPostponementRepository;

        public PostponementService(ContractPostponementRepository contractPostponementRepository)
        {
            _contractPostponementRepository = contractPostponementRepository;
        }

        public List<ContractPostponement> GetByContractId(int contractId) =>
            _contractPostponementRepository.GetByContractId(contractId);
    }
}
