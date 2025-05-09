using Pawnshop.Data.Models.OnlineApplications;
using System.Threading.Tasks;

namespace Pawnshop.Services.OnlineApplications
{
    public interface IOnlineApplicationService
    {
        Task<OnlineApplication> FindByContractNumberAsync(string contractNumber);

        void Save(OnlineApplication entity);
    }
}
