using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class PenaltyAccrualRepository : RepositoryBase
    {
        public PenaltyAccrualRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public decimal GetCorrectionAmount(int contractId, DateTime accrualDate, int settingId)
        {
            return UnitOfWork.Session.ExecuteScalar<decimal>($@"
SELECT ISNULL(SUM(Amount),0)
FROM UTLPenaltyAccrualCorrection WHERE ContractId = @contractId AND Date <= @accrualDate AND AccountSettingId = @settingId AND DeleteDate IS NULL", 
new {contractId, accrualDate, settingId}, UnitOfWork.Transaction);
        }

        public decimal GetCorrectionAmountOkt2021(int contractId, int boSettingId, DateTime accrualDate)
        {
            return UnitOfWork.Session.ExecuteScalar<decimal>($@"
                SELECT ISNULL(Sum(IIF(StornoId is null, OrderCost, -OrderCost)),0)
                FROM CashOrders WHERE ContractId = @contractId AND BusinessOperationSettingId = @boSettingId AND DeleteDate IS NULL AND OrderDate < @accrualDate",
                new { contractId, boSettingId, accrualDate }, UnitOfWork.Transaction);
        }
    }
}