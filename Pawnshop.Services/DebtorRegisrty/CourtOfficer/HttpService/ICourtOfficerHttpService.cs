using System.Threading.Tasks;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Services.DebtorRegisrty.CourtOfficer.HttpService
{
    public interface ICourtOfficerHttpService
    {
        public Task<PagedResponse<CourtOfficerDto>> Filtered(CourtOfficersHttRequest request);
    }
}