using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientEmploymentService
    {
        List<ClientEmployment> Get(int clientId);
        List<ClientEmployment> Save(int clientId, List<ClientEmploymentDto> employments);
    }
}
