using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Membership
{
    public class LoanPercentQueryModel
    {
        public int? BranchId { get; set; }

        public CollateralType? CollateralType { get; set; }

        public CardType? CardType { get; set; }

        public decimal? LoanCost { get; set; }

        public int? LoanPeriod { get; set; }

        public bool IsProduct { get; set; } = false;

        public bool IsActual { get; set; } = true;

        public int OrganizationId { get; set; }
        public ContractClass? ContractClass { get; set; }
    }
}
