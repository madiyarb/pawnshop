using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Services.Estimation.v2.Request
{
    public class ApplyCreationRequest
    {
        public int Amount { get; set; }
        public int Pledge_Id { get; set; }
        public int Client_Id { get; set; }
        public int Credit_Type { get; set; }
        public string Tasonline_Crm_Id { get; set; }
        public int? Parent_Contract_Id { get; set; }
        public int? Auto_Price { get; set; }
        public IEnumerable<int> Refinance_Contract_Ids { get; set; }


        public ApplyCreationRequest() { }

        public ApplyCreationRequest
            (int amount,
            int pledgeId,
            int clientId,
            int creditType,
            string tasonlineCrmId,
            int? parentContractId = null,
            int? autoPrice = null,
            IEnumerable<int> refinanceContractIds = null)
        {
            Amount = amount;
            Pledge_Id = pledgeId;
            Client_Id = clientId;
            Credit_Type = creditType;
            Tasonline_Crm_Id = tasonlineCrmId;
            Parent_Contract_Id = parentContractId;
            Auto_Price = autoPrice;
            Refinance_Contract_Ids = refinanceContractIds != null && refinanceContractIds.Any() ? refinanceContractIds : null;
        }
    }
}
