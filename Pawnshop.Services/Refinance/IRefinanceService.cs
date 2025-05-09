using System;
using System.Threading.Tasks;

namespace Pawnshop.Services.Refinance
{
    [Obsolete]
    public interface IRefinanceService
    {
        [Obsolete]
        public Task<bool> RefinanceAllAssociatedContracts(int contractId);
        [Obsolete]
        public Task<decimal> CalculateRefinanceAmountForContract(int contractId);
    }
}
