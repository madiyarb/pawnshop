using System.Collections.Generic;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class CreditLineDutyViewModel
    {
        public List<ContractDutyViewModel> ContractDuties { get; set; } = new List<ContractDutyViewModel>();

        public decimal BuyoutCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal ExtraExpensesCost { get; set; }
    }
}
