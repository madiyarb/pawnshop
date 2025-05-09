using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection.Entities;

namespace Pawnshop.Data.Access.LegalCollection
{
    public interface ILegalCaseContractsStatusRepository
    {
        public Task<LegalCaseContractsStatus> GetByLegalCaseIdAsync(int legalCaseId);
        public LegalCaseContractsStatus GetByLegalCaseId(int legalCaseId);
        public Task<LegalCaseContractsStatus?> GetActiveLegalCaseByContractIdAsync(int contractId);
        public LegalCaseContractsStatus? GetActiveLegalCaseByContractId(int contractId);
        public Task<LegalCaseContractsStatus?> GetNotActiveLegalCaseByContractIdAsync(int contractId);
        public LegalCaseContractsStatus? GetNotActiveLegalCaseByContractId(int contractId);
        public Task CloseAsync(int legalCaseId);
        public void Close(int legalCaseId);
        public Task ChangeActivityAsync(int legalCaseId, bool active);
        public void ChangeActivity(int legalCaseId, bool active);
        public Task InsertAsync(int contractId, int legalCaseId, bool isActive = true);
        public void Insert(int contractId, int legalCaseId, bool isActive = true);
        public LegalCaseContractsStatus? GetContractLegalCase(int contractId, bool isActive = true);
        public Task<LegalCaseContractsStatus?> GetContractLegalCaseAsync(int contractId, bool isActive = true);
        public Task<Contract?> GetContractByLegalCaseIdAsync(int legalCaseId);
        public Contract? GetContractByLegalCaseId(int legalCaseId);
    }
}