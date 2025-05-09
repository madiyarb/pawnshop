using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class AddGeoCommand : GetLogNotification, IRequest<int>
    {
        public HCGeoData GeoData { get; set; }
    }
}
