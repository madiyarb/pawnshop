using System;

namespace Pawnshop.Data.Models.Bankruptcy
{
    public class VATOrganizationInfo
    {
        public string Status { get; set; }
        public string Id { get; set; }
        public string Number { get; set; }
        public string Series { get; set; }
        public string Xin { get; set; }
        public string NpNameKz { get; set; }
        public string NpNameRu { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string OgdBin { get; set; }
        public string OgdNameKz { get; set; }
        public string OgdNameRu { get; set; }
        public DateTime CertificateDeliveryDate { get; set; }
        public DateTime? UnRegistrationDate { get; set; }
        public string UnRegistrationCause { get; set; }
        public string UnRegistrationCauseNameKz { get; set; }
        public string UnRegistrationCauseNameRu { get; set; }
    }
}