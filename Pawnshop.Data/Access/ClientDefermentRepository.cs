using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using System;
using System.Linq;
using Pawnshop.Data.Models.ClientDeferments;
using Dapper;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Access
{
    public class ClientDefermentRepository : RepositoryBase
    {
        public ClientDefermentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public bool IsClientDeffered(int clientId, int contractId)
        {
            return UnitOfWork.Session.ExecuteScalar<bool>(@$"
                SELECT 
                    IsRestructured 
                FROM 
                    ClientDeferments 
                WHERE ClientId = @clientId 
                    AND ContractId = @contractId
                    AND DeleteDate IS NULL
                UNION
                SELECT 
                    cd.IsRestructured 
                FROM 
                    dogs.Tranches tr
	                JOIN ClientDeferments cd ON cd.ContractId = tr.Id
                WHERE tr.CreditLineId = @contractId 
	                AND cd.ClientId = @clientId
                    AND cd.DeleteDate IS NULL", new { clientId, contractId }, UnitOfWork.Transaction);
        }

        public ClientDeferment GetContractDeferment(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientDeferment>(@$"
                SELECT cd.Id
                      ,cd.ClientId
                      ,cd.DefermentTypeId
                      ,cd.StartDate
                      ,cd.EndDate
                      ,cd.DeleteDate
                      ,cd.CreateDate
                      ,cd.UpdateDate
                      ,cd.Status
                      ,cd.ContractId
                      ,cd.IsRestructured
                      ,cd.RecruitStatus
                  FROM ClientDeferments cd
                  INNER JOIN Contracts c ON c.Id = cd.ContractId
                  INNER JOIN Clients cl on cl.Id = cd.ClientId
                  WHERE cd.DeleteDate IS NULL
                  AND c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND c.Status = {(int)ContractStatus.Signed}
                  AND cd.ContractId = @contractId", new { contractId}, UnitOfWork.Transaction);
        }

        public IEnumerable<ClientDeferment> GetCreditLineDeferments(int creditLineId)
        {
            return UnitOfWork.Session.Query<ClientDeferment>(@$"
                SELECT cd.Id
                      ,cd.ClientId
                      ,cd.DefermentTypeId
                      ,cd.StartDate
                      ,cd.EndDate
                      ,cd.DeleteDate
                      ,cd.CreateDate
                      ,cd.UpdateDate
                      ,cd.Status
                      ,cd.ContractId
                      ,cd.IsRestructured
                      ,cd.RecruitStatus
                  FROM ClientDeferments cd
                  INNER JOIN dogs.Tranches t ON t.Id = cd.ContractId
                  INNER JOIN Contracts c ON c.Id = t.Id
                  INNER JOIN Clients cl on cl.Id = cd.ClientId
                  WHERE cd.DeleteDate IS NULL
                  AND c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND c.Status = {(int)ContractStatus.Signed}
                  AND t.CreditLineId = @creditLineId", new { creditLineId }, UnitOfWork.Transaction);
        }

        public ClientDeferment GetContractDeferment(int contractId, bool recruitStatus)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientDeferment>(@$"
                SELECT cd.Id
                      ,cd.ClientId
                      ,cd.DefermentTypeId
                      ,cd.StartDate
                      ,cd.EndDate
                      ,cd.DeleteDate
                      ,cd.CreateDate
                      ,cd.UpdateDate
                      ,cd.Status
                      ,cd.ContractId
                      ,cd.IsRestructured
                      ,cd.RecruitStatus
                  FROM ClientDeferments cd
                  INNER JOIN Contracts c ON c.Id = cd.ContractId
                  INNER JOIN Clients cl on cl.Id = cd.ClientId
                  WHERE cd.DeleteDate IS NULL
                  AND c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND c.Status = {(int)ContractStatus.Signed}
                  AND cd.RecruitStatus = @recruitStatus
                  AND cd.ContractId = @contractId", new { contractId, recruitStatus }, UnitOfWork.Transaction);
        }

        public IEnumerable<ClientDeferment> GetClientDeferment(int clientId, bool recruitStatus)
        {
            return UnitOfWork.Session.Query<ClientDeferment>(@$"
                SELECT cd.Id
                      ,cd.ClientId
                      ,cd.DefermentTypeId
                      ,cd.StartDate
                      ,cd.EndDate
                      ,cd.DeleteDate
                      ,cd.CreateDate
                      ,cd.UpdateDate
                      ,cd.Status
                      ,cd.ContractId
                      ,cd.IsRestructured
                      ,cd.RecruitStatus
                  FROM ClientDeferments cd
                  INNER JOIN Contracts c ON c.Id = cd.ContractId
                  INNER JOIN Clients cl on cl.Id = cd.ClientId
                  WHERE cd.DeleteDate IS NULL
                  AND c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND c.Status = {(int)ContractStatus.Signed}
                  AND cd.RecruitStatus = @recruitStatus
                  AND cd.ClientId = @clientId", new { clientId, recruitStatus }, UnitOfWork.Transaction);
        }

        public List<ClientDeferment> Find(ListQuery listQuery, object query = null)
        {
            var predicate = new StringBuilder();
            predicate.Append("WHERE cd.DeleteDate IS NULL");
            var order = listQuery != null ? @"
ORDER BY
CASE
    WHEN cd.Status = 1 AND cd.StartDate IS NOT NULL AND cd.EndDate IS NULL AND cd.CreateDate >= DATEADD(week, -1, dbo.GETASTANADATE()) THEN 1
    ELSE 4
END,
CASE
    WHEN cd.Status = 0 AND cd.StartDate IS NOT NULL AND cd.EndDate IS NOT NULL AND cd.CreateDate >= DATEADD(week, -1, dbo.GETASTANADATE()) THEN 2
    ELSE 4
END,
CASE
    WHEN cd.Status = 0 AND cd.StartDate IS NULL AND cd.EndDate IS NOT NULL AND cd.CreateDate >= DATEADD(week, -1, dbo.GETASTANADATE()) THEN 3
    ELSE 4
END,
CASE
    WHEN cd.CreateDate < DATEADD(week, -1, GETDATE()) THEN 4
    ELSE 5
END,
cd.id DESC" : "";
            var filter = listQuery != null && !string.IsNullOrEmpty(listQuery.Filter) ? $" AND (cl.FullName LIKE N'%{listQuery.Filter}%' OR cl.IdentityNumber LIKE '%{listQuery.Filter}%')" : "";

            var id = query?.Val<int?>("Id");
            var clientId = query?.Val<int?>("ClientId");
            var defermentTypeId = query?.Val<int?>("DefermentTypeId");
            var recruitStatus = query?.Val<bool?>("RecruitStatus");
            var contractId = query?.Val<int?>("ContractId");
            var createDate = query?.Val<DateTime?>("CreateDate");
            var status = query?.Val<short?>("Status");
            var startDate = query?.Val<DateTime?>("StartDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var updateDate = query?.Val<DateTime?>("UpdateDate");

            predicate.Append(id.HasValue ? " AND cd.Id = @id" : string.Empty);
            predicate.Append(clientId.HasValue ? " AND cd.ClientId = @clientId" : string.Empty);
            predicate.Append(defermentTypeId.HasValue ? " AND cd.DefermentTypeId = @defermentTypeId" : string.Empty);
            predicate.Append(recruitStatus.HasValue ? $" AND cd.RecruitStatus = @RecruitStatus AND con.Status IN ({(int)ContractStatus.Signed})" : string.Empty);
            predicate.Append(contractId.HasValue ? " AND cd.ContractId = @contractId" : string.Empty);
            predicate.Append(status.HasValue ? " AND cd.Status = @Status" : string.Empty);
            predicate.Append(createDate.HasValue ? " AND cd.CreateDate = @createDate" : string.Empty);
            predicate.Append(startDate.HasValue ? " AND cd.StartDate >= @startDate" : string.Empty);
            predicate.Append(endDate.HasValue ? " AND cd.EndDate <= @endDate" : string.Empty);
            predicate.Append(updateDate.HasValue ? " AND cd.UpdateDate = @updateDate" : string.Empty);
            predicate.Append(filter);

            var offset = $"OFFSET {listQuery.Page.Offset} ROWS";
            var limit = $"FETCH NEXT {listQuery.Page.Limit} ROWS ONLY";

            return UnitOfWork.Session.Query<ClientDeferment>(@$"
                SELECT
	                cd. *
                FROM
	                ClientDeferments cd
	                JOIN Clients cl ON cd.ClientId = cl.Id
                    JOIN Contracts con ON cd.ContractId = con.Id
                    {predicate}
                    {order}
                    {offset}
                    {limit}

            ", new { 
                id, 
                clientId, 
                defermentTypeId, 
                status, 
                contractId, 
                createDate,
                startDate, 
                endDate, 
                updateDate,
                recruitStatus
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var predicate = new StringBuilder();
            predicate.Append("WHERE cd.DeleteDate IS NULL");
            var filter = listQuery != null && !string.IsNullOrEmpty(listQuery.Filter) ? $" AND (cl.FullName LIKE N'%{listQuery.Filter}%' OR cl.IdentityNumber LIKE '%{listQuery.Filter}%')" : "";

            var id = query?.Val<int?>("Id");
            var clientId = query?.Val<int?>("ClientId");
            var defermentTypeId = query?.Val<int?>("DefermentTypeId");
            var status = query?.Val<int?>("Status");
            var recruitStatus = query?.Val<bool?>("RecruitStatus");
            var contractId = query?.Val<int?>("ContractId");
            var createDate = query?.Val<DateTime?>("CreateDate");
            var startDate = query?.Val<DateTime?>("StartDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var updateDate = query?.Val<DateTime?>("UpdateDate");

            predicate.Append(id.HasValue ? " AND cd.Id = @id" : string.Empty);
            predicate.Append(clientId.HasValue ? " AND cd.ClientId = @clientId" : string.Empty);
            predicate.Append(defermentTypeId.HasValue ? " AND cd.DefermentTypeId = @defermentTypeId" : string.Empty);
            predicate.Append(status.HasValue ? $" AND cd.Status = @status AND con.Status IN ({(int)ContractStatus.Signed})" : string.Empty);
            predicate.Append(contractId.HasValue ? " AND cd.ContractId = @contractId" : string.Empty);
            predicate.Append(createDate.HasValue ? " AND cd.CreateDate = @createDate" : string.Empty);
            predicate.Append(startDate.HasValue ? " AND cd.StartDate >= @startDate" : string.Empty);
            predicate.Append(endDate.HasValue ? " AND cd.EndDate <= @endDate" : string.Empty);
            predicate.Append(updateDate.HasValue ? " AND cd.UpdateDate = @updateDate" : string.Empty);
            predicate.Append(recruitStatus.HasValue ? " AND cd.RecruitStatus = @recruitStatus" : string.Empty);

            predicate.Append(filter);

            var offset = $"OFFSET {listQuery.Page.Offset} ROWS";
            var limit = $"FETCH NEXT {listQuery.Page.Limit} ROWS ONLY";

            return UnitOfWork.Session.ExecuteScalar<int>(@$"
                SELECT 
                    Count(*) 
                FROM 
                    ClientDeferments cd
	                JOIN Clients cl ON cd.ClientId = cl.Id
                    {predicate}
            ", new
            {
                id,
                clientId,
                defermentTypeId,
                status,
                contractId,
                createDate,
                startDate,
                endDate,
                updateDate,
                recruitStatus
            }, UnitOfWork.Transaction);
        }

        public void Insert(ClientDeferment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientDeferments (ClientId, DefermentTypeId, StartDate, EndDate, Status, CreateDate, UpdateDate, ContractId, RecruitStatus, IsRestructured)
                    VALUES (@ClientId, @DefermentTypeId, @StartDate, @EndDate, @Status, @CreateDate, @UpdateDate, @ContractId, @RecruitStatus, @IsRestructured)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientDeferment entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientDeferments SET ClientId = @ClientId, DefermentTypeId = @DefermentTypeId, StartDate = @StartDate, EndDate = @EndDate, DeleteDate = @DeleteDate, 
                        Status = @Status, CreateDate = @CreateDate, UpdateDate = @UpdateDate, ContractId = @ContractId , RecruitStatus = @RecruitStatus
                    WHERE Id = @id",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
