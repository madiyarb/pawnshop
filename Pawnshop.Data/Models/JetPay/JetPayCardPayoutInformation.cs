using Pawnshop.Data.Models.Clients;
using System;

namespace Pawnshop.Data.Models.JetPay
{
    public class JetPayCardPayoutInformation
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int ClientRequisiteId { get; set; }
        public ClientRequisite ClientRequisite { get; set; }
        public string Token { get; set; }
        public string CustomerIp { get; set; }
        public string CustomerId { get; set; }


        public JetPayCardPayoutInformation() { }

        public JetPayCardPayoutInformation(int clientRequisiteId, string token, string customerIp, string customerId)
        {
            ClientRequisiteId = clientRequisiteId;
            Token = token;
            CustomerIp = customerIp;
            CustomerId = customerId;
        }

        public bool Compare(string token, string customerIp, string customerId, string value)
        {
            return Token.Equals(token) && CustomerIp.Equals(customerIp) && CustomerId.Equals(customerId) && (ClientRequisite?.Value?.Equals(value) ?? false);
        }
    }
}
