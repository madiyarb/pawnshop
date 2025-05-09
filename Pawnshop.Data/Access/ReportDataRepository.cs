using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ReportData;
using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Access
{
    public class ReportDataRepository : RepositoryBase
    {
        public ReportDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public void CreateDataForYesterday()
        {
            UnitOfWork.Session.Execute(@"CreateReportDataAndRowsForAll",new { date = DateTime.Today.AddDays(-1) },commandTimeout: 150000, commandType: System.Data.CommandType.StoredProcedure);
        }


        public ReportData Insert(ReportData entity)
        {
            if (entity == null)
                throw new ArgumentException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                var query = @"INSERT INTO ReportData (OrganizationId,BranchId,[Date]) VALUES (@OrganizationId,@BranchId,@Date) SELECT SCOPE_IDENTITY()";
                    entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(query, entity, UnitOfWork.Transaction);
                transaction.Commit();
            }

            return entity;
        }

        public void Update(ReportData entity)
        {

            if (entity == null)
                throw new ArgumentException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                var query = @"UPDATE ReportData SET OrganizationId = @OrganisationId, BranchId = @BranchId, [Date] = @Date WHERE Id = @Id";

                UnitOfWork.Session.Execute(query, entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {

                var query = @"UPDATE ReportData SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @Id";
                UnitOfWork.Session.Execute(query, new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }

        }

        public ReportData Get(int id)
        {
            var entity = UnitOfWork.Session.QuerySingleOrDefault<ReportData>(@"SELECT rd.* FROM ReportData rd WHERE Id = @Id", new { id }, UnitOfWork.Transaction);
            entity.Rows = new List<ReportDataRow>();
            return entity;

        }

        public void Find()
        {
            throw new NotImplementedException();
        }

        public void List()
        {
            throw new NotImplementedException();
        }

        public void Count()
        {
            throw new NotImplementedException();
        }


        public int GetForDelete(int organizationId, int branchId, DateTime date)
        {
            var query = @"SELECT TOP 1 Id FROM ReportData WHERE OrganizationId = @organizationId AND BranchId = @branchId AND [Date] = @date AND DeleteDate IS NULL";
            return  UnitOfWork.Session.QuerySingleOrDefault<int>(query, new { organizationId, branchId, date}, UnitOfWork.Transaction);
        }



        public DateTime Now()
        {
            return UnitOfWork.Session.QuerySingleOrDefault<DateTime>("SELECT dbo.GETASTANADATE()", 0, UnitOfWork.Transaction);
        }
    }
}
