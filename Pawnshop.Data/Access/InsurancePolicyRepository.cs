using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Data.Models.Contracts.Views;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Data.Access
{
    public class InsurancePolicyRepository : RepositoryBase, IRepository<InsurancePolicy>
    {
        private readonly InsurancePoliceRequestRepository _insurancePoliceRequestRepository;

        public InsurancePolicyRepository(IUnitOfWork unitOfWork, InsurancePoliceRequestRepository insurancePoliceRequestRepository) : base(unitOfWork)
        {
            _insurancePoliceRequestRepository = insurancePoliceRequestRepository;
        }

        public void Insert(InsurancePolicy entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO InsurancePolicies(RootContractId, ContractId, StartDate, EndDate, InsuranceAmount, InsurancePremium, PoliceNumber, CreateDate, AuthorId, PoliceRequestId, SurchargeAmount, YearPremium, AlgorithmVersion, EsbdAmount)
                        VALUES(@RootContractId, @ContractId, @StartDate, @EndDate, @InsuranceAmount, @InsurancePremium, @PoliceNumber, @CreateDate, @AuthorId, @PoliceRequestId, @SurchargeAmount, @YearPremium, @AlgorithmVersion, @EsbdAmount)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(InsurancePolicy entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE InsurancePolicies SET RootContractId = @RootContractId, ContractId = @ContractId, StartDate = @StartDate, EndDate = @EndDate, InsuranceAmount = @InsuranceAmount,
                            InsurancePremium= @InsurancePremium, PoliceNumber = @PoliceNumber, PoliceRequestId = @PoliceRequestId, EsbdAmount = @EsbdAmount, DeleteDate = @DeleteDate
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsurancePolicy Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<InsurancePolicy>(@"
                SELECT * 
                    FROM InsurancePolicies
                        WHERE Id = @id
                        AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public InsurancePolicy Find(object query)
        {
            var isCancel = query?.Val<bool>("IsCancel");
            var policeRequestId = query?.Val<int?>("PoliceRequestId");
            var contractId = query?.Val<int?>("ContractId");
            var startDate = query?.Val<DateTime?>("StartDate") ?? DateTime.Now;
            
            var condition = isCancel.HasValue && isCancel.Value ? "WHERE DeleteDate IS NULL" : "WHERE DeleteDate IS NULL AND StartDate = CONVERT(DATE, @startDate)";

            condition += policeRequestId.HasValue ? " AND PoliceRequestId = @policeRequestId" : string.Empty;
            condition += contractId.HasValue ? " AND ContractId = @contractId" : string.Empty;

            var policy = UnitOfWork.Session.Query<InsurancePolicy>($@"
                SELECT * FROM InsurancePolicies {condition} ORDER BY CreateDate DESC", new { policeRequestId, contractId, startDate },
                UnitOfWork.Transaction).FirstOrDefault();

            if (policy != null)
                policy.PoliceRequest = _insurancePoliceRequestRepository.Get(policy.PoliceRequestId);

            return policy;
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsurancePolicies SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<InsurancePolicy> List(ListQuery listQuery, object query = null)
        {
            var rootContractId = query?.Val<int?>("RootContractId");
            var startDate = query?.Val<DateTime?>("StartDate");
            var contractId = query?.Val<int?>("ContractId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += rootContractId.HasValue ? " AND RootContractId = @rootContractId" : string.Empty;
            condition += contractId.HasValue ? " AND ContractId = @contractId" : string.Empty;
            condition += startDate.HasValue ? " AND StartDate >= CONVERT(DATE, @startDate)" : string.Empty;

            var list = UnitOfWork.Session.Query<InsurancePolicy>($@"
                SELECT * FROM InsurancePolicies {condition}", new { rootContractId, contractId },
                UnitOfWork.Transaction).ToList();

            foreach (var policy in list)
                policy.PoliceRequest = _insurancePoliceRequestRepository.Get(policy.PoliceRequestId);

            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var rootContractId = query?.Val<int?>("RootContractId");
            var startDate = query?.Val<DateTime?>("StartDate");
            var contractId = query?.Val<int?>("ContractId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += rootContractId.HasValue ? " AND RootContractId = @rootContractId" : string.Empty;
            condition += startDate.HasValue ? " AND StartDate >= CONVERT(DATE, @startDate)" : string.Empty;
            condition += contractId.HasValue ? " AND ContractId = @contractId" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM InsurancePolicies {condition}", new { rootContractId, contractId },
                    UnitOfWork.Transaction);
        }

        public int GetCountOfAllPoliciesByContractId(int contractId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT Count(*) FROM InsurancePolicies
                        WHERE PoliceRequestId IN (SELECT Id FROM InsurancePoliceRequests WHERE ContractId = @contractId)", new { contractId },
                UnitOfWork.Transaction);
        }

        public bool ContractHasCompletedInsurancePolicy(int contractId)
        {
            var result = UnitOfWork.Session.QueryFirstOrDefault<bool?>(@"SELECT 1
  FROM InsurancePolicies inp
  JOIN InsurancePoliceRequests inpr ON inpr.Id = inp.PoliceRequestId
 WHERE inp.DeleteDate IS NULL
   AND inpr.DeleteDate IS NULL
   AND inpr.Status = 60
   AND inp.ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);

            return result ?? false;
        }

        public InsurancePolicy GetLasted(int contractId)
        {
            return UnitOfWork.Session.Query<InsurancePolicy, InsurancePoliceRequest, InsurancePolicy>(@"SELECT inp.*, inpr.*
  FROM InsurancePolicies inp
  JOIN InsurancePoliceRequests inpr ON inpr.Id = inp.PoliceRequestId
 WHERE inp.DeleteDate IS NULL
   AND inpr.DeleteDate IS NULL
   AND inp.ContractId = @contractId
 ORDER BY inp.Id DESC",
                (inp, inpr) =>
                {
                    if (inp != null)
                        inp.PoliceRequest = inpr;
                    
                    return inp;
                },
                new { contractId }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public List<InsurancePolicy> GetListByPeriod(DateTime beginDate, DateTime endDate)
        {
            var list = UnitOfWork.Session.Query<InsurancePolicy>($@"
                SELECT * FROM InsurancePolicies 
                WHERE DeleteDate IS NULL 
                    AND StartDate BETWEEN @beginDate AND @endDate ", new { beginDate, endDate },
                UnitOfWork.Transaction).ToList();

            return list;
        }

        public InsurancePolicy GetDeleted(string policeNumber)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<InsurancePolicy>(@"
                SELECT ir.* FROM InsurancePolicies ir 
                    JOIN InsurancePoliceRequests ipr ON ipr.Id=ir.PoliceRequestId
                    JOIN InsurancePoliciesClosingReasons ipcr ON ipcr.PoliceRequestId=ipr.Id
                WHERE ipcr.ClosingReasonId=199 AND ipr.Status=40 AND ir.PoliceNumber=@policeNumber", 
                new { policeNumber }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<InsurancePolicyView>> GetInsurancePoliciesOnlineInfo(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("InsurancePolicies.ContractId");
            builder.Select("InsurancePolicies.PoliceNumber");
            builder.Select("InsurancePolicies.InsurancePremium");
            #endregion


            #region Where

            builder.Where("InsurancePolicies.ContractId IN @contractIds", new { contractIds = contractIds });
            builder.Where("DeleteDate IS NULL");

            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM InsurancePolicies 
            /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<InsurancePolicyView>(selector.RawSql, selector.Parameters);
        }

    }
}