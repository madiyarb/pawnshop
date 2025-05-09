using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ClientDebtorRegistryRequests
{
    [Table("ClientDebtorRegistryRequests")]
    public sealed class ClientDebtorRegistryRequest
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public DateTime CreateDate { get; set; }

        public int ClientId { get; set; }

        public ClientDebtorRegistryRequest()
        {
            
        }

        public ClientDebtorRegistryRequest(Guid id, int clientId)
        {
            Id = id;
            CreateDate = DateTime.Now;
            ClientId = clientId;
        }
    }
}
