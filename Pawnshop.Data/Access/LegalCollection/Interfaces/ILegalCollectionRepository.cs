using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;

namespace Pawnshop.Data.Access.LegalCollection
{
    public interface ILegalCollectionRepository
    {
        public Task<ContractInfoCollectionStatusDto> GetContractInfoCollectionStatusAsync(int contractId);
        public ContractInfoCollectionStatusDto GetContractInfoCollectionStatus(int contractId);
        public Task<LegalCaseContractInfoDto> GetContractInfoAsync(int contractId);
        public LegalCaseContractInfoDto GetContractInfo(int contractId);
        public Task<ClientDto> GetClientByContractIdAsync(int contractId);
        public ClientDto GetClientByContractId(int contractId);
        public Task<int> GetPennyAccountIdByContractIdAsync(int contractId);
        public int GetPennyAccountIdByContractId(int contractId);
        public Task<int> GetPennyProfitIdByContractIdAsync(int contractId);
        public int GetPennyProfitIdByContractId(int contractId);
        
        Task<CountResult<Contract>> GetContractsByFilterDataAsync(
            int page,
            int size,
            string? contractNumber,
            string? identityNumber,
            string? fullName,
            string? carNumber,
            string? RKA,
            int? parkingStatusId,
            int? regionId,
            int? branchId,
            int? collateralType
        );
        
        public Task<List<ClientDto>> GetClientByFullNameAsync(string fullName);
        public Task<LegalCasePrintTemplateModel> GetPrintFormDataAsync(int contractId);
        public Task<int?> GetDelayDaysAsync(int contractId);
        public int? GetDelayDays(int contractId);
        
        Task<IEnumerable<LegalCasePrintTemplateClientData>> GetPrintTemplateSubjectList(int contractId, string subjectCode);
        Task<IEnumerable<LegalCasePrintTemplateCarData>> GetPrintTemplateCarPositionList(int contractId);
        public Task<List<Account>> GetAccountsByAccountSettingsAsync(int contractId);
        public List<Account> GetAccountsByAccountSettings(int contractId);
        
        public LegalCasePrintTemplateModel GetPrintFormData(int contractId);
        public Task<LegalCaseContractDto> GetLegalCaseContractInfoAsync(int contractId);
        public LegalCaseContractDto GetLegalCaseContractInfo(int contractId);
    }
}
