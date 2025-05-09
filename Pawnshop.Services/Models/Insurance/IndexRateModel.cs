using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Services.Models.Insurance
{
    public class IndexRateModel
    {
        public InsuranceRate InsuranceRate { get; set; }
        public decimal PreviousAmountFrom { get; set; }
    }
}