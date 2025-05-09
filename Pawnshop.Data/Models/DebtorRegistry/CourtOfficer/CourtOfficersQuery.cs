using Pawnshop.Core.Queries;

namespace Pawnshop.Data.Models.DebtorRegistry.CourtOfficer
{
    public class CourtOfficersQuery
    {
        public Page Page { get; set; }
        public string? Filter { get; set; }
    }
}