using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientAdditionalIncomeHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ClientAdditionalIncomeRepository : RepositoryBase
    {
        public ClientAdditionalIncomeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<int> GetNotActualIncomesIds(int clientId)
        {
            return UnitOfWork.Session.Query<int>(@"
                  select Id from ClientAdditionalIncomes
                  where 
                        ClientId = @clientId and
                        DeleteDate is null and
                        CAST(CreateDate AS DATE) < DATEADD(WEEK, -1, CAST(dbo.GETASTANADATE() AS DATE))",
                  new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public void Insert(
            ClientAdditionalIncome entity,
            int? operationAuthorId = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = (int)UnitOfWork.Session.Insert(entity, UnitOfWork.Transaction);
            LogChanges(entity, OperationType.Insert, operationAuthorId);
        }

        public void Update(
            ClientAdditionalIncome entity,
            int? operationAuthorId = null,
            string logReason = null,
            OperationType operationType = OperationType.Update)
        {
            using var transaction = BeginTransaction();
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Update(entity, UnitOfWork.Transaction);
            LogChanges(entity, operationType, operationAuthorId, logReason);
            transaction.Commit();
        }

        public void Delete(int id,
            int? operationAuthorId = null,
            string logReason = null)
        {
            using var transaction = BeginTransaction();
            var entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Запись ClientAdditionalIncomes с Id {id} не найдена");

            entity.DeleteDate = DateTime.Now;
            UnitOfWork.Session.Update(entity, UnitOfWork.Transaction);
            LogChanges(
               entity,
               OperationType.SoftDelete,
               operationAuthorId,
               logReason);
            transaction.Commit();
        }

        public void Delete(
            ClientAdditionalIncome entity,
            int? operationAuthorId = null,
            string logReason = null)
        {
            using var transaction = BeginTransaction();
            entity.DeleteDate = DateTime.Now;
            UnitOfWork.Session.Update(entity, UnitOfWork.Transaction);
            LogChanges(
                entity,
                OperationType.SoftDelete,
                operationAuthorId,
                logReason);
            transaction.Commit();
        }

        public ClientAdditionalIncome Get(int id)
        {
            return UnitOfWork.Session.Query<ClientAdditionalIncome>(@"
                SELECT cai.*
                FROM ClientAdditionalIncomes cai
                WHERE cai.Id = @id AND cai.DeleteDate IS NULL",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientAdditionalIncome> GetListByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientAdditionalIncome>(@"
                SELECT cai.*
                FROM ClientAdditionalIncomes cai
                WHERE cai.ClientId = @clientId AND cai.DeleteDate IS NULL",
            new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public ClientAdditionalIncome Find(object query)
        {
            throw new NotImplementedException();
        }
        public List<ClientAdditionalIncome> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(
            ClientAdditionalIncome entity,
            OperationType operationType,
            int? operationAuthorId = null,
            string logReason = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = ClientAdditionalIncomeActualLog.MapFromBaseEntity(entity);
            log.OperationAuthorId = operationAuthorId != null ? operationAuthorId.Value : entity.AuthorId;
            log.OperationType = operationType;
            log.LogReason = logReason;

            UnitOfWork.Session.Insert(log, UnitOfWork.Transaction);
        }

        public async Task<int> GetHistoryCountByFilterData(int clientId, ClientAdditionalIncomeHistoryQuery query)
        {
            using var transaction = BeginTransaction();
            var predicateList = new List<string>();
            var pre = string.Empty;

            if (clientId != default)
            {
                predicateList.Add("cal.ClientId = @clientId");
            }

            if (predicateList.Any())
            {
                pre = "where " + string.Join(" and ", predicateList);
            }

            var sqlQuery = $@"
                               SELECT count(*)
                               FROM ClientAdditionalIncomeActualLogs cal
                               {pre}";

            return await UnitOfWork.Session.ExecuteScalarAsync<int>(sqlQuery, new
            {
                clientId,
            }, UnitOfWork.Transaction);
        }

        public async Task<List<ClientAdditionalIncomeHistory>> GetHistoryByFilterData(
            int clientId,
            ClientAdditionalIncomeHistoryQuery query)
        {
            using var transaction = BeginTransaction();
            var predicateList = new List<string>();
            var pre = string.Empty;

            if (clientId != default)
            {
                predicateList.Add("cal.ClientId = @clientId");
            }

            if (predicateList.Any())
            {
                pre += "where " + string.Join(" and ", predicateList);
            }

            var sqlQuery = $@"
                                SELECT cal.*,
                                    c.ContractNumber as ContractNumber,
			                        dv.Name AS ConfirmationDocumentName,
                                    authors.FullName as OperationAuthorFullName,
                                    ROW_NUMBER() OVER (ORDER BY cal.id desc) AS RowNum
                                FROM ClientAdditionalIncomeActualLogs cal
                                LEFT JOIN Contracts c on cal.ContractId = c.Id
                                LEFT JOIN Users authors on cal.OperationAuthorId = authors.Id
		                        LEFT JOIN DomainValues dv on cal.TypeId = dv.Id
                                {pre}
                                ORDER BY cal.Id desc 
                                OFFSET @offset ROWS
								FETCH NEXT @size ROWS ONLY";

            var clientAdditionalIncomeHistories = await UnitOfWork.Session.QueryAsync<ClientAdditionalIncomeHistory>(sqlQuery,
                new
                {
                    offset = query.Offset,
                    size = query.Limit,
                    clientId
                }, UnitOfWork.Transaction);

            return clientAdditionalIncomeHistories.ToList();
        }
    }
}
