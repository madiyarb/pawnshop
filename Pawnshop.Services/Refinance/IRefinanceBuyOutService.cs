using System;
using System.Threading.Tasks;

namespace Pawnshop.Services.Refinance
{
    public interface IRefinanceBuyOutService
    {
        [Obsolete]
        public Task<bool> BuyOutAllRefinancedContracts(int contractId);
        public Task Buyout(int contractId);
        public Task ApproveBuyout(int contractId);
        public Task<bool> BuyOutAllRefinancedContractsForApplicationsOnline(int contractId);
    }
}
