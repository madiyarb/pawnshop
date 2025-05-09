using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Services.Models.TasOnline
{
    public class PaymentModel
    {
        public Client Client { get; set; }
        public TasOnlineContract Contract { get; set; }
        public decimal Cost { get; set; }
        public string Note { get; set; }
    }
}