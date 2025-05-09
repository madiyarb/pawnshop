using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.DebtorRegistry
{
    public class LegalCaseDebtRegistryDto
    {
        public int Id { get; set; }
        public string LegalCaseNumber { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public LegalCaseStageDto? Stage { get; set; }
        public LegalCaseCourseDto? Course { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public List<LegalCaseHistoryViewModel> Histories { get; set; }
    }
}