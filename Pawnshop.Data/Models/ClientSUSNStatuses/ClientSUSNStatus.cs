using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ClientSUSNStatuses
{
    [Table("ClientSUSNStatuses")]
    public sealed class ClientSUSNStatus
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int ClientId { get; set; }
        public int SUSNStatusId { get; set; }
        public Guid SUSNRequestId { get; set; }

        public ClientSUSNStatus()
        {
            
        }

        public ClientSUSNStatus(int clientId, int susnStatusId, Guid susnRequestId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            ClientId = clientId;
            SUSNStatusId = susnStatusId;
            SUSNRequestId = susnRequestId;
        }
    }
}
