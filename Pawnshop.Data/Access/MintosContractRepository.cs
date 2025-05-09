using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;

namespace Pawnshop.Data.Access
{
    public class MintosContractRepository : RepositoryBase, IRepository<MintosContract>
    {
        public MintosContractRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(MintosContract entity)
        {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO MintosContracts ( ContractId, MintosId, MintosPublicId, InvestorInterestRate, MintosStatus, ContractCurrencyId,
MintosCurrencyId, ExchangeRate, UploadDate, DeleteDate, LoanCost, LoanCostAssigned, FinalPaymentDate, OrganizationId )
VALUES ( @ContractId, @MintosId, @MintosPublicId, @InvestorInterestRate, @MintosStatus, @ContractCurrencyId, @MintosCurrencyId,
@ExchangeRate, @UploadDate, @DeleteDate, @LoanCost, @LoanCostAssigned, @FinalPaymentDate, @OrganizationId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                foreach (var item in entity.PaymentSchedule)
                {
                    item.MintosContractId = entity.Id;
                    item.Id = UnitOfWork.Session.Execute(@"
INSERT INTO MintosInvestorPaymentSchedules ( ContractId, MintosContractId, Number, SendedDate, MintosDate, IsRepaid, SendedPrincipalAmount, MintosPrincipalAmount,
PrincipalAmountPaid, SendedInterestAmount, MintosInterestAmount, InterestAmountPaid, SendedDelayedAmount,  MintosDelayedAmount, DelayedAmountPaid, 
SendedTotalSum, MintosTotalSum, TotalSumPaid, SendedTotalRemainingPrincipal, MintosTotalRemainingPrincipal, Status, CancelDate )
VALUES ( @ContractId, @MintosContractId, @Number, @SendedDate, @MintosDate, @IsRepaid, @SendedPrincipalAmount, @MintosPrincipalAmount,
@PrincipalAmountPaid, @SendedInterestAmount, @MintosInterestAmount, @InterestAmountPaid, @SendedDelayedAmount, @MintosDelayedAmount, @DelayedAmountPaid,
@SendedTotalSum, @MintosTotalSum, @TotalSumPaid, @SendedTotalRemainingPrincipal, @MintosTotalRemainingPrincipal, @Status, @CancelDate )
SELECT SCOPE_IDENTITY()", item, UnitOfWork.Transaction);
                }
        }

        public void InsertExtendSchedule(MintosInvestorPaymentScheduleItem entity)
        {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO MintosInvestorPaymentSchedules ( ContractId, MintosContractId, Number, SendedDate, MintosDate, IsRepaid, SendedPrincipalAmount, MintosPrincipalAmount,
PrincipalAmountPaid, SendedInterestAmount, MintosInterestAmount, InterestAmountPaid, SendedDelayedAmount,  MintosDelayedAmount, DelayedAmountPaid, 
SendedTotalSum, MintosTotalSum, TotalSumPaid, SendedTotalRemainingPrincipal, MintosTotalRemainingPrincipal, Status, CancelDate )
VALUES ( @ContractId, @MintosContractId, @Number, @SendedDate, @MintosDate, @IsRepaid, @SendedPrincipalAmount, @MintosPrincipalAmount,
@PrincipalAmountPaid, @SendedInterestAmount, @MintosInterestAmount, @InterestAmountPaid, @SendedDelayedAmount, @MintosDelayedAmount, @DelayedAmountPaid,
@SendedTotalSum, @MintosTotalSum, @TotalSumPaid, @SendedTotalRemainingPrincipal, @MintosTotalRemainingPrincipal, @Status, @CancelDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(MintosContract entity)
        {
                UnitOfWork.Session.Execute(@"
UPDATE MintosContracts
SET MintosStatus = @MintosStatus, UploadDate = @UploadDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                foreach (var item in entity.PaymentSchedule)
                {
                    item.MintosContractId = entity.Id;
                    item.Id = UnitOfWork.Session.Execute(@"
UPDATE MintosInvestorPaymentSchedules
SET Status = @Status, CancelDate = @CancelDate, PrincipalAmountPaid = @PrincipalAmountPaid, InterestAmountPaid = @InterestAmountPaid,
DelayedAmountPaid = @DelayedAmountPaid, TotalSumPaid = @TotalSumPaid, IsRepaid = @IsRepaid
WHERE Id = @Id", item, UnitOfWork.Transaction);
                }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE MintosContracts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void DeleteSchedule(int mintosContractId)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"DELETE FROM MintosInvestorPaymentSchedules WHERE MintosContractId = @mintosContractId", new { mintosContractId }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public MintosContract Get(int id)
        {
            var contract = UnitOfWork.Session.Query<MintosContract, Currency, Currency, MintosContract>(@"
SELECT mc.*, cc.*, cm.*
FROM MintosContracts mc
JOIN Currencies cm ON mc.ContractCurrencyId = cm.Id
JOIN Currencies cc ON cc.Id=mc.MintosCurrencyId
WHERE mc.Id = @id", (mc, c, o) => {
                mc.ContractCurrency = c;
                mc.MintosCurrency = o;
                return mc;
            }, new { id }).FirstOrDefault();

            contract.PaymentSchedule = UnitOfWork.Session.Query<MintosInvestorPaymentScheduleItem>(@"
SELECT *
FROM MintosInvestorPaymentSchedules
WHERE MintosContractId = @id", new { id = contract.Id }, UnitOfWork.Transaction).ToList();

            return contract;
        }

        public List<MintosContract> GetByContractId(int id)
        {
            return UnitOfWork.Session.Query<MintosContract>(@"
SELECT *
FROM MintosContracts mc
WHERE mc.ContractId = @id", new { id }, UnitOfWork.Transaction).ToList();
        }

        public MintosContract Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<MintosContract> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var status = query?.Val<string>("Status");

            var pre = "mc.DeleteDate IS NULL";
            pre += !String.IsNullOrEmpty(status) ? " AND mc.MintosStatus like @status" : string.Empty;

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<MintosContract, Currency, Currency, MintosContract>($@"
SELECT mc.*, cc.*, cm.*
FROM MintosContracts mc
JOIN Currencies cm ON mc.ContractCurrencyId = cm.Id
JOIN Currencies cc ON cc.Id=mc.MintosCurrencyId
{condition} {page}", (mc, c, o) =>
            {
                mc.ContractCurrency = c;
                mc.MintosCurrency = o;
                return mc;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                status
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var status = query?.Val<string>("Status");

            var pre = "mc.DeleteDate IS NULL";
            pre += !String.IsNullOrEmpty(status) ? " AND mc.MintosStatus like @status" : string.Empty;

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM MintosContracts mc
{condition}", new
            {
                listQuery.Filter,
                status
            }, UnitOfWork.Transaction);
        }

        public bool CheckForBlackList(int contractId)
        {
            return UnitOfWork.Session.ExecuteScalar<bool>(@"
SELECT IIF(COUNT(*)>0,1,0)
FROM MintosBlackList
WHERE LockUntilDate>dbo.GETASTANADATE() AND ContractId = @id", new { id = contractId }, UnitOfWork.Transaction);
        }

        public List<MintosValidationResultModel> ValidateContractsWithMintos(List<MintosValidationModel> all)
        {
            var dt = new DataTable();
            var fields = all.FirstOrDefault().GetType().GetFields(BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Public);
            foreach (var field in fields)
            {
                dt.Columns.Add(field.Name, field.FieldType);
            }

            foreach (var item in all)
            {
                var values = item.GetType().GetFields(BindingFlags.Instance |
                                                      BindingFlags.Static |
                                                      BindingFlags.NonPublic |
                                                      BindingFlags.Public).Select(field => field.GetValue(item)).ToArray();
                dt.Rows.Add(values);
            }

            return UnitOfWork.Session.Query<MintosValidationResultModel>(@"
DECLARE @temp AS TABLE(
MintosContractId int,
ContractId int, 
MintosId int,
MintosStatus nvarchar(100),
UploadDate DATETIME2
)

INSERT INTO @temp(MintosContractId, ContractId, MintosId, MintosStatus, UploadDate)
SELECT Id, ContractId, MintosId, MintosStatus, UploadDate FROM MintosContracts


--Не сохранился договор у нас
SELECT t.*, 10 AS ErrorCode, mc.*, NULL AS ContractActionId FROM @t as t
LEFT JOIN @temp mc ON mc.MintosId = t.id
WHERE mc.MintosContractId IS NULL
UNION
--Ошибка статуса Mintos
SELECT t.*, 20 AS ErrorCode, mc.*, NULL AS ContractActionId  FROM @t as t
LEFT JOIN @temp mc ON mc.MintosId = t.id
WHERE mc.MintosContractId IS NOT NULL AND t.Status != mc.MintosStatus
UNION
--Ошибка даты выгрузки
SELECT t.*, 30 AS ErrorCode, mc.*, NULL AS ContractActionId FROM @t as t
LEFT JOIN @temp mc ON mc.MintosId = t.id
WHERE mc.MintosContractId IS NOT NULL AND CAST(t.CreatedAt AS DATE) != CAST(mc.UploadDate AS DATE)
UNION
--Есть не выгруженная оплата
SELECT t.*, 40 AS ErrorCode, mc.*, ca.Id AS ContractActionId FROM @t as t
LEFT JOIN @temp mc ON mc.MintosId = t.id
JOIN ContractActions ca ON ca.ContractId = mc.ContractId AND ca.DeleteDate IS NULL AND ca.Date>mc.UploadDate AND ca.ActionType IN (10, 20, 30, 40, 80, 90, 110)
LEFT JOIN MintosContractActions mca ON mca.ContractActionId = ca.Id AND mca.DeleteDate IS NULL
WHERE mc.MintosContractId IS NOT NULL AND mca.Id IS NULL AND t.Status='active' AND ca.Date != CAST(dbo.GETASTANADATE() AS DATE)
", new { t = dt.AsTableValuedParameter("dbo.MintosValidationModel") }, UnitOfWork.Transaction).AsList();
        }
    }
}