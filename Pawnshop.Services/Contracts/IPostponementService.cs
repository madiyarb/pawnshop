using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts.Postponements;

namespace Pawnshop.Services.Contracts
{
    public interface IPostponementService
    {
        List<ContractPostponement> GetByContractId(int contractId);
    }
}
