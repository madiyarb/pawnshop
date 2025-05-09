using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.Details
{
    public class LegalCasesDetailsViewModel
    {
        public int Id { get; set; }
        public string LegalCaseNumber { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractBranch { get; set; }
        public DateTime ContractDate { get; set; }
        public ClientDto? Client { get; set; }
        public string? Status { get; set; }
        public string? Action { get; set; }
        public string? Court { get; set; }
        public LegalCaseCourseDto? Course { get; set; }
        public LegalCaseStageDto? Stage { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? DateStart { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int DelayDay { get; set; }
        public string? User { get; set; }
        public bool AccrualStopped { get; set; }
        public DateTimeOffset? AccrualStopDate { get; set; }
        public int LoanPeriod { get; set; }
        public string TotalSum { get; set; }
        public int? DaysUntilExecution { get; set; }
        public LegalCaseTaskStatusDto? CaseTaskStatus { get; set; }
        public List<LegalCaseHistoryViewModel> Histories { get; set; }
        public List<LegalCaseDocumentViewModel>? Documents { get; set; }
        public CarDto? Car { get; set; }
    }
}