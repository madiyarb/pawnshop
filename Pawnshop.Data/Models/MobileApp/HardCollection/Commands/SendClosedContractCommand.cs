using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SendClosedContractCommand : GetLogNotification, IRequest<bool>
    {
        public int ContractId { get; set; }
        public ContractDataOnly ContractDataOnly { get; set; }
    }
}
