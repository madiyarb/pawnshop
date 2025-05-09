using System.Collections.Generic;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.LoanSettings;

namespace Pawnshop.Services.Clients
{
    public interface IClientModelValidateService
    {
        void ValidateClientModel(Client client, bool isOnline = false, bool printCheck = false);
        void ValidateMerchantClientModel(Client client);
        void ValidateMobileAppClientModel(Client client);
        bool ValidateIdentityNumber(string number);
        List<string> ValidateFIO(string fieldName, string fieldCode, string value);
    }
}