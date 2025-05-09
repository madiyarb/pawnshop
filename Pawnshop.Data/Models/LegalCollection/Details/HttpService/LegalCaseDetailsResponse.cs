using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.Details.HttpService
{
    public class LegalCaseDetailsResponse
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string LegalCaseNumber { get; set; }
        public int DelayCurrentDay { get; set; }
        public int CaseReasonId { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
        public decimal StateFeeAmount { get; set; }
        public int? DaysUntilExecution { get; set; }
        public int? AuthorId { get; set; }
        public string? Status { get; set; }
        public string? Action { get; set; }
        public string? Court { get; set; }
        public LegalCaseCourseDto? Course { get; set; }
        public LegalCaseStageDto? Stage { get; set; }
        public LegalCaseTaskStatusDto? CaseTaskStatus { get; set; }
        public List<LegalCaseHistoryResponseDto> Histories { get; set; }
        public List<LegalCaseDocumentViewModel>? Documents { get; set; }
    }
}