using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SendContractDataCommand : GetLogNotification, IRequest<bool>
    {
        public bool IsJobWorking { get; set; } = false;
        public int ContractId { get; set; }
        public ContractData ContractData { get; set; }
    }
}
