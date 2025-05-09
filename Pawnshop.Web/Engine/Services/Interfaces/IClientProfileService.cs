using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientProfileService
    {
        ClientProfile Get(int clientId);
        ClientProfile Save(int clientId, ClientProfileDto clientProfile);
        bool IsClientProfileFilled(int clientId);
    }
}
