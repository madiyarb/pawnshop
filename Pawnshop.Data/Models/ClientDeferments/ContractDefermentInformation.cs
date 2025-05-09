using Pawnshop.Data.Models.Restructuring;

namespace Pawnshop.Data.Models.ClientDeferments
{
    public class ContractDefermentInformation
    {
        public RestructuringStatusEnum Status { get; set; }
        public bool IsInDefermentPeriod { get; set; }
    }
}
