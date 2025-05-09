using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class InsuranceRepository : RepositoryBase, IRepository<Insurance>
    {
        private readonly ClientRepository _clientRepository;
        public InsuranceRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public void Insert(Insurance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Insurances ( ContractId, InsuranceNumber, InsuranceCost, InsurancePeriod, BeginDate, EndDate, 
    CashbackCost, PrevInsuranceId, InsuranceData, Status, CreateDate, BranchId, UserId, OwnerId, DeleteDate )
VALUES ( @ContractId, @InsuranceNumber, @InsuranceCost, @InsurancePeriod, @BeginDate, @EndDate, 
    @CashbackCost, @PrevInsuranceId, @InsuranceData, @Status, @CreateDate, @BranchId, @UserId, @OwnerId, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Insurance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Insurances
SET ContractId = @ContractId, InsuranceNumber = @InsuranceNumber, InsuranceCost = @InsuranceCost, InsurancePeriod = @InsurancePeriod, 
    BeginDate = @BeginDate, EndDate = @EndDate, CashbackCost = @CashbackCost, PrevInsuranceId = @PrevInsuranceId, InsuranceData = @InsuranceData, 
    Status = @Status, CreateDate = @CreateDate, BranchId = @BranchId, UserId = @UserId, OwnerId = @OwnerId, DeleteDate = @DeleteDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Insurances SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Insurance Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Insurance, Contract, Group, Insurance>(@"
SELECT TOP 1 i.*, c.*, b.*
FROM Insurances i
JOIN Contracts c ON i.ContractId = c.Id
JOIN Groups b ON i.BranchId = b.Id
WHERE i.Id = @id", (i, c, b) => 
            {
                i.Contract = c;
                i.Contract.Client = _clientRepository.Get(i.Contract.ClientId);
                i.Branch = b;
                return i; 
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null) return entity;

            entity.Actions = UnitOfWork.Session.Query<InsuranceAction>(@"
SELECT ia.*
FROM InsuranceActions ia
WHERE ia.InsuranceId = @insuranceId
    AND ia.DeleteDate IS NULL
ORDER BY ia.ActionDate", new { insuranceId = entity.Id }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public Insurance Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var contractId = query?.Val<int>("ContractId");

            var condition = @"
WHERE i.DeleteDate IS NULL
    AND i.ContractId = @contractId";

            var entity = UnitOfWork.Session.Query<Insurance, Contract, Group, Insurance>($@"
SELECT TOP 1 i.*, c.*, b.*
FROM Insurances i
JOIN Contracts c ON i.ContractId = c.Id
JOIN Groups b ON i.BranchId = b.Id
{condition}", (i, c, b) =>
            {
                i.Contract = c;
                i.Contract.Client = _clientRepository.Get(i.Contract.ClientId);
                i.Branch = b;
                return i;
            }, new { contractId }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null) return entity;

            entity.Actions = UnitOfWork.Session.Query<InsuranceAction>(@"
SELECT ia.*
FROM InsuranceActions ia
WHERE ia.InsuranceId = @insuranceId
    AND ia.DeleteDate IS NULL
ORDER BY ia.ActionDate", new { insuranceId = entity.Id }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public List<Insurance> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<InsuranceStatus?>("Status");
            var ownerId = query?.Val<int>("OwnerId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND i.BeginDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND i.BeginDate <= @endDate" : string.Empty;
            pre += status.HasValue ? " AND i.Status = @status" : string.Empty;
            pre += " AND mr.LeftMemberId = @ownerId";

            var condition = listQuery.Like(pre, "i.InsuranceNumber", "c.ContractNumber", "JSON_VALUE(c.ContractData, '$.Client.FullName')", "JSON_VALUE(c.ContractData, '$.Client.IdentityNumber')");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "i.BeginDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Insurance, Contract, Group, Insurance>($@"
SELECT i.*, c.*, b.*
FROM Insurances i
JOIN MemberRelations mr ON mr.RightMemberId = i.OwnerId
JOIN Contracts c ON i.ContractId = c.Id
JOIN Groups b ON i.BranchId = b.Id
{condition} {order} {page}", (i, c, b) =>
            {
                i.Contract = c;
                i.Contract.Client = _clientRepository.Get(i.Contract.ClientId);
                i.Branch = b;
                return i;
            }, new
            {
                beginDate,
                endDate,
                status,
                ownerId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<InsuranceStatus?>("Status");
            var ownerId = query?.Val<int>("OwnerId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND i.BeginDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND i.BeginDate <= @endDate" : string.Empty;
            pre += status.HasValue ? " AND i.Status = @status" : string.Empty;
            pre += " AND mr.LeftMemberId = @ownerId";

            var condition = listQuery.Like(pre, "i.InsuranceNumber", "c.ContractNumber", "JSON_VALUE(c.ContractData, '$.Client.FullName')", "JSON_VALUE(c.ContractData, '$.Client.IdentityNumber')");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Insurances i
JOIN MemberRelations mr ON mr.RightMemberId = i.OwnerId
JOIN Contracts c ON i.ContractId = c.Id
JOIN Groups b ON i.BranchId = b.Id
{condition}", new
            {
                beginDate,
                endDate,
                status,
                ownerId,
                listQuery.Filter
            });
        }
    }
}
