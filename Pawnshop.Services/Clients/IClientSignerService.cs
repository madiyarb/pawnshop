using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Services.Clients
{
    public interface IClientSignerService : IBaseService<ClientSigner>
    {
        ClientSigner CheckClientSigner(Client client, DateTime contractDate, int? signerId);
        List<ClientSignersAllowedDocumentType> GetClientSignersAllowedDocumentTypes(int legalFormId);
        List<ClientSigner> GetList(int companyId);
        List<ClientSigner> Save(int companyId, List<ClientSigner> companySignersRequest);
    }
}