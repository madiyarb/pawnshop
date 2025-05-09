using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Contracts;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Crm;

namespace Pawnshop.Services.Crm
{
    public interface ICrmUploadService
    {
        Task<dynamic> BitrixAPI(string url, object filter = null, object select = null, int? objectId = null, object fields = null);

        Task<List<CrmContact>> SearchContactInCRM(Client client);

        Task<CrmContact> CreateContactInCRM(Client client);

        Task<bool> UpdateContactsInCrm(int сrmId, Client client);

        Task<int> CreateCrmContract(int clientCrmId, Contract contract);

        Task UpdateCrmContract(Contract contract, CrmContract deal, int? bitrixId = null);

        Task CreateDeal(Contract contract, decimal loanCostLeft, decimal loanPercentCost, decimal penaltyPercentCost, decimal prepayment, decimal buyoutAmount, decimal prolongAmount);

        Task<bool> UpdateDeal(Contract contract, decimal loanCostLeft, decimal loanPercentCost, decimal penaltyPercentCost, decimal prepayment, decimal buyoutAmount, decimal prolongAmount);

        Task<int> CreateOrUpdateContactInCrm(Client client);
    }
}
