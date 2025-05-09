using System;

namespace Pawnshop.Data.Models.AccountRecords
{
    public class MovementsOfDepoAccount
    {
        public DateTime Date { get; set; }
        public decimal TotalAcountAmount { get; set; }
        public decimal TotalProfitAmount { get; set; }
        public decimal PenyAmount { get; set; }
    }
}
