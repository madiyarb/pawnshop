using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientAdditionalIncomeHistory;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public interface IClientAdditionalIncomeService
    {
        List<ClientAdditionalIncome> Get(int clientId);
        Task<ListModel<ClientAdditionalIncomeHistory>> GetHistoryFiltered(int clientId,ClientAdditionalIncomeHistoryQuery query);
        List<ClientAdditionalIncome> Save(int clientId, List<ClientAdditionalIncomeDto> additionalIncomes);
        void DeleteIncome(int id);
        void RemoveAdditionalIncomesAfterSign(int contractId, int clientId);
    }
}
