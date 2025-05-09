using Pawnshop.Data.Models.Membership;
using System;
using Pawnshop.Core;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.UKassa
{
    public class UKassaSection : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
