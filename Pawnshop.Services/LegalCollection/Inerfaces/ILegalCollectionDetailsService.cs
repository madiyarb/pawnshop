using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Details;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionDetailsService
    {
        public Task<List<LegalCasesDetailsViewModel>> GetDetailsAsync(LegalCaseDetailsQuery query);
    }
}