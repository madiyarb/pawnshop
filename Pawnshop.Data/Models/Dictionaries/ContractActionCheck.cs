using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ContractActionCheck : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int? PayTypeId { get; set; }
        public ContractActionType? ActionType { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
