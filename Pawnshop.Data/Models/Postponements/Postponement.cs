using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Postponements
{
    public class Postponement : IEntity, IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
    }
}
