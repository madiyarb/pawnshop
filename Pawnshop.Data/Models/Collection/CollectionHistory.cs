using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionHistory
    {
        public int? Id { get; set; }
        public int ContractId { get; set; }
        public int StatusBeforeId { get; set; }
        public CollectionStatus StatusBefore { get; set; }
        public int StatusAfterId { get; set; }
        public CollectionStatus StatusAfter { get; set; }
        public int DelayDays { get; set; }
        public int ReasonId { get; set; }
        public CollectionReason Reason { get; set; }
        public int ActionId { get; set; }
        public CollectionActions Action { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
        public int? FincoreActionId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public User User { get; set; }
    }
}
