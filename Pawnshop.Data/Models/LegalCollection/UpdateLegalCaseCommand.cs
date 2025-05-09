using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class UpdateLegalCaseCommand
    {
        public int LegalCaseId { get; set; }
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int? CaseCourtId { get; set; }
        public decimal? StateFeeAmount { get; set; }
        public decimal? TotalCost { get; set; }
        public LegalCaseActionDto Action { get; set; }
        public List<InscriptionRow>? Rows { get; set; }
        public string? Note { get; set; }
        public int? FileId { get; set; }
        public int? DocumentTypeId { get; set; }
    }
}