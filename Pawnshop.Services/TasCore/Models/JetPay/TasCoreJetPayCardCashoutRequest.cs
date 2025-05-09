using System;

namespace Pawnshop.Services.TasCore.Models.JetPay
{
    public class TasCoreJetPayCardCashoutRequest
    {
        public int ClientId { get; set; }
        public int ContractId { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Token { get; set; }
        public string CustomerIp { get; set; }
        public string CustomerId { get; set; }
        public Guid PaymentId { get; set; }


        public TasCoreJetPayCardCashoutRequest(
            int clientId,
            int contractId,
            int amount,
            string currency,
            string token,
            string customerIp,
            string customerId,
            Guid paymentId)
        {
            ClientId = clientId;
            ContractId = contractId;
            Amount = amount;
            Currency = currency;
            Token = token;
            CustomerIp = customerIp;
            CustomerId = customerId;
            PaymentId = paymentId;
        }

    }
}
