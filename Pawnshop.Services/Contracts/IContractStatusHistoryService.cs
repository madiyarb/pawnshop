using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IContractStatusHistoryService
    {
        Task<ContractStatusHistory> SaveToStatusChangeHistory(int contractId, ContractStatus status, DateTime date, int userId, bool enforceSavingToHistory = false);
    }
}
