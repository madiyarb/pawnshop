using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SendContractOnlyCommand : GetLogNotification, IRequest<bool>
    {
        public bool IsJobWorking { get; set; } = false;
        public int ContractId { get; set; }
        public ContractDataOnly ContractDataOnly { get; set; }
    }
}
