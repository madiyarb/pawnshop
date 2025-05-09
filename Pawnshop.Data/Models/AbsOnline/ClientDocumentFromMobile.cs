using Pawnshop.Data.Models.Dictionaries;
using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public class ClientDocumentFromMobile
    {
        public string TypeCode { get; set; }
        public string Number { get; set; }
        public string IssuerCode { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string BirthPlace { get; set; }
    }
}
