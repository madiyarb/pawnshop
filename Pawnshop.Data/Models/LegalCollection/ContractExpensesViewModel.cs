using System;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class ContractExpensesViewModel
    {
        public DateTime? Date { get; set; }
        public string? Name { get; set; }
        public decimal? Cost { get; set; }
        public bool? IsPayed { get; set; }
        public string? Status { get; set; }
    }
}