using System.Collections.Generic;

namespace Pawnshop.Data.Models.LegalCollection.GetFiltered.HttpService
{
    public class FilteredHttpRequest : PagedRequest
    {
        public List<int>? ContractIds { get; set; } = new List<int>();
        public int? StatusId { get; set; }
        public int? CourseId { get; set; }
        public int? StageId { get; set; }
        public int? TaskStatusId { get; set; }
        public bool? HasDebtProcessByContractIin { get; set; }
    }
}