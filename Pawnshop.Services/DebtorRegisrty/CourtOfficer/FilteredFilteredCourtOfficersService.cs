using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer.HttpService;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer.Interfaces;

namespace Pawnshop.Services.DebtorRegisrty.CourtOfficer
{
    public class FilteredFilteredCourtOfficersService : IFilteredCourtOfficersService
    {
        private readonly ICourtOfficerHttpService _courtOfficerHttpService;

        public FilteredFilteredCourtOfficersService(ICourtOfficerHttpService courtOfficerHttpService)
        {
            _courtOfficerHttpService = courtOfficerHttpService;
        }

        public async Task<PagedResponse<CourtOfficerDto>> List(CourtOfficersQuery request)
        {
            request.Page = TransformPage(request);
            
            var httpRequest = new CourtOfficersHttRequest
            {
                Page = request.Page.Offset,
                Size = request.Page.Limit,
                SearchData = request.Filter
            };
            
            return await _courtOfficerHttpService.Filtered(httpRequest);
        }
        
        
        private Page TransformPage(CourtOfficersQuery request)
        {
            if (request.Page is null)
            {
                return new Page
                {
                    Offset = 0,
                    Limit = 20
                };
            }
            
            return new Page
            {
                Offset = request.Page.Offset / request.Page.Limit,
                Limit = request.Page.Limit
            };
        }
    }
}