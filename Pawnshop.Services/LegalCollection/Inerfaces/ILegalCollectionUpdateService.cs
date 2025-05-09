using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Details;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionUpdateService
    {
        public Task<List<LegalCasesDetailsViewModel>> UpdateLegalCase(UpdateLegalCaseCommand request);
    }
}