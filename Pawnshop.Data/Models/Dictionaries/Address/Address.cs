using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class Address : ITranslatedDictionary
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int? ATETypeId { get; set; }
        public int? GeonimTypeId { get; set; }
        public string NameRus { get; set; }
        public string NameKaz { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public string KATOCode { get; set; }
        public bool HasChild { get; set; }
        public AddressATEType ATEType { get; set; }
        public AddressGeonimType GeonimType { get; set; }
    }
}
