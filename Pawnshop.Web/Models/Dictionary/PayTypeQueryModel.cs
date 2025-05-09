using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Web.Models.Dictionary
{
    public class PayTypeQueryModel
    {
        public ContractActionType? ActionType { get; set; }
        public CollateralType? CollateralType { get; set; }
        public UseSystemType? UseSystemType { get; set; }
    }
}
