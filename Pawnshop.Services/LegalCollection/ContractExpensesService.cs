using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class ContractExpensesService : IContractExpensesService
    {
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly ContractRepository _contractRepository;

        public ContractExpensesService( ContractExpenseRepository contractExpenseRepository, ContractRepository contractRepository)
        {
            _contractExpenseRepository = contractExpenseRepository;
            _contractRepository = contractRepository;
        }

        public async Task<List<ContractExpensesViewModel>> GetContractAdditionalExpensesAsync(int contractId)
        {
            List<ContractExpense> contractExpenses;
            var contract = await _contractRepository.GetOnlyContractAsync(contractId);
            if (contract.CreditLineId.HasValue)
            {
                contractExpenses = await _contractExpenseRepository.GetContractExpenseAsync((int)contract.CreditLineId);
            }
            else
            {
                contractExpenses = await _contractExpenseRepository.GetContractExpenseAsync(contractId);
            }
            
            
            return contractExpenses?.Select(c => new ContractExpensesViewModel
            {
                Date = c.Date,
                Name = c.Name,
                Cost = c.TotalCost,
                IsPayed = c.IsPayed,
                Status = c.ContractExpenseRows?.Last().ContractExpenseRowOrders?.Last().Order.ApproveStatus.GetDescription()
            }).ToList();
        }
    }
}