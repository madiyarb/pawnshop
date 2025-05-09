using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.SUSNRequests
{
    [Table("SUSNRequests")]
    public sealed class SUSNRequest
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string IIN { get; set; }
        public int ClientId { get; set; }
        public bool Successfully { get; set; }
        public bool AnyAsp { get; set; }

        public string ExceptionMessage { get; set; }

        public SUSNRequest()
        {
            
        }

        public SUSNRequest(Guid id, string iin, int clientId, bool successfully, bool anyAsp, string exceptionMessage)
        {
            Id = id;
            CreateDate = DateTime.Now;
            IIN = iin;
            ClientId = clientId;
            Successfully = successfully;
            AnyAsp = anyAsp;
            ExceptionMessage = exceptionMessage;
        }
    }
}
