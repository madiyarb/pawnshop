using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PayTypeContractAction : IEntity
    {
        public int Id { get; set; }
        public int PayTypeId { get; set; }
        public ContractActionType ActionType { get; set; }
        public CollateralType? CollateralType { get; set; }
    }
}