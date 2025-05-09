using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientAssetService
    {
        List<ClientAsset> Get(int clientId);
        List<ClientAsset> Save(int clientId, List<ClientAssetDto> assets);
    }
}
