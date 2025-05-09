using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Insurance;
using System;
using System.Collections.Generic;

namespace Pawnshop.Services.Insurance
{
    public interface IInsurancePoliceRequestService : IBaseService<InsurancePoliceRequest>
    {
        InsurancePoliceRequest GetNewPoliceRequest(int contractId);
        IEnumerable<InsurancePoliceRequest> GetActivePoliceRequests(int contractId);
        InsurancePoliceRequest GetApprovedPoliceRequest(int contractId);
        InsurancePoliceRequest GetByGUID(Guid guid);
        ClientAddress GetActualAdressForInsurance(Client client);
        ClientDocument GetLastClientDocumentForInsurance(Client client);
        ClientContact GetDefaultClientContact(Client client);
        InsurancePoliceRequest GetPoliceRequest(ContractModel model);
        void SetContractIdAndNumber(InsurancePoliceRequest insurancePoliceRequest, Contract contract);
        InsurancePoliceRequest GetLatestPoliceRequest(int contractId) => Find(new { ContractId = contractId });
        InsurancePoliceRequest GetLatestPoliceRequestAllStatus(int contractId) => Find(new { ContractId = contractId });
        void UpdateInsuranceRequired(InsurancePoliceRequest actualPoliceRequest);
        InsurancePoliceRequest GetErrorPoliceRequest(int contractId);
        InsuranceRequestData GetInsuranceRequestData(Contract contract, InsurancePoliceRequest insurancePoliceRequest);
        void FillRequest(InsurancePoliceRequest insurancePoliceRequest);
        InsuranceRequestData SetInsuranceRequestData(InsuranceRequestDataModel model);
        InsurancePoliceRequest CopyInsurancePoliceRequest(int contractId);
        void DeletePoliceRequestsForContract(int contractId);
        void DeleteInsurancePoliceRequestsByContractId(int contractId);
        bool isPensioner(InsurancePoliceRequest policeRequest);
        InsurancePoliceRequest ChangeForPensioner(InsurancePoliceRequest policeRequest);
        bool ContractHasCompletedInsurancePolicy(int contractId);
        InsurancePolicy GetLastInsurancePolicy(int contractId);
    }
}