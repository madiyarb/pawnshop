using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface IContractExpensesService
    {
        public Task<List<ContractExpensesViewModel>> GetContractAdditionalExpensesAsync(int contractId);
    }
}