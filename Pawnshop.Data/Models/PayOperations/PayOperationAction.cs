using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.PayOperations
{
    public class PayOperationAction : IEntity
    {
        public int Id { get; set; }
        public int OperationId { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public PayOperationActionType ActionType { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
    }
}
