using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionTaskStatusHttpService
    {
        public Task<PagedResponse<LegalCaseTaskStatusDto>> List();
    }
}