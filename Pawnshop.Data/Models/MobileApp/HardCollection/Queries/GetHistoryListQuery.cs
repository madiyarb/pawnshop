using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class GetHistoryListQuery : GetLogNotification, IRequest<List<HCActionHistoryVM>>
    {
        public int ContractId { get; set; }
    }
}
