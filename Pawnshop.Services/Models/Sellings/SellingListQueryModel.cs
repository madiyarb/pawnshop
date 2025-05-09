using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Sellings;
using System;

namespace Pawnshop.Services.Models.Sellings
{
    public class SellingListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public CollateralType? CollateralType { get; set; }

        public SellingStatus? Status { get; set; }

        public int? OwnerId { get; set; }

        public bool? IsAllBranchesList { get; set; }

        public string? CurrentBranchName { get; set; }

        public int? CurrentBranchId { get; set; }

    }
}
