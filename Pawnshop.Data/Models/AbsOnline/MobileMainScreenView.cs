using System.Linq;

namespace Pawnshop.Data.Models.AbsOnline
{
    public sealed class MobileMainScreenView
    {
        public IOrderedEnumerable<AbsOnlineContractMobileView> Contracts { get; set; }
        public IOrderedEnumerable<AbsOnlineCreditLineViewMobileMainScreen> CreditLines { get; set; }
    }
}
