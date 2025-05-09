using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class GetContractOnlyQuery : GetLogNotification, IRequest<ContractDataOnly>
    {
        public bool IsJobWorking { get; set; } = false;
        public int ContractId { get; set; }
    }
}
