using System;

namespace Pawnshop.Web.Models.JetPay
{
    public class JetPayCardCashoutStatusBinding
    {
        public int ContractId { get; set; }
        public Guid PaymentId { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
