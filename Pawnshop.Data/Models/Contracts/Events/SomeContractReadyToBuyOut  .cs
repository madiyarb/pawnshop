using System;

namespace Pawnshop.Data.Models.Contracts.Events
{
    public sealed class SomeContractReadyToBuyOut
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public int ClientId { get; set; }
        public string ClientIdentityNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string Type => nameof(SomeContractReadyToBuyOut);
    }
}
