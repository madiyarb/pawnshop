using System;
using Pawnshop.Core;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.UKassa
{
    public class UKassaKassa : IEntity
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime DeleteDate { get; set; }

        public virtual UKassaSection Section { get; set; }
    }
}
