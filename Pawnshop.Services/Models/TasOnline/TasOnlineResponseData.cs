using System;
using System.Collections.Generic;

namespace Pawnshop.Services.Models.TasOnline
{
    public class TasOnlineResponseData : TasOnlineBaseResponse
    {
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
        public List<TasOnlineContract> Contracts { get; set; }
    }
}