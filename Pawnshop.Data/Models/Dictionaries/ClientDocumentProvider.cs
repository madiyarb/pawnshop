using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ClientDocumentProvider : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviature { get; set; }
        public string NameKaz { get; set; }
        public string AbbreviatureKaz { get; set; }
        public string Code { get; set; }
        public List<ClientDocumentType> PossibleDocumentTypes { get; set; } = new List<ClientDocumentType>();
        public DateTime? DeleteDate { get; set; }
    }
}
