using MediatR;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SendAdditionalContactCommand : GetLogNotification, IRequest<bool>
    {
        public int ClientId { get; set; }
        public ClientAdditionalContact[] ContactList {  get; set; }
    }
}
