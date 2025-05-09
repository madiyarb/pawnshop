using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.AccountingCore;

namespace Pawnshop.Data.Access
{
    public class ContractRateRepository : RepositoryBase, IRepository<ContractRate>
    {
        public ContractRateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ContractRates(ContractId, RateSettingId, Date, Rate, ActionId, CreateDate, AuthorId)
                        VALUES(@ContractId, @RateSettingId, @Date, @Rate, @ActionId, @CreateDate, @AuthorId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void DeleteAndInsert(List<ContractRate> entities, bool? isFloatingDiscrete = null)
        {
            using (var transaction = BeginTransaction())
            {
                var condition = @"WHERE 
                            ContractId = @ContractId
                            AND Date = @Date
                            AND RateSettingId = @RateSettingId";

                var deleteRate = isFloatingDiscrete == true ?
                    @$"BEGIN
		                	UPDATE ContractRates SET DeleteDate = dbo.GETASTANADATE() WHERE ContractId = @ContractId and RateSettingId = 3
		               END" :
                    string.Empty;

                UnitOfWork.Session.Execute(@$"
                DECLARE @ContractRateId INT

                    SELECT @ContractRateId = Id 
                    FROM ContractRates 
                        {condition} 
                        AND Rate != @Rate

                    IF @ContractRateId > 0
		                BEGIN
		                	UPDATE ContractRates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @ContractRateId
		                END
                    
                    IF NOT EXISTS (
                        SELECT * 
                        FROM ContractRates 
                        {condition} 
                        AND Rate = @Rate
                        AND DeleteDate IS NULL)
                    BEGIN
                       INSERT INTO ContractRates(ContractId, RateSettingId, Date, Rate, ActionId, CreateDate, AuthorId)
                        VALUES(@ContractId, @RateSettingId, @Date, @Rate, @ActionId, @CreateDate, @AuthorId)
                    END
					{deleteRate}
                    ", entities, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractRates SET ContractId = @ContractId, RateSettingId = @RateSettingId, Date = @Date, Rate = @Rate, ActionId = @ActionId
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractRate Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractRate>(@"
                SELECT * 
                    FROM ContractRates
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractRate Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractRate FindRateOnDateByContractAndCode(int contractId, string rateSettingCode, DateTime date)
        {
            return
                UnitOfWork.Session.QuerySingleOrDefault<ContractRate>(@"SELECT TOP 1 *
                                                                FROM ContractRates cr 
                                                                JOIN AccountSettings acs ON cr.RateSettingId = acs.Id 
                                                                    WHERE cr.ContractId = @contractId
                                                                        AND acs.Code = @rateSettingCode
                                                                        AND cr.Date <= @date 
                                                                        AND cr.DeleteDate IS NULL
                                                                ORDER BY Date DESC",
                    new { contractId, rateSettingCode, date }, UnitOfWork.Transaction);
        }

        public ContractRate FindRateOnDateByContractAndRateSettingId(int contractId, int rateSettingId, DateTime date)
        {
            return
                UnitOfWork.Session.Query<ContractRate, AccountSetting, ContractRate>(@"SELECT TOP 1 cr.*, acs.*
                                                                FROM ContractRates cr 
                                                                JOIN AccountSettings acs ON cr.RateSettingId = acs.Id 
                                                                    WHERE cr.ContractId = @contractId
                                                                        AND acs.Id = @rateSettingId
                                                                        AND cr.Date <= @date 
                                                                        AND cr.DeleteDate IS NULL
                                                                ORDER BY cr.Date DESC",
                                                                (cr, acs) =>
                                                                {
                                                                    cr.RateSetting = acs;
                                                                    return cr;
                                                                },
                    new { contractId, rateSettingId, date }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public IEnumerable<ContractRate> FindRateOnDateByFloatingContractAndRateSettingId(int contractId)
        {
            return
                UnitOfWork.Session.Query<ContractRate, AccountSetting, ContractRate>(@"SELECT cr.*, acs.*
  FROM ContractRates cr
  JOIN AccountSettings acs ON cr.RateSettingId = acs.Id
 WHERE acs.IsConsolidated = 0
   AND acs.Code = 'PROFIT'
   AND cr.ContractId = @contractId",
                    (cr, acs) =>
                    {
                        cr.RateSetting = acs;
                        return cr;
                    },
                    new { contractId }, UnitOfWork.Transaction);
        }

        public List<ContractRate> FindLastTwoRatesOnDateByContractAndRateSettingId(int contractId, int rateSettingId, DateTime date)
        {
            return
                UnitOfWork.Session.Query<ContractRate, AccountSetting, ContractRate>(@"SELECT TOP 2 cr.*, acs.*
                                                                FROM ContractRates cr 
                                                                JOIN AccountSettings acs ON cr.RateSettingId = acs.Id 
                                                                    WHERE cr.ContractId = @contractId
                                                                        AND acs.Id = @rateSettingId
                                                                        AND cr.Date <= @date 
                                                                        AND cr.DeleteDate IS NULL
                                                                ORDER BY cr.Date DESC",
                                                                (cr, acs) =>
                                                                {
                                                                    cr.RateSetting = acs;
                                                                    return cr;
                                                                },
                    new { contractId, rateSettingId, date }, UnitOfWork.Transaction).ToList();
        }

        public ContractRate FindRateOnDateByContractAndCodeWithoutBankRate(int contractId, string rateSettingCode, DateTime date)
        {
            return
                UnitOfWork.Session.QuerySingleOrDefault<ContractRate>(@"SELECT TOP 1 * 
                                                                FROM ContractRates cr 
                                                                JOIN AccountSettings acs ON cr.RateSettingId = acs.Id 
                                                                    WHERE cr.ContractId = @contractId
                                                                        AND acs.Code = @rateSettingCode
                                                                        AND cr.Date <= @date 
                                                                        AND cr.DeleteDate IS NULL
                                                                        AND cr.Rate <> @bankRate
                                                                ORDER BY Date DESC",
                    new { contractId, rateSettingCode, date, bankRate = Constants.NBRK_PENALTY_RATE }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractRates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void DeleteContractRateForCancelAction(int actionId)
        {
            var contractRatesForDelete = UnitOfWork.Session.Query<ContractRate>(@"
                SELECT * 
                    FROM ContractRates
                        WHERE ActionId = @actionId", new { actionId }, UnitOfWork.Transaction).ToList();

            if (!contractRatesForDelete.Any())
                throw new NullReferenceException($"Не найдены ставки пени, установленные в действий с Id {actionId} для отмены");

            contractRatesForDelete.ForEach(x =>
            {
                Delete(x.Id);
            });
        }

        public List<ContractRate> List(ListQuery listQuery, object query = null)
        {
            var condition = "WHERE cr.DeleteDate IS NULL";

            var contractId = query?.Val<int?>("ContractId");
            if (contractId.HasValue)
                condition += " AND cr.ContractId = @contractId";

            return UnitOfWork.Session.Query<ContractRate, AccountSetting, ContractRate>($@"
                SELECT cr.*, a.* FROM ContractRates cr
                    JOIN AccountSettings a ON a.Id = cr.RateSettingId
                        {condition} 
                    ORDER BY cr.RateSettingId, cr.Date",
                (cr, a) =>
                {
                    cr.RateSetting = a;
                    return cr;
                },
                new { contractId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var condition = "WHERE cr.DeleteDate IS NULL";

            var contractId = query?.Val<int?>("ContractId");
            if (contractId.HasValue)
                condition += " AND cr.ContractId = @contractId";

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM ContractRates cr
                        JOIN AccountSettings a ON a.Id = cr.RateSettingId {condition}", new { contractId },
                UnitOfWork.Transaction);
        }
    }
}