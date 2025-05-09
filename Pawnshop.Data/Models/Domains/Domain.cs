using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Domains
{
    public class Domain : IEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public List<DomainValue> DomainValues { get; set; }
        public User Author { get; set; }
    }
}
