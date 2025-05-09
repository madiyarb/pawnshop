using System.Threading.Tasks;
using Pawnshop.Data.Models.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Services.DebtorRegisrty.Interfaces
{
    public interface IFilteredDebtRegistryService
    {
        public Task<PagedResponse<DebtRegistriesViewModel>> GetFilteringAsync(DebtorRegistriesQuery request);
    }
}