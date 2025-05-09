using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionsFilteringService
    {
        public Task<PagedResponse<LegalCasesViewModel>> GetFilteredAsync(LegalCasesQuery request);
    }
}