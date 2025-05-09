using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Sellings;

namespace Pawnshop.Web.Models.Sellings
{
    public class SellingListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public CollateralType? CollateralType { get; set; }

        public SellingStatus? Status { get; set; }

        public int OwnerId { get; set; }

    }
}