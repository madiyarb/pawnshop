using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts.ContractActionOnlineExecutionCheckerService
{
    public interface IContractActionOnlineExecutionCheckerService
    {
        public Task<ContractActionOnlineExecutionCheckResult> Check(int contractId);
    }
}
