using System;

namespace Pawnshop.Data.Models.Contracts.Events
{
    public sealed class ContractBuyOuted
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime BuyOutDate { get; set; }
        public int ClientId { get; set; }
        public string ClientIdentityNumber { get; set; }
        public string Type => nameof(ContractBuyOuted);
    }
}
