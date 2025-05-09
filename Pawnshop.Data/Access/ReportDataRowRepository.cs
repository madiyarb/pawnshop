using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ReportData;
using System;
using Dapper;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ReportDataRowRepository : RepositoryBase
    {

        public ReportDataRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public int Insert(ReportDataRow entity)
        {
            if (entity == null)
                throw new ArgumentException(nameof(entity));



            using (var transaction = BeginTransaction())
            {
                var query = @"INSERT INTO ReportDataRows (ReportDataId,[Key],[Value]) VALUES(@ReportDataId,@Key,@Value) SELECT SCOPE_IDENTITY()"; ;
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(query, entity, UnitOfWork.Transaction);
                transaction.Commit();
            }

            return entity.Id;
        }

        public void Insert(List<ReportDataRow> entities)
        {
            if (entities == null)
                throw new ArgumentException(nameof(entities));

            foreach(ReportDataRow entity in entities)
            {
                this.Insert(entity);
            }
        }

        public void Update(ReportDataRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                var query = @"UPDATE ReportDataRows SET ReportDataId = @ReportDataId, [Key] = @Key, [Value] =@Value WHERE Id = @Id";

                    UnitOfWork.Session.Execute(query, entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public ReportDataRow Get(int id)
        {
            var query = @"SELECT * FROM ReportDataRow WHERE Id = @Id";
            var entity = UnitOfWork.Session.QuerySingleOrDefault<ReportDataRow>(query,new { id},UnitOfWork.Transaction);

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


    }
}
