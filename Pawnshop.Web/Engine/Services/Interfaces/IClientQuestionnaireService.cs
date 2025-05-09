using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientQuestionnaireService
    {
        bool IsClientHasFilledQuestionnaire(int clientId);
        bool CanFillQuestionnaire(int clientId);
    }
}
