using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Services.Models.Clients
{
    public class ClientExpenseDto
    {
        [Required]
        public decimal? AllLoan {  get; set; }
        public decimal? Loan { get; set; }
        public decimal? Other { get; set; }
        public decimal? Housing { get; set; }
        public decimal? Family { get; set; }
        public decimal? Vehicle { get; set; }
        public decimal? Dependents { get; set; }
        public decimal? AvgPaymentToday { get; set; }
    }
}
