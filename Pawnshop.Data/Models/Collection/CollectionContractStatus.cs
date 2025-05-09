using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionContractStatus : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int FincoreStatusId { get; set; }
        public string CollectionStatusCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDelayDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
