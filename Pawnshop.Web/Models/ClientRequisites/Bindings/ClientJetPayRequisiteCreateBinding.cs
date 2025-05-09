using System;

namespace Pawnshop.Web.Models.ClientRequisites.Bindings
{
    public class ClientJetPayRequisiteCreateBinding
    {
        public string MaskedCardNumber { get; set; }

        public string HolderName { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string Token { get; set; }

        public string CustomerIp { get; set; }

        public string CustomerId { get; set; }
    }
}
