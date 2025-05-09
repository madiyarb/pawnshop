using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.Action.HttpService
{
    public class LegalCaseActionOptionsResponse
    {
        public decimal StateFeeAmount { get; set; }
        public string? CaseCourt { get; set; }
        public int? CaseCourtId { get; set; }
        public List<LegalCaseActionDto> Actions { get; set; }
        public List<LegalCaseCourseDto> Courses { get; set; }
        public List<CourtDto> Courts { get; set; }
    }
}