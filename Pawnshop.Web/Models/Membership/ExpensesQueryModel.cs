using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Web.Models.Membership
{
    public class ExpensesQueryModel
    {
        public CollateralType CollateralType { get; set; }

        public ContractActionType ActionType { get; set; }
    }
}