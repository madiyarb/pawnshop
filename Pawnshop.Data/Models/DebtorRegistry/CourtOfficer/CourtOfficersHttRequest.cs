using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Data.Models.DebtorRegistry.CourtOfficer
{
    public class CourtOfficersHttRequest : PagedRequest
    {
        public string? SearchData { get; set; }
    }
}