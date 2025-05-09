using Pawnshop.Services.Models.Calculation;

namespace Pawnshop.Services.Calculation
{
    public interface IContractDutyService
    {
        ContractDuty GetContractDuty(ContractDutyCheckModel model);
    }
}
