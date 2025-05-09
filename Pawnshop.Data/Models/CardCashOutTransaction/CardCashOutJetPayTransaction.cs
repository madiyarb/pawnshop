using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.JetPay;
using System;

namespace Pawnshop.Data.Models.CardCashOutTransaction
{
    public class CardCashOutJetPayTransaction
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        public int JetPayCardPayoutInformationId { get; set; }
        public JetPayCardPayoutInformation JetPayCardPayoutInformation { get; set; }
        public CardCashOutTransactionStatus Status { get; set; }
        public string Message { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }


        public CardCashOutJetPayTransaction() { }

        public CardCashOutJetPayTransaction(
            int clientId,
            int contractId,
            int jetPayCardPayoutInformationId,
            Guid paymentId,
            decimal amount)
        {
            CreateDate = DateTime.Now;
            ClientId = clientId;
            ContractId = contractId;
            JetPayCardPayoutInformationId = jetPayCardPayoutInformationId;
            Status = CardCashOutTransactionStatus.Created;
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
