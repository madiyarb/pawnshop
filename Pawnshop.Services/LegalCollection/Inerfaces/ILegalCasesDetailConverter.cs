using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCasesDetailConverter
    {
        public Task<List<LegalCasesDetailsViewModel>> ConvertAsync(List<LegalCaseDetailsResponse> detailsResponse);
    }
}