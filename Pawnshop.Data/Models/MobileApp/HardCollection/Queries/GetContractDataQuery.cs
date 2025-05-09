using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class GetContractDataQuery : GetLogNotification, IRequest<ContractData>
    {
        public bool IsJobWorking { get; set; } = false;
        public int ContractId { get; set; }
    }
}
