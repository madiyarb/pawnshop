using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Domains
{
    public class DomainValue : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        public string Code { get; set; }
        public int AuthorId { get; set; }
        public string DomainCode { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool IsActive { get; set; }
        public string AdditionalData { get; set; }
        public Domain Domain { get; set; }
        public User Author { get; set; }
    }
}
