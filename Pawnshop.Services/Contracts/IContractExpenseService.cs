using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Services.Models.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IContractExpenseService : IDictionaryWithSearchService<ContractExpense, ContractExpenseFilter>
    {
        IDbTransaction BeginTransaction();
    }
}
