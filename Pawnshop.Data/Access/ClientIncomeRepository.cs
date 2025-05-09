using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientIncomeHistory;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ClientIncomeRepository : RepositoryBase
    {
        public ClientIncomeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<int> GetNotActualIncomesByClientId(int clientId, int incomeType)
        {
            return UnitOfWork.Session.Query<int>(@"
                  select Id from ClientIncomes
                  where ClientId = @clientId and
                        IncomeType = @incomeType and
		                DeleteDate is null and
		                CAST(CreateDate AS DATE) < DATEADD(WEEK, -1, CAST(dbo.GETASTANADATE() AS DATE))",
                  new { clientId, incomeType }, UnitOfWork.Transaction).ToList();
        }

        public List<ClientIncome> GetListByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientIncome>(@"
                SELECT ci.*
                FROM ClientIncomes ci
                WHERE ci.ClientId = @clientId AND ci.DeleteDate IS NULL",
            new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public List<ClientIncome> GetListByClientIdAndIncomeType(int clientId, int incomeType)
        {
            return UnitOfWork.Session.Query<ClientIncome, FileRow, ClientIncome>(@"
                SELECT ci.*, f.*
                FROM ClientIncomes ci
                LEFT JOIN FileRows f ON f.Id = ci.FileRowId
                WHERE ci.ClientId = @clientId AND ci.IncomeType = @incomeType AND ci.DeleteDate IS NULL",
                (ci, f) =>
                {
                    ci.FileRow = f;
                    return ci;
                },
                new { clientId, incomeType }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "IncomeType",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ClientIncomes
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }

        public void Delete(int id,
            int? operationAuthorId = null,
            string logReason = null)
        {
            using var transaction = BeginTransaction();
            var entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Запись ClientIncomes с Id {id} не найдена");

            entity.DeleteDate = DateTime.Now;
            UnitOfWork.Session.Update(entity, UnitOfWork.Transaction);
            LogChanges(
               entity,
               OperationType.SoftDelete,
               operationAuthorId,
               logReason);
            transaction.Commit();
        }

        public void Delete(ClientIncome entity,
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

        public ClientIncome Find(object query)
        {
            throw new NotImplementedException();
        }

        public ClientIncome Get(int id)
        {
            return UnitOfWork.Session.Get<ClientIncome>(id, UnitOfWork.Transaction);
        }

        public void Insert(ClientIncome entity, int? operationAuthorId = null)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = (int)UnitOfWork.Session.Insert(entity, UnitOfWork.Transaction);
                LogChanges(entity, OperationType.Insert, operationAuthorId);
                transaction.Commit();
            }
        }

        public List<ClientIncome> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "IncomeType",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ClientIncome>($@"
                SELECT *
                FROM ClientIncomes
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public void Update(
            ClientIncome entity,
            int? operationAuthorId = null,
            string logReason = null,
            OperationType operationType = OperationType.Update)
        {
            using var transaction = BeginTransaction();
            UnitOfWork.Session.Update(entity, UnitOfWork.Transaction);

            LogChanges(entity, operationType, operationAuthorId, logReason);
            transaction.Commit();
        }

        public void LogChanges(
            ClientIncome entity,
            OperationType operationType,
            int? operationAuthorId = null,
            string logReason = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = ClientIncomeLog.MapFromBaseEntity(entity);
            log.OperationAuthorId = operationAuthorId != null ? operationAuthorId.Value : entity.AuthorId;
            log.OperationType = operationType;
            log.LogReason = logReason;

            UnitOfWork.Session.Insert(log, UnitOfWork.Transaction);
        }

        public async Task<int> GetHistoryCountByFilterData(int clientId, ClientIncomeHistoryQuery query)
        {
            using var transaction = BeginTransaction();
            var predicateList = new List<string>();
            var pre = string.Empty;

            if (clientId != default)
            {
                predicateList.Add("cil.ClientId = @clientId");
            }

            if (!string.IsNullOrEmpty(query.DomainCode))
            {
                predicateList.Add("dv.DomainCode = @domainCode");
            }

            if (predicateList.Any())
            {
                pre = $"where {string.Join(" and ", predicateList)}";
            }

            var sqlQuery = $@"
					              SELECT count(*)
                                  FROM ClientIncomeLogs cil
                                  LEFT JOIN DomainValues dv on cil.ConfirmationDocumentTypeId = dv.Id
                                  {pre}";

            return await UnitOfWork.Session.ExecuteScalarAsync<int>(sqlQuery, new
            {
                clientId,
                domainCode = query.DomainCode,
            }, UnitOfWork.Transaction);
        }

        public async Task<List<ClientIncomeHistory>> GetHistoryByFilterData(int clientId, ClientIncomeHistoryQuery query)
        {
            using var transaction = BeginTransaction();
            var predicateList = new List<string>();
            var pre = string.Empty;

            if (clientId != default)
            {
                predicateList.Add("cil.ClientId = @clientId");
            }

            if (!string.IsNullOrEmpty(query.DomainCode))
            {
                predicateList.Add("dv.DomainCode = @domainCode");
            }

            if (predicateList.Any())
            {
                pre = $"where {string.Join(" and ", predicateList)}";
            }

            var sqlQuery = $@"
                        SELECT cil.*,
                            c.ContractNumber as ContractNumber,
                            dv.Name AS ConfirmationDocumentName,
                            authors.FullName as OperationAuthorFullName,
                            ROW_NUMBER() OVER (ORDER BY cil.id desc) AS RowNum,
                            f.*
                        FROM ClientIncomeLogs cil
                        LEFT JOIN Contracts c on cil.ContractId = c.Id
                        LEFT JOIN Users authors on cil.OperationAuthorId = authors.Id
                        LEFT JOIN DomainValues dv on cil.ConfirmationDocumentTypeId = dv.Id
                        LEFT JOIN FileRows f on cil.FileRowId = f.Id
					    {pre}
                        ORDER BY cil.id desc
                        OFFSET @offset ROWS
					    FETCH NEXT @size ROWS ONLY";

            var clientIncomeHistories = await UnitOfWork.Session.QueryAsync<ClientIncomeHistory, FileRow, ClientIncomeHistory>(sqlQuery,
                (cih, f) =>
                {
                    cih.FileRow = f;
                    return cih;
                },
                new
                {
                    offset = query.Offset,
                    size = query.Limit,
                    clientId,
                    domainCode = query.DomainCode,
                }, UnitOfWork.Transaction);

            return clientIncomeHistories.ToList();
        }
    }
}
