using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.DebtorRegisrty.Dtos;

namespace Pawnshop.Services.DebtorRegisrty.HttpService
{
    public interface IDebtorRegisterHttpService
    {
        public Task<PagedResponse<DebtorDetailsDto>> GetByFilters(FilteredDebtorRegisterHttpRequest request);
        public Task<List<DebtorDetailsResponseDto>> Details(string iin);
        public Task<List<DebtorDetailsDto>> GetListByIdentityNumbers(List<string> identityNumbers);
    }
}