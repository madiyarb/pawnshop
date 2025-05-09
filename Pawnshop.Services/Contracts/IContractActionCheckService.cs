using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Contracts
{
    public interface IContractActionCheckService : IService
    {
        void ContractActionCheck(ContractAction action);
    }
}
