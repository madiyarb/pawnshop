using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Entities
{
    public class HCContractStatus : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int StageId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
