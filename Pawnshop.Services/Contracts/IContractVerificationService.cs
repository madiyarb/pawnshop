using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IContractVerificationService
    {
        void ClientAccordanceToProduct(Contract contract);
        void LoanPurposeAccordanceToFinancePlanPurposes(Contract contract);
        void CheckClientContractsAmountWithProductLoanCost(Contract contract);
        void CheckDAMUContract(Contract contract);
        void CheckContractPositions(Contract contract);
        void CheckGuarantorSubjects(List<ContractLoanSubject> contractSubjects, string? ProductTypeCode = null);
        void CheckDAMUContractMaturityDate(Contract contract);
        void CheckCoborrowerSubjects(List<ContractLoanSubject> contractSubjects, string productTypeCode);
        Task CheckPositionSubjects(Contract contract);
        Task CheckLtvForContractEstimatedCost(Contract contract);
        Task CheckPositionEstimateDate(Contract contract);
        Task CheckPositionSubjectClients(Contract contract);
        Task CheckEIfEstimatedCostPositiveForPositions(Contract contract);
        Task CheckIfRealtyContractConfirmed(Contract contract);
        Task CheckClientsAge(Contract contract);
        Task CheckClientCoborrowerAccountAmountLimit(Contract contract);
    }
}
