using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressType : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsIndividual { get; set; }
        public int? CBId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool IsMandatory
        {
            get => Code != "BIRTHPLACE" && Code != "WORKINGPLACE";
        }
    }
}
