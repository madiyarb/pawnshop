using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Kdn
{
    public class ApplicationOnlineKdnPosition : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int ClientId { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public CollateralType CollateralType { get; set; }
        public string Name { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
