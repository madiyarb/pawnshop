using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.MobileApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ApplicationDetailsRepository : RepositoryBase, IRepository<ApplicationDetails>
    {
        public ApplicationDetailsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Insert(ApplicationDetails entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ApplicationDetails (ApplicationId, ContractId, ProdKind, InsuranceRequired, AdditionAmount, PersonalDiscountId, OverIssueAmount, IsFirstTransh, TotalAmount4AllTransh)
                VALUES(@ApplicationId, @ContractId, @ProdKind, @InsuranceRequired, @AdditionAmount, @PersonalDiscountId, @OverIssueAmount, @IsFirstTransh, @TotalAmount4AllTransh)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationDetails Get(int applicationId)
        {
            return UnitOfWork.Session.Query<ApplicationDetails>(@"
                SELECT appDetails.*
                FROM ApplicationDetails appDetails
                WHERE appDetails.ApplicationId = @applicationId
                ORDER BY appDetails.Id DESC",
                new { applicationId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Update(ApplicationDetails entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ApplicationDetails 
                    SET ContractId = @ContractId
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public ApplicationDetails Find(object query)
        {
            var contractId = query?.Val<int?>("ContractId");

            return UnitOfWork.Session.Query<ApplicationDetails>(@"
                SELECT *
                FROM ApplicationDetails ad
                WHERE ad.ContractId = @contractId
                ORDER BY ad.Id DESC",
                new { contractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ApplicationDetails> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int GetCreatedPersonalDiscountId(int applicationId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
            SELECT ISNULL(MAX(appDetails.PersonalDiscountId),'0') PersonalDiscountId
                FROM ApplicationDetails appDetails
                WHERE appDetails.ApplicationId = @applicationId
                GROUP BY appDetails.ApplicationId",
                new { applicationId }, UnitOfWork.Transaction);
        }

        public List<ApplicationDetails> GetAll(int applicationId)
        {
            return UnitOfWork.Session.Query<ApplicationDetails>(@"
                SELECT appDetails.*
                FROM Applications app
                LEFT JOIN ApplicationDetails appDetails ON app.Id=appDetails.ApplicationId
                WHERE app.Id = @applicationId
                AND app.IsAddition=0
                ORDER BY appDetails.Id DESC",
                new { applicationId }, UnitOfWork.Transaction).ToList();
        }
    }
}
