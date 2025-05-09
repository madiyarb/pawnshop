using MediatR;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SendContactCommand : GetLogNotification, IRequest<bool>
    {
        public int ClientId { get; set; }
        public ClientContact[] ContactList {  get; set; }
    }
}
