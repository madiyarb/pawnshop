using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Action;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionActionOptionsService
    {
        public Task<ActionOptionsLegalCaseViewModel> GetActionOptionsAsync(LegalCaseActionOptionsQuery request);
    }
}