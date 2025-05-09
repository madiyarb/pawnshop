using System.Collections.Generic;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractReadyToMoneySendListView
    {
        public IEnumerable<ContractReadyToMoneySendListItemView> Items { get; set; } = new List<ContractReadyToMoneySendListItemView>();
        public int Count { get; set; }
    }
}
