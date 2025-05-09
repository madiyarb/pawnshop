using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Entities
{
    public class HCActionHistory : IEntity
    {
        public int Id { get; set; }
        public int HCContractStatusId { get; set; }
        public string ActionName { get; set; }
        public int ActionId { get; set; }
        public int? Value { get; set; }
        public int AuthorId { get; set; }
        public string Comment { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
