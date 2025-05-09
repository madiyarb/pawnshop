using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IClientOtherPaymentsInfoService
    {
        List<ContractKdnDetail> GetClientOtherPayments(int contractId);
        List<ContractKdnDetail> GetClientOtherPayments(int contractId, int clientId);
        List<ContractKdnDetailModel> GetClientOtherPaymentsModels(int contractId);
        //List<ContractKdnDetail> Save(List<ContractKdnDetail> clientOtherPaymentsInfo);
        //void Delete(int id);
        decimal GetClientOtherPaymentsVal(int contractId, int clientId, decimal debt, List<string> validationErrors);

        Task<ContractKdnDetailModel> GetClientOtherPaymentsModels(Stream xmlStream, int contractId, int clientId, int subjectTypeId, int AuthorId, bool IsFromAdditionRequest);
        Task UpdateFcbContract(UpdateFcbContractRequest request, int userId);
        Task UpdateContractId(int Id, int newContractId);
        Task<ContractKdnDetailModel> GetExistingContracts(FcbContractsExistsRequest request);
    }
}
