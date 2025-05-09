using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Models.Insurance;

namespace Pawnshop.Services.Insurance
{
    public interface IInsuranceOnlineRequestService : IBaseService<InsuranceOnlineRequest>
    {
        InsuranceOnlineRequest Save(InsuranceRequestData requestData);
    }
}