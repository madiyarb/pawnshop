using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.PayOperations;
using System;

namespace Pawnshop.Data.Models._1c
{
    public class PayOperationQuery : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        public int OperationId { get; set; }
        public QueryType QueryType { get; set; }
        public QueryStatus Status { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? QueryDate { get; set; }
        public DateTime? DeleteDate { get; set; }

        public int GetLinkedEntityId()
        {
            return OperationId;
        }
    }
}
