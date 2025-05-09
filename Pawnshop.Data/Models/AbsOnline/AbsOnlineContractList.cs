using System.Collections.Generic;

namespace Pawnshop.Data.Models.AbsOnline
{
    public class AbsOnlineContractList
    {
        public List<AbsOnlineContractView> Contracts { get; set; } = new List<AbsOnlineContractView>();

        public List<AbsOnlineCreditLineView> CreditLines { get; set; } = new List<AbsOnlineCreditLineView>();
    }
}
