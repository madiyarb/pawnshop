using System;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.HttpService
{
    public class FilteredLegalCasesResponse
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string LegalCaseNumber { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset ContractDate { get; set; }
        public int StatusId { get; set; }
        public int? CourseId { get; set; }
        public int? StageId { get; set; }
        public string ContractNumber { get; set; }
        public int CaseReasonId { get; set; }
        public int? DaysUntilExecution { get; set; }
        public int? CaseTaskStatusId { get; set; }
        public bool HasDebtProcessByContractIin { get; set; }
    }
}