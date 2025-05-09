using System.Threading.Tasks;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Services.DebtorRegisrty.CourtOfficer.Interfaces
{
    public interface IFilteredCourtOfficersService
    {
        public Task<PagedResponse<CourtOfficerDto>> List(CourtOfficersQuery request);
    }
}