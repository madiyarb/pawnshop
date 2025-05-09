using Pawnshop.Core.Impl;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Insurances;
using Dapper;
using System.Linq;
using Pawnshop.Core.Exceptions;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Access
{
    public class InsuranceReviseRepository : RepositoryBase, IRepository<InsuranceRevise>
    {
        public InsuranceReviseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InsuranceRevise entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO InsuranceRevises ( CreateDate, Period, InsuranceCompanyId, InsuranceCompanyName, Status, AutorId, AutorName, TotalInsurancePoliciesFinCore, 
                        TotalInsurancePoliciesInsuranceCompany, TotalInsuranceAmountFinCore, TotalInsuranceAmountInsuranceCompany, TotalSurchargeAmountFinCore, 
                        TotalSurchargeAmountInsuranceCompany, TotalAgencyFeesFinCore, TotalAgencyFeesInsuranceCompany, ReturnPolicies, ReturnAgencyFees )
                    VALUES ( @CreateDate, @Period, @InsuranceCompanyId, @InsuranceCompanyName, @Status, @AutorId, @AutorName, @TotalInsurancePoliciesFinCore, 
                        @TotalInsurancePoliciesInsuranceCompany, @TotalInsuranceAmountFinCore, @TotalInsuranceAmountInsuranceCompany, @TotalSurchargeAmountFinCore, 
                        @TotalSurchargeAmountInsuranceCompany, @TotalAgencyFeesFinCore, @TotalAgencyFeesInsuranceCompany, @ReturnPolicies, @ReturnAgencyFees )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }

        public void Update(InsuranceRevise entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE InsuranceRevises
                    SET CreateDate = @CreateDate, Period = @Period, InsuranceCompanyId = @InsuranceCompanyId, InsuranceCompanyName = @InsuranceCompanyName, Status = @Status, 
                        AutorId = @AutorId, AutorName = @AutorName, TotalInsurancePoliciesFinCore = @TotalInsurancePoliciesFinCore, 
                        TotalInsurancePoliciesInsuranceCompany = @TotalInsurancePoliciesInsuranceCompany, TotalInsuranceAmountFinCore = @TotalInsuranceAmountFinCore, 
                        TotalInsuranceAmountInsuranceCompany = @TotalInsuranceAmountInsuranceCompany, TotalSurchargeAmountFinCore = @TotalSurchargeAmountFinCore, 
                        TotalSurchargeAmountInsuranceCompany = @TotalSurchargeAmountInsuranceCompany, TotalAgencyFeesFinCore = @TotalAgencyFeesFinCore, 
                        TotalAgencyFeesInsuranceCompany = @TotalAgencyFeesInsuranceCompany, ReturnPolicies = @ReturnPolicies, ReturnAgencyFees = @ReturnAgencyFees
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                if (entity.Rows != null && entity.Rows.Count > 0)
                {
                    if (entity.Rows != null && entity.Rows.Count > 0)
                    {
                        foreach (var row in entity.Rows)
                        {
                            row.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                            INSERT INTO InsuranceReviseRows ( InsuranceReviseId, InsurancePolicyId, InsurancePolicyNumber, InsuranceStartDate, InsuranceEndDate, SurchargeAmount, 
                                AgencyFees, InsuranceAmount, ClientId, ClientFullName, ClientIdentityNumber, BranchId, BranchName, Status, Message, CreateDate )
                            VALUES ( @InsuranceReviseId, @InsurancePolicyId, @InsurancePolicyNumber, @InsuranceStartDate, @InsuranceEndDate, @SurchargeAmount, @AgencyFees, 
                                @InsuranceAmount, @ClientId, @ClientFullName, @ClientIdentityNumber, @BranchId, @BranchName, @Status, @Message, @CreateDate )
                            SELECT SCOPE_IDENTITY()", row, UnitOfWork.Transaction);
                        }
                    }
                }
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsuranceRevises SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsuranceRevise Get(int id)
        {
            var entity = UnitOfWork.Session.Query<InsuranceRevise>(@"
                SELECT TOP 1 *
                FROM InsuranceRevises i
                WHERE i.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null) 
                throw new PawnshopApplicationException($"Сверка отчетов со страховой компанией не найден по {nameof(InsuranceRevise.Id)} {id}");

            entity.Rows = UnitOfWork.Session.Query<InsuranceReviseRow>(@"
                SELECT *
                FROM InsuranceReviseRows i
                WHERE InsuranceReviseId = @id ORDER BY Status DESC", new { id }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public InsuranceRevise Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<InsuranceRevise> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var insuranceCompanyId = query?.Val<int>("InsuranceCompanyId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND i.CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND i.CreateDate <= @endDate" : string.Empty;
            pre += insuranceCompanyId!=0 ? " AND i.InsuranceCompanyId = @insuranceCompanyId" : string.Empty;

            var condition = listQuery.Like(pre, "i.DeleteDate",
                "i.InsuranceCompanyId",
                "i.CreateDate");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "i.CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<InsuranceRevise>($@"
                SELECT *
                FROM InsuranceRevises i
                {condition} {order} {page}", new
                    {
                        beginDate,
                        endDate,
                        insuranceCompanyId,
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
            var insuranceCompanyId = query?.Val<int>("InsuranceCompanyId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND i.CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND i.CreateDate <= @endDate" : string.Empty;
            pre += insuranceCompanyId != 0 ? " AND i.InsuranceCompanyId = @insuranceCompanyId" : string.Empty;

            var condition = listQuery.Like(pre, "i.DeleteDate",
                "i.InsuranceCompanyId",
                "i.CreateDate");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM InsuranceRevises i
                {condition}", new
                {
                    beginDate,
                    endDate,
                    insuranceCompanyId,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}
