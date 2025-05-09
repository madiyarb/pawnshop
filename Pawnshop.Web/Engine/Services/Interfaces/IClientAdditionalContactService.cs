using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Clients.Profiles;
using System.Collections.Generic;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientAdditionalContactService
    {
        List<ClientAdditionalContact> Get(int clientId);
        List<ClientAdditionalContact> Save(int clientId, List<ClientAdditionalContactDto> additionalContacts);
        void SaveFromMobile(int clientId, List<ClientAdditionalContactDto> additionalContacts);
    }
}
