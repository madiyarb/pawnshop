using Pawnshop.Data.Models.Contracts.Views;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.Contracts
{
    public interface IContractBalancesService
    {
        public Task<ContractBalanceOnlineView> GetContractBalanceOnline(Contract contract);
    }
}
