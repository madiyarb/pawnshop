using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractKdnCalculationLogRepository : RepositoryBase, IRepository<ContractKdnCalculationLog>
    {
        public ContractKdnCalculationLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ContractKdnCalculationLog Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractKdnCalculationLog GetByContractId(int contractId, bool isAddition = false)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ContractKdnCalculationLog>(@"
                SELECT *
                FROM ContractKdnCalculationLogs
                WHERE ContractId = @contractId AND DeleteDate IS NULL AND IsAddition = @isAddition", new { contractId, isAddition }, UnitOfWork.Transaction);
        }

        public ContractKdnCalculationLog GetByParentContractId(int parentContractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ContractKdnCalculationLog>(@"
                SELECT *
                FROM ContractKdnCalculationLogs
                WHERE ParentContractId = @parentContractId AND DeleteDate IS NULL", new { parentContractId }, UnitOfWork.Transaction);
        }

        public ContractKdnCalculationLog GetByContractIdAndAddition(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ContractKdnCalculationLog>(@"
                SELECT *
                FROM ContractKdnCalculationLogs
                WHERE ContractId = @contractId AND DeleteDate IS NULL AND IsAddition = 1", new { contractId }, UnitOfWork.Transaction);
        }

        public ContractKdnCalculationLog Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ContractKdnCalculationLog>(@"
                SELECT *
                FROM ContractKdnCalculationLogs
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractKdnCalculationLog entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QueryFirstOrDefault<int>(@"
                    INSERT INTO ContractKdnCalculationLogs ( ContractId, KDNCalculated, AuthorId, CreateDate, IsAddition, IsGambler )
                    VALUES ( @ContractId, @KDNCalculated, @AuthorId, @CreateDate, @IsAddition, @IsGambler )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractKdnCalculationLog> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractKdnCalculationLog entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnCalculationLogs
                    SET ContractId = @ContractId, KDNCalculated = @KDNCalculated, IsKdnPassed = @IsKdnPassed, AuthorId = @AuthorId, IsGambler = @IsGambler
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdateKdnResultOnly(ContractKdnCalculationLog entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnCalculationLogs
                    SET IsKdnPassed = @IsKdnPassed, AuthorId = @AuthorId, KdnError = @KdnError,
                        TotalAspIncome = @TotalAspIncome, TotalContractDebts = @TotalContractDebts, TotalOtherClientPayments = @TotalOtherClientPayments,
                        TotalIncome = @TotalIncome, TotalDebt = @TotalDebt, TotalFcbDebt = @TotalFcbDebt, TotalFamilyDebt = @TotalFamilyDebt, KDNCalculated = @KDNCalculated,
                        IsAddition = @IsAddition, ParentContractId = @ParentContractId, UpdateDate = @UpdateDate, 
                        ChildSettingId = @ChildSettingId, ChildLoanPeriod = @ChildLoanPeriod, ChildSubjectId = @ChildSubjectId, PositionEstimatedCost = @PositionEstimatedCost,
                        IsGambler = @IsGambler, IsStopCredit = @IsStopCredit, KDNK4Calculated = @KDNK4Calculated
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public string GetCalculationLogMessage(int contractId)
        {
            var query = $"select top 1 KdnError from [ContractKdnCalculationLogs] WHERE ContractId = {contractId} AND (ContractId != ParentContractId OR ParentContractId is null) order by Id";

            return UnitOfWork.Session.QueryFirstOrDefault<string>(query, UnitOfWork.Transaction);
        }
    }
}
