using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public interface IClientEconomicActivityService
    {
        List<ClientEconomicActivity> GetList(int clientId);
        List<ClientEconomicActivity> Save(int clientId, List<ClientEconomicActivity> clientEconomicActivitiesRequest);
    }
}
