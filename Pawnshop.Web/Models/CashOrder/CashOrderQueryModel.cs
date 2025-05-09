using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Web.Models.CashOrder
{
    public class CashOrderQueryModel
    {
        public int ContractId { get; set; }

        public ContractActionType ActionType { get; set; }
    }
}