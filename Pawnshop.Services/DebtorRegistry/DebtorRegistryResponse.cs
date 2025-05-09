using System.Collections.Generic;

namespace Pawnshop.Services.DebtorRegistry
{
    public sealed class DebtorRegistryResponse
    {
        public List<DebtorRegistryResponseData> data { get; set; }
        public int count { get; set; }
    }
}
