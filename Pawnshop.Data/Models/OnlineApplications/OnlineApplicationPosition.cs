using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.OnlineApplications
{
    public class OnlineApplicationPosition : IEntity
    {
        public int Id { get; set; }

        public DateTime CreateDate { get; set; }

        public CollateralType CollateralType { get; set; }

        public decimal? LoanCost { get; set; }

        public int? EstimatedCost { get; set; }

        public OnlineApplicationCar Car { get; set; }
    }
}
