using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Core.Extensions;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Access
{
    public class InsurancePoliceRequestRepository : RepositoryBase, IRepository<InsurancePoliceRequest>
    {
        private readonly ClientRepository _clientRepository;

        public InsurancePoliceRequestRepository(IUnitOfWork unitOfWork,
            ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public void Insert(InsurancePoliceRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO InsurancePoliceRequests(ContractId, InsuranceCompanyId, Status, IsInsuranceRequired, CancelledUserId, CancelDate, CancelReason, OnlineRequestId, CreateDate, AuthorId, RequestData, AlgorithmVersion, GUID, RequestDataBPM)
                        VALUES(@ContractId, @InsuranceCompanyId, @Status, @IsInsuranceRequired, @CancelledUserId, @CancelDate, @CancelReason, @OnlineRequestId, @CreateDate, @AuthorId, @RequestData, @AlgorithmVersion, @GUID, @RequestDataBPM)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(InsurancePoliceRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE InsurancePoliceRequests SET ContractId = @ContractId, InsuranceCompanyId = @InsuranceCompanyId, Status = @Status, 
                        IsInsuranceRequired = @IsInsuranceRequired, CancelledUserId = @CancelledUserId, CancelReason = @CancelReason, CancelDate = @CancelDate,
                            OnlineRequestId = @OnlineRequestId, RequestData = @RequestData, GUID = @GUID, RequestDataBPM = @RequestDataBPM
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsurancePoliceRequest Get(int id)
        {
            return UnitOfWork.Session.Query<InsurancePoliceRequest, Client, InsurancePoliceRequest>(@"
                SELECT ipr.*, cl.*  FROM InsurancePoliceRequests ipr
                    LEFT JOIN Clients cl ON ipr.InsuranceCompanyId = cl.Id
                        WHERE ipr.Id = @id", (ipr, cl) =>
            {
                ipr.InsuranceCompany = cl;
                return ipr;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public InsurancePoliceRequest Find(object query)
        {
            var contractId = query?.Val<int?>("ContractId");
            var guid = query?.Val<Guid?>("Guid");
            var isCanceled = query?.Val<bool?>("IsCanceled");
            var statuses = query?.Val<List<int>>("Status");
            var notInStatuses = query?.Val<List<int>>("NotInStatus");
            var isRejected = query?.Val<bool?>("IsRejected");

            var condition = "WHERE ipr.DeleteDate IS NULL";

            condition += contractId.HasValue ? " AND ipr.ContractId = @contractId" : string.Empty;
            condition += guid.HasValue ? " AND ipr.GUID = @Guid" : string.Empty;
            condition += isCanceled.HasValue && isCanceled is false ? " AND ipr.CancelDate IS NULL AND ipr.CancelReason IS NULL AND ipr.CancelledUserId IS NULL" : string.Empty;
            condition += statuses != null && statuses.Any() ? @$" AND ipr.Status IN @statuses" : string.Empty;
            condition += notInStatuses != null && notInStatuses.Any() ? @$" AND ipr.Status NOT IN @notInStatuses" : string.Empty;
            condition += isRejected.HasValue && isRejected is false ? " AND ipr.Status NOT IN (30, 31)" : string.Empty;

            return UnitOfWork.Session.Query<InsurancePoliceRequest, Client, InsurancePoliceRequest>($@"
                SELECT ipr.*, cl.*  FROM InsurancePoliceRequests ipr
                    LEFT JOIN Clients cl ON ipr.InsuranceCompanyId = cl.Id
                {condition}
                ORDER BY ipr.CreateDate DESC",
                (ipr, cl) =>
                {
                    ipr.InsuranceCompany = cl;
                    return ipr;
                },
                new { contractId, statuses, notInStatuses, guid },
                UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsurancePoliceRequests SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<InsurancePoliceRequest> List(ListQuery listQuery, object query = null)
        {
            var contractId = query?.Val<int?>("ContractId");

            if (contractId is null)
                throw new PawnshopApplicationException("Договор не указан");

            var list = UnitOfWork.Session.Query<InsurancePoliceRequest, User, InsurancePoliceRequest>($@"
                    declare @ID int
                    set @ID = isnull((SELECT co.ClosedParentId FROM Contracts co WHERE co.Id = @contractId), @contractId)

                    SELECT cop.ContractNumber, ipr.*
                    ,(case
                        when exists(select * from InsurancePolicies where PoliceRequestId = ipr.Id and DeleteDate is null) then 1
                        else 0
                        end) as InsurancePoliceExists
                    ,(select Date from InsurancePoliciesClosingReasons where PoliceRequestId = ipr.Id and DeleteDate is null) as CanceledDateByClient
                    , u.* 
                    FROM InsurancePoliceRequests ipr 
                    LEFT JOIN Users u ON u.Id = ipr.CancelledUserId
                    JOIN Contracts cop ON ipr.ContractId = cop.Id AND cop.DeleteDate IS NULL
                    WHERE ((cop.ClosedParentId is null and cop.id = @ID) or cop.ClosedParentId = @ID)
                    AND ipr.ContractId <= @contractId
                    AND ipr.DeleteDate IS NULL
                    ORDER BY ipr.Id",
                (ipr, u) =>
                {
                    ipr.CancelledUser = u;
                    return ipr;
                }, new { contractId },
                UnitOfWork.Transaction).ToList();

            foreach (var policeRequest in list)
                policeRequest.InsuranceCompany = _clientRepository.GetClientWithLegalForm(policeRequest.InsuranceCompanyId);

            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var contractId = query?.Val<int?>("ContractId");

            if (contractId is null)
                throw new PawnshopApplicationException("Договор не указан");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    declare @ID int
                    set @ID = isnull((SELECT co.ClosedParentId FROM Contracts co WHERE co.Id = @contractId), @contractId)

                    SELECT ipr.* 
                    FROM InsurancePoliceRequests ipr 
                    JOIN Contracts cop ON ipr.ContractId = cop.Id AND cop.DeleteDate IS NULL
                    WHERE ((cop.ClosedParentId is null and cop.id = @ID) or cop.ClosedParentId = @ID)
                    AND ipr.ContractId <= @contractId
                    AND ipr.DeleteDate IS NULL;", new { contractId },
                    UnitOfWork.Transaction);
        }

        public IEnumerable<InsurancePoliceRequest> GetInsurancePolicyRequestsToCancel()
        {
            var sql = @$"SELECT req.* FROM InsurancePoliceRequests req 
                            INNER JOIN InsurancePolicies pol ON pol.PoliceRequestId = req.Id  
                            INNER JOIN Contracts c ON c.Id = pol.ContractId AND req.ContractId = c.Id  
                            WHERE req.Status in ({(int)InsuranceRequestStatus.Error}, {(int)InsuranceRequestStatus.Completed}) AND c.Status < {(int)ContractStatus.Signed}";

            return UnitOfWork.Session.Query<InsurancePoliceRequest>(sql, UnitOfWork.Transaction);
        }

        public IEnumerable<InsurancePoliceRequest> GetInsurancePolicyRequestsToCancel(int contractId)
        {
            var sql = @$"SELECT req.* FROM InsurancePoliceRequests req 
                            INNER JOIN InsurancePolicies pol ON pol.PoliceRequestId = req.Id  
                            INNER JOIN Contracts c ON c.Id = pol.ContractId AND req.ContractId = c.Id 
                            AND c.Id = {contractId}
                            WHERE req.Status in ({(int)InsuranceRequestStatus.Sent},{(int)InsuranceRequestStatus.Error}, {(int)InsuranceRequestStatus.Completed}) AND c.Status < {(int)ContractStatus.Signed}";

            return UnitOfWork.Session.Query<InsurancePoliceRequest>(sql, UnitOfWork.Transaction);
        }

        public List<InsurancePoliceRequest> GetCancelRequestList(object query)
        {
            var policeStatuses = query?.Val<List<int>>("PoliceStatuses");
            
            var condition = "WHERE ipr.DeleteDate IS NULL AND convert(DATE, ipr.CreateDate) = convert(DATE, dbo.GETASTANADATE())";

            var firstCondition = condition + (policeStatuses != null && policeStatuses.Any() ? " AND ipr.Status IN @policeStatuses" : string.Empty);
            var secondCondition = condition + $" AND ipr.Status NOT IN (30,31,40) AND EXISTS (SELECT * FROM Contracts c WHERE c.DeleteDate IS NOT NULL AND ipr.ContractId=c.id)";
            var thirdCondition = condition + $" AND ipr.Status NOT IN (30,31,40,60) AND ir.Id IS NOT NULL AND ir.DeleteDate IS NULL ";

            var entity = UnitOfWork.Session.Query<InsurancePoliceRequest, Client, Contract, InsurancePoliceRequest>($@"
                SELECT ipr.*, cl.*, ct.*
                    FROM InsurancePoliceRequests ipr
                    LEFT JOIN Clients cl ON ipr.InsuranceCompanyId = cl.Id
                    LEFT JOIN Contracts ct ON ipr.ContractId = ct.Id
                {firstCondition}
                ORDER BY ipr.CreateDate",
                (ipr, cl, ct) =>
                {
                    ipr.InsuranceCompany = cl;
                    ipr.Contract = ct;
                    return ipr;
                },
                new { policeStatuses },
                UnitOfWork.Transaction).ToList();

            var entityWhenContractDeleted = UnitOfWork.Session.Query<InsurancePoliceRequest, Client, Contract, InsurancePoliceRequest>($@"
                SELECT ipr.*, cl.*, ct.*
                    FROM InsurancePoliceRequests ipr
	                LEFT JOIN Clients cl ON ipr.InsuranceCompanyId = cl.Id
	                LEFT JOIN Contracts ct ON ipr.ContractId = ct.Id
	                LEFT JOIN InsurancePolicies ir ON ir.PoliceRequestId=ipr.Id
                {secondCondition}
                ORDER BY ipr.CreateDate",
                (ipr, cl, ct) =>
                {
                    ipr.InsuranceCompany = cl;
                    ipr.Contract = ct;
                    return ipr;
                },
                UnitOfWork.Transaction).ToList();

            var entityWhenHasPolicy = UnitOfWork.Session.Query<InsurancePoliceRequest, Client, Contract, InsurancePoliceRequest>($@"
                select ipr.*, cl.*, ct.*
                    FROM InsurancePoliceRequests ipr
	                LEFT JOIN Clients cl ON ipr.InsuranceCompanyId = cl.Id
	                LEFT JOIN Contracts ct ON ipr.ContractId = ct.Id
	                LEFT JOIN InsurancePolicies ir ON ir.PoliceRequestId=ipr.Id
                {thirdCondition}
	            ORDER BY ipr.CreateDate",
                (ipr, cl, ct) =>
                {
                    ipr.InsuranceCompany = cl; 
                    ipr.Contract = ct;
                    return ipr;
                },
                UnitOfWork.Transaction).ToList();

            var combinedEntity = entity.Union(entityWhenContractDeleted).Union(entityWhenHasPolicy).ToList();

            var hashset = new HashSet<int>();
            combinedEntity.RemoveAll(x => !hashset.Add(x.Id)); ;

            combinedEntity.ForEach(e =>
            {
                e.Contract.Branch = UnitOfWork.Session.QuerySingleOrDefault<Group>(@"
                SELECT *
                  FROM Groups WITH(NOLOCK)
                  JOIN Members ON Members.Id = Groups.Id
                 WHERE Groups.Id = @id", new { id = e.Contract.BranchId }, UnitOfWork.Transaction);
            });

            return combinedEntity;
        }

        public void DeleteInsurancePoliceRequestsByContractId(int contractId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                            UPDATE InsurancePoliceRequests 
                                SET DeleteDate = dbo.GETASTANADATE() 
                                    WHERE ContractId = @contractId
                                    AND Status < 20",
                    new { contractId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}