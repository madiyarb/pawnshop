namespace Pawnshop.Data.Models.Contracts
{
    public class CreditLineSettings
    {
        public int Id { get; set; }
        public bool IsRevolving { get; set; }
        public int StopIssueDaysOnMaturity { get; set; }
        public bool IsLiquidityOn { get; set; }
        public bool IsInsuranceAdditionalLimitOn { get; set; }
    }
}
