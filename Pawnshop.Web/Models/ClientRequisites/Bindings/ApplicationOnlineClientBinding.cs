using System;

namespace Pawnshop.Web.Models.ClientRequisites.Bindings
{
    public sealed class ApplicationOnlineClientBinding
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
        public string? AdditionalPhone { get; set; }
    }
}
