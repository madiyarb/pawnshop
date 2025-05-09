using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Services.Models.Insurance
{
    public class InsuranceOnlineRequestValidateModel
    {
        public Contract Contract { get; set; }
        public Client Client { get; set; }
        public ClientAddress ClientAddress { get; set; }
        public ClientContact ClientContact { get; set; }
        public ClientDocument ClientDocument { get; set; }
        public InsuranceRequestData PoliceRequestData { get; set; }
    }
}