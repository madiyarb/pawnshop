using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class RequisiteType : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mask { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
