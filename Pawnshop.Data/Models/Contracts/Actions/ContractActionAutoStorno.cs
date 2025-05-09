using System;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class ContractActionAutoStorno
    {
        public Contract? Contract { get; set; }
        public int ContractId { get; set; }
        public int AuthorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AccountCheckDate { get; set; }
    }
}
