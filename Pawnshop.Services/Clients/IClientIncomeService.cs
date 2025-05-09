using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientIncomeHistory;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public interface IClientIncomeService
    {
        List<ClientIncome> GetClientIncomes(int clientId);
        List<ClientIncome> GetClientIncomes(int clientId, int incomeType);
        List<ClientIncome> Save(int clientId, List<ClientIncomeDto> incomes, IncomeType incomeType);
        Task<ListModel<ClientIncomeHistory>> GetHistoryFiltered(int clientId, ClientIncomeHistoryQuery query);
        ClientIncomeDto CalcIncomeAmount(ClientIncomeDto request);
        decimal GetTotalFormalIncome(int clientId);
        decimal GetTotalInformalApprovedIncome(int clientId);
        decimal GetTotalInformalUnapprovedIncome(int clientId);
        decimal GetClientExpenses(int clientId);
        decimal GetClientAllLoanExpense(int clientId);
        decimal GetFamilyDebt(int clientId);
        decimal GetTotalFamilyDebt(int clientId);
        public ClientExpense GetClientFullExpenses(int clientId);
        void SaveClientExpense(int clientId, decimal fcbDebt, decimal familyDebt, decimal avgPaymentToday);
        NotionalRate GetNotionalRate(string domainCode, string domainValuesCode);
        void RemoveIncomesAfterSign(int contractId, int clientId);
    }
}
