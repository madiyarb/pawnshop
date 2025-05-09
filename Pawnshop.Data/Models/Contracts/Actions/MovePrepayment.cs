using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Files;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class MovePrepayment
    {
        public int SourceContractId { get; set; }
        public int RecipientContractId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public List<FileRow> Files { get; set; }
        public string Note { get; set; }
    }
}