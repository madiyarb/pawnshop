namespace Pawnshop.Data.Models.Contracts.Query
{
    public sealed class ContractReadyToMoneySendListQuery
    {
        public string IIN { get; set; }

        public string CarNumber { get; set; }

        public bool? IsEncumbranceRegistered { get; set; }
    }
}
