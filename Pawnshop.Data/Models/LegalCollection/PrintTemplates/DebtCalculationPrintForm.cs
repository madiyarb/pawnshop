using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class DebtCalculationPrintForm
    {
        public string? ContractNumber { get; set; }
        public DateTimeOffset ContractDate { get; set; }
        public List<ContractAmountsInfoDto>? Amounts { get; set; }
        public string? DebtDate { get; set; }
        public int? DelayDays { get; set; }
        public string? TotalCost { get; set; }
        public string? BranchName { get; set; }
    }
}