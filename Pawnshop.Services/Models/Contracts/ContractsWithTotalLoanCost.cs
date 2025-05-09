using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.Models.Contracts
{
    public class ContractsWithTotalLoanCost
    {
        public IList<Contract> Contracts { get; set; }

        public decimal TotalLoanCost { get; set; }

        public int? PositionId { get; set; }

        public void CalculateTotalCost()
        {
            if(Contracts != null && Contracts.Any())
            {
                TotalLoanCost = Contracts.Sum(x => x.LoanCost);
            }
        }
    }
}
