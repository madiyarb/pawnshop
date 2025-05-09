using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class ContractActionCheckValue : IEntity
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public int CheckId { get; set; }
        public ContractActionCheck Check { get; set; }
        public bool Value { get; set; }
    }
}
