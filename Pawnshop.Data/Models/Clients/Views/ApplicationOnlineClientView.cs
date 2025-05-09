using System;

namespace Pawnshop.Data.Models.Clients.Views
{
    public sealed class ApplicationOnlineClientView
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? BirthDay { get; set; }
        public bool? IsMale { get; set; }
        public string? PartnerCode { get; set; }
        public bool? ReceivesASP { get; set; }
        public string? EMail { get; set; }
        public string? MobilePhone { get; set; }
        public string? AdditionalPhone { get; set; }
    }
}
