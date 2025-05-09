using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressATEType : IEGOVType
    {
        public int Id { get; set; }
        public string NameRus { get; set; }
        public string NameKaz { get; set; }
        public string ShortNameRus { get; set; }
        public string ShortNameKaz { get; set; }
        public string Code { get; set; }
        public bool IsActual { get; set; }
    }
}
