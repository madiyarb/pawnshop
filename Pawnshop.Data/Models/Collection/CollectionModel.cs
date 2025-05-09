using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionModel
    {
        public int CollectionContractStatusId { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        public int FincoreStatusId { get; set; }
        public int? FincoreActionId { get; set; }
        public int CollectionActionId { get; set; }
        public CollectionActions CollectionActions { get; set; }
        public int CollectionStatusId { get; set; }
        public string CollectionStatusCode { get; set; }
        public CollectionStatus CollectionStatus { get; set; }
        public int CollectionStatusAfterId { get; set; }    
        public CollectionStatus CollectionStatusAfter { get; set; }
        public int DelayDays { get; set; }
        public List<int> CollectionReasonId { get; set; }
        public List<CollectionReason> CollectionReason { get; set; }
        public int? SelectedReasonId { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }
        public string CategoryCode { get; set; }
        public DateTime StartDelayDate { get; set; }
    }
}
