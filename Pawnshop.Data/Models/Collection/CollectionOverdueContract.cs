using Pawnshop.AccountingCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionOverdueContract
    {
        public int ContractId {  get; set; }
        public int FincoreStatusId { get; set; }
        public string CollectionStatusCode { get; set; }
        public string ContractNumber { get; set; }
        public int DelayDays { get; set; }
        public string StatusAfterCode { get; set; }
        public CollectionStatus StatusAfter { get; set; }
        public CollectionStatus StatusBefore { get; set; }
        public CollectionActions Action { get; set; }
        public int ReasonId { get; set; }
        public DateTime StartDelayDate { get; set; }
        public CollateralType CollateralType { get; set; }
    }
}
