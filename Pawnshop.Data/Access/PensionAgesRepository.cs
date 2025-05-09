using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.PensionAge;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class PensionAgesRepository : RepositoryBase
    {
        public PensionAgesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public PensionAge Insert(PensionAge entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PensionAges
                       (Age, IsMale, ActivationDate, CreateDate, AuthorId)
                    VALUES
                       (@Age, @IsMale, @ActivationDate, @CreateDate, @AuthorId)", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
            return entity;
        }

        public PensionAge Update(PensionAge entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PensionAges
                    SET 
                        Age = @Age,
                        IsMale = @IsMale,
                        ActivationDate = @ActivationDate,
                        CreateDate = @CreateDate,
                        AuthorId = @AuthorId,
                        DeleteDate = @DeleteDate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                entity = Get(entity.Id);

                transaction.Commit();
            }

            return entity;

        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PensionAges
                    SET 
                        DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @Id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public PensionAge Get(int id)
        {
            var entity = UnitOfWork.Session.QuerySingleOrDefault<PensionAge>(@"SELECT pa.* FROM PensionAges pa WHERE Id = @Id", new { id }, UnitOfWork.Transaction);
            return entity;
        }

        public PensionAge Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<PensionAge> List(ListQuery listQuery, object query = null)
        {
            var list = UnitOfWork.Session.Query<PensionAge>(@"SELECT * FROM PensionAges WHERE DeleteDate IS NULL", UnitOfWork.Transaction).ToList();
            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public double GetMaleAge()
        {
            var age = UnitOfWork.Session.QuerySingleOrDefault<double>(@"
                    SELECT TOP 1 Age
                    FROM PensionAges
                    WHERE ActivationDate < dbo.GETASTANADATE()
                    AND IsMale = 1
                    AND DeleteDate IS NULL
                    ORDER BY ActivationDate DESC", new { id = 0 }, UnitOfWork.Transaction);
            return age;
        }

        public double GetFemaleAge()
        {
            var age = UnitOfWork.Session.QuerySingleOrDefault<double>(@"
                    SELECT TOP 1 Age
                    FROM PensionAges
                    WHERE ActivationDate < dbo.GETASTANADATE()
                    AND IsMale = 0
                    AND DeleteDate IS NULL
                    ORDER BY ActivationDate DESC", new { id = 0 } , UnitOfWork.Transaction);
            return age;
        }
    }
}