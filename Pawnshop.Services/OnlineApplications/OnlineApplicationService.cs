using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OnlineApplications;
using System.Threading.Tasks;

namespace Pawnshop.Services.OnlineApplications
{
    public class OnlineApplicationService : IOnlineApplicationService
    {
        private readonly OnlineApplicationRepository _onlineApplicationRepository;

        public OnlineApplicationService(OnlineApplicationRepository onlineApplicationRepository)
        {
            _onlineApplicationRepository = onlineApplicationRepository;
        }

        public async Task<OnlineApplication> FindByContractNumberAsync(string contractNumber) =>
            await _onlineApplicationRepository.FindAsync(new { ContractNumber = contractNumber });

        public void Save(OnlineApplication entity)
        {
            if (entity.Id > 0)
                _onlineApplicationRepository.Update(entity);
            else
                _onlineApplicationRepository.Insert(entity);
        }


    }
}
