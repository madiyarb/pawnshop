using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFCheckAccountRequest 
    {
        public string IIN { get; set; }
        public string IdentityNumber { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Fullname { get; set; }
        public bool IsMale { get; set; }
        public DateTime BirthDay { get; set; }
        public string MobilePhone { get; set; }
        public string LegalForm { get; set; }
        public bool IsResident { get; set; }
        public bool IsPEP { get; set; }
        public string Citizenship { get; set; }
        public string ClientId { get; set; }
    }
}
