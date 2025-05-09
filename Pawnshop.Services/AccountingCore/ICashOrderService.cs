using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.AccountingCore
{
    public interface ICashOrderService : IDictionaryWithSearchService<CashOrder, CashOrderFilter>
    {
        IDbTransaction BeginCashOrderTransaction();
        void UndoDelete(int id);
        CashOrder Register(OrderType orderType, Contract contract, ContractAction action, ContractActionRow row, 
            int authorId, Group branch, Currency currency);
        CashOrder Migrate(CashOrder order);
        (CashOrder, List<AccountRecord>) Build(Account debit, Account credit, decimal amount, DateTime date, string reason, string reasonKaz,
            int authorId, Group branch, OrderType orderType, int? clientId = null, int? userId = null, int? contractId = null, int? businessOperationId = null,
            int? businessOperationSettingId = null, OrderStatus? status = null, Currency currency = null, int? contractActionId = null,
            int? payOperationId = null, BusinessOperationSetting businessOperationSetting = null, ProcessingType? processingType = null, long? processingId = null, 
            string note = null);
        
        (CashOrder, List<AccountRecord>) Register((CashOrder, List<AccountRecord>) order, Group branch, bool isMigration = false);
        CashOrder Register(CashOrder order, Group branch);
        CashOrder Cancel(CashOrder order, int authorId, Group branch);
        CashOrder Find(CashOrderFilter filter);
        int RelationCount(int id);
        IDictionary<int, (int, DateTime)> Delete(int id, int authorId, int branchId);
        List<CashOrder> GetCashOrdersForApprove(List<CashOrder> cashOrders);
        decimal GetSumOfCashOrderCostByBusinessOperationSettingCodesAndContractId(List<string> codes, int contractId, DateTime date);

        Task ChangeStatusForRelatedOrders(CashOrder cashOrder, OrderStatus status, int userId, Group branch, bool forSupport);
        Task ChangeStatusForRelatedOrders(int contractActionId, OrderStatus status, int userId, Group branch, bool forSupport);

        Task<(bool, List<int>)> CheckOrdersForConfirmation(int contractActionId);
        decimal GetAccountSettingDebitTurnsByActionIds(List<int> actionIds, string accountSettingCode);
        decimal GetAccountSettingCreditTurnsByActionIds(List<int> actionIds, string accountSettingCode);

        Task<(ContractExpenseRowOrder, List<CashOrder>)> GetRelatedContractExpenseOrders(int orderId);
        Task<(ContractExpenseRowOrder, List<ContractExpenseRowOrder>)> GetRelatedContractExpenseRowOrderList(int orderId);

        Task ChangeStatusForOrders(List<int> relatedContractActions, OrderStatus status, int userId, Group branch, bool forSupport);

        Task<bool> CashOrdersExists(int contractActionId);
        Task<List<int>> GetAllRelatedOrdersByContractActionId(int contractActionId);
        Task ChangeLanguageForRelatedCashOrder(CashOrder orderId, int languageId);
        Task ChangeLanguageForOrders(List<int> relatedContractActions, int languageId);
        Task DeleteCashOrderPrintLanguageForOrder(CashOrder cashOrder);
        Task DeleteCashOrderPrintLanguageForContractActions(List<int> relatedContractActions);
        Task SetLanguageIfNecessary(CashOrder cashOrder, int? languageId);     
        
        /// <summary>
        /// Возвращает общую проведенную сумму ордеров договора по списку бизнес-операций за период
        /// </summary>
        /// <param name="contractId">Id договора</param>
        /// <param name="boOperationSettings">список с кодами бизнес-операций</param>
        /// <param name="startDate">дата начала периода (включительно)</param>
        /// <param name="endDate">дата окончания периода (исключительно)</param>
        /// <returns></returns>
        Task<decimal> GetContractTotalOperationAmount(int contractId, List<string> boOperationSettings, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Возвращает дату последней операции договора из списка бизнес-операций до определенной даты
        /// </summary>
        /// <param name="contractId">Id договора</param>
        /// <param name="boOperationSettings">список с кодами бизнес-операций</param>
        /// <param name="tillDate">дата окончания периода (исключительно)</param>
        /// <returns></returns>
        Task<DateTime> GetContractLastOperationDate(int contractId, List<string> boOperationSettings, DateTime tillDate);
        Task<CashOrder> GetByStornoIdAsync(int stornoId);
        Task CancelWithoutRecalculateAsync(CashOrder order, int authorId, Group branch);
        
        /// <summary>
        /// Получение коллекции ордеров по коллекции Id
        /// </summary>
        Task<IEnumerable<CashOrder>> GetMultipleByIds(IEnumerable<int> cashOrderIds);
    }
}
