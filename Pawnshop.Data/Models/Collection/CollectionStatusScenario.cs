using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionStatusScenario
    {
        public int? Id { get; set; }
        public int ActionId { get; set; }
        public CollectionActions Action { get; set; }
        public int ReasonId { get; set; }
        public CollectionReason Reason { get; set; }
        public int StatusBeforeId { get; set; }
        public CollectionStatus StatusBefore { get; set; }
        public int StatusAfterId { get; set; }
        public CollectionStatus StatusAfter { get; set; }
        public string CategoryCode { get; set; }
    }
}
