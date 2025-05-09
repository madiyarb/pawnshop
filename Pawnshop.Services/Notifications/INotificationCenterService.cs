using Pawnshop.Data.Models.Contracts.Actions;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.Notifications
{
    public interface INotificationCenterService
    {
        public Task NotifyAboutContractBuyOuted(Contract contract, ContractAction contractAction);
        public Task NotifyAboutSomeContractReadyToBuyOut(int contractWhatCanBeBuyout, ContractAction contractAction);

    }
}
