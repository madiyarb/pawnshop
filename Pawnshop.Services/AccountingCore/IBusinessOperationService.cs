using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.Models.Filters;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.AccountingCore
{
    public interface IBusinessOperationService : IDictionaryWithSearchService<BusinessOperation, BusinessOperationFilter>
    {
        List<(CashOrder, List<AccountRecord>)> Register(IContract contract, DateTime date, string code, Group branch,
            int authorId, IDictionary<AmountType, decimal> amounts, int? payTypeId = null, bool isMigration = false, 
            ContractAction action = null, OrderStatus? orderStatus = null, int? typeId = null, int? orderUserId = null,
            PayOperation payOperation = null, IContract creditLine = null);

        List<(CashOrder, List<AccountRecord>)> Register(DateTime date, string code, int branchId, int authorId, IDictionary<AmountType, decimal> amounts,
                    int payTypeId, OrderStatus? orderStatus = null, int? orderUserId = null, string note = null, int? remittanceBranchId = null, int? clientId = null);

        /// <summary>
        /// без привязки к Contract/ContractAction
        /// </summary>
        /// <returns></returns>
        Task<List<(CashOrder, List<AccountRecord>)>> ExecuteRegistrationAsync(
            DateTime date,
            string businessOperationCode,
            int branchFromId,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            string typeCode,
            int? branchToId = null,
            OrderStatus? orderStatus = null,
            int? orderUserId = null,
            string note = null,
            int? clientId = null);
        
        /// <summary>
        /// /////////c привязкой к Contract/ContractAction
        /// </summary>
        Task<List<(CashOrder, List<AccountRecord>)>> ExecuteRegistrationAsync(
            DateTime date,
            string businessOperationCode,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            string typeCode,
            int remittanceBranchId,
            int? contractActionId,
            Contract? contract,
            OrderStatus? orderStatus = null,
            int? orderUserId = null,
            string note = null,
            int? clientId = null);

        BusinessOperation FindBusinessOperation(int contractTypeId, string code, int branchId, int organizationId);
        string GetOperationCode(ContractActionType actionType, CollateralType collateralType, bool isFromEmployee = false, bool isReceivable = false, bool isInitialFee = false);
        Account FindAccountForOperation(List<Account> accounts, AccountSetting setting, IContract contract, Group branch);
    }
}
