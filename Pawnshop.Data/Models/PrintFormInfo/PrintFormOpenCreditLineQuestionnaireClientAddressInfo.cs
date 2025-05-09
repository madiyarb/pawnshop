using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Models.PrintFormInfo
{
    public sealed class PrintFormOpenCreditLineQuestionnaireClientAddressInfo
    {
        public string RegistrationAddressRus { get; set; }
        public string RegistrationAddressKaz { get; set; }
        public string ResidenceAddressRus { get; set; }
        public string ResidenceAddressKaz { get; set; }

        public PrintFormOpenCreditLineQuestionnaireClientAddressInfo(IEnumerable<PrintFormOpenCreditLineQuestionnaireAddressInfo> addresses)
        {
            if (addresses != null && addresses.Any())
            {
                var registrationAddress = addresses.FirstOrDefault(a => a.Code == "REGISTRATION");
                if (registrationAddress != null)
                {
                    RegistrationAddressRus = registrationAddress.AddressNameRus;
                    RegistrationAddressKaz = registrationAddress.AddressNameKaz;
                }
                var residenceAddress = addresses.FirstOrDefault(a => a.Code == "RESIDENCE");
                if (residenceAddress != null)
                {
                    ResidenceAddressRus = residenceAddress.AddressNameRus;
                    ResidenceAddressKaz = residenceAddress.AddressNameKaz;
                }
            }
        }
    }
}
