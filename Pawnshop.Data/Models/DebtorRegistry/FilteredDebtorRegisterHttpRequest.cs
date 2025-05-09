using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Data.Models.DebtorRegistry
{
    public class FilteredDebtorRegisterHttpRequest : PagedRequest
    {
        public int? CourtOfficerId { get; set; }
        public string? CourtOfficerRegion { get; set; }
        public bool? IsTravelBan { get; set; }
        public List<string>? Iins { get; set; }
    }
}