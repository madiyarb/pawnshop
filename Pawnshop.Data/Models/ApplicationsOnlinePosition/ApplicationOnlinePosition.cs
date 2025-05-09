using Pawnshop.AccountingCore.Models;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnlinePosition
{
    public sealed class ApplicationOnlinePosition
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public CollateralType CollateralType { get; set; }
        public decimal? LoanCost { get; set; }
        public decimal? EstimatedCost { get; set; }

        public ApplicationOnlinePosition() { }

        public ApplicationOnlinePosition(Guid id)
        {
            Id = id;
            CreateDate = DateTime.UtcNow;
            CollateralType = CollateralType.Car;
        }

        public void Update(decimal? loanCost, decimal? estimatedCost)
        {
            if (loanCost.HasValue && LoanCost != loanCost)
            {
                LoanCost = loanCost;
            }
            if (estimatedCost.HasValue && estimatedCost != EstimatedCost)
            {
                EstimatedCost = estimatedCost;
            }
        }
    }
}
