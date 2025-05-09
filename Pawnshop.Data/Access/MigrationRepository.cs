using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Migration;

namespace Pawnshop.Data.Access
{
    public class MigrationRepository : RepositoryBase
    {
        public MigrationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public MigrationOperationSetting TryToFind(MigrationOperationSettingFilter filter)
        {
            if(filter==null) throw new ArgumentNullException(nameof(filter));

            var condition = "WHERE DeleteDate IS NULL AND BusinessOperationSettingId IS NOT NULL";
            condition += filter.ActionType != null ? " AND ActionType = @actionType" : string.Empty;
            condition += filter.PaymentType != null ? " AND PaymentType = @paymentType" : string.Empty;
            condition += filter.ContractTypeId != null ? " AND ContractTypeId = @contractTypeId" : string.Empty;
            condition += filter.ContractPeriodId != null ? " AND ContractPeriodId = @contractPeriodId" : string.Empty;
            condition += filter.DebitAccountId != null ? " AND DebitAccountId = @debitAccountId" : string.Empty;
            condition += filter.CreditAccountId != null ? " AND CreditAccountId = @creditAccountId" : string.Empty;
            condition += filter.PayTypeId != null ? " AND PayTypeId = @payTypeId" : string.Empty;

            var sql = $"SELECT TOP 1 * FROM MigrationOperationMaps {condition}";

            var result = UnitOfWork.Session.Query<MigrationOperationSetting>(sql, filter, UnitOfWork.Transaction);

            if (!result.Any()) throw new PawnshopApplicationException($"Не найдены настройки миграции для: {JsonConvert.SerializeObject(filter)}, [{sql}]");
            if (result.Count()>1) throw new PawnshopApplicationException($"Слишком много настроек найдены для: {JsonConvert.SerializeObject(filter)}, [{sql}]");

            return result.FirstOrDefault();
        }

        public List<int> FindContractsWithoutAccounts(object query)
        {
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<ContractStatus?>("Status");
            var branchId = query?.Val<int>("BranchId");
            var hasActionsAfterDate = query?.Val<DateTime?>("HasActionsAfterDate");

            var pre = "WHERE c.DeleteDate IS NULL AND NOT EXISTS (SELECT * FROM Accounts WHERE ContractId = c.Id)";
            pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
            pre += status.HasValue ? " AND c.Status = @status" : string.Empty;
            pre += branchId.HasValue ? " AND c.BranchId = @branchId" : string.Empty;
            pre += hasActionsAfterDate.HasValue ? " AND EXISTS (SELECT * FROM ContractActions ca WHERE ca.DeleteDate IS NULL AND ca.ContractId = c.Id AND ca.Date >= @hasActionsAfterDate)" : string.Empty;

            return UnitOfWork.Session.Query<int>($"SELECT Id FROM Contracts c {pre}",
                new
                {
                    beginDate,
                    endDate,
                    status,
                    branchId,
                    hasActionsAfterDate
                }, UnitOfWork.Transaction, commandTimeout: 1500).AsList();
        }

        public List<int> FindAccountsWithRecords(int? accountPlanId)
        {
            var pre = "WHERE a.DeleteDate IS NULL AND EXISTS (SELECT * FROM AccountRecords WHERE AccountId = a.Id)";
            pre += accountPlanId.HasValue ? " AND a.AccountPlanId = @accountPlanId" : "AND AccountPlanId IS NULL";
            return UnitOfWork.Session.Query<int>($"SELECT Id FROM Accounts a {pre}", new { accountPlanId }, transaction: UnitOfWork.Transaction, commandTimeout: 1500).AsList();
        }

        public List<DateTime> FindLateNextPaymentDates(int branchId, DateTime untilDate, List<ContractStatus> statuses)
        {
            var pre = "WHERE NextPaymentDate IS NOT NULL AND BranchId = @branchId AND Status IN @statuses AND NextPaymentDate < dbo.GETASTANADATE()";
            return UnitOfWork.Session.Query<DateTime>($"SELECT DISTINCT NextPaymentDate FROM Contracts {pre}", new { branchId, untilDate, statuses }, UnitOfWork.Transaction, commandTimeout: 1500).AsList();
        }
    }
}
